using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Views;
using Cirrious.CrossCore;
using Android.Widget;
using Cirrious.CrossCore.Droid.Platform;
using System.Threading.Tasks;

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
		                tcs.SetResult(true);
		            })
		            .SetNegativeButton(cancelButton, delegate
		            {
		                tcs.SetResult(false);
		            })
		            .Show());
		    }
	        else
	        {
	            tcs.SetResult(false);
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
                            tcs.SetResult(true);
                        })
                        .Show();
                });
            }
            else
            {
                tcs.SetResult(false);
            }

		    return tcs.Task;
		}

		public void Input(string message, Action<string> okClicked, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel")
		{
			Input(message, (ok, text) => {
				if (ok)
					okClicked(text);
			},
			placeholder, title, okButton, cancelButton);
		}

		public void Input(string message, Action<bool, string> answer, string hint = null, string title = null, string okButton = "OK", string cancelButton = "Cancel")
		{
		    if (CurrentActivity != null)
		    {
		        CurrentActivity.RunOnUiThread(() =>
		        {
		            if (CurrentActivity == null) return;
		            var input = new EditText(CurrentActivity) {Hint = hint};

		            new AlertDialog.Builder(CurrentActivity)
		                .SetMessage(message)
		                .SetTitle(title)
		                .SetView(input)
		                .SetPositiveButton(okButton, delegate
		                {
		                    if (answer != null)
		                        answer(true, input.Text);
		                })
		                .SetNegativeButton(cancelButton, delegate
		                {
		                    if (answer != null)
		                        answer(false, input.Text);
		                })
		                .Show();
		        });
		    }
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
	        }, TaskContinuationOptions.NotOnCanceled);

	        return cancellationTokenSource.Token;	   
        }

        public Task ActivityIndicator(CancellationToken dismiss, double? apparitionDelay = null, uint? argbColor = null)
        {
            var tcs = new TaskCompletionSource<int>();

            Task.Delay((int)((apparitionDelay ?? 0)*1000+.5), dismiss).ContinueWith(t => 
            {
                if (t.IsCompleted && CurrentActivity != null)
                {
                    CurrentActivity.RunOnUiThread(() =>
                    {
                        var input = new ProgressBar(CurrentActivity) //, null, Android.Resource.Attribute.ProgressBarStyle)
                        {
                            Indeterminate = true,
                            LayoutParameters = new LinearLayout.LayoutParams(DpToPixel(50), DpToPixel(50)) {Gravity = GravityFlags.CenterHorizontal | GravityFlags.CenterVertical},
                        };

                        var dialog = new AlertDialog.Builder(CurrentActivity)
                            .SetView(input)
                            .SetCancelable(false);

                        dialog.SetOnCancelListener(new DialogCancelledListener(() => tcs.TrySetResult(0)));

                        var dlg = dialog.Show();
                        dismiss.Register(() =>
                        {
                            CurrentActivity.RunOnUiThread(dlg.Dismiss);
                            tcs.TrySetResult(0);
                        });
                    });
                }
                else
                    tcs.TrySetResult(0);
            });

	        return tcs.Task;	   
        }

	    public Task<int> Menu(CancellationToken dismiss, bool userCanDismiss, string title, string cancelButton, string destroyButton, params string[] otherButtons)
        {
            throw new NotImplementedException();

            var tcs = new TaskCompletionSource<int>();

	        if (CurrentActivity != null)
	        {
	            CurrentActivity.RunOnUiThread(() =>
	            {
	            });
	        }
	        else
	        {
	            tcs.SetResult(-2);
	        }

	        return tcs.Task;	   
        }

        public Task Toast(string text, ToastStyle style = ToastStyle.Notice, ToastDuration duration = ToastDuration.Normal, ToastPosition position = ToastPosition.Bottom, int positionOffset = 20, CancellationToken? dismiss = null)
        {
            throw new NotImplementedException();

            var tcs = new TaskCompletionSource<int>();

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

