# Your First Project in 10 Minutes (C#)

> **Hands-on Quick Start** - Create a working trading project with MetaTrader 5 and CSharpMT5 from scratch.

---

## Step 1: Create Your Project

Create a new directory for your trading bot:

```bash
mkdir my_csharpmt5_bot
cd my_csharpmt5_bot
```

Install the package:

```bash
dotnet add package MetaRPC.MT5
```

---

## Step 2: Write Your Trading Code

Create your main application file and paste the following snippet:

```
using mt5_term_api;
using MetaRPC.MT5;

var account = new MT5Account(user, password, grpcServer, null);
await account.ConnectByServerNameAsync(serverName, "EURUSD", 30);
var summary = await account.AccountSummaryAsync();
Console.WriteLine($"Balance: {summary.AccountBalance}, Equity: {summary.AccountEquity}");
```

---

## Step 3: Run the Program

Run your application:

```bash
# Verify connection output
# Balance: 10000.00, Equity: 10000.00
```

---

## 🚀 Next Steps

Congratulations! You have successfully established a direct gRPC connection to MetaTrader 5. Next:
- Explore **[gRPC Streaming](GRPC_STREAM_MANAGEMENT.md)** to listen to live ticks.
- Check the **[API Reference](../API_Reference/MT5Account.md)** for all 40+ available terminal methods.
- Learn about high-level risk management and auto-normalization in **[MT5Sugar](../API_Reference/MT5Sugar.md)**.
