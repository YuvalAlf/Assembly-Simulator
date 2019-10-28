namespace Simulator



type RunningEnvironment(pc : Address,
                        cc : CC,
                        registers : Map<Register, Value>,
                        labels : Map<string, Address>,
                        memmory : Memmory) =
    member env.CurrentOperation = memmory.OpCodeAt pc
    member env.DoOperation() =
        let didHalt, newPc, newCc, newRegisters, newMemmory = env.InvokeOperation env.CurrentOperation
        if didHalt then None
        else Some(RunningEnvironment(newPc, newCc, newRegisters, labels, newMemmory))

    
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