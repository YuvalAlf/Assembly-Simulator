namespace Simulator

type OpCode =
    | AddRegister of dest : Register * source1 : Register * source2 : Register
    | AddImmediate of dest : Register * source1 : Register * immediate : Immediate
    | AndRegister of dest : Register * source1 : Register * source2 : Register
    | AndImmediate of dest : Register * source1 : Register * immediate : Immediate
    | BranchOperation of n : bool * z : bool * p : bool * address : Choice<string, int16>
    | JumpOperation of Register
    | JumpSubroutineOperation of label : string
    | JumpSubroutineRegisterOperation of register : Register
    | LoadOperation of dest : Register * label : string
    | LoadIndirectOperation of dest : Register * label : string
    | LoadRegisterOperation of dest : Register * baseRegister : Register * offset : Immediate
    | LoadEffectiveAddressOperation of dest : Register * label : string
    | NotOperation of dest : Register * source : Register
    | StoreOperation of source : Register * label : string
    | StoreIndirectOperation of source : Register * label : string
    | StoreRegisterOperation of source : Register * baseRegister : Register * offset : Immediate
    | TrapOperation of Immediate
    | RET
    | NoOperation
