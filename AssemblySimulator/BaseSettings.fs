namespace Simulator

type Address = System.Int16
type Value = System.Int16
type Immediate = System.Int16

type CC =
    | N
    | Z
    | P
    static member Calc (value : Value) =
        if value < 0s then N
        elif value > 0s then P
        else Z

type Register =
    | R0
    | R1
    | R2
    | R3
    | R4
    | R5
    | R6
    | R7

