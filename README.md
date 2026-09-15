# CSharpMT5 SDK — Cloud .NET / C# SDK for MetaTrader 5 (MT5)

[![NuGet](https://img.shields.io/nuget/v/MetaRPC.MT5.svg)](https://www.nuget.org/packages/MetaRPC.MT5)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Docs](https://img.shields.io/badge/docs-CSharpMT5-0083ff.svg)](https://metarpc.github.io/CSharpMT5/)
[![Cloud](https://img.shields.io/badge/VPS-Not_Required-success.svg)](https://mrpc.pro)
[![Platform](https://img.shields.io/badge/.NET-8.0%20|%209.0%20|%2010.0-purple.svg)](https://dotnet.microsoft.com/)

> **Official C# / .NET SDK for MetaTrader 5 Cloud API via gRPC & REST.**  
> Connect, stream live ticks, and execute trades on any MT5 broker or prop firm from Linux containers, Windows, or macOS — **without running a Windows VPS or desktop terminal.**

---

## ⚡ Why MetaRPC CSharpMT5?

- **Zero Windows VPS**: Stop paying $20–$80/month for buggy Windows servers. Run .NET trading bots in lightweight Linux Docker containers or Kubernetes.
- **Ultra-Low Latency**: High-speed gRPC streaming and execution co-located with London (LD4) and New York (NY4) broker data centers (<20ms execution).
- **Universal MT5 Broker & Prop Firm Support**: Connects to 500+ brokers and prop firms including **FTMO, IC Markets, Pepperstone, Exness, FundedNext, Tickmill, XM, FXCM**.
- **Automatic Identity Derivation**: Account GUID (`id`) and cryptographic headers are derived automatically from your credentials.
- **Modern Async/Await**: Strongly-typed async methods with automatic reconnects and stream multiplexing.

---

## 📦 Installation

```bash
dotnet add package MetaRPC.MT5
```

---

## 🚀 30-Second Quick Start

```csharp
using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Initialize account credentials
        // Sign up at https://mrpc.pro/signup to get your free API key
        var account = new MT5Account(
            user: 12345678,                          // MT5 Login
            password: "your_mt5_password",            // MT5 Password
            grpcServer: "https://mt5.mrpc.pro:443",   // Cloud gRPC Endpoint
            apiKey: "your_mrpc_api_key"               // From https://mrpc.pro/my
        );

        // 2. Connect by broker server name
        Console.WriteLine("Connecting to MetaTrader 5 Cloud...");
        await account.ConnectByServerNameAsync("MetaQuotes-Demo", baseChartSymbol: "EURUSD", timeoutSeconds: 30);
        Console.WriteLine("Connected successfully!");

        // 3. Get real-time account summary
        var summary = await account.AccountSummaryAsync();
        Console.WriteLine($"Balance: ${summary.AccountBalance:N2}");
        Console.WriteLine($"Equity:  ${summary.AccountEquity:N2}");
        Console.WriteLine($"Free Margin: ${summary.AccountMarginFree:N2}");
    }
}
```

---

## 🔑 Getting Your API Key & Free Trial

1. **Sign Up**: Create your free account at [https://mrpc.pro/signup](https://mrpc.pro/signup).
2. **Copy API Key**: Open your portal dashboard at [https://mrpc.pro/my](https://mrpc.pro/my) and grab your personal API token.
3. **Connect**: Pass your key in code or set the `MRPC_API_KEY` environment variable:
   ```bash
   export MRPC_API_KEY="your_api_token_here"
   ```

---

## 🌐 Production Endpoints

| Environment | Host / URL | Port | Protocol | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| **MT5 Production gRPC** | `mt5.mrpc.pro` | `443` | TLS / gRPC | High-throughput trading & streaming |
| **Interactive API UI (Swagger)** | [https://mt5.mrpc.pro/apiui](https://mt5.mrpc.pro/apiui) | `443` | HTTPS / REST | Interactive REST endpoints & testing |
| **Portal Dashboard** | [https://mrpc.pro/my](https://mrpc.pro/my) | `443` | HTTPS | Manage terminals, copiers & keys |
| **Account Registration** | [https://mrpc.pro/signup](https://mrpc.pro/signup) | `443` | HTTPS | Instant free trial registration |

---

## 🏢 Compatible Brokers & Prop Firms

Tested and verified with over 500+ MetaTrader server environments:
- **Prop Firms**: FTMO, FundedNext, The Funded Trader, E8 Funding, Alpha Capital, SurgeTrader.
- **Brokers**: IC Markets, Pepperstone, Exness, Tickmill, XM, FXCM, FP Markets, Eightcap, AvaTrade.

---

## 📚 Complete Documentation & Guides

- 📖 [Comprehensive Documentation](https://metarpc.github.io/CSharpMT5/)
- 🚀 [Quick Start & First Project](https://metarpc.github.io/CSharpMT5/All_Guides/Your_First_Project/)
- 📡 [Live Market Data & gRPC Streaming](https://metarpc.github.io/CSharpMT5/All_Guides/GRPC_STREAM_MANAGEMENT/)
- 💼 [Account Management & Order Execution](https://metarpc.github.io/CSharpMT5/API_Reference/MT5Account/)
- 📊 [Return Codes & Error Handling](https://metarpc.github.io/CSharpMT5/All_Guides/RETURN_CODES_REFERENCE/)

---

## 📄 License

This SDK is open-sourced under the [MIT License](LICENSE).  
Cloud infrastructure and API services are operated by [MetaRPC](https://mrpc.pro).
