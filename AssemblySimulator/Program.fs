// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace Simulator

module Program = 
    [<EntryPoint>]
    let main argv = 
        let path = @"C:\Users\Yuval\Programming\FSharp\AssemblySimulator\Input Samples\Hw2.asm"
        let maxCycles = 100000
        let befRegisters = 
            Register.AllZeros.Add(R0, 5s).Add(R1, 13s)
        printfn "Registers before: %s" <| Register.ToString befRegisters
        let setupEnvironment = 
            RunningEnvironment.InitEmpty(befRegisters)
                              .LoadCode(path)
                              .SetRegisters([|(R0, 5s); (R1, 13s)|])

        match setupEnvironment.Run(maxCycles) with
        | None -> printfn "Infinite Loop"
        | Some(environmentAfterRunning) -> printfn "Registers after: %s" <| Register.ToString environmentAfterRunning.Registers

        0 // return an integer exit code
