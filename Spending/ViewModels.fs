namespace Spending
open System

module ViewModels = 

    module Formatting =
        let addComma (x: float<dollars>) = String.Format("{0:n0}", x)
        let addDollar x = "$" + x

    let spendingViewModel(csvData: string) = 

        let format = Formatting.addComma >> Formatting.addDollar

        (BankStatements.processFile csvData)
         |> Map.toSeq 
                        |> Seq.map (fun x -> 
                                {Category = (fst x); Amount = format ((snd x).Total)}
                    )
