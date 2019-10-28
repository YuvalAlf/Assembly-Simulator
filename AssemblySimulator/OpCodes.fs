namespace Simulator

open System.Linq.Expressions
    
type Add =
    | AddRegister of dest : Register * source1 : Register * source2 : Register
    | AddImmediate of dest : Register * source1 : Register * immediate : Immediate


type And =
    | AndRegister of dest : Register * source1 : Register * source2 : Register
    | AndImmediate of dest : Register * source1 : Register * immediate : Immediate


type Branch =
    | BranchOperation of n : bool * z : bool * p : bool * label : string
   
type Jump =
    | JumpOperation of Register
    
type JumpSubroutine =
    | JumpSubroutineOperation of label : string
    
type JumpSubroutineRegister =
    | JumpSubroutineRegisterOperation of register : Register

type Load =
    | LoadOperation of dest : Register * label : string

type LoadIndirect =
    | LoadIndirectOperation of dest : Register * label : string

type LoadRegister =
    | LoadRegisterOperation of dest : Register * baseRegister : Register * offset : Immediate

type LoadEffectiveAddress =
    | LoadEffectiveAddressOperation of dest : Register * label : string

type Not =
    | NotOperation of dest : Register * source : Register

type Store =
    | StoreOperation of source : Register * label : string

type StoreIndirect =
    | StoreIndirectOperation of source : Register * label : string

type StoreRegister =
    | StoreRegisterOperation of source : Register * baseRegister : Register * offset : Immediate

type Trap =
    | TrapOperation of Immediate

type OpCode =
    | ADD of Add
    | AND of And
    | BR of Branch
    | JMP of Jump
    | JSR of JumpSubroutine
    | JSRR of JumpSubroutineRegister
    | LD of Load
    | LDI of LoadIndirect
    | LDR of LoadRegister
    | LEA of LoadEffectiveAddress
    | NOT of Not
    | RET
    | ST of Store
    | STI of StoreIndirect
    | STR of StoreRegister
    | TRAP of Trap
