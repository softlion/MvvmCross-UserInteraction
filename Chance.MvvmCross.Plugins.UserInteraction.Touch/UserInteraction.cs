using System;
using System.Runtime.CompilerServices;
using System.Threading;
using MonoTouch.UIKit;
using System.Threading.Tasks;

namespace Chance.MvvmCross.Plugins.UserInteraction.Touch
{
    /// <summary>
    /// BM: ajout de WaitIndicator
    /// </summary>
	public class UserInteraction : IUserInteraction
	{
		public void Confirm(string message, Action okClicked, string title = "", string okButton = "OK", string cancelButton = "Cancel")
		{
			Confirm(message, confirmed =>
			{
				if (confirmed)
					okClicked();
			},
			title, okButton, cancelButton);
		}

		public void Confirm(string message, Action<bool> answer, string title = "", string okButton = "OK", string cancelButton = "Cancel")
		{
			UIApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var confirm = new UIAlertView(title ?? string.Empty, message,
				                              null, cancelButton, okButton);
				if (answer != null)
				{
					confirm.Clicked +=
						(sender, args) =>
							answer(confirm.CancelButtonIndex != args.ButtonIndex);
				}
				confirm.Show();
			});
		}

        public void ConfirmThreeButtons(string message, Action<ConfirmThreeButtonsResponse> answer, string title = null, string positive = "Yes", string negative = "No", string neutral = "Maybe")
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var confirm = new UIAlertView(title ?? string.Empty, message, null, negative, positive, neutral);
                if (answer != null)
                {
                    confirm.Clicked +=
                        (sender, args) =>
                        {
                            var buttonIndex = args.ButtonIndex;
                            if (buttonIndex == confirm.CancelButtonIndex)
                                answer(ConfirmThreeButtonsResponse.Negative);
                            else if (buttonIndex == confirm.FirstOtherButtonIndex)
                                answer(ConfirmThreeButtonsResponse.Positive);
                            else
                                answer(ConfirmThreeButtonsResponse.Neutral);
                        };
                    confirm.Show();
                }
            });
        }

		public void Alert(string message, Action done = null, string title = "", string okButton = "OK")
		{
			UIApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var alert = new UIAlertView(title ?? string.Empty, message, null, okButton);
				if (done != null)
				{
					alert.Clicked += (sender, args) => done();
				}
				alert.Show();
			});

		}

		public void Input(string message, Action<string> okClicked, string placeholder = null, string title = null, string okButton = "OK",
		                string cancelButton = "Cancel")
		{
			Input(message, (ok, text) =>
			{
				if (ok)
					okClicked(text);
			},
			placeholder, title, okButton, cancelButton);
		}

		public void Input(string message, Action<bool, string> answer, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel")
		{
			UIApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var input = new UIAlertView(title ?? string.Empty, message, null, cancelButton, okButton);
				input.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
				var textField = input.GetTextField(0);
				textField.Placeholder = placeholder;
				if (answer != null)
				{
					input.Clicked +=
						(sender, args) =>
							answer(input.CancelButtonIndex != args.ButtonIndex, textField.Text);
				}
				input.Show();
			});
		}

        public CancellationToken WaitIndicator(CancellationToken dismiss, string message = null, string title=null, int? displayAfterSeconds = null, bool userCanDismiss = true)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            //var currentView = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
				var input = new UIAlertView { Title = title ?? string.Empty, Message = message };
                
                //Adding an indicator by either of these 2 methods won't work. Why ?

                //var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
                //input.Add(indicator);

                //var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge) { TranslatesAutoresizingMaskIntoConstraints = false };
                //input.Add(indicator);
                //input.AddConstraint(NSLayoutConstraint.Create(indicator, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, input, NSLayoutAttribute.CenterX, 1, 0));
                ////input.AddConstraint(NSLayoutConstraint.Create(indicator, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, input, NSLayoutAttribute.CenterY, 1, 0));
                //input.AddConstraint(NSLayoutConstraint.Create(indicator, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 50));
                //input.AddConstraint(NSLayoutConstraint.Create(indicator, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 50));

                if(userCanDismiss)
                    input.Clicked += (s,e) => cancellationTokenSource.Cancel();

                input.BackgroundColor = UIColor.FromWhiteAlpha(0, 0);
                input.Show();

                dismiss.Register(() => UIApplication.SharedApplication.InvokeOnMainThread(() => input.DismissWithClickedButtonIndex(0, true)), true);

                //TODO: dismiss if app goes into background mode
                //NSNotificationCenter.UIApplicationDidEnterBackgroundNotification
            });

            return cancellationTokenSource.Token;
        }
    }
}


/*
public override void ViewDidDisappear (bool animated)
{
    if (this.NavigationController != null) {
        var controllers = this.NavigationController.ViewControllers;
        var newcontrollers = new UIViewController[controllers.Length - 1];
        int index = 0;
        foreach (var item in controllers) {
            if (item != this) {
                newcontrollers [index] = item;
                index++;
            }

        }
        this.NavigationController.ViewControllers = newcontrollers;
    }
    base.ViewDidDisappear(animated);
}
*/


