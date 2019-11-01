namespace Simulator

open System


[<AutoOpen>]
module TypeExtensions = 
    open System.Globalization

    type String with
        member private str.Format() =
            if str.ToLower().StartsWith "x" then
                (NumberStyles.HexNumber, str.[1..])
            elif str.StartsWith "#" then
                (NumberStyles.Integer, str.[1..])
            else
                (NumberStyles.HexNumber, str)

        member str.IsInt16() : bool =
            let value = 0s
            let numberStyle, trimmedStr = str.Format()
            Int16.TryParse(trimmedStr, numberStyle, CultureInfo.InvariantCulture, ref value)
        
        member str.ToInt16() : Int16 =
            let numberStyle, trimmedStr = str.Format()
            Int16.Parse(trimmedStr, numberStyle)


