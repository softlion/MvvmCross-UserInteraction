MvvmCross-UserInteraction Plugin (rewritten)
============================================

MvvmCross plugin for interacting with the user from a view model.

##Changes from the original version
Completely rewritten. Using Tasks. Customizable.

##Features
1. Alert - simple alert to the user, async or optional callback when done
2. Confirm - dialog with ok/cancel, async or callback with button clicked or just when ok clicked
3. Input - asks user for input with ok/cancel, async or callback with button clicked and data or just data when ok clicked
4. Toast
5. WaitIndicator
6. ActivityIndicator
7. ConfirmThreeButtons
8. Menu (standard action sheet with single item choice) UIAlertController on iOS. AlertDialog on android.
9. Custom theme

##Usage
####Alert
```csharp
	var ui = Mvx.IoCProvider.Resolve<IUserInteraction>();
	await ui.Alert("First Name is Required");
```

####Confirm/Input callback
```csharp
	var ui = Mvx.IoCProvider.Resolve<IUserInteraction>();
	var ok = await ui.Confirm("Are you sure?");
```

####Wait for an operation to complete
```csharp
	var ui = Mvx.IoCProvider.Resolve<IUserInteraction>();
    var dismiss = new CancellationTokenSource();
    var userCancelled = ui.WaitIndicator(dismiss.Token, "Please wait", "Loggin in");

    await Task.Delay(3000, userCancelled);
    dismiss.Cancel();
```

```
	var ui = Mvx.IoCProvider.Resolve<IUserInteraction>();
    var dismiss = new CancellationTokenSource();
	await ui.ActivityIndicator(dismiss.Token, apparitionDelay: 0.5, argbColor: (uint)0xFFFFFF);

    await Task.Delay(3000);
    dismiss.Cancel();
```

####Single choice menu with optional cancel and destroy items
```csharp
	var ui = Mvx.IoCProvider.Resolve<IUserInteraction>();
    var menu = await ui.Menu(BackCancel.Token, true, "Choose something", "Cancel", null, "item1", "item2"); //You can add as many items as your want
    if (menu >= 2)
	{
		//0 => cancel action
		//1 => destroy action
		//2+ => item1, item2, ...
	}
```

####Theming on Android

Create a theme

```xml
    <style name="MyAlertDialogStyle" parent="Theme.AppCompat.Light.Dialog.Alert">
       <!-- Used for the buttons -->
       <item name="colorAccent">#FFC107</item>
       <!-- Used for the title and text -->
       <item name="android:textColorPrimary">#FFFFFF</item>
       <!-- Used for the background -->
       <item name="android:background">#4CAF50</item>
    </style>
    
    In order to change the Appearance of the Title, you can do the following. First add a new style:
    
    <style name="MyTitleTextStyle">
       <item name="android:textColor">#FFEB3B</item>
       <item name="android:textAppearance">@style/TextAppearance.AppCompat.Title</item>
    </style>
    afterwards simply reference this style in your MyAlertDialogStyle:
    
    <style name="MyAlertDialogStyle" parent="Theme.AppCompat.Light.Dialog.Alert">
       ...
       <item name="android:windowTitleStyle">@style/MyTitleTextStyle</item>
    </style>    
```

Apply theme (in Setup.cs)

```
        public override void LoadPlugins(IMvxPluginManager pluginManager)
        {
            var ioc = Mvx.IoCProvider;
            ioc.LazyConstructAndRegisterSingleton<IUserInteraction>(() =>
            {
                var ui = ioc.IoCConstruct<UserInteraction>();
                ui.ThemeResId = Resource.Style.MyAlertDialog;
                return ui;
            });
        }
```

