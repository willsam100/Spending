namespace Spending.Droid
open System;

open Android.App;
open Android.Content;
open Android.Content.PM;
open Android.Runtime;
open Android.Views;
open Android.Widget;
open System.IO
open Android.OS;

[<Activity (Label = "Spending.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation))>]
type MainActivity() =
    inherit Xamarin.Forms.Platform.Android.FormsApplicationActivity()
    override this.OnCreate (bundle: Bundle) =
        base.OnCreate (bundle)

        let path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
        let transactionsFile = Path.Combine(path, "simple.csv")

        let csvFileData = 
            match File.Exists(transactionsFile) with 
                | true ->  File.ReadAllText transactionsFile
                | flse -> ""

        let databasePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "database.db");
        let logger (x: string) = Console.WriteLine(x)

        Xamarin.Forms.Forms.Init (this, bundle)
        this.LoadApplication (new Spending.App (csvFileData, logger, databasePath))

