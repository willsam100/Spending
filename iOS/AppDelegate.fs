namespace Spending.iOS

open System
open UIKit
open Foundation
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS
open SQLite.Net.Platform.XamarinIOS

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit FormsApplicationDelegate ()

    override this.FinishedLaunching (app, options) =
        Forms.Init()

        let sqlPlatform = new SQLitePlatformIOS();
        let logger x = ("" |> ignore)
        this.LoadApplication (new Spending.App("", logger, sqlPlatform, ""))
        base.FinishedLaunching(app, options)

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main(args, null, "AppDelegate")
        0

