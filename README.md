MvvmCross-UserInteraction Plugin
================================

MvvmCross plugin for interacting with the user from a view model.

##Versions
new: WaitIndicator

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
