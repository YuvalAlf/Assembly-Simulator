
namespace Simulator


type Memory(memory : Map<Address, Choice<OpCode, Value>>) =
    static member InitEmpty() =
        new Memory(Map.empty)
    
    member this.Item address =
        memory
        |> Map.tryFind address
        |> Option.defaultWith (fun () -> printfn "Access to uninitilized address %X" address
                                         Choice2Of2 0s)
        

    member this.ValueAt address =
        match this.[address] with
        | Choice2Of2 number -> number
        | _ -> printfn "No value at address %X, but an opcode" address
               0s
        
    member this.OpCodeAt address =
        match this.[address] with
        | Choice2Of2 number -> printfn "No opcode at address %X, but value %d" address number
                               NoOperation
        | Choice1Of2 opCode -> opCode
    
    member this.SetValue (address, value) =
        new Memory(memory.Add(address, Choice2Of2 value))

    member this.SetOpCode(address, opCode) =
        new Memory(memory.Add(address, Choice1Of2 opCode))

        

