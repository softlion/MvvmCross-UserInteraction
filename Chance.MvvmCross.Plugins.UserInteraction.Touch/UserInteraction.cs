using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UIKit;
using System.Threading.Tasks;
using MvvmCross.Platform;

namespace Chance.MvvmCross.Plugins.UserInteraction.Touch
{
	public class UserInteraction : IUserInteraction
    {
        private static UIColor defaultColor;
        public uint DefaultColor { set { defaultColor = FromArgb(value); } }

        UIColor FromArgb(uint value)
        {
            return new UIColor((value >> 16 & 0xff)/255f, (value >> 8 & 0xff)/255f, (value & 0xff)/255f, (value >> 24 & 0xff)/255f);
        }

		public Task<bool> Confirm(string message, string title = null, string okButton = "OK", string cancelButton = "Cancel", CancellationToken? dismiss = null)
		{
		    var tcs = new TaskCompletionSource<bool>();
			UIApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var confirm = new UIAlertView(title ?? string.Empty, message, (IUIAlertViewDelegate)null, cancelButton, okButton);
				confirm.Clicked += (sender, args) => tcs.TrySetResult(confirm.CancelButtonIndex != args.ButtonIndex);
				confirm.Show();

			    if (dismiss != null)
			    {
			        var registration = dismiss.Value.Register(() =>
			        {
			            UIApplication.SharedApplication.InvokeOnMainThread(() =>
			            {
			                if (confirm.Visible)
			                    confirm.DismissWithClickedButtonIndex(0, true);
                        });
			        });
			        tcs.Task.ContinueWith(t => registration.Dispose());
			    }
			});
		    return tcs.Task;
		}

        public void ConfirmThreeButtons(string message, Action<ConfirmThreeButtonsResponse> answer, string title = null, string positive = "Yes", string negative = "No", string neutral = "Maybe")
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var confirm = new UIAlertView(title ?? string.Empty, message, (IUIAlertViewDelegate)null, negative, positive, neutral);
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

		public Task Alert(string message, string title = "", string okButton = "OK")
		{
		    var tcs = new TaskCompletionSource<bool>();
			UIApplication.SharedApplication.InvokeOnMainThread(() =>
			{
				var alert = new UIAlertView(title ?? string.Empty, message, (IUIAlertViewDelegate)null, okButton);
				alert.Clicked += (sender, args) => tcs.TrySetResult(true);
				alert.Show();
			});
		    return tcs.Task;
		}

	    public Task<string> Input(string message, string defaultValue = null, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel", FieldType fieldType = FieldType.Default, int maxLength = 0)
	    {
	        var tcs = new TaskCompletionSource<string>();

	        void ConfigureTextField(UITextField textField)
	        {
	            if (placeholder != null)
	                textField.Placeholder = placeholder;
	            if (defaultValue != null)
	                textField.Text = defaultValue;
	            if (fieldType != FieldType.Default)
	            {
	                if (fieldType == FieldType.Email)
	                    textField.KeyboardType = UIKeyboardType.EmailAddress;
	                else if (fieldType == FieldType.Integer)
	                {
	                    textField.KeyboardType = UIKeyboardType.NumberPad;
	                    textField.ValueChanged += (sender, args) =>
	                    {
	                        var text = textField.Text;
	                        var newText = Regex.Replace(text, "[^0-9]", "");
	                        if (text != newText)
	                            textField.Text = newText;
	                    };
	                }
	            }

	            if (maxLength > 0)
	            {
	                textField.ValueChanged += (sender, args) =>
	                {
	                    var text = textField.Text;
	                    if (text.Length > maxLength)
	                    {
	                        textField.Text = text.Substring(0, maxLength);
	                    }
	                };
	            }
	        }

	        UIApplication.SharedApplication.InvokeOnMainThread(() =>
	        {
	            var alert = UIAlertController.Create(title ?? string.Empty, message, UIAlertControllerStyle.Alert);
	            alert.AddAction(UIAlertAction.Create(okButton, UIAlertActionStyle.Default, action => tcs.TrySetResult(alert.TextFields[0].Text)));
	            alert.AddAction(UIAlertAction.Create(cancelButton, UIAlertActionStyle.Cancel, action => tcs.TrySetResult(null)));
	            alert.AddTextField(ConfigureTextField);
	            var currentView = UIApplication.SharedApplication.Windows.LastOrDefault(w => w.WindowLevel == UIWindowLevel.Normal);
	            if (currentView != null && currentView.RootViewController != null)
	                currentView.RootViewController.PresentViewController(alert, true, null);
	            else
	                Mvx.Warning("Input: no window/nav controller on which to display");
	        });

	        return tcs.Task;
	    }

        class WaitIndicatorImpl : IWaitIndicator
        {
            private string title, body;

            public UIAlertView Dialog { get; set; }

            public WaitIndicatorImpl(CancellationToken userDismissedToken)
            {
                UserDismissedToken = userDismissedToken;
            }

            public CancellationToken UserDismissedToken { get; }
            public string Title { set { title = value; if(Dialog!=null) Dialog.Title = value; } get => title; }
            public string Body { set { body = value; if (Dialog != null) Dialog.Message = value; } get => body; }
        }

        /// <summary>
        /// </summary>
        /// <param name="dismiss">CancellationToken that dismiss the indicator when cancelled</param>
        /// <param name="message">body</param>
        /// <param name="title"></param>
        /// <param name="displayAfterSeconds">delay show. Can be cancelled before it is displayed.</param>
        /// <param name="userCanDismiss">Enable tap to dismiss</param>
        /// <returns>CancellationToken is cancelled if the indicator is dismissed by the user (if userCanDismiss is true)</returns>
        public IWaitIndicator WaitIndicator(CancellationToken dismiss, string message = null, string title=null, int? displayAfterSeconds = null, bool userCanDismiss = true)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var wi = new WaitIndicatorImpl(cancellationTokenSource.Token)
            {
                Title = title,
                Body = message
            };

            //var currentView = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();

            Task.Delay((displayAfterSeconds ?? 0)*1000, dismiss).ContinueWith(t => UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
				var input = new UIAlertView { Title = wi.Title ?? string.Empty, Message = wi.Body ?? string.Empty };
                wi.Dialog = input;
                
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

            return wi;
        }

        public Task ActivityIndicator(CancellationToken dismiss, double? apparitionDelay = null, uint? argbColor = null)
        {
            var tcs = new TaskCompletionSource<int>();

            Task.Delay((int)((apparitionDelay ?? 0)*1000+.5), dismiss).ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    UIApplication.SharedApplication.InvokeOnMainThread(() =>
                    {
                        var currentView = UIApplication.SharedApplication.Windows.LastOrDefault(w => w.WindowLevel == UIWindowLevel.Normal);
                        if (currentView != null)
                        {
                            var waitView = new UIView {Alpha = 0};
                            var overlay = new UIView {BackgroundColor = UIColor.White, Alpha = 0.7f};
                            var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge) {HidesWhenStopped = true};
                            if (argbColor.HasValue || defaultColor != null)
                                indicator.Color = argbColor.HasValue ? FromArgb(argbColor.Value) : defaultColor;

                            waitView.Add(overlay);
                            waitView.Add(indicator);
                            currentView.Add(waitView);

                            waitView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
                            waitView.AddConstraints(
                                overlay.AtTopOf(waitView),
                                overlay.AtLeftOf(waitView),
                                overlay.AtBottomOf(waitView),
                                overlay.AtRightOf(waitView),

                                indicator.WithSameCenterX(waitView),
                                indicator.WithSameCenterY(waitView),
                                indicator.Width().EqualTo(60),
                                indicator.Height().EqualTo(60)
                                );

                            waitView.TranslatesAutoresizingMaskIntoConstraints = false;
                            currentView.AddConstraints(
                                waitView.AtTopOf(currentView),
                                waitView.AtLeftOf(currentView),
                                waitView.AtRightOf(currentView),
                                waitView.AtBottomOf(currentView)
                                );

                            UIView.Animate(0.4, () => { waitView.Alpha = 1; });
                            indicator.StartAnimating();

                            var registration = dismiss.Register(() => UIApplication.SharedApplication.InvokeOnMainThread(() =>
                            {
                                indicator.StopAnimating();
                                waitView.RemoveFromSuperview();
                                waitView.Dispose();
                                tcs.TrySetResult(0);
                            }), true);
                            // ReSharper disable once MethodSupportsCancellation
                            tcs.Task.ContinueWith(tt => registration.Dispose());
                        }
                        else
                        {
                            Mvx.Warning("UserInteraction.ActivityIndicator: no window on which to display");
                        }
                    });
                }
                else
                    tcs.TrySetResult(0);
            });
            
            return tcs.Task;
        }


        /// <summary>
        /// Displays a system menu.
        /// If otherButtons is null, the indexes are still incremented, but the button won't appear. 
        /// This enables easy scenario where the otherButtons array is changing between calls.
        /// </summary>
        /// <param name="dismiss"></param>
        /// <param name="userCanDismiss"></param>
        /// <param name="title"></param>
        /// <param name="cancelButton"></param>
        /// <param name="destroyButton"></param>
        /// <param name="otherButtons">If a button is null, the index are still incremented, but the button won't appear</param>
        /// <returns></returns>
        /// <remarks>
        /// Button indexes:
        /// cancel: 0
        /// destroy: 1
        /// others: 2+index
        /// 
        /// 
        /// </remarks>
	    public Task<int> Menu(CancellationToken dismiss, bool userCanDismiss, string title, string cancelButton, string destroyButton, params string[] otherButtons)
        {
            var tcs = new TaskCompletionSource<int>();

	        UIApplication.SharedApplication.InvokeOnMainThread(() =>
	        {
	            var actionSheet = new UIActionSheet(title, (IUIActionSheetDelegate)null, cancelButton, destroyButton, otherButtons.Where(b => b!=null).ToArray());
	            var registration = dismiss.Register(() => UIApplication.SharedApplication.InvokeOnMainThread(() => actionSheet.DismissWithClickedButtonIndex(actionSheet.CancelButtonIndex, false)));
	            // ReSharper disable once MethodSupportsCancellation
	            tcs.Task.ContinueWith(t => registration.Dispose());

                actionSheet.Canceled += (sender, args) => tcs.TrySetResult(0);
	            actionSheet.Clicked += (sender, args) =>
	            {
	                //Mvx.Warning("clicked: {0}, FirstOtherButtonIndex: {1}, cancel index: {2}, destroy index: {3}", args.ButtonIndex, actionSheet.FirstOtherButtonIndex, actionSheet.CancelButtonIndex, actionSheet.DestructiveButtonIndex);
	                if ((cancelButton!=null && args.ButtonIndex == actionSheet.CancelButtonIndex) || args.ButtonIndex < 0)
	                    tcs.TrySetResult(0);
	                else if (destroyButton!=null && args.ButtonIndex == actionSheet.DestructiveButtonIndex)
	                    tcs.TrySetResult(1);
	                else
	                {
	                    var titleClicked = actionSheet.ButtonTitle(args.ButtonIndex);
	                    var index = 2 + Array.IndexOf(otherButtons, titleClicked);
                        //ios scrumbles the position of buttons. Method not usable.
	                    //var index = actionSheet.FirstOtherButtonIndex < 0 ? args.ButtonIndex - 1 : args.ButtonIndex - actionSheet.FirstOtherButtonIndex;
	                    tcs.TrySetResult(index);
	                }
	            };

                var currentView = UIApplication.SharedApplication.Windows.LastOrDefault(w => w.WindowLevel == UIWindowLevel.Normal);
	            if (currentView != null)
	            {
	                //Show from bottom
	                actionSheet.ShowFrom(new CGRect(0, currentView.Bounds.Bottom - 1, currentView.Bounds.Width, 1), currentView, true);
	            }
	            else
	            {
	                Mvx.Warning("UserInteraction.Menu: no window on which to display");
	            }
	        });

	        return tcs.Task;
        }

        public Task Toast(string text, ToastStyle style = ToastStyle.Notice, ToastDuration duration = ToastDuration.Normal, ToastPosition position = ToastPosition.Bottom, int positionOffset = 20, CancellationToken? dismiss = null)
        {
            var tcs = new TaskCompletionSource<int>();

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                //Find the view on which to display the toast
                var currentView = UIApplication.SharedApplication.Windows.LastOrDefault(w => w.WindowLevel == UIWindowLevel.Normal);
                if (currentView == null)
                {
                    Mvx.Warning("UserInteraction.Toast: no window on which to display");
                    tcs.TrySetResult(-1);
                    return;
                }

                //UI items
                var font = UIFont.SystemFontOfSize(UIFont.SmallSystemFontSize);
                var holder = new UIView {Alpha = 0, BackgroundColor = UIColor.Black };
                holder.Layer.CornerRadius = font.LineHeight/2;
                holder.Layer.MasksToBounds = true;
                var label = new UILabelEx {Text = text, Font = font, TextColor = UIColor.White, TextAlignment = UITextAlignment.Center, LineBreakMode = UILineBreakMode.WordWrap, Lines = 0};

                //orders
                holder.Add(label);
                currentView.Add(holder);
                currentView.BringSubviewToFront(holder);

                //constraints
                holder.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
                holder.AddConstraints(
                    label.AtLeftOf(holder, 10),
                    label.AtRightOf(holder, 10),
                    label.AtTopOf(holder, 5),
                    label.AtBottomOf(holder, 5)
                    );

                holder.TranslatesAutoresizingMaskIntoConstraints = false;
                currentView.AddConstraints(
                    holder.WithSameCenterX(currentView),
                    holder.Width().LessThanOrEqualTo().WidthOf(currentView).Minus(15*2),
                    position == ToastPosition.Top ? holder.AtTopOf(currentView, positionOffset) :
                        position == ToastPosition.Bottom ? holder.AtBottomOf(currentView, positionOffset) :
                        holder.WithSameCenterY(currentView)
                    );

                //interactions
                var inCall = false; //Prevent rebond on tap
                Action<bool> hideHolder = animated =>
                {
                    if (inCall)
                        return;
                    inCall = true;

                    if (animated)
                    {
                        UIView.Animate(1f, () =>
                        {
                            holder.Alpha = 0;
                        }, () =>
                        {
                            holder.RemoveFromSuperview();
                            holder.Dispose();
                            tcs.TrySetResult(0);
                        });
                    }
                    else
                    {
                        holder.Hidden = true;
                        holder.RemoveFromSuperview();
                        holder.Dispose();
                        tcs.TrySetResult(0);
                    }
                };

                CancellationTokenRegistration registration;
                if (dismiss.HasValue)
                {
                    registration = dismiss.Value.Register(() => UIApplication.SharedApplication.InvokeOnMainThread(() => hideHolder(false)));
                    tcs.Task.ContinueWith(t => registration.Dispose());
                }
                holder.AddGestureRecognizer(new UITapGestureRecognizer(() => hideHolder(true)));

                UIView.Animate(1f, () => holder.Alpha = .7f, async () =>
                {
                    await Task.Delay((int)duration, dismiss.HasValue ? dismiss.Value : CancellationToken.None).ConfigureAwait(false);
                    UIApplication.SharedApplication.InvokeOnMainThread(() => hideHolder(true));
                });
            });

	        return tcs.Task;
        }
    }

    /// <summary>
    /// A UILabel which automatically sets its PreferredMaxLayoutWidth to its constraint width,
    /// so it can work nicely with MvxAutolayoutTableViewSource, without additional work
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class UILabelEx : UILabel
    {
        public override void LayoutSubviews()
        {
            // Clear the preferred max layout width in case the text of the label is a single line taking less width than what would be taken from the constraints of the left and right edges to the label's superview
            PreferredMaxLayoutWidth = 0;

            base.LayoutSubviews();
            
            // Now that you know what the constraints gave you for the label's width, use that for the preferredMaxLayoutWidth
            PreferredMaxLayoutWidth = Bounds.Width;

            // And then layout again to get the label's correct height
            base.LayoutSubviews();
        }
    }
}

