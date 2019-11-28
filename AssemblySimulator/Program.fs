// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace Simulator

module Program = 
    open System.IO
    open System.Text.RegularExpressions
    open System
    open System.Diagnostics

    let extractIds str = seq {
            for id in Regex.Split(str, @"\d\d\d\d+") do
                if not <| System.String.IsNullOrEmpty id then
                    yield id
        }

    [<EntryPoint>]
    let main argv = 
        let dirIndex = 0
        let baseDirPath = @"C:\Users\Yuval\Downloads\assembly"
        let submittersDirFile = 
            Directory.EnumerateDirectories(baseDirPath)
            |> List.ofSeq
            |> List.sort
            |> List.item dirIndex
        let submittersTxtData = 
            Directory.EnumerateFiles submittersDirFile 
            |> Seq.find (fun path -> path.EndsWith ".txt")
            |> File.ReadAllText
        let submittersAsmPath = 
            Directory.EnumerateFiles submittersDirFile 
            |> Seq.find (fun path -> path.EndsWith ".asm")

        for id in extractIds (baseDirPath) do
            printfn "%s" id
        let maxOperations = 1000000
        
        let setupEnvironment = RunningEnvironment.InitEmpty().LoadCode(submittersAsmPath)
        let solutionTester = new EnvironmentTester(setupEnvironment, maxOperations)
        
        Hw1.check(solutionTester)

        printfn "Type any key to end and open asm file"
        Console.ReadKey(false) |> ignore
        Process.Start(submittersAsmPath) |> ignore
        0 // return an integer exit code
