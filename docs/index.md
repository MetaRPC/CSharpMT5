# Getting Started with MetaTrader 5

This guide walks you through the steps required to:

* ✅ Register and log in to a MetaTrader 5 account
* ✅ Connect the C# gRPC client to your MT5 terminal
* ✅ Retrieve live account and trading data programmatically

---

## 🧾 Account Setup

### 1. Launch MetaTrader 5

Open the MetaTrader 5 application on your machine.

### 2. Access Account Panel

[![](ImagesForGuidance/1..png)](ImagesForGuidance/1..png)
Click the **person icon** in the top-right corner to access account options.

### 3. Register or Log In

[![](ImagesForGuidance/2..png)](ImagesForGuidance/2..png)
Choose to log into an existing account or register a new one.

### 4. Registering an Account

[![](ImagesForGuidance/3..png)](ImagesForGuidance/3..png)
Enter your email and desired username, then click **Create**.

* You will receive an email with your **login credentials** and a button to activate the account.

### 5. Activate Your Account

Click the **Activate Account** button from the email. Once confirmed, return to MetaTrader 5 and log in with the provided credentials.

---

## 📥 Opening a Trading Account

### 6. Open Account in MT5

[![](ImagesForGuidance/5..png)](ImagesForGuidance/5..png)
Go to `File` → `Open an Account`

### 7. Select Your Broker

[![](ImagesForGuidance/6..png)](ImagesForGuidance/6..png)
Choose `MetaQuotes Ltd` or your preferred broker.

### 8. Choose Account Type

[![](ImagesForGuidance/7..png)](ImagesForGuidance/7..png)
Select a demo or real account type. Click **Next**.

### 9. Fill in Your Details

Enter personal info, accept terms, and click **Next**.

### 10. Save Your Login Credentials

[![](ImagesForGuidance/8..png)](ImagesForGuidance/8..png)
Your login, password, and investor password will be shown. **Save them securely.**

### 11. Log in to the Trading Account

[![](ImagesForGuidance/9..png)](ImagesForGuidance/9..png)
Go to `File` → `Log in to Trading Account` and enter your saved credentials.

### 12. Confirm Login

[![](ImagesForGuidance/10..png)](ImagesForGuidance/10..png)
Check `Save Password` if you want to auto-login next time.

---

## 🌐 Accessing WebTrader

### 13–16. Log In to WebTrader

Follow these steps to access your account online:

* [Web login](ImagesForGuidance/11..png)
* [Enter credentials](ImagesForGuidance/12..png)
* [Click `Connect to Account`](ImagesForGuidance/13..png)
* [Access WebTrader Dashboard](ImagesForGuidance/14..png)

---

## ⚙️ Connecting the MT5 C# Application

This section explains how to run the MT5 C# gRPC client.

### 🔧 Prerequisites

* A valid MetaTrader 5 account (from steps above)
* Visual Studio or any C# IDE installed
* The MetaRPC MT5 repository cloned locally

### Steps

#### 1. Clone the MT5 Repository

Download the [CSharpMT5](https://github.com/MetaRPC/CSharpMT5) project to your machine.

#### 2. Open the Project in Your Code Editor

Open the repo in Visual Studio or VS Code.

#### 3. Open `profiles.json`

Fill in the relevant information.
You can also create multiple profiles and switch through them quickly.

```json
{
  "default": {
    "AccountId": 21455,
    "Password": "1nJeS+Ae",
    "Host": "95.217.147.61",
    "Port": 443,
    "GrpcServer": "https://mt5.mrpc.pro:443",
    "DefaultSymbol": "EURUSD",
    "DefaultVolume": 0.1
  },
  "demo": {
    "AccountId": 95591860,
    "Password": "GyI@7a1m",
    "ServerName": "MetaQuotes-Demo",
    "GrpcServer": "https://mt5.mrpc.pro:443",
    "DefaultSymbol": "GBPUSD",
    "DefaultVolume": 0.2
  }
}
```

---

## ▶️ After filling `profiles.json`

Once your **profiles.json** is ready, the next step is to actually run the MT5 C# CLI and test connectivity.

### 1. Restore and build

From the repo root:

```powershell
dotnet restore
```

### 2. Run commands

The general pattern is:

```powershell
dotnet run -- <command> [options]
```

Examples:

```powershell
# Show all available profiles
dotnet run -- profiles list

# Show details for your "default" profile
dotnet run -- profiles show -p default

# Health check (ping)
dotnet run -- health -p default

# Get one quote
dotnet run -- quote -p default -s EURUSD
```

### 3. Place your first order (demo profile)

```powershell
dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --deviation 10
```

> Tip: use `--dry-run` to preview what would be sent without touching the account.

---

## ⚡ Shortcasts (optional but handy)

You can also load PowerShell aliases once per session:

```powershell
. .\ps\shortcasts.ps1
use-pf demo
use-sym EURUSD

info   # expands to: mt5 info -p demo
q      # expands to: mt5 quote -p demo -s EURUSD
b -v 0.10   # market buy with defaults
```

See also: **CLI Shortcasts & Live Examples**.

---

## 🧠 SL/TP rules (quick)

* **BUY**: enter at **Ask** → `SL < Ask`, `TP > Ask`
* **SELL**: enter at **Bid** → `SL > Bid`, `TP < Bid`
* Use `position.modify.points` with `--from entry|market` to set distances in **points**.

---

## ⏱ Timeouts & retries

* `--timeout-ms` bounds each RPC. Internally we wrap operations in `UseOpTimeout` and per‑call CTS via `StartOpCts`.
* Calls go through `CallWithRetry(...)` to automatically retry selected transient errors.
* For CI, reduce timeout (fast fail). For slow terminals, increase to 60–120s.

---

## 🛟 Troubleshooting

* **“Set Host or MtClusterName”** → profile not picked up. Run `profiles show` and verify `profiles.json` path.
* **Hidden symbol** → `symbol ensure -s <SYM>` before trading or pending changes.
* **Timeouts** → raise `--timeout-ms`, test with `--trace` to see where it stuck.
* **Zero Margin/FreeMargin** on empty accounts is normal; equity ≈ balance when flat.

---

## 🔗 What next

* **Profiles** → details & tips: `Profiles_Reference.md`
* **Account / Info** → `Info.md`
* **Market data** → `Quote.md`, `Symbol/Show.md`, `Symbol/Limits.md`, `Symbol/Ensure.md`
* **Orders & Positions** → `Orders_Positions_Overview.md`
* **History** → `History.md`, `History_Export.md`
* **Misc tools** → `Ticket.md`, `Lot_Calc.md`, `Panic.md`

If something is unclear, open an issue or ping in the repo discussions — happy to help! 🎯
