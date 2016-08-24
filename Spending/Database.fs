namespace Spending
open System
open SQLite
open FSharp.Data.Sql

type DbConnection = Choice<SQLiteConnection, (string * exn)>




module Database =


    module MigrationTypes = 
        type AccountTransaction() =
            member val Amount: float = 0. with get, set
            member val Details: string = null with get, set
            member val Particulars: string = null with get, set
            member val Date: DateTime = DateTime.MinValue with get, set
            member val Type: string = null with get, set

    module Actions = 
        let [<Literal>] resolutionPath = __SOURCE_DIRECTORY__ + @"../packages/SQLite.Net.Core-PCL.3.1.1/lib/portable-win8+net45+wp8+wpa81+MonoAndroid1+MonoTouch1/SQLite.Net.dll" 
        let [<Literal>] connectionString = "Data Source=" + __SOURCE_DIRECTORY__ + @"../spending.db;Version=3"

        //type sql = SqlDataProvider< 
        //      ConnectionString = connectionString,
        //      DatabaseVendor = Common.DatabaseProviderTypes.SQLITE,
        //      ResolutionPath = resolutionPath,
        //      IndividualsAmount = 1000,
        //      UseOptionTypes = true >

        let addTransaction (db: SQLiteConnection) (trans: AccountTransaction) =

            let simpleTrans = MigrationTypes.AccountTransaction()
            simpleTrans.Amount <- (float) trans.Amount 
            simpleTrans.Date <- trans.Date
            simpleTrans.Details <- (Option.toObj trans.Details)
            simpleTrans.Particulars <- (Option.toObj trans.Particulars)
            simpleTrans.Type <- trans.Type
            db.Insert simpleTrans

        let addTransactions (db: SQLiteConnection) (trans: AccountTransaction list) =
            db.BeginTransaction()
            trans |> List.map (addTransaction db) |> ignore
            db.Commit()

        let createTables (db: SQLiteConnection) =
            db.CreateTable<MigrationTypes.AccountTransaction>()

        let getAccountTransactions(db: SQLiteConnection) = 
            query {
                for a in db.Table<MigrationTypes.AccountTransaction>() do 
                select a
            } |> Seq.toList 
            |> List.map (fun x -> 
                { 
                Amount = (x.Amount * 1.<cents>)
                Details = (Option.ofObj x.Details)
                Particulars = (Option.ofObj x.Particulars)
                Date = x.Date
                Type = x.Type })

        let connect (dbPath) =
            new SQLiteConnection(dbPath, false)

    let connect (dbPath): DbConnection = 
        (dbPath) |> 
        ((tryCatch "Error creating actiivty table" Actions.connect)
        >> bind (tryCatch "create actiivty table" (tee Actions.createTables)))

    let getTransactions (a: DbConnection) = 
        (bind (tryCatch "Error loading transaction data" Actions.getAccountTransactions)) a
    //let addTransaction trans = bind (tryCatch "Error adding transaction" (tee2 Actions.addTransaction trans))
    let addTransactions (transactions: AccountTransaction list) (a: DbConnection) = 
        a |> bind (tryCatch "Error adding transactions" (tee2 Actions.addTransactions transactions))
     