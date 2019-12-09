
namespace Simulator

module Hw1 =

    let modulu (nom : int16, dem : int16) = abs(nom % dem)

    let tests = seq{
        yield ("MUL", 1, [(R0, 0s);     (R1, 220s)],    [(R2, 0s * 220s)])
        yield ("MUL", 2, [(R0, 915s);   (R1, 11s)],     [(R2, 915s * 11s)])
        yield ("MUL", 3, [(R0, -11s);   (R1, 55s)],     [(R2, -11s * 55s)])
        yield ("MUL", 4, [(R0, -2s);    (R1, -2s)],     [(R2, -2s * -2s)])
        yield ("MUL", 5, [(R0, 12s);    (R1, 123s)],    [(R2, 12s * 123s)])
        yield ("MUL", 6, [(R0, 0s);     (R1, 0s)],      [(R2, 0s * 0s)])
        yield ("MUL", 7, [(R0, -5s);    (R1, -20s)],    [(R2, -5s * -20s)])
        yield ("MUL", 8, [(R0, 33s);    (R1, 15555s)],  [(R2, 33s * 15555s)])
        yield ("MUL", 9, [(R0, -7s);    (R1, 0s)],      [(R2, -7s * 0s)])
            
        yield ("DIV", 1, [(R0, 0s);    (R1, 2s)],    [(R2, 0s / 2s);      (R3, modulu(0s, 2s))])
        yield ("DIV", 2, [(R0, 2s);    (R1, 0s)],    [(R2, -1s);          (R3, -1s)])
        yield ("DIV", 3, [(R0, -9s);   (R1, 0s)],    [(R2, -1s);          (R3, -1s)])
        yield ("DIV", 4, [(R0, 0s);    (R1, 0s)],    [(R2, -1s);          (R3, -1s)])
        yield ("DIV", 5, [(R0, 22s);   (R1, 10s)],   [(R2, 22s / 10s);    (R3, modulu(22s, 10s))])
        yield ("DIV", 6, [(R0, 155s);  (R1, -213s)], [(R2, 155s / -213s); (R3, modulu(155s, -213s))])
        yield ("DIV", 7, [(R0, -724s); (R1, -9s)],   [(R2, -724s / -9s);  (R3, modulu(-724s, -9s))])
        yield ("DIV", 8, [(R0, -198s); (R1, 99s)],   [(R2, -198s / 99s);  (R3, modulu(-198s, 99s))])
        yield ("DIV", 9, [(R0, 0s);    (R1, -900s)], [(R2, 0s / -900s);   (R3, modulu(0s, -900s))])
            
        yield ("TRIANGLEINEQUALITY", 1, [(R0, 3s);  (R1, 4s);   (R2, 5s)],   [(R3, 1s)])
        yield ("TRIANGLEINEQUALITY", 2, [(R0, 6s);  (R1, 6s);   (R2, 12s)],  [(R3, 1s)])
        yield ("TRIANGLEINEQUALITY", 3, [(R0, 6s);  (R1, 6s);   (R2, 13s)],  [(R3, 0s)])
        yield ("TRIANGLEINEQUALITY", 4, [(R0, -2s); (R1, 0s);   (R2, 4s)],   [(R3, 0s)])
        yield ("TRIANGLEINEQUALITY", 5, [(R0, -5s); (R1, -10s); (R2, -8s)],  [(R3, 0s)])
        yield ("TRIANGLEINEQUALITY", 6, [(R0, 71s); (R1, 46s);  (R2, 30s)],  [(R3, 1s)])
        yield ("TRIANGLEINEQUALITY", 7, [(R0, 41s); (R1, 82s);  (R2, 40s)],  [(R3, 0s)])
    }

    let check (tester : EnvironmentTester) =
        for (subroutineLabel, index, input, output) in tests do
            printf "%s %d) " subroutineLabel index
            match tester.TestSubroutine(subroutineLabel, input, output) with
            | None -> printfn "Success"
            | Some(error) -> printfn "%s" error
        

