using System;

namespace GlyssenEngine.Utilities
{
    public interface IMessageModal
    {
        MessageResult Show(string text, string caption, Buttons buttons, Icon icon, DefaultButton defaultButton);
    }

    public static class MessageModal
    {
        private static IMessageModal s_messageModal;

        public static IMessageModal Default
        {
            get
            {
                if (s_messageModal == null)
                    throw new InvalidOperationException("Not Initialized. Set MessageModal.Default first.");
                return s_messageModal;
            }
            set => s_messageModal = value;
        }

        public static MessageResult Show(string text = "", string caption = "", Buttons buttons = Buttons.OK, Icon icon = Icon.None, DefaultButton defaultButton = DefaultButton.Button1)
        {
            return Default.Show(text, caption, buttons, icon, defaultButton);
        }
    }

    public enum MessageResult
    {
		None,
        Abort, // TODO: If possible, let's eliminate the need for this
		Ignore, // TODO: If possible, let's eliminate the need for this
		OK,
        Retry, // TODO: If possible, let's eliminate the need for this
	}

    public enum Icon
    {
        None,
        Warning,
    }

    public enum Buttons
    {
        OK,
		AbortRetryIgnore, // TODO: If possible, let's eliminate the need for this
    }

    // TODO: If possible, let's eliminate the need for this enumeration.
	public enum DefaultButton
    {
        Button1,
        Button2,
        Button3
    }
}
