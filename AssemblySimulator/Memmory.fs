
namespace Simulator


type Memmory(memmory : Map<Address, Choice<OpCode, Value>>) =
    static member InitEmpty() =
        new Memmory(Map.empty)

    member this.ValueAt address =
        let content =
            memmory
            |> Map.tryFind address
            |> Option.defaultWith (fun () -> failwith <| "Address " + address.ToString() + " Isn't initiated")
        match content with
        | Choice2Of2 number -> number
        | _ -> failwith <| "Address " + address.ToString() + " contains an opcode"
        
    member this.OpCodeAt address =
        let content =
            memmory
            |> Map.tryFind address
            |> Option.defaultWith (fun () -> failwith <| "Address " + address.ToString() + " Isn't initiated")
        match content with
        | Choice2Of2 number -> failwith <| "Address " + address.ToString() + " contains the value " + number.ToString()
        | Choice1Of2 opCode -> opCode
    
    member this.SetValue (address, value) =
        Memmory(memmory.Add(address, Choice2Of2 value))

    member this.SetOpCode(address, opCode) =
        Memmory(memmory.Add(address, Choice1Of2 opCode))

        

