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
        let csvFile = File.ReadAllText (Path.Combine(path, "simple.csv"))

        let logger (x: string) = Console.WriteLine(x)

        Xamarin.Forms.Forms.Init (this, bundle)

        this.LoadApplication (new Spending.App (csvFile, logger))

