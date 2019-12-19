using System;
using System.Collections.Generic;
using System.Text;

namespace GlyssenEngine.Utilities
{
    public interface IMessageModal
    {
        void ShowMessage(string message);

        void ShowWarning(string message);
        
        void ShowError(string message); 
        
        bool AskQuestion(string question);
        
        bool AskRetry(string question);
        
        int AbortRetryIgnore(string question);
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

        public static void ShowMessage(string message)
        {
            Default.ShowMessage(message);
        }

        public static void ShowWarning(string message)
        {
            Default.ShowWarning(message);
        }

        public static void ShowError(string message)
        {
            Default.ShowError(message);
        }

        public static bool AskQuestion(string question)
        {
            return Default.AskQuestion(question);
        }

        public static bool AskRetry(string question)
        {
            return Default.AskRetry(question);
        }

        public static int AbortRetryIgnore(string question)
        {
            return Default.AbortRetryIgnore(question);
        }
    }
}
