namespace Simulator

module Hw2 =
    open System.IO
    open System.Text.RegularExpressions

    let ioDirectory = @"C:\Users\Yuval\Google Drive\Assembly Course\Hws\My Hw\Hw2\IO"
    let ioPathes = 
        Directory.EnumerateFiles ioDirectory 
        |> Seq.toArray
    let inputPathes = 
        ioPathes 
        |> Array.filter (fun file -> Regex.IsMatch(file, @".*input\d.txt"))
        |> Array.sort
    let outputPathes = 
        ioPathes |> Array.filter (fun file -> Regex.IsMatch(file, @".*output\d.txt"))
        |> Array.sort
    let ioFiles = Array.zip inputPathes outputPathes

    let check (tester : EnvironmentTester) =
        for (inputPath, outputPath) in ioFiles do
            let input = (File.ReadAllText inputPath).Replace("\r", "")
            let output = (File.ReadAllText outputPath).Replace("\r", "")
            printfn "****************************"
            printfn "%s %s) " (Path.GetFileNameWithoutExtension inputPath) (Path.GetFileNameWithoutExtension outputPath)
            match tester.TestInputOutput(input, output) with
            | None -> printfn "Success"
            | Some(error) -> printfn "%s" error

