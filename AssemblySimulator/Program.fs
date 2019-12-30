// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace Simulator

module Program = 
    open System.IO
    open System.Text.RegularExpressions
    open System
    open System.Diagnostics

    let extractIds str = seq {
            for idMatch in Regex.Matches(str, @"\d\d\d\d+") do
                yield idMatch.Value
        }

    [<EntryPoint>]
    let main argv = 
        let hwNum = (2).ToString()
        let dirIndex = 5

        let baseDirPath = @"C:\Users\Yuval\Google Drive\Assembly Course\Hw Checking\Hw" + hwNum + @"\Moodle"
        let submittersDirFile = 
            Directory.EnumerateDirectories(baseDirPath)
            |> List.ofSeq
            |> List.sort
            |> List.item dirIndex
        let submittersTxtData = 
            Directory.EnumerateFiles submittersDirFile 
            |> Seq.find (fun path -> path.ToLower().EndsWith ".txt")
            |> File.ReadAllText
        let submittersAsmPath = 
            Directory.EnumerateFiles submittersDirFile 
            |> Seq.find (fun path -> path.ToLower().EndsWith ".asm")

        printfn "%s" submittersDirFile
        for id in extractIds(submittersTxtData) do
            printfn "%s" id
        let maxOperations = 1000000
        
        Process.Start(submittersAsmPath) |> ignore

        let setupEnvironment = RunningEnvironment.InitEmpty().LoadCode(submittersAsmPath)
        let solutionTester = new EnvironmentTester(setupEnvironment, maxOperations)
        
        Hw2.check(solutionTester)

        printfn "Type any key to end and open asm file"
        Console.ReadKey(false) |> ignore
        0 // return an integer exit code
