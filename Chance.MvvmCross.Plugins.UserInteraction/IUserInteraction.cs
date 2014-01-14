using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chance.MvvmCross.Plugins.UserInteraction
{
	public interface IUserInteraction
	{
		void Confirm(string message, Action okClicked, string title = null, string okButton = "OK", string cancelButton = "Cancel");
		void Confirm(string message, Action<bool> answer, string title = null, string okButton = "OK", string cancelButton = "Cancel");

		void Alert(string message, Action done = null, string title = "", string okButton = "OK");

		void Input(string message, Action<string> okClicked, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel");
		void Input(string message, Action<bool, string> answer, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel");

	    void ConfirmThreeButtons(string message, Action<ConfirmThreeButtonsResponse> answer, string title = null, string positive = "Yes", string negative = "No",
	        string neutral = "Maybe");

        CancellationToken WaitIndicator(CancellationToken dismiss, string message = null, string title=null, int? displayAfterSeconds = null, bool userCanDismiss = true);
	}
}
