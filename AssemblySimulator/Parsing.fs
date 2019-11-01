namespace Simulator

open System

type TokenType =
    | ORIG
    | Number of Value
    | FILL
    | BKLW
    | HALT
    | END
    | GETC
    | PUTS
    | IN
    | OUT
    | Reg of int
    | OpADD
    | OpAND
    | OpBR
    | OpBRz
    | OpBRn
    | OpBRp
    | OpBRnp
    | OpBRnz
    | OpBRzp
    | OpBRnzp
    | OpJMP
    | OpJSR
    | OpJSRR
    | OpLD
    | OpLDI
    | OpLDR
    | OpLEA
    | OpNOT
    | OpRET
    | OpST
    | OpSTI
    | OpSTR
    | OpTRAP
    | Label of string
    static member Match = function
        | ".ORIG" -> ORIG
        | ".FILL" -> FILL
        | ".BKLW" -> BKLW
        | "HALT" -> HALT
        | ".END" -> END
        | "GETC" -> GETC
        | "PUTS" -> PUTS
        | "IN" -> IN
        | "R0" -> Reg 0
        | "R1" -> Reg 1
        | "R2" -> Reg 2
        | "R3" -> Reg 3
        | "R4" -> Reg 4
        | "R5" -> Reg 5
        | "R6" -> Reg 6
        | "R7" -> Reg 7
        | "ADD" -> OpADD
        | "AND" -> OpAND
        | "NOT" -> OpNOT
        | "BR" -> OpBR
        | "BRZ" -> OpBRz
        | "BRN" -> OpBRn
        | "BRP" -> OpBRp
        | "BRNP" -> OpBRnp
        | "BRNZ" -> OpBRnz
        | "BRZP" -> OpBRzp
        | "BRNZP" -> OpBRnzp
        | "JMP" -> OpJMP
        | "JSR" -> OpJSR
        | "JSRR" -> OpJSRR
        | "LD" -> OpLD
        | "LDI" -> OpLDI
        | "LDR" -> OpLDR
        | "LEA" -> OpLEA
        | "NOT" -> OpNOT
        | "RET" -> OpRET
        | "ST" -> OpST
        | "STI" -> OpSTI
        | "STR" -> OpSTR
        | "TRAP" -> OpTRAP
        | numStr when numStr.IsInt16() -> Number(numStr.ToInt16())
        | label -> Label label


type ParsedCommand =
    | LabelCommand of string
    | OrigCommand of Address
    | EndCommand
    | FillCommand of Value
    | ArrayCommand of Value
    | OpCodeCommand of OpCode
    static member ParseCommand (tokens : TokenType list) : Option<ParsedCommand * TokenType list> =
        match tokens with
        | OpADD::(Reg dr)::(Reg sr1)::(Reg sr2)::rest   -> Some(OpCodeCommand(ADD(AddRegister(Register.Get dr, Register.Get sr1, Register.Get sr2))), rest)
        | OpADD::(Reg dr)::(Reg sr)::(Number num)::rest -> Some(OpCodeCommand(ADD(AddImmediate(Register.Get dr, Register.Get sr, num))), rest)
        | OpAND::(Reg dr)::(Reg sr1)::(Reg sr2)::rest   -> Some(OpCodeCommand(AND(AndRegister(Register.Get dr, Register.Get sr1, Register.Get sr2))), rest)
        | OpAND::(Reg dr)::(Reg sr)::(Number num)::rest -> Some(OpCodeCommand(AND(AndImmediate(Register.Get dr, Register.Get sr, num))), rest)
        | OpBR::(Label label)::rest    -> Some(OpCodeCommand(BR(BranchOperation(true, true, true, label))), rest)
        | OpBRn::(Label label)::rest   -> Some(OpCodeCommand(BR(BranchOperation(true, false, false, label))), rest)
        | OpBRz::(Label label)::rest   -> Some(OpCodeCommand(BR(BranchOperation(false, true, false, label))), rest)
        | OpBRp::(Label label)::rest   -> Some(OpCodeCommand(BR(BranchOperation(false, false, true, label))), rest)
        | OpBRnz::(Label label)::rest  -> Some(OpCodeCommand(BR(BranchOperation(true, true, false, label))), rest)
        | OpBRnp::(Label label)::rest  -> Some(OpCodeCommand(BR(BranchOperation(true, false, true, label))), rest)
        | OpBRzp::(Label label)::rest  -> Some(OpCodeCommand(BR(BranchOperation(false, true, true, label))), rest)
        | OpBRnzp::(Label label)::rest -> Some(OpCodeCommand(BR(BranchOperation(true, true, true, label))), rest)
        | OpJMP::(Reg rg)::rest -> Some(OpCodeCommand(JMP(JumpOperation(Register.Get rg))), rest)
        | OpJSR::(Label label)::rest -> Some(OpCodeCommand(JSR(JumpSubroutineOperation(label))), rest)
        | OpJSRR::(Reg rg)::rest -> Some(OpCodeCommand(JSRR(JumpSubroutineRegisterOperation(Register.Get rg))), rest)
        | OpLD::(Reg dr)::(Label label)::rest -> Some(OpCodeCommand(LD(LoadOperation(Register.Get dr, label))), rest)
        | OpLDI::(Reg dr)::(Label label)::rest -> Some(OpCodeCommand(LDI(LoadIndirectOperation(Register.Get dr, label))), rest)
        | OpLDR::(Reg dr)::(Reg br)::(Number offset)::rest -> Some(OpCodeCommand(LDR(LoadRegisterOperation(Register.Get dr,Register.Get br, offset))), rest)
        | OpLEA::(Reg dr)::(Label label)::rest -> Some(OpCodeCommand(LEA(LoadEffectiveAddressOperation(Register.Get dr, label))), rest)
        | OpNOT::(Reg dr)::(Reg sr)::rest   -> Some(OpCodeCommand(NOT(NotOperation(Register.Get dr, Register.Get sr))), rest)
        | OpRET::rest   -> Some(OpCodeCommand(RET), rest)
        | OpST::(Reg sr)::(Label label)::rest -> Some(OpCodeCommand(ST(StoreOperation(Register.Get sr, label))), rest)
        | OpSTI::(Reg sr)::(Label label)::rest -> Some(OpCodeCommand(STI(StoreIndirectOperation(Register.Get sr, label))), rest)
        | OpSTR::(Reg sr)::(Reg br)::(Number offset)::rest -> Some(OpCodeCommand(STR(StoreRegisterOperation(Register.Get sr,Register.Get br, offset))), rest)
        | OpTRAP::(Number trapNumber)::rest -> Some(OpCodeCommand(TRAP(TrapOperation(trapNumber))), rest)
        | GETC::rest -> Some(OpCodeCommand(TRAP(TrapOperation(0x20s))), rest)
        | OUT::rest -> Some(OpCodeCommand(TRAP(TrapOperation(0x21s))), rest)
        | PUTS::rest -> Some(OpCodeCommand(TRAP(TrapOperation(0x22s))), rest)
        | IN::rest -> Some(OpCodeCommand(TRAP(TrapOperation(0x23s))), rest)
        | HALT::rest -> Some(OpCodeCommand(TRAP(TrapOperation(0x25s))), rest)
        | FILL::(Number number)::rest -> Some(FillCommand(number), rest)
        | BKLW::(Number number)::rest -> Some(ArrayCommand(number), rest)
        | ORIG::(Number number)::rest -> Some(OrigCommand(number), rest)
        | END::rest -> Some(EndCommand, rest)
        | (Label label)::rest -> Some(LabelCommand label, rest)
        | _ -> None

module Parsing =
    let tokenizeLine (line : string) =
        let lineWithoutComment = line.Split(';').[0]
        let tokens = lineWithoutComment.ToUpper().Split(" \t,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
        tokens |> Array.map TokenType.Match
            
    let parseLines (lines : string []) =
        [ for i = 0 to lines.Length - 1 do
            for tokenType in tokenizeLine(lines.[i]) do
                yield (i, tokenType)]