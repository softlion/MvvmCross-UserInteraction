MvvmCross-UserInteraction Plugin
================================

MvvmCross plugin for interacting with the user from a view model. 

##Versions
new: WaitIndicator
update: async methods were removed, as all methods are already posting code asynchronously on the main thread.
fix: use RunOnUiThread on android to prevent some random crashes

##Features
1. Alert - simple alert to the user, async or optional callback when done
2. Confirm - dialog with ok/cancel, async or callback with button clicked or just when ok clicked
3. Input - asks user for input with ok/cancel, async or callback with button clicked and data or just data when ok clicked

##Usage
####Alert
```
public ICommand SubmitCommand
{
		get
		{
			return new MvxCommand(() =>
					                      {
					                        if (string.IsNullOrEmpty(FirstName)) 
					                        {
					                          Mvx.Resolve<IUserInteraction>().Alert("First Name is Required");
					                          return;
					                        }
					                        //do work
					                      });
		}
}
```

####Confirm/Input callback
```
public ICommand SubmitCommand
{
		get
		{
			return new MvxCommand(() =>
					                      {
					                        Mvx.Resolve<IUserInteraction>().Confirm("Are you sure?", async () => 
					                          {
					                            //Do work
					                          });
					                      });
		}
}
```

####Wait for an operation to complete
```
public ICommand SubmitCommand
{
		get
		{
                return new MvxCommand(async () =>
                {
                    var dismiss = new CancellationTokenSource();
                    var userCancelled = Mvx.Resolve<IUserInteraction>().WaitIndicator(dismiss.Token, "Please wait", "Loggin in");

                    await Task.Delay(3000, userCancelled);
                    dismiss.Cancel();
                });
		}
}
```

##Adding to your project
1. Follow stuarts directions (step 3) - http://slodge.blogspot.com/2012/10/build-new-plugin-for-mvvmcrosss.html
2. Grab the UserInteractionPluginBootstrap file from appropriate Droid/Touch folder. Drop into your project in the Bootstrap folder, change the namespace.

There is an existing nuget package.
