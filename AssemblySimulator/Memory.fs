
namespace Simulator


type Memory(memory : Map<Address, Choice<OpCode, Value>>) =
    static member InitEmpty() =
        new Memory(Map.empty)
    
    member this.Item address =
        memory
        |> Map.tryFind address
        |> Option.defaultWith (fun () -> failwith <| "Address " + address.ToString() + " Isn't initiated")
        

    member this.ValueAt address =
        match this.[address] with
        | Choice2Of2 number -> number
        | _ -> failwith <| "Address " + address.ToString() + " contains an opcode"
        
    member this.OpCodeAt address =
        match this.[address] with
        | Choice2Of2 number -> failwith <| "Address " + address.ToString() + " contains the value " + number.ToString()
        | Choice1Of2 opCode -> opCode
    
    member this.SetValue (address, value) =
        new Memory(memory.Add(address, Choice2Of2 value))

    member this.SetOpCode(address, opCode) =
        new Memory(memory.Add(address, Choice1Of2 opCode))

        

