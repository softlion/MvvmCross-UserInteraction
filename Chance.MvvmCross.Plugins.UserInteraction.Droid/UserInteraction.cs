using System;
using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Cirrious.CrossCore;
using Android.Widget;
using Cirrious.CrossCore.Droid.Platform;
using System.Threading.Tasks;
using KeyboardType = Android.Content.Res.KeyboardType;

namespace Chance.MvvmCross.Plugins.UserInteraction.Droid
{
    /// <summary>
    /// BM: ajout de WaitIndicator
    /// BM: suppression des actions async inutiles (puisqu'on post sur la main thread)
    /// BM: remplacement par RunOnUiThread qui ne plante pas
    /// </summary>
	public class UserInteraction : IUserInteraction
	{
		protected Activity CurrentActivity {
			get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
		}

        /// <summary>
        /// Not used. In android use global styles instead.
        /// </summary>
        public uint DefaultColor { set {} }


		public void Confirm(string message, Action okClicked, string title = null, string okButton = "OK", string cancelButton = "Cancel")
		{
			Confirm(message, confirmed => {
				if (confirmed)
					okClicked();
			},
			title, okButton, cancelButton);
		}

		public void Confirm(string message, Action<bool> answer, string title = null, string okButton = "OK", string cancelButton = "Cancel")
		{
			//Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction();
		    if (CurrentActivity != null)
		    {
		        CurrentActivity.RunOnUiThread(() => 
                    new AlertDialog.Builder(CurrentActivity)
		            .SetMessage(message)
		            .SetTitle(title)
		            .SetPositiveButton(okButton, delegate
		            {
		                if (answer != null)
		                    answer(true);
		            })
		            .SetNegativeButton(cancelButton, delegate
		            {
		                if (answer != null)
		                    answer(false);
		            })
		            .Show());
		    }
		}

        public Task<bool> Confirm(string message, string title = null, string okButton = "OK", string cancelButton = "Cancel")
		{
		    var tcs = new TaskCompletionSource<bool>();
		    if (CurrentActivity != null)
		    {
		        CurrentActivity.RunOnUiThread(() => 
                    new AlertDialog.Builder(CurrentActivity)
		            .SetMessage(message)
		            .SetTitle(title)
		            .SetPositiveButton(okButton, delegate
		            {
		                tcs.TrySetResult(true);
		            })
		            .SetNegativeButton(cancelButton, delegate
		            {
		                tcs.TrySetResult(false);
		            })
		            .Show());
		    }
	        else
	        {
	            tcs.TrySetResult(false);
	        }
		    return tcs.Task;
		}

	    public void ConfirmThreeButtons(string message, Action<ConfirmThreeButtonsResponse> answer, string title = null, string positive = "Yes", string negative = "No",
	        string neutral = "Maybe")
	    {
	        if (CurrentActivity != null)
	        {
	            CurrentActivity.RunOnUiThread(() =>
	            {
	                new AlertDialog.Builder(CurrentActivity)
	                    .SetMessage(message)
	                    .SetTitle(title)
	                    .SetPositiveButton(positive, delegate
	                    {
	                        if (answer != null)
	                            answer(ConfirmThreeButtonsResponse.Positive);
	                    })
	                    .SetNegativeButton(negative, delegate
	                    {
	                        if (answer != null)
	                            answer(ConfirmThreeButtonsResponse.Negative);
	                    })
	                    .SetNeutralButton(neutral, delegate
	                    {
	                        if (answer != null)
	                            answer(ConfirmThreeButtonsResponse.Neutral);
	                    })
	                    .Show();
	            });
	        }
	    }

	    public void Alert(string message, Action done = null, string title = "", string okButton = "OK")
		{
            if (CurrentActivity != null)
            {
                CurrentActivity.RunOnUiThread(() =>
                {
                    new AlertDialog.Builder(CurrentActivity)
                        .SetMessage(message)
                        .SetTitle(title)
                        .SetPositiveButton(okButton, (s,e) =>
                        {
                            if (done != null)
                                done();
                        })
                        .Show();
                });
            }
        }

		public Task Alert(string message, string title = "", string okButton = "OK")
		{
            var tcs = new TaskCompletionSource<object>();
            if (CurrentActivity != null)
            {
                CurrentActivity.RunOnUiThread(() =>
                {
                    new AlertDialog.Builder(CurrentActivity)
                        .SetMessage(message)
                        .SetTitle(title)
                        .SetPositiveButton(okButton, (s,e) =>
                        {
                            tcs.TrySetResult(true);
                        })
                        .Show();
                });
            }
            else
            {
                tcs.TrySetResult(false);
            }

		    return tcs.Task;
		}

	    public Task<string> Input(string message, string defaultValue = null, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel", FieldType fieldType = FieldType.Default)
	    {
	        var tcs = new TaskCompletionSource<string>();

	        if (CurrentActivity != null)
	        {
	            CurrentActivity.RunOnUiThread(() =>
	            {
	                if (CurrentActivity == null)
	                {
        	            tcs.TrySetCanceled();
	                    return;
	                }

		            var input = new EditText(CurrentActivity) {Hint = placeholder, Text = defaultValue };
	                if (fieldType == FieldType.Email)
	                    input.InputType = InputTypes.ClassText | InputTypes.TextVariationEmailAddress;
			        else if (fieldType == FieldType.Integer)
                        input.InputType = InputTypes.ClassNumber; // | InputTypes.NumberFlagDecimal;

	                var dialog = new AlertDialog.Builder(CurrentActivity)
		                .SetMessage(message)
		                .SetTitle(title)
		                .SetView(input)
		                .SetPositiveButton(okButton, delegate
		                {
		                    tcs.TrySetResult(input.Text);
		                })
		                .SetNegativeButton(cancelButton, delegate
		                {
		                    tcs.TrySetResult(null);
		                })
                        .Create();

	                if (CurrentActivity.Resources.Configuration.Keyboard == KeyboardType.Nokeys
	                    || CurrentActivity.Resources.Configuration.Keyboard == KeyboardType.Undefined
	                    || CurrentActivity.Resources.Configuration.HardKeyboardHidden == HardKeyboardHidden.Yes)
	                {
	                    //Show keyboard when input has focus
	                    input.FocusChange += (sender, args) =>
	                    {
	                        if (args.HasFocus)
	                            dialog.Window.SetSoftInputMode(SoftInput.StateVisible);
	                    };
	                }

	                dialog.Show();

	            });
	        }
	        else
	        {
	            tcs.TrySetCanceled();
	        }

	        return tcs.Task;
	    }


	    public CancellationToken WaitIndicator(CancellationToken dismiss, string message = null, string title = null, int? displayAfterSeconds = null, bool userCanDismiss = true)
	    {
            var cancellationTokenSource = new CancellationTokenSource();

	        Task.Delay((displayAfterSeconds ?? 0)*1000, dismiss).ContinueWith(t =>
	        {
	            if (CurrentActivity != null)
	            {
	                CurrentActivity.RunOnUiThread(() =>
	                {
	                    var input = new ProgressBar(CurrentActivity, null, Android.Resource.Attribute.ProgressBarStyle)
	                    {
	                        Indeterminate = true,
	                        LayoutParameters = new LinearLayout.LayoutParams(DpToPixel(50), DpToPixel(50)) {Gravity = GravityFlags.CenterHorizontal | GravityFlags.CenterVertical}
	                    };

	                    var dialog = new AlertDialog.Builder(CurrentActivity)
	                        .SetMessage(message)
	                        .SetTitle(title)
	                        .SetView(input)
	                        .SetCancelable(userCanDismiss);

	                    dialog.SetOnCancelListener(new DialogCancelledListener(cancellationTokenSource.Cancel));

	                    var dlg = dialog.Show();
	                    dismiss.Register(() => CurrentActivity.RunOnUiThread(dlg.Dismiss));
	                });
	            }
	        }, TaskContinuationOptions.OnlyOnRanToCompletion);

	        return cancellationTokenSource.Token;	   
        }

        public Task ActivityIndicator(CancellationToken dismiss, double? apparitionDelay = null, uint? argbColor = null)
        {
            var tcs = new TaskCompletionSource<int>();

            Task.Delay((int)((apparitionDelay ?? 0)*1000+.5), dismiss).ContinueWith(t => 
            {
                if (t.Status == TaskStatus.RanToCompletion && CurrentActivity != null)
                {
                    CurrentActivity.RunOnUiThread(() =>
                    {
                        var layout = new FrameLayout(CurrentActivity) {LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,ViewGroup.LayoutParams.FillParent)};
                        var input = new ProgressBar(CurrentActivity) { Indeterminate = true, LayoutParameters = new FrameLayout.LayoutParams(DpToPixel(100), DpToPixel(100)) {Gravity = GravityFlags.Center}};
                        layout.AddView(input);

                        /*var dialog = new AlertDialog.Builder(CurrentActivity)
                            .SetView(input)
                            .SetCancelable(false)
                            .Create();*/

                        var dialog = new Dialog(CurrentActivity, Android.Resource.Style.ThemeNoTitleBarFullScreen); //Theme_Translucent
                        dialog.SetContentView(layout);
                        dialog.SetCancelable(false);
                        //dialog.CancelEvent += (sender, args) => tcs.TrySetResult(0);
                        dialog.DismissEvent += (sender, args) => tcs.TrySetResult(0);

                        //Make translucent. ThemeTranslucentNoTitleBarFullScreen does not work on wiko.
                        dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Argb(175,255,255,255))); 
                        dialog.Show();

                        dismiss.Register(() =>
                        {
                            CurrentActivity.RunOnUiThread(() =>
                            {
                                if(dialog.IsShowing)
                                    dialog.Dismiss();
                            });
                            tcs.TrySetResult(0);
                        });
                    });
                }
                else
                    tcs.TrySetResult(0);
            });

	        return tcs.Task;	   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dismiss"></param>
        /// <param name="userCanDismiss"></param>
        /// <param name="title"></param>
        /// <param name="cancelButton"></param>
        /// <param name="destroyButton"></param>
        /// <param name="otherButtons"></param>
        /// <returns></returns>
        /// <remarks>
        /// Button indexes:
        /// cancel: 0
        /// destroy: 1
        /// others: 2+index
        /// </remarks>
        public Task<int> Menu(CancellationToken dismiss, bool userCanDismiss, string title, string cancelButton, string destroyButton, params string[] otherButtons)
        {
            var tcs = new TaskCompletionSource<int>();

            Action cancelAction = () => tcs.TrySetResult(0);

	        if (CurrentActivity != null)
	        {
	            CurrentActivity.RunOnUiThread(() =>
	            {
	                var cancelButtonIndex = -1;
	                var destructiveButtonIndex = -1;

                    var items = new List<string>();
	                if (destroyButton != null)
	                {
	                    items.Add(destroyButton);
	                    destructiveButtonIndex = 0;
	                }
	                if(otherButtons != null)
                        items.AddRange(otherButtons);
	                if (cancelButton != null)
	                {
	                    items.Add(cancelButton);
	                    cancelButtonIndex = items.Count - 1;
	                }

	                var ad = new AlertDialog.Builder(CurrentActivity)
                        .SetTitle(title)
                        .SetItems(items.ToArray(), (s, args) =>
                        {
                            var buttonIndex = args.Which;
                            Mvx.Trace("Dialog item clicked: {0}", buttonIndex);

                            if ((cancelButton != null && buttonIndex == cancelButtonIndex) || buttonIndex < 0)
                                tcs.TrySetResult(0);
                            else if (destroyButton != null && buttonIndex == destructiveButtonIndex)
                                tcs.TrySetResult(1);
                            else
                            {
                                if (destructiveButtonIndex >= 0)
                                    buttonIndex++;
                                else
                                    buttonIndex += 2;

                                tcs.TrySetResult(buttonIndex);
                            }

                        }).SetInverseBackgroundForced(true)
                        .SetCancelable(true)
                        .Create();

	                ad.CancelEvent += (sender, args) =>
	                {
                        Mvx.Trace("Dialog cancelled");
	                    cancelAction();
	                };
	                ad.DismissEvent += (sender, args) =>
	                {
                        Mvx.Trace("Dialog dismissed");
	                    cancelAction();
	                };
                    ad.Show();

	                dismiss.Register(() =>
	                {
	                    ad.Dismiss();
	                    cancelAction();
	                });
	            });
	        }
	        else
	        {
	            tcs.TrySetResult(0);
	        }

	        return tcs.Task;	   
        }

        public Task Toast(string text, ToastStyle style = ToastStyle.Notice, ToastDuration duration = ToastDuration.Normal, ToastPosition position = ToastPosition.Bottom, int positionOffset = 20, CancellationToken? dismiss = null)
        {
            var tcs = new TaskCompletionSource<int>();

            if (CurrentActivity != null)
            {
                CurrentActivity.RunOnUiThread(() =>
                {
                    var toast = Android.Widget.Toast.MakeText(CurrentActivity, text, duration == ToastDuration.Short ? ToastLength.Short : ToastLength.Long);
                    toast.SetGravity((position == ToastPosition.Bottom ? GravityFlags.Bottom : (position == ToastPosition.Top ? GravityFlags.Top : GravityFlags.CenterVertical))|GravityFlags.CenterHorizontal, 0, positionOffset);

                    if (dismiss.HasValue)
                        dismiss.Value.Register(toast.Cancel);
                    toast.Show();

                    tcs.TrySetResult(0);
                });
            }
	        else
	        {
	            tcs.TrySetResult(0);
	        }

            return tcs.Task;	   
        }

        private static int DpToPixel(float dp)
	    {
	        return (int)(dp*((int)Application.Context.Resources.DisplayMetrics.DensityDpi)/160f+.5);
	    }
	}

    internal class DialogCancelledListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        readonly Action action;

        public DialogCancelledListener(Action action)
        {
            this.action = action;
        }

        public void OnCancel(IDialogInterface dialog)
        {
            if (action != null)
                action();
        }
    }
}

