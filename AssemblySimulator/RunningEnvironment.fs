namespace Simulator

open System.IO

type RunningEnvironment(pc : Address,
                        cc : CC,
                        registers : Map<Register, Value>,
                        labels : Map<string, Address>,
                        memmory : Memmory) =
    static member InitEmpty(registers) =
        new RunningEnvironment(0s, CC.Z, registers, Map.empty, Memmory.InitEmpty())
        
    member private env.AddLabel (label, address) =
        new RunningEnvironment(pc, cc, registers, labels.Add(label, address), memmory)
    member private env.WithMemmory newMemmory =
        new RunningEnvironment(pc, cc, registers, labels, newMemmory)
    member private env.WithPc newPc =
        new RunningEnvironment(newPc, cc, registers, labels, memmory)
    member env.Registers = registers
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

    member private env.CreateEnvironment pc commands =
        match commands with
        | [] -> env
        | _  ->
            match ParsedCommand.ParseCommand commands with
            | Some (LabelCommand(label), rest) ->
                env.AddLabel(label, pc).CreateEnvironment pc rest
            | Some (FillCommand(number), rest) ->
                env.WithMemmory(memmory.SetValue(pc, number)).CreateEnvironment (pc + 1s) rest
            | Some (ArrayCommand(number), rest) ->
                env.CreateEnvironment (pc + number) rest
            | Some (OpCodeCommand(opCode), rest) ->
                env.WithMemmory(memmory.SetOpCode(pc, opCode)).CreateEnvironment (pc + 1s) rest
            | _ -> failwith <| "Compilation error at code line 0x" + (pc.ToString("X"))

    member env.SetRegisters (registersValues : (Register * Value) array) =
        let newRegisters = 
            registersValues
            |> Array.fold (fun (regs : Map<Register, Value>) (reg : Register, value : Value) -> regs.Add(reg, value)) registers
        new RunningEnvironment(pc, cc, newRegisters, labels, memmory)
    member env.CurrentOperation = memmory.OpCodeAt pc
    member env.DoOperation() =
        let didHalt, newPc, newCc, newRegisters, newMemmory = env.InvokeOperation env.CurrentOperation
        if didHalt then None
        else Some(RunningEnvironment(newPc, newCc, newRegisters, labels, newMemmory))

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
        | ADD(AddRegister(dr, sr1, sr2)) -> 
            let newRegisters = registers.Add(dr, registers.[sr1] + registers.[sr2])
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | ADD(AddImmediate(dr, sr, imm)) -> 
            let newRegisters = registers.Add(dr, registers.[sr] + imm)
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | AND(AndRegister(dr, sr1, sr2)) -> 
            let newRegisters = registers.Add(dr, registers.[sr1] &&& registers.[sr2])
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | AND(AndImmediate(dr, sr, imm)) -> 
            let newRegisters = registers.Add(dr, registers.[sr] &&& imm)
            let newCC = CC.Calc(newRegisters.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | BR(BranchOperation(n,z,p,label)) -> 
            let nextPc = 
                match cc with
                | N -> if n then labels.[label] else pc + 1s
                | Z -> if z then labels.[label] else pc + 1s
                | P -> if p then labels.[label] else pc + 1s
            (false, nextPc, cc, registers, memmory)
        | JMP(JumpOperation(reg)) -> (false, registers.[reg], cc, registers, memmory)
        | JSR(JumpSubroutineOperation(label)) -> 
            let newRegisters = registers.Add(R7, pc)
            (false, labels.[label], cc, newRegisters, memmory)
        | JSRR(JumpSubroutineRegisterOperation(reg)) -> 
            let newRegisters = registers.Add(R7, pc)
            (false, registers.[reg], cc, newRegisters, memmory)
        | LD(LoadOperation(dr, label)) -> 
            let newRegisters = registers.Add(dr, memmory.ValueAt(labels.[label]))
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | LDI(LoadIndirectOperation(dr, label)) -> 
            let newRegisters = registers.Add(dr, memmory.ValueAt(memmory.ValueAt(labels.[label])))
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | LDR(LoadRegisterOperation(dr, reg, offset)) ->
            let newRegisters = registers.Add(dr, memmory.ValueAt(registers.[reg] + offset))
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | LEA(LoadEffectiveAddressOperation(dr, label)) -> 
            let newRegisters = registers.Add(dr, labels.[label])
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | NOT(NotOperation(dr, sr)) ->  
            let newRegisters = registers.Add(dr, ~~~registers.[sr])
            let newCC = CC.Calc(registers.[dr])
            (false, pc + 1s, newCC, newRegisters, memmory)
        | RET -> (false, registers.[R7] + 1s, cc, registers, memmory)
        | ST(StoreOperation(sr, label)) -> 
            let newMemmory = memmory.SetValue(labels.[label], registers.[sr])
            (false, pc + 1s, cc, registers, newMemmory)
        | STI(StoreIndirectOperation(sr, label)) -> 
            let newMemmory = memmory.SetValue(memmory.ValueAt(labels.[label]), registers.[sr])
            (false, pc + 1s, cc, registers, newMemmory)
        | STR(StoreRegisterOperation(sr, reg, offset)) -> 
            let newMemmory = memmory.SetValue(registers.[reg] + offset, registers.[sr])
            (false, pc + 1s, cc, registers, newMemmory)
        | TRAP(TrapOperation(imm)) ->
            match imm with
            | 0x20s -> failwith "Trap not supprted"
            | 0x21s -> failwith "Trap not supprted"
            | 0x22s -> failwith "Trap not supprted"
            | 0x23s -> failwith "Trap not supprted"
            | 0x24s -> failwith "Trap not supprted"
            | 0x25s -> (true, pc + 1s, cc, registers, memmory)
            | num  -> failwith <| "Unsupported trap " + num.ToString()