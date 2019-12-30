namespace Simulator

open System.Text.RegularExpressions
open System

type failMessage = string

type EnvironmentTester(environment : RunningEnvironment, maxOperations : int) =
    member tester.TestSubroutine (subroutineName : string,
                                  inputRegisters : (Register * Value) list,
                                  expectedRegisters : (Register * Value) list) =
        let rec run (cyclesLeft : int) (env : RunningEnvironment) : Option<failMessage> =
            match cyclesLeft with
            | 0 -> Some(sprintf "Infinite loop after %d operations" maxOperations)
            | cycles ->
                if env.PC <> 0s then
                    match env.DoOperation() with
                    | None -> Some(sprintf "Subroutine halted at pc = %d" env.PC)
                    | Some(environment) -> run (cyclesLeft - 1) environment
                else
                    let incorrectRegisters =
                        expectedRegisters
                        |> Seq.map (fun (reg, expectedValue) -> (reg, expectedValue, env.Registers.[reg]))
                        |> Seq.filter (fun (reg, expectedValue, realValue) -> expectedValue <> realValue)
                        |> Seq.toArray
                    if incorrectRegisters.Length = 0 then
                        None
                    else
                        let toString (reg, expectedValue, realValue) = sprintf "Reg %A was expected to be %d instead of %d" reg expectedValue realValue
                        Some(incorrectRegisters |> Utils.mkString ", " toString)

        environment.SetRegisters(inputRegisters).SetRegister(R7, 0s).SetPcAtLabel subroutineName
        |> run maxOperations

    static member StringsEqual(s1 : string, s2 : string) =
        let optionalChars = @"\n|\r|\t| |\.|,|!|\(|\)|"
        let str1 = Regex.Replace(s1, optionalChars, "").ToLower()
        let str2 = Regex.Replace(s2, optionalChars, "").ToLower()
        str1.Equals str2



    member tester.TestInputOutput (input : string, expectedOutput : string) =
        let rec run (env : RunningEnvironment) (cyclesLeft : int) : Option<failMessage> =
            match cyclesLeft with
            | 0 -> Some(sprintf "Infinite loop after %d operations" maxOperations)
            | cycles ->
                match env.DoOperation() with
                | Some(environment) -> run environment (cyclesLeft - 1)
                | None -> 
                    let givenOutput = env.GetOutput()
                    match EnvironmentTester.StringsEqual(givenOutput, expectedOutput) with
                    | true -> None
                    | false -> Some ((sprintf "Output isn't as expected:\n")
                                     + (sprintf "\t given: \n%s\n" (givenOutput.ToLower()))
                                     + (sprintf "\t expected: \n%s\n" (expectedOutput.ToLower())))

        
        run (environment.SetInput(input)) maxOperations

            
