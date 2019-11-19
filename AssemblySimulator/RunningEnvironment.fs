namespace Simulator

open System.IO
open System

type RunningEnvironment(pc : Address,
                        cc : CC,
                        registers : Map<Register, Value>,
                        labels : Map<string, Address>,
                        input : char list,
                        output : char list,
                        memory : Memory) =
    static member InitEmpty() =
        new RunningEnvironment(0s, CC.Z, Register.AllZeros, Map.empty, [], [], Memory.InitEmpty())
        
    member env.GetOutput() =
        output
        |> List.rev
        |> List.toArray
        |> fun chs -> new String(chs)
    member env.CurrentOperation = memory.OpCodeAt pc
    member env.Registers = registers
    member env.Memory = memory
    member env.PC = pc
    member env.SetRegister (register, value) =
        let newRegisters = registers.Add(register, value)
        new RunningEnvironment(pc, cc, newRegisters, labels, input, output, memory)
    member env.SetRegisters registersValues =
        registersValues
        |> Seq.fold (fun (e : RunningEnvironment) (reg, value) -> e.SetRegister(reg, value)) env
    member private env.AddLabel (label, address) =
        new RunningEnvironment(pc, cc, registers, labels.Add(label, address), input, output, memory)
    member env.SetInput (newInput : string) =
        new RunningEnvironment(pc, cc, registers, labels, Array.toList(newInput.ToCharArray()), output, memory)
    member private env.WithMemmory newMemory =
        new RunningEnvironment(pc, cc, registers, labels, input, output, newMemory)
    member private env.WithPc newPc =
        new RunningEnvironment(newPc, cc, registers, labels, input, output, memory)
    member env.SetPcAtLabel(label : string) =
        labels.TryFind label
        |> Option.map (fun address -> env.WithPc address)
    member env.LoadCode(path : string) =
        let lines = File.ReadAllLines path
        let tokenizedIndexedLines = Parsing.parseLines lines
        let tokenizedLines = tokenizedIndexedLines |> List.map snd

        match ParsedCommand.ParseCommand tokenizedLines with
        | Some(OrigCommand(orig), rest) ->
            match ParsedCommand.ParseCommand(List.rev rest) with
            | Some(EndCommand, init) ->
                env.WithPc(orig).CreateEnvironment orig (List.rev init)
            | _ -> failwith "Code doesn't end with an end operation"
        | x -> failwith "Code doesn't start with orig"

    member private env.WriteValue value =
        env.WithMemmory(env.Memory.SetValue(pc, value))
           .WithPc(env.PC + 1s)

    member private env.CreateEnvironment pc commands =
        match commands with
        | [] -> env
        | _  ->
            match ParsedCommand.ParseCommand commands with
            | Some (LabelCommand(label), rest) ->
                env.AddLabel(label, pc).CreateEnvironment pc rest
            | Some (FillCommandValue(number), rest) ->
                env.WithMemmory(memory.SetValue(pc, number)).CreateEnvironment (pc + 1s) rest
            | Some (ArrayCommand(amount, value), rest) ->
                Array.init ((int)amount) (fun _ -> value)
                |> Array.fold (fun (e : RunningEnvironment) v -> e.WriteValue v) env
            | Some (StringzCommand(str), rest) ->         
                Array.append (str.ToCharArray()) ("\0".ToCharArray())
                |> Array.map (fun ch -> ch.ToAsciiInt16())
                |> Array.fold (fun (e : RunningEnvironment) v -> e.WriteValue v) env
            | Some (OpCodeCommand(opCode), rest) ->
                env.WithMemmory(memory.SetOpCode(pc, opCode)).CreateEnvironment (pc + 1s) rest
            | _ -> failwith <| "Compilation error at code line 0x" + (pc.ToString("X"))
            
    member env.DoOperation() =
        let didHalt, newPc, newCc, newRegisters, newMemmory = env.InvokeOperation env.CurrentOperation
        if didHalt then None
        else Some(RunningEnvironment(newPc, newCc, newRegisters, labels, input, output, newMemmory))

    member env.Run (maxOperations : int) =
        match maxOperations with
        | 0 -> None
        | _ ->
            match env.DoOperation() with
            | None -> Some(env)
            | Some (envAfterOp) -> envAfterOp.Run(maxOperations - 1)

    //member TestSubroutine (label : string, initRegisters : Map<Register, Value>, maxCycles : int)
        //let initRegisters = initRegisters.Add(R7, 0s)
    member env.InvokeOperation = function
        | AddRegister(dr, sr1, sr2) -> 
            let newRegisters = registers.Add(dr, registers.[sr1] + registers.[sr2])
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | AddImmediate(dr, sr, imm) -> 
            let newRegisters = registers.Add(dr, registers.[sr] + imm)
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | AndRegister(dr, sr1, sr2) -> 
            let newRegisters = registers.Add(dr, registers.[sr1] &&& registers.[sr2])
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | AndImmediate(dr, sr, imm) -> 
            let newRegisters = registers.Add(dr, registers.[sr] &&& imm)
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | BranchOperation(n,z,p,label) -> 
            let nextPc = 
                match cc with
                | N -> if n then labels.[label] else pc + 1s
                | Z -> if z then labels.[label] else pc + 1s
                | P -> if p then labels.[label] else pc + 1s
            (false, nextPc, cc, registers, memory)
        | JumpOperation(reg) -> (false, registers.[reg], cc, registers, memory)
        | JumpSubroutineOperation(label) -> 
            let newRegisters = registers.Add(R7, pc)
            (false, labels.[label], cc, newRegisters, memory)
        | JumpSubroutineRegisterOperation(reg) -> 
            let newRegisters = registers.Add(R7, pc)
            (false, registers.[reg], cc, newRegisters, memory)
        | LoadOperation(dr, label) -> 
            let newRegisters = registers.Add(dr, memory.ValueAt(labels.[label]))
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | LoadIndirectOperation(dr, label) -> 
            let newRegisters = registers.Add(dr, memory.ValueAt(memory.ValueAt(labels.[label])))
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | LoadRegisterOperation(dr, reg, offset) ->
            let newRegisters = registers.Add(dr, memory.ValueAt(registers.[reg] + offset))
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | LoadEffectiveAddressOperation(dr, label) -> 
            let newRegisters = registers.Add(dr, labels.[label])
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | NotOperation(dr, sr) ->  
            let newRegisters = registers.Add(dr, ~~~registers.[sr])
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memory)
        | StoreOperation(sr, label) -> 
            let newMemmory = memory.SetValue(labels.[label], registers.[sr])
            (false, pc + 1s, cc, registers, newMemmory)
        | StoreIndirectOperation(sr, label) -> 
            let newMemmory = memory.SetValue(memory.ValueAt(labels.[label]), registers.[sr])
            (false, pc + 1s, cc, registers, newMemmory)
        | StoreRegisterOperation(sr, reg, offset) -> 
            let newMemmory = memory.SetValue(registers.[reg] + offset, registers.[sr])
            (false, pc + 1s, cc, registers, newMemmory)
        | TrapOperation(imm) ->
            match imm with
            | 0x20s -> failwith "Trap not supprted"
            | 0x21s -> failwith "Trap not supprted"
            | 0x22s -> failwith "Trap not supprted"
            | 0x23s -> failwith "Trap not supprted"
            | 0x24s -> failwith "Trap not supprted"
            | 0x25s -> (true, pc + 1s, cc, registers, memory)
            | num  -> failwith <| "Unsupported trap " + num.ToString()
        | RET -> (false, registers.[R7] + 1s, cc, registers, memory)