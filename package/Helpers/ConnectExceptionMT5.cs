using System;

namespace mt5_term_api
{

    [Serializable]
    internal class ConnectExceptionMT5 : Exception
    {
        public ConnectExceptionMT5()
        {
        }

        public ConnectExceptionMT5(string? message) : base(message)
        {
        }

        public ConnectExceptionMT5(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}