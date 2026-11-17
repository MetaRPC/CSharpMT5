using System.Linq;
using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mt5_term_api;
using Mt5TermApi;
using Xunit;

namespace MetaRPC.CSharpMT5.Tests;

/// <summary>
/// In-process gRPC server tests: a trade call is never re-sent after a transport failure (duplicate fill risk),
/// while reads and in-band TERMINAL_INSTANCE_NOT_FOUND keep their reconnect + retry.
/// </summary>
public sealed class NoTradeReplayTests : IAsyncLifetime
{
    private sealed class State
    {
        public int ConnectEx, OrderSend, OrderModify, OrderClose, AccountSummary, AccountInfoDouble;
        public StatusCode? TradeFailure;          // trade calls throw this status (after "executing")
        public bool TradeNotFoundOnce;            // first trade call answers TERMINAL_INSTANCE_NOT_FOUND in-band
    }

    private sealed class FakeConnection(State s) : Connection.ConnectionBase
    {
        public override Task<ConnectExReply> ConnectEx(ConnectExRequest request, ServerCallContext context)
        {
            Interlocked.Increment(ref s.ConnectEx);
            return Task.FromResult(new ConnectExReply { Data = new ConnectData { TerminalInstanceGuid = Guid.NewGuid().ToString() } });
        }
    }

    private sealed class FakeTrading(State s) : TradingHelper.TradingHelperBase
    {
        public override Task<OrderSendReply> OrderSend(OrderSendRequest request, ServerCallContext context)
        {
            int n = Interlocked.Increment(ref s.OrderSend);
            if (s.TradeFailure is { } code) throw new RpcException(new Status(code, "simulated failure after execution"));
            if (s.TradeNotFoundOnce && n == 1)
                return Task.FromResult(new OrderSendReply { Error = new Error { ErrorCode = "TERMINAL_INSTANCE_NOT_FOUND" } });
            return Task.FromResult(new OrderSendReply { Data = new OrderSendData { Order = 42, Deal = 7 } });
        }

        public override Task<OrderModifyReply> OrderModify(OrderModifyRequest request, ServerCallContext context)
        {
            Interlocked.Increment(ref s.OrderModify);
            throw new RpcException(new Status(s.TradeFailure ?? StatusCode.Unavailable, "simulated failure after execution"));
        }

        public override Task<OrderCloseReply> OrderClose(OrderCloseRequest request, ServerCallContext context)
        {
            Interlocked.Increment(ref s.OrderClose);
            throw new RpcException(new Status(s.TradeFailure ?? StatusCode.Unavailable, "simulated failure after partial close"));
        }
    }

    private sealed class FakeAccount(State s) : AccountHelper.AccountHelperBase
    {
        public override Task<AccountSummaryReply> AccountSummary(AccountSummaryRequest request, ServerCallContext context)
        {
            int n = Interlocked.Increment(ref s.AccountSummary);
            if (n == 1) throw new RpcException(new Status(StatusCode.Unavailable, "simulated outage"));
            return Task.FromResult(new AccountSummaryReply { Data = new AccountSummaryData { AccountBalance = 123 } });
        }
    }

    private sealed class FakeAccountInfo(State s) : AccountInformation.AccountInformationBase
    {
        public override Task<AccountInfoDoubleReply> AccountInfoDouble(AccountInfoDoubleRequest request, ServerCallContext context)
        {
            Interlocked.Increment(ref s.AccountInfoDouble);
            return Task.FromResult(new AccountInfoDoubleReply { Error = new Error { ErrorCode = "API_KEY_INVALID", ErrorMessage = "refused" } });
        }
    }

    private readonly State _s = new();
    private WebApplication _app = null!;
    private MT5Account _acc = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0, o => o.Protocols = HttpProtocols.Http2));
        builder.Services.AddGrpc();
        builder.Services.AddSingleton(_s);
        _app = builder.Build();
        _app.MapGrpcService<FakeConnection>();
        _app.MapGrpcService<FakeTrading>();
        _app.MapGrpcService<FakeAccount>();
        _app.MapGrpcService<FakeAccountInfo>();
        await _app.StartAsync();
        string address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();

        _acc = new MT5Account(1, "pw", address, default, "TEST_KEY");
        await _acc.ConnectByServerNameAsync("Demo-Server");
        _s.ConnectEx = 0;
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Theory]
    [InlineData(StatusCode.Unavailable)]
    [InlineData(StatusCode.DeadlineExceeded)]
    [InlineData(StatusCode.Unknown)]
    [InlineData(StatusCode.Internal)]
    public async Task OrderSend_TransportFailure_IsNotResent(StatusCode code)
    {
        _s.TradeFailure = code;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var e = await Assert.ThrowsAsync<TradeOutcomeUnknownExceptionMT5>(() =>
            _acc.OrderSendAsync(new OrderSendRequest { Symbol = "EURUSD", Volume = 0.01 }, null, cts.Token));
        Assert.Equal(1, _s.OrderSend);
        Assert.Equal(0, _s.ConnectEx);
        Assert.Equal("TRADE_OUTCOME_UNKNOWN", e.ErrorCode);
        Assert.Equal("OrderSend", e.Operation);
        Assert.Equal(code, e.StatusCode);
        Assert.IsAssignableFrom<RpcException>(e);
    }

    [Fact]
    public async Task OrderModify_And_PartialClose_Unavailable_AreNotResent()
    {
        _s.TradeFailure = StatusCode.Unavailable;
        await Assert.ThrowsAsync<TradeOutcomeUnknownExceptionMT5>(() => _acc.OrderModifyAsync(new OrderModifyRequest { Ticket = 5 }));
        var e = await Assert.ThrowsAsync<TradeOutcomeUnknownExceptionMT5>(() => _acc.OrderCloseAsync(new OrderCloseRequest { Ticket = 5, Volume = 0.01 }));
        Assert.Equal("OrderClose", e.Operation);
        Assert.Equal(1, _s.OrderModify);
        Assert.Equal(1, _s.OrderClose);
        Assert.Equal(0, _s.ConnectEx);
    }

    [Fact]
    public async Task OrderSend_TerminalInstanceNotFound_ReconnectsAndResends()
    {
        _s.TradeNotFoundOnce = true;
        var d = await _acc.OrderSendAsync(new OrderSendRequest { Symbol = "EURUSD", Volume = 0.01 });
        Assert.Equal(42UL, d.Order);
        Assert.Equal(2, _s.OrderSend);
        Assert.Equal(1, _s.ConnectEx);
    }

    [Fact]
    public async Task Read_Unavailable_IsRetried()
    {
        var d = await _acc.AccountSummaryAsync();
        Assert.Equal(123, d.AccountBalance);
        Assert.Equal(2, _s.AccountSummary);
        Assert.Equal(1, _s.ConnectEx);
    }

    [Fact]
    public async Task AccountInfoDouble_ApiKeyInvalid_KeepsCode_NoReconnect()
    {
        var e = await Assert.ThrowsAsync<ApiExceptionMT5>(() => _acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance));
        Assert.Equal("API_KEY_INVALID", e.ErrorCode);
        Assert.Equal(1, _s.AccountInfoDouble);
        Assert.Equal(0, _s.ConnectEx);
    }
}
