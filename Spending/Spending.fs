namespace Spending

open System
open Xamarin.Forms
open Xamarin.Forms.Xaml
open SQLite
open Database
open BankStatements

type SpendingDetails(catgeory: string, csvData: AccountTransaction list) =
    inherit ContentPage()

    let _ = base.LoadFromXaml(typeof<SpendingDetails>)
    let listView = base.FindByName<ListView>("listView")
    let data =  ViewModels.spendingDetailsViewModel csvData catgeory
    do  listView.ItemsSource <- ((data) :> Collections.IEnumerable)
        listView.SeparatorVisibility <- SeparatorVisibility.Default
        listView.SeparatorColor <- Color.Black
        listView.ItemTapped.AddHandler(new EventHandler<ItemTappedEventArgs>(fun x y -> 
           (x :?> ListView).SelectedItem <- null
        ))

type Spending(maybeData : Choice<AccountTransaction list, string>, logger: string -> unit) as this = 
    inherit ContentPage() 

    do base.LoadFromXaml(typeof<Spending>) |> ignore

    do either (fun x -> this.updateList(x)) (fun x -> this.updateList([])) maybeData

    member this.updateList(csvData: AccountTransaction list) = 

        let data =  ViewModels.spendingViewModel(csvData)
        do logger (data |> Seq.fold (fun sum x -> sum + ", " + x.Category) "")

        let spendingList = base.FindByName<ListView>("listView")
        do spendingList.ItemsSource <- (data :> Collections.IEnumerable)

        let onSelection sender (e:ItemTappedEventArgs) = 
            logger ("Item Selected" + e.Item.ToString())

            match e.Item with 
                | :? Item as item -> 
                    this.Navigation.PushAsync(SpendingDetails(item.Category, csvData)) |> ignore
                | _ -> this.DisplayAlert("Item Selected", e.Item.ToString(), "Ok") |> ignore
            spendingList.SelectedItem <- null

        do spendingList.ItemTapped.AddHandler(new System.EventHandler<ItemTappedEventArgs>(onSelection))
           spendingList.SeparatorVisibility <- SeparatorVisibility.Default
           spendingList.SeparatorColor <- Color.Black

type ErrorPage(errorMessage: string) as this = 
    inherit ContentPage()
    do this.Content <-  Label(Text = errorMessage)

    
type App(csvFile: string, logger: string -> unit, dbPath : string) as this = 
    inherit Application()

    let db = connect dbPath
    let transactions = getTransactions db

    let accountList = parseTranasctionData csvFile
    let addAndUpdate = (addTransactions accountList) >> getTransactions

    let updatePage items = 
        do this.MainPage <- NavigationPage(Spending(items, logger))

    let errorPage (message, exp: exn) = 
        logger exp.StackTrace
        do this.MainPage <- NavigationPage(ErrorPage(message + "\n" + exp.GetType().ToString()))

    let eitherWithFailure f = 
        either f (fun x -> errorPage x)
    let items x = 
        match x with 
            | [] -> eitherWithFailure (fun xs -> updatePage (succeed xs)) (addAndUpdate db)
            | xs -> updatePage (succeed xs)

    do eitherWithFailure items transactions



