using Xunit;
using MetaRPC.CSharpMT5;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Tests;

public class MT5ServiceTests
{
    [Fact]
    public void TestMT5Account_ClientConstruction()
    {
        var account = new MT5Account(12345678, "demo_pass", "https://mt5.mrpc.pro:443", Guid.NewGuid());
        Assert.NotNull(account);
        Assert.Equal(12345678UL, account.User);
        Assert.Equal("demo_pass", account.Password);
        Assert.Equal("https://mt5.mrpc.pro:443", account.GrpcServer);
    }

    [Fact]
    public void TestConnectRequest_ProtoSerialization()
    {
        var req = new ConnectRequest
        {
            User = 12345678,
            Password = "test_password",
            Host = "127.0.0.1",
            Port = 443
        };

        Assert.Equal(12345678UL, req.User);
        Assert.Equal("test_password", req.Password);
        Assert.Equal("127.0.0.1", req.Host);
        Assert.Equal(443, req.Port);
    }

    [Fact]
    public void TestAccountSummaryData_ProtoSerialization()
    {
        var data = new AccountSummaryData
        {
            AccountLogin = 12345678,
            AccountBalance = 10000.50,
            AccountEquity = 10250.75,
            AccountCurrency = "USD",
            AccountLeverage = 100
        };

        Assert.Equal(12345678L, data.AccountLogin);
        Assert.Equal(10000.50, data.AccountBalance);
        Assert.Equal(10250.75, data.AccountEquity);
        Assert.Equal("USD", data.AccountCurrency);
        Assert.Equal(100L, data.AccountLeverage);
    }

    [Fact]
    public void TestExceptions()
    {
        var ex = new ConnectExceptionMT5("connection error");
        Assert.Contains("connection error", ex.Message);
    }

    [Fact]
    public void TestGetIdRequest_ProtoSerialization()
    {
        var req = new GetIdRequest
        {
            User = "12345678",
            Password = "demo_password"
        };
        Assert.Equal("12345678", req.User);
        Assert.Equal("demo_password", req.Password);

        var reply = new GetIdReply
        {
            Data = new GetIdData { Id = "68c935ee-a2b1-4f3e-bb36-3982845cfa85" }
        };
        Assert.NotNull(reply.Data);
        Assert.Equal("68c935ee-a2b1-4f3e-bb36-3982845cfa85", reply.Data.Id);
    }
}
