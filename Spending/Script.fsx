#r "../packages/FSharp.Data.2.3.1/lib/net40/FSharp.Data.dll"

open System
open FSharp.Data

let main = 
    let anz = HtmlDocument.Load("https://digital.anz.co.nz/preauth/web/service/login")
    let links = anz.Descendants ["a"] 
                    |> Seq.choose (fun x -> 
                        x.TryGetAttribute("href")
                        |> Option.map (fun y -> x.InnerText(), y.Value())
                )

    printfn "Links found:"
    links 
        //|> Seq.filter (fun x -> (fst x).ToLower().Contains("personal")) 
        |> Seq.iter (fun x -> printfn "%s %s" (fst x) (snd x))