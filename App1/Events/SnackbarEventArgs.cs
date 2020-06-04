using System;

namespace App1.Events
{
    public class SnackbarEventArgs
        : EventArgs
    {
        private string _Message;
        public Exception? Exception { get; }

        public SnackbarEventArgs(string message)
        {
            _Message = message;
        }

        public SnackbarEventArgs(string message, Exception ex)
            : this(message) 
        {
            Exception = ex;
        }

        public string Message
        {
            get => 
                Exception is null ?
                _Message :
                Exception.Message;
        }
    }
}