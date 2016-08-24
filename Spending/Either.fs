[<AutoOpen>]
module Spending.Either

    let (|Success|Failure|) =
        function 
        | Choice1Of2 s -> Success s
        | Choice2Of2 f -> Failure f

    let succeed x = Choice1Of2 x
    let fail x = Choice2Of2 x

    let either successFunc failureFunc twoTrackInput =
        match twoTrackInput with
        | Success s -> successFunc s
        | Failure f -> failureFunc f

    let bind f = 
        either f fail

    let tee f x= 
        f x |> ignore
        x

    let tee2 f p x = 
        f x p |> ignore
        x

    let tryCatch message f x =
        try
            f x |> succeed
        with
        | ex -> fail (message + ": " + ex.Message, ex)
    
    let (>>=) f g = 
        f >> (bind g)