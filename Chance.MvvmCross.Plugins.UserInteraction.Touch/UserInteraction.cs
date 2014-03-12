using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cirrious.FluentLayouts.Touch;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System.Threading.Tasks;

namespace Chance.MvvmCross.Plugins.UserInteraction.Touch
{
    /// <summary>
    /// BM: ajout de WaitIndicator
    /// </summary>
	public class UserInteraction : IUserInteraction
    {
        private static UIColor defaultColor;
        public uint DefaultColor { set { defaultColor = FromArgb(value); } }

        UIColor FromArgb(uint value)
        {
            return new UIColor((value >> 16 & 0xff)/255f, (value >> 8 & 0xff)/255f, (value & 0xff)/255f, (value >> 24 & 0xff)/255f);
        }

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

            Task.Delay((displayAfterSeconds ?? 0)*1000, dismiss).ContinueWith(t => UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
				var input = new UIAlertView { Title = title ?? string.Empty, Message = message ?? string.Empty };
                
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
            }), TaskContinuationOptions.NotOnCanceled);

            return cancellationTokenSource.Token;
        }

        public CancellationToken ActivityIndicator(CancellationToken dismiss, double? apparitionDelay = null, uint? argbColor = null)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Task.Delay((int)((apparitionDelay ?? 0)*1000+.5), dismiss).ContinueWith(t => UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var currentView = UIApplication.SharedApplication.KeyWindow.Subviews.LastOrDefault();
                if (currentView != null)
                {
                    var waitView = new UIView(currentView.Bounds) { AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleBottomMargin};
                    var overlay = new UIView {BackgroundColor = UIColor.White, Alpha = 0.7f};
                    var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge) { HidesWhenStopped = true };
                    if (argbColor.HasValue || defaultColor != null)
                        indicator.Color = argbColor.HasValue ? FromArgb(argbColor.Value) : defaultColor;
                    indicator.StartAnimating();

                    waitView.Add(overlay);
                    waitView.Add(indicator);

                    waitView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
                    waitView.AddConstraints(
                        overlay.AtTopLeftOf(waitView),
                        overlay.AtBottomOf(waitView),
                        overlay.AtRightOf(waitView),

                        indicator.WithSameCenterX(waitView),
                        indicator.WithSameCenterY(waitView),
                        indicator.Width().EqualTo(60),
                        indicator.Height().EqualTo(60)
                        );

                    waitView.Alpha = 0;
                    currentView.Add(waitView);
                    UIView.Animate(0.3, () => { waitView.Alpha = 1; });

                    dismiss.Register(() => UIApplication.SharedApplication.InvokeOnMainThread(() =>
                    {
                        indicator.StopAnimating();
                        waitView.RemoveFromSuperview();
                    }), true);
                    //indicator.Release();
                }
            }), TaskContinuationOptions.NotOnCanceled);
            
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


