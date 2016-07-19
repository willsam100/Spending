namespace Spending
open System


[<Measure>] type cents
[<Measure>] type dollars

module Financial = 
    let centsToDollars = 100.0<cents/dollars>

type AccountTransaction = 
    { Amount: float<cents>
      Details : string option
      Particulars : string option
      Date: DateTime
      Type: string }

type SimpleClassified = 
    { Total : float<dollars>
      TransactionList : AccountTransaction list }


type Item = {
    Category: string
    Amount:string
}