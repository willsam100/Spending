namespace Spending

open System
open Xamarin.Forms
open Xamarin.Forms.Xaml


type Spending(csvData : string, logger: string -> unit) = 
    inherit ContentPage() 

    do base.LoadFromXaml(typeof<Spending>) |> ignore

    let data =  ViewModels.spendingViewModel(csvData)
    do logger (data |> Seq.fold (fun sum x -> sum + ", " + x.Category) "")

    let spendingList = base.FindByName<ListView>("listView")
    do spendingList.ItemsSource <- ((data) :> Collections.IEnumerable)


type App(csvFile: string, logger: string -> unit) = 
    inherit Application(MainPage = Spending(csvFile, logger))

