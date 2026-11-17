using Grpc.Core;

namespace mt5_term_api
{
    /// <summary>
    /// Thrown by a trade call (<c>OrderSend</c>, <c>OrderSendWithoutChecks</c>, <c>OrderModify</c>, <c>OrderClose</c>) when the transport failed
    /// (gRPC Unavailable, DeadlineExceeded, Unknown or Internal) after the request may already have reached the
    /// terminal. The trade may or may not have been executed. The SDK does NOT re-send it, because a re-send could open
    /// a second position or close a second part of one: check the open positions / orders (and the history) before
    /// sending it again.
    /// It derives from <see cref="RpcException"/> and keeps the original status, so existing
    /// <c>catch (RpcException)</c> handlers still see the same <see cref="RpcException.StatusCode"/>.
    /// </summary>
    public class TradeOutcomeUnknownExceptionMT5 : RpcException
    {
        /// <summary>Error code of this condition, the same in every MetaRPC SDK.</summary>
        public const string Code = "TRADE_OUTCOME_UNKNOWN";

        /// <summary>The trade call whose outcome is unknown (e.g. <c>OrderSend</c>).</summary>
        public string Operation { get; }

        /// <summary>Always <see cref="Code"/>.</summary>
        public string ErrorCode => Code;

        /// <summary>The transport failure that left the outcome unknown.</summary>
        public RpcException TransportError { get; }

        /// <summary>Creates the exception for <paramref name="operation"/> from its transport failure.</summary>
        public TradeOutcomeUnknownExceptionMT5(string operation, RpcException transportError)
            : base(transportError.Status, transportError.Trailers,
                $"{Code}: {operation} failed with gRPC {transportError.StatusCode} after the request may have reached the terminal; " +
                "the trade may or may not have been executed. It was NOT re-sent automatically: check open positions/orders before retrying. " +
                $"({transportError.Status.Detail})")
        {
            Operation = operation;
            TransportError = transportError;
        }

        /// <summary>True for the gRPC status codes after which a trade's outcome is unknown.</summary>
        public static bool IsUnknownOutcome(StatusCode code) =>
            code == StatusCode.Unavailable || code == StatusCode.DeadlineExceeded ||
            code == StatusCode.Unknown || code == StatusCode.Internal;
    }
}
