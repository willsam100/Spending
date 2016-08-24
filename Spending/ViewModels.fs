namespace Spending
open System

module ViewModels = 

    module Formatting =
        let addComma (x: float<dollars>) = String.Format("{0:n0}", x)
        let addDollar x = "$" + x
        let prittyPrint x = 
            match x with
                | None -> ""
                | Some x -> x

    let spendingViewModel transactions = 

        let format = Formatting.addComma >> Formatting.addDollar

        (BankStatements.processFile Map.empty transactions)
        |> Map.toSeq 
        |> Seq.map (fun x -> 
            {Category = (fst x); Amount = format ((snd x).Total)})

    let spendingDetailsViewModel transactions category = 

        let format = Formatting.addComma >> Formatting.addDollar
        let format = Formatting.addComma >> Formatting.addDollar
        let pp = Formatting.prittyPrint

        (BankStatements.processFile Map.empty transactions)
        |> Map.toSeq 
        |> Seq.filter (fun x -> (fst x) = category)
        |> Seq.take 1
        |> Seq.map (fun x -> 
            (snd x).TransactionList 
            |> List.map (fun y -> 
                {Category = pp y.Details; Amount = format (y.Amount/Financial.centsToDollars)}) )
        |> Seq.head
        |> List.toSeq
