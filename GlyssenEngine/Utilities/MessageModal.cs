using System;

namespace GlyssenEngine.Utilities
{
    public interface IMessageModal
    {
        void Show(string text, bool warningIcon);
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

        public static void Show(string text, bool warningIcon = false)
        {
            Default.Show(text, warningIcon);
        }
    }
}
