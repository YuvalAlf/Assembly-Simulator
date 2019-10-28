
namespace Simulator

type Memmory(memmory : Map<Address, Choice<Value, OpCode>>) =
    member this.ValueAt address =
        let content =
            memmory
            |> Map.tryFind address
            |> Option.defaultWith (fun () -> failwith <| "Address " + address.ToString() + " Isn't initiated")
        match content with
        | Choice1Of2(value) -> value
        | Choice2Of2(opCode) -> failwith <| "Address " + address.ToString() + " contains an opcode"
        
    member this.OpCodeAt address =
        let content =
            memmory
            |> Map.tryFind address
            |> Option.defaultWith (fun () -> failwith <| "Address " + address.ToString() + " Isn't initiated")
        match content with
        | Choice1Of2(value) -> failwith <| "Address " + address.ToString() + " contains the value " + value.ToString()
        | Choice2Of2(opCode) -> opCode
    
    member this.SetValue (address, value) =
        Memmory(memmory.Add(address, Choice1Of2(value)))

    member this.SetOpCode(address, opCode) =
        Memmory(memmory.Add(address, Choice2Of2(opCode)))

    static member Init(data : string) =
        do ()
        

