/*══════════════════════════════════════════════════════════════════════════════
 FILE: Errors/ApiExceptionMT5.cs - MT5 API ERROR EXCEPTION
 PURPOSE:
   Exception thrown when MT5 gRPC API returns an error response from the server.

  WHEN THIS EXCEPTION IS THROWN:
   • MT5 server responds with an error in the reply message
   • Trade operation fails (e.g., invalid order parameters, insufficient margin)
   • MQL script execution error on the server side
   • Invalid request parameters or forbidden operations

  WHAT IT CONTAINS:
   • ErrorCode - API-level error code (e.g., "INVALID_SYMBOL", "TRADE_DISABLED")
   • ErrorMessage - Human-readable error description
   • MqlErrorCode - MQL5 error code (e.g., ERR_TRADE_NOT_ALLOWED)
   • MqlErrorTradeCode - Trade-specific error code
   • RemoteStackTrace - Server-side stack trace (if available)
   • CommandTypeName - Which gRPC method caused the error

  HOW TO HANDLE:
   try {
       var result = await account.OrderSend(request);
   }
   catch (ApiExceptionMT5 ex) {
       Console.WriteLine($"Trade failed: {ex.ErrorCode}");
       Console.WriteLine($"MQL Error: {ex.MqlErrorDescription}");
   }

 ⚠️ IMPORTANT:
   This is NOT a connection/network error. This means the server received your
   request successfully but rejected it due to business logic (invalid parameters,
   insufficient funds, trade restrictions, etc.)

══════════════════════════════════════════════════════════════════════════════*/

using Mt5TermApi;

namespace MetaRPC.CSharpMT5;

public class ApiExceptionMT5 : Exception
{
    public Error Error { get; }

    public string? ErrorCode => Error?.ErrorCode;
    public string? ErrorMessageText => Error?.ErrorMessage;
    public ErrorType ErrorType => Error?.Type ?? default;
    public MqlErrorCode MqlErrorCode => Error?.MqlErrorCode ?? default;
    public int MqlErrorIntCode => Error?.MqlErrorIntCode ?? 0;
    public string? MqlErrorDescription => Error?.MqlErrorDescription;
    public MqlErrorTradeCode MqlErrorTradeCode => Error?.MqlErrorTradeCode ?? default;
    public int MqlErrorTradeIntCode => Error?.MqlErrorTradeIntCode ?? 0;
    public string? MqlErrorTradeDescription => Error?.MqlErrorTradeDescription;
    public string? RemoteStackTrace => Error?.StackTrace;
    public string? CommandTypeName => Error?.CommandTypeName;
    public long CommandId => Error?.CommandId ?? 0;

    public ApiExceptionMT5(Error error)
        : base(error?.ErrorMessage)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }

    public override string ToString()
    {
        return $"API Exception: {ErrorCode} - {ErrorMessageText}\n" +
               $"MQL: {MqlErrorCode} ({MqlErrorIntCode}) - {MqlErrorDescription}\n" +
               $"Trade: {MqlErrorTradeCode} ({MqlErrorTradeIntCode}) - {MqlErrorTradeDescription}\n" +
               $"Command: {CommandTypeName} (ID: {CommandId})\n" +
               $"Stack: {RemoteStackTrace}";
    }
}
