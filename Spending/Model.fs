namespace Spending

open FSharp.Data
open System
open System.Collections.Generic
open FSharp.Data.CsvExtensions

module BankStatements = 
   
    type Classfier = AccountTransaction -> bool

    let parseTranasctionData filename = 
        let readTransactionRows headerRow =
            let transactionHeaders = [|"Type";"Details";"Particulars";"Code";"Reference";"Amount";"Date";"ForeignCurrencyAmount";"ConversionCharge"|]
            let creditCardHeaders = [|"Card";"Type";"Amount";"Details";"TransactionDate";"ProcessedDate";"ForeignCurrencyAmount";"ConversionCharge"|]
            let maybeString = 
                function
                    | "" -> None
                    | x  -> Some x
        
            let toCreditCard inputData = 
                inputData
                |> Seq.map (fun row -> 
                            {
                                Amount = (float)(row?Amount.AsDecimal() * 100m) * -1.<cents>;
                                Details = maybeString (row?Details);
                                Particulars = None
                                Date = DateTime.Parse(row?TransactionDate);
                                Type = row?Type
                            })
                |> Seq.toList
        
            let toTranscation inputData = 
                inputData
                |> Seq.map (fun row -> 
                            {
                                Amount = (float)(row?Amount.AsDecimal() * 100m) * 1.<cents>;
                                Details = maybeString (row?Details);
                                Particulars = maybeString (row?Particulars);
                                Date = DateTime.Parse(row?Date);
                                Type = row?Type
                            })
                |> Seq.toList
                
            match headerRow with 
                | Some headers when headers = transactionHeaders -> toTranscation
                | Some headers when headers = creditCardHeaders -> toCreditCard
                | _ -> (fun x -> List.Empty)

        let dataLines = CsvFile.Parse(filename)
        readTransactionRows dataLines.Headers
        |> (fun f -> f(dataLines.Rows))


    let processFile map fileData =  

        let categories = 
            let isPositive (t: AccountTransaction) = t.Amount > 0.0<cents>
            let isIncome (t: AccountTransaction) =
                match t with 
                | t when isPositive t -> 
                        match t.Details with 
                            | Some d when d = "eroad limited"        -> true
                            | Some d when d = "anz bank new zealand" -> true
                            | _                                      -> false
                | _                                                  -> false
            
            let descriptionContainsWords (xs: string list) (t: AccountTransaction) = 
                xs
                |> Seq.map (fun x -> x.ToLower())
                |> Seq.map (fun s -> 
                    match t.Details with
                        | Some detail -> detail.ToLower().Contains(s)
                        | _           -> false )
                |> Seq.fold (||) false
                
            [
                ("Income", isIncome)
                ("Food", descriptionContainsWords ["Pak N Save"; "Paknsave"; "Countdown"; "FRESH CHOICE"; "New World"; "Seven Mart"; "Dairy"; "My Food Bag Ltd"; "Wang Mart"])
                ("Rent", descriptionContainsWords ["7110Lachie"; "Allan Shepperd"; "Crockers Property Mn"])
                ("Isagenix", descriptionContainsWords ["Isagenix"])
                ("Eating out", descriptionContainsWords ["Golden Palace"; "Cafe"; "Sierra"; "sals";"Genta";"The Globe Bar";"Burger King";"Subway";"McDonalds";"Wendy's";"Carls Jr";"Burgerfuel";"Burger Fuel";"Fish & Chips";"kfc"; "Malaysian Noodles";"Kebabs";"Pita Pit"; "Starbucks"; "Pizza"; "La Porchet"; "Tank Juice"; "Mojo"; "Thai Noodles"; "Classic Bakery & Caf";"WENDYS OLD FASHIONED"; "Park"; "Umiya Sushi"; "Taro Tepanyaki"; "Go Believe"; "Shaky Isles"; "Deep Creek"; "Gateau"; "Bbq King"; "Bk And Sons Nz"; "Umaiya"; "BAKERY"; "The Conservatory"; "the store"; "Vietnamese"; "NANDO'S"; "Mac Roast"; "Tasting China"; "Mr Zhou Dumplings"; "Hanyang"; "Lonestar"; "Restaurant"; "Seafood Central"; "Thai"; "Roast & Bbq"])
                ("Expenses", descriptionContainsWords ["Sklenars";"Line of credit"; "Spotify Premium x 12";"Professional Earcare";"Just Cuts";"Repco";"Prepaid";"Pb Technologies";"Kiwivelo"; "2Degrees";"World Vision Nz"])
                ("Automotive", descriptionContainsWords ["Z New Lynn"; "gull"; "Tyres"; "Z Avondale"; "Caltex"; "BP"; "Z Lincoln"; "North Harbour Auto"; "Nz Transport Agency"; "Euro Master"])
                ("Entertainment", descriptionContainsWords ["Event"; "netflix"; "Grabone"; "Mighty Ape Limited"; "Reading Lynn Mall"])
                ("Clothing" , descriptionContainsWords ["HANNAHS"; "MAX FASHIONS"; "HALLENSTEINS"; "Kathmandu"; "Jeanswest"; "Barkers Mens"; "Paypal *Asoscomltd"; "Paypal *The Iconic The"])
                ("Home" , descriptionContainsWords ["Briscoes"; "LIVING & GIVING"; "MITRE 10"; "TWL 119 ALBANY"; "Jb Hi-Fi"; "Bed Bath N' Table"])
                ("Queenstown and Wedding", descriptionContainsWords ["VILLA DEL LAGO"; "BLACK BARN CONCERTS"; "To: 88297801-1001"; "Kim Mcmillan"; "SKYLINE QUEENSTOWN"; "Black Barn Vineyards"])
                ("Rental House" , descriptionContainsWords ["To: 88072863-1004";"To: 88072863-1003"; "Franklin Rates - Akl"; "To: 88072863-1007";"To: 88072863-1014"; "Rockgas Limited"; "Watercare Online"])
                ("books and education", descriptionContainsWords ["Amazon Services-Kindle"; "Audible Us"; "Bookdepository.Com"])
                ("Beatuty", descriptionContainsWords ["R W Shampoo N Things"; "Elouise"])
                ("Utilities", descriptionContainsWords ["Contact Energy Ltd"; "Bigpipe"])
                ("Insurance", descriptionContainsWords ["Nib Medical Insuranc";"AMI Insurance"; "Life Receipts"; "A M I - Tele Scc"])
                ("Donations", descriptionContainsWords ["World Vision Recurring"; "Paypal *Life Church"; "Paypal *Gospelrevol"])
                ("career", descriptionContainsWords ["Amazon Web Services"; "Codingwithsam"])
                ("Medical", descriptionContainsWords ["Unichem"])
                ("Positive", isPositive)
              ]
    
        let addMoney x = (fst x)/Financial.centsToDollars

        let updateCategory s (d: float<cents>*AccountTransaction) (map: Map<string,SimpleClassified>) = 
            if map.ContainsKey s then 
                let entry = map.[s] 
                let map' = map.Remove s
                map.Add (s, {Total = entry.Total + (addMoney d); TransactionList = (snd d) :: entry.TransactionList})
            else
                map.Add (s, {Total = addMoney d; TransactionList = List.singleton (snd d)})   
    
        let rec toCategory items m (t: AccountTransaction) =
            match items with
                | []                            -> updateCategory "Other" (t.Amount, t) m
                | x::xs when (snd x)(t) = true  -> updateCategory (fst x) (t.Amount, t) m
                | x::xs                         -> toCategory xs m t
                
        fileData
        |> List.fold (toCategory categories) map
