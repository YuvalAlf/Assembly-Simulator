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
    override reg.ToString() =
        match reg with 
        | R0 -> "R0"
        | R1 -> "R1"
        | R2 -> "R2"
        | R3 -> "R3"
        | R4 -> "R4"
        | R5 -> "R5"
        | R6 -> "R6"
        | R7 -> "R7"
    static member Get = function
        | 0 -> R0
        | 1 -> R1
        | 2 -> R2
        | 3 -> R3
        | 4 -> R4
        | 5 -> R5
        | 6 -> R6
        | 7 -> R7
        | _ -> failwith "Invalid Register!"
    static member All = [|R0; R1; R2; R3; R4; R5; R6; R7|]
    static member AllZeros : Map<Register, Value> =
        Register.All |> Array.fold (fun map reg -> map.Add(reg, 0s)) Map.empty
    static member ToString (registers : Map<Register, Value>) =
        Register.All
        |> Array.map (fun reg -> sprintf "%s=%d" (reg.ToString()) registers.[reg])
        |> Array.fold (fun acc str -> acc + ", " + str) ""
        

