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

    member env.CurrentOperation = memory.OpCodeAt pc
    member env.Registers = registers
    member env.Memory = memory
    member env.PC = pc
    member env.GetOutput() =
        output
        |> List.rev
        |> List.toArray
        |> fun chs -> new String(chs)

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
    member private env.WithMemory newMemory =
        new RunningEnvironment(pc, cc, registers, labels, input, output, newMemory)
    member private env.WithPc newPc =
        new RunningEnvironment(newPc, cc, registers, labels, input, output, memory)
    member env.SetPcAtLabel(label : string) =
        labels.TryFind label
        |> Option.map (fun address -> env.WithPc address)
        |> Option.defaultWith (fun () -> failwith <| sprintf "Label %s doesnt exist" label)
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
        env.WithMemory(env.Memory.SetValue(pc, value))
           .WithPc(env.PC + 1s)

    member private env.CreateEnvironment pc commands =
        match commands with
        | [] -> env
        | _  ->
            match ParsedCommand.ParseCommand commands with
            | Some (LabelCommand(label), rest) ->
                env.AddLabel(label, pc).CreateEnvironment pc rest
            | Some (FillCommandValue(number), rest) ->
                env.WithMemory(memory.SetValue(pc, number)).CreateEnvironment (pc + 1s) rest
            | Some (OpCodeCommand(opCode), rest) ->
                env.WithMemory(memory.SetOpCode(pc, opCode)).CreateEnvironment (pc + 1s) rest
            | Some (ArrayCommand(amount, value), rest) ->
                Array.init ((int)amount) (fun _ -> value)
                |> Array.fold (fun (e : RunningEnvironment) v -> e.WriteValue v) env
                |> fun environment -> environment.CreateEnvironment (environment.PC) rest
            | Some (StringzCommand(str), rest) ->         
                Array.append (str.ToCharArray()) ("\0".ToCharArray())
                |> Array.map (fun ch -> ch.ToAsciiInt16())
                |> Array.fold (fun (e : RunningEnvironment) v -> e.WriteValue v) env
                |> fun environment -> environment.CreateEnvironment (environment.PC) rest
            | _ -> failwith <| "Compilation error at code line 0x" + (pc.ToString("X"))
        
    member env.DoOperation () : RunningEnvironment option =
        match env.CurrentOperation with
        | AddRegister(dr, sr1, sr2) -> 
            let newRegisters = registers.Add(dr, registers.[sr1] + registers.[sr2])
            let newCC = CC.Calc(newRegisters.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters, labels, input, output, memory))
        | AddImmediate(dr, sr, imm) -> 
            let newRegisters = registers.Add(dr, registers.[sr] + imm)
            let newCC = CC.Calc(newRegisters.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters, labels, input, output, memory))
        | AndRegister(dr, sr1, sr2) -> 
            let newRegisters = registers.Add(dr, registers.[sr1] &&& registers.[sr2])
            let newCC = CC.Calc(newRegisters.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters, labels, input, output, memory))
        | AndImmediate(dr, sr, imm) -> 
            let newRegisters = registers.Add(dr, registers.[sr] &&& imm)
            let newCC = CC.Calc(newRegisters.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters, labels, input, output, memory))
        | BranchOperation(n,z,p,label) -> 
            let nextPc = 
                match cc with
                | N -> if n then labels.[label] else pc + 1s
                | Z -> if z then labels.[label] else pc + 1s
                | P -> if p then labels.[label] else pc + 1s
            Some(RunningEnvironment(nextPc, cc, registers, labels, input, output, memory))
        | JumpOperation(reg) -> Some(RunningEnvironment(registers.[reg], cc, registers, labels, input, output, memory))
        | JumpSubroutineOperation(label) -> 
            let newRegisters = registers.Add(R7, pc)
            Some(RunningEnvironment(labels.[label], cc, newRegisters, labels, input, output, memory))
        | JumpSubroutineRegisterOperation(reg) -> 
            let newRegisters = registers.Add(R7, pc)
            Some(RunningEnvironment(registers.[reg], cc, newRegisters, labels, input, output, memory))
        | LoadOperation(dr, label) -> 
            let newRegisters = registers.Add(dr, memory.ValueAt(labels.[label]))
            let newCC = CC.Calc(registers.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters,labels, input, output, memory))
        | LoadIndirectOperation(dr, label) -> 
            let newRegisters = registers.Add(dr, memory.ValueAt(memory.ValueAt(labels.[label])))
            let newCC = CC.Calc(registers.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters, labels, input, output, memory))
        | LoadRegisterOperation(dr, reg, offset) ->
            let newRegisters = registers.Add(dr, memory.ValueAt(registers.[reg] + offset))
            let newCC = CC.Calc(registers.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters, labels, input, output, memory))
        | LoadEffectiveAddressOperation(dr, label) -> 
            let newRegisters = registers.Add(dr, labels.[label])
            let newCC = CC.Calc(registers.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters,labels, input, output, memory))
        | NotOperation(dr, sr) ->  
            let newRegisters = registers.Add(dr, ~~~registers.[sr])
            let newCC = CC.Calc(registers.[dr])
            Some(RunningEnvironment(pc + 1s, newCC, newRegisters,labels, input, output, memory))
        | StoreOperation(sr, label) -> 
            let newMemory = memory.SetValue(labels.[label], registers.[sr])
            Some(RunningEnvironment(pc + 1s, cc, registers,labels, input, output, newMemory))
        | StoreIndirectOperation(sr, label) -> 
            let newMemory = memory.SetValue(memory.ValueAt(labels.[label]), registers.[sr])
            Some(RunningEnvironment(pc + 1s, cc, registers,labels, input, output, newMemory))
        | StoreRegisterOperation(sr, reg, offset) -> 
            let newMemory = memory.SetValue(registers.[reg] + offset, registers.[sr])
            Some(RunningEnvironment(pc + 1s, cc, registers,labels, input, output, newMemory))
        | RET -> Some(RunningEnvironment(registers.[R7] + 1s, cc, registers,labels, input, output, memory))
        | TrapOperation(imm) ->
            match imm with
            | 0x20s -> // GETC
                match input with
                | [] -> failwith "GETC with no more input!"
                | ch::restInput ->
                    let newRegisters = registers.Add(R0, ch.ToAsciiInt16()).Add(R7, pc + 1s)
                    Some(RunningEnvironment(pc + 1s, cc, newRegisters, labels, restInput, output, memory))
            | 0x21s -> // OUT
                let newRegisters = registers.Add(R7, pc + 1s)
                let newOutput = registers.[R0].ToChar() :: output
                Some(RunningEnvironment(pc + 1s, cc, newRegisters, labels, input, newOutput, memory))
            | 0x22s -> // PUTS
                let newRegisters = registers.Add(R7, pc + 1s)
                let stringStartingAddress = registers.[R0]
                let newOutput =
                    Seq.initInfinite (fun i -> stringStartingAddress + int16(i))
                    |> Seq.map (fun address -> memory.ValueAt address)
                    |> Seq.takeWhile (fun value -> value <> 0s)
                    |> Seq.map (fun value -> value.ToChar())
                    |> Seq.fold (fun chars ch -> ch::chars) output
                Some(RunningEnvironment(pc + 1s, cc, newRegisters, labels, input, newOutput, memory))
            | 0x23s -> // IN
                match input with
                | [] -> failwith "IN with no more input!"
                | ch::restInput ->
                    let inputValue = ch.ToAsciiInt16()
                    let newRegisters = registers.Add(R0, inputValue).Add(R7, pc + 1s)
                    let newOutput = inputValue.ToChar()::output
                    Some(RunningEnvironment(pc + 1s, cc, newRegisters, labels, restInput, newOutput, memory))
            | 0x24s -> // PUTSP
                failwith "Trap PUTSP not supprted"
            | 0x25s -> None
            | num  -> failwith <| "Unsupported trap " + num.ToString()