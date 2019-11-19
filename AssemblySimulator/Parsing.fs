namespace Simulator

open System

type TokenType =
    | Number of Value
    | Reg of Register
    | Label of string
    | StringValue of string
    | ORIG
    | FILL
    | BLKW
    | STRINGZ
    | HALT
    | END
    | GETC
    | PUTS
    | IN
    | OUT
    | OpADD
    | OpAND
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
    static member Match = function
        | ".ORIG" -> ORIG
        | ".FILL" -> FILL
        | ".BLKW" -> BLKW
        | "HALT" -> HALT
        | ".STRINGZ" -> STRINGZ
        | ".END" -> END
        | "GETC" -> GETC
        | "PUTS" -> PUTS
        | "IN" -> IN
        | "R0" -> Reg R0
        | "R1" -> Reg R1
        | "R2" -> Reg R2
        | "R3" -> Reg R3
        | "R4" -> Reg R4
        | "R5" -> Reg R5
        | "R6" -> Reg R6
        | "R7" -> Reg R7
        | "ADD" -> OpADD
        | "AND" -> OpAND
        | "NOT" -> OpNOT
        | "BR" -> OpBRnzp
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
        | str when str.[0] = '"' && str.[str.Length - 1] = '"' -> StringValue(str.[1..str.Length - 2])
        | numStr when numStr.IsInt16() -> Number(numStr.ToInt16())
        | label ->
            if label.EndsWith ":" then
                Label label.[0..label.Length - 2]
            else
                Label label


type ParsedCommand =
    | LabelCommand of string
    | OrigCommand of Address
    | EndCommand
    | FillCommandValue of Value
    //| FillCommandLabel of string
    | ArrayCommand of amount : Value * value : Value
    | StringzCommand of string
    | OpCodeCommand of OpCode
    static member ParseCommand (tokens : TokenType list) : Option<ParsedCommand * TokenType list> =
        match tokens with
        | OpADD::(Reg dr)::(Reg sr1)::(Reg sr2)::rest   -> Some(OpCodeCommand(AddRegister(dr, sr1, sr2)), rest)
        | OpADD::(Reg dr)::(Reg sr)::(Number num)::rest -> Some(OpCodeCommand(AddImmediate(dr, sr, num)), rest)
        | OpAND::(Reg dr)::(Reg sr1)::(Reg sr2)::rest   -> Some(OpCodeCommand(AndRegister(dr, sr1, sr2)), rest)
        | OpAND::(Reg dr)::(Reg sr)::(Number num)::rest -> Some(OpCodeCommand(AndImmediate(dr, sr, num)), rest)
        | OpBRn::(Label label)::rest   -> Some(OpCodeCommand(BranchOperation(true, false, false, label)), rest)
        | OpBRz::(Label label)::rest   -> Some(OpCodeCommand(BranchOperation(false, true, false, label)), rest)
        | OpBRp::(Label label)::rest   -> Some(OpCodeCommand(BranchOperation(false, false, true, label)), rest)
        | OpBRnz::(Label label)::rest  -> Some(OpCodeCommand(BranchOperation(true, true, false, label)), rest)
        | OpBRnp::(Label label)::rest  -> Some(OpCodeCommand(BranchOperation(true, false, true, label)), rest)
        | OpBRzp::(Label label)::rest  -> Some(OpCodeCommand(BranchOperation(false, true, true, label)), rest)
        | OpBRnzp::(Label label)::rest -> Some(OpCodeCommand(BranchOperation(true, true, true, label)), rest)
        | OpJMP::(Reg rg)::rest -> Some(OpCodeCommand(JumpOperation(rg)), rest)
        | OpJSR::(Label label)::rest -> Some(OpCodeCommand(JumpSubroutineOperation(label)), rest)
        | OpJSRR::(Reg rg)::rest -> Some(OpCodeCommand(JumpSubroutineRegisterOperation(rg)), rest)
        | OpLD::(Reg dr)::(Label label)::rest -> Some(OpCodeCommand(LoadOperation(dr, label)), rest)
        | OpLDI::(Reg dr)::(Label label)::rest -> Some(OpCodeCommand(LoadIndirectOperation(dr, label)), rest)
        | OpLDR::(Reg dr)::(Reg br)::(Number offset)::rest -> Some(OpCodeCommand(LoadRegisterOperation(dr,br, offset)), rest)
        | OpLEA::(Reg dr)::(Label label)::rest -> Some(OpCodeCommand(LoadEffectiveAddressOperation(dr, label)), rest)
        | OpNOT::(Reg dr)::(Reg sr)::rest   -> Some(OpCodeCommand(NotOperation(dr, sr)), rest)
        | OpRET::rest   -> Some(OpCodeCommand(RET), rest)
        | OpST::(Reg sr)::(Label label)::rest -> Some(OpCodeCommand(StoreOperation(sr, label)), rest)
        | OpSTI::(Reg sr)::(Label label)::rest -> Some(OpCodeCommand(StoreIndirectOperation(sr, label)), rest)
        | OpSTR::(Reg sr)::(Reg br)::(Number offset)::rest -> Some(OpCodeCommand(StoreRegisterOperation(sr, br, offset)), rest)
        | OpTRAP::(Number trapNumber)::rest -> Some(OpCodeCommand(TrapOperation(trapNumber)), rest)
        | GETC::rest -> Some(OpCodeCommand(TrapOperation(0x20s)), rest)
        | OUT::rest -> Some(OpCodeCommand(TrapOperation(0x21s)), rest)
        | PUTS::rest -> Some(OpCodeCommand(TrapOperation(0x22s)), rest)
        | IN::rest -> Some(OpCodeCommand(TrapOperation(0x23s)), rest)
        | HALT::rest -> Some(OpCodeCommand(TrapOperation(0x25s)), rest)
        | FILL::(Number number)::rest -> Some(FillCommandValue(number), rest)
       // | FILL::(Label label)::rest -> Some(FillCommandLabel(label), rest)
        | STRINGZ::(StringValue string)::rest -> Some(StringzCommand(string), rest)
        | BLKW::(Number amount)::(Number value)::rest -> Some(ArrayCommand(amount, value), rest)
        | BLKW::(Number amount)::rest -> Some(ArrayCommand(amount, 0s), rest)
        | ORIG::(Number address)::rest -> Some(OrigCommand(address), rest)
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