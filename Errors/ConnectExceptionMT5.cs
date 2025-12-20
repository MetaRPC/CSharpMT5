/*══════════════════════════════════════════════════════════════════════════════
 FILE: Errors/ConnectExceptionMT5.cs - MT5 CONNECTION EXCEPTION
 PURPOSE:
   Exception thrown when attempting to perform operations on an MT5Account that
   is not yet connected to the MT5 terminal.

  WHEN THIS EXCEPTION IS THROWN:
   • Calling API methods (OrderSend, AccountSummary, etc.) before Connect()
   • Connection was closed or lost
   • MT5Account instance was created but never connected

 ⚠️ NOTE:
   This is a CLIENT-SIDE validation error that prevents you from making API
   calls before establishing a connection. It's NOT a network/gRPC error.

══════════════════════════════════════════════════════════════════════════════*/

namespace MetaRPC.CSharpMT5;

[Serializable]
public class ConnectExceptionMT5 : Exception
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