using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chance.MvvmCross.Plugins.UserInteraction
{
    /// <summary>
    /// Impact displayed keyboard
    /// </summary>
    public enum FieldType
    {
        Default,
        Email,
        Integer
    }

	public interface IUserInteraction
	{
        /// <summary>
        /// Set default color for all activity indicators
        /// </summary>
	    uint DefaultColor { set; }

        //void Confirm(string message, Action okClicked, string title = null, string okButton = "OK", string cancelButton = "Cancel");
        //void Confirm(string message, Action<bool> answer, string title = null, string okButton = "OK", string cancelButton = "Cancel");
		Task<bool> Confirm(string message, string title = null, string okButton = "OK", string cancelButton = "Cancel", CancellationToken? dismiss = null);

        //void Alert(string message, Action done = null, string title = "", string okButton = "OK");
		Task Alert(string message, string title = "", string okButton = "OK");

	    Task<string> Input(string message, string defaultValue = null, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel", FieldType fieldType = FieldType.Default);

	    void ConfirmThreeButtons(string message, Action<ConfirmThreeButtonsResponse> answer, string title = null, string positive = "Yes", string negative = "No",
	        string neutral = "Maybe");

        CancellationToken WaitIndicator(CancellationToken dismiss, string message = null, string title=null, int? displayAfterSeconds = null, bool userCanDismiss = true);

        /// <summary>
        /// Display an activity indicator which blocks user interaction.
        /// </summary>
        /// <param name="dismiss">cancel this token to dismiss the activity indicator</param>
        /// <param name="apparitionDelay">show indicator after this delay. The user interaction is not disabled during this delay: this may be an issue.</param>
        /// <param name="argbColor">activity indicator tint</param>
        /// <returns></returns>
        Task ActivityIndicator(CancellationToken dismiss, double? apparitionDelay = null, uint? argbColor = null);

	    /// <summary>
	    /// Display a single choice menu
	    /// </summary>
	    /// <param name="dismiss">optional. Can be used to close the menu programatically.</param>
	    /// <param name="userCanDismiss">true to allow the user to close the menu using a hardware key.</param>
	    /// <param name="title">optional title</param>
	    /// <param name="cancelButton">optional cancel button. If null the cancel button is not shown.</param>
	    /// <param name="destroyButton">optional destroy button. Will be red.</param>
	    /// <param name="otherButtons">other buttons</param>
	    /// <returns>
	    /// A task which completes when the menu has disappeared
	    /// 0: cancel button or hardware key is pressed
	    /// 1: destroy button is pressed
	    /// 2-n: other matching button is pressed
	    /// </returns>
	    Task<int> Menu(CancellationToken dismiss, bool userCanDismiss, string title, string cancelButton, string destroyButton, params string[] otherButtons);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="style"></param>
        /// <param name="duration"></param>
        /// <param name="position"></param>
        /// <param name="positionOffset"></param>
        /// <param name="dismiss">optional. Can be used to close the toast programatically.</param>
        /// <returns>A task which completes when the toast has disappeared</returns>
	    Task Toast(string text, ToastStyle style = ToastStyle.Notice, ToastDuration duration = ToastDuration.Normal, ToastPosition position = ToastPosition.Bottom, int positionOffset = 20, CancellationToken? dismiss = null);
	}

    public enum ToastStyle
    {
        Custom,
        Info,
        Notice,
        Warning,
        Error
    }

    public enum ToastDuration
    {
        Short = 1000,
        Normal = 2500,
        Long = 8000
    }

    public enum ToastPosition
    {
        Top,
        Middle,
        Bottom
    }
}
