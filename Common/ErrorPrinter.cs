using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace MetaRPC.CSharpMT5
{
    // English-only comments (as requested)
    internal static class ErrorPrinter
    {
        public static void Print(ILogger logger, Exception ex, bool detailed)
        {
            // Friendly, one-line message for users; stacktrace only if detailed=true
            switch (ex)
            {
                case ApiExceptionMT5 apiEx:
                    logger.LogError("MT5 API error: Code={Code} MqlCode={Mql} Msg={Msg}",
                        apiEx.ErrorCode, apiEx.MqlErrorCode, apiEx.Message);
                    if (detailed) logger.LogError(apiEx, "MT5 API exception details.");
                    break;

                case ConnectExceptionMT5 connEx:
                    logger.LogError("Connection error: {Msg}", connEx.Message);
                    if (detailed) logger.LogError(connEx, "Connection exception details.");
                    break;

                case RpcException rpcEx:
                    logger.LogError("gRPC error: Status={Status} Detail={Detail}",
                        rpcEx.Status.StatusCode, rpcEx.Status.Detail);
                    if (detailed) logger.LogError(rpcEx, "gRPC exception details.");
                    break;

                case OperationCanceledException:
                    logger.LogWarning("Operation cancelled.");
                    break;

                default:
                    logger.LogError("Unexpected error: {Msg}", ex.Message);
                    if (detailed) logger.LogError(ex, "Unexpected exception details.");
                    break;
            }
        }
    }
}
