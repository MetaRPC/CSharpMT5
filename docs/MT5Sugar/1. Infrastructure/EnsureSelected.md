# ✅ Ensure Symbol Selected and Synchronized (`EnsureSelected`)

> **Sugar method:** Ensures a symbol is selected in MarketWatch and synchronized with server before using it. Throws exception if synchronization fails.

**API Information:**

* **Extension method:** `MT5Service.EnsureSelected(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Underlying calls:** `SymbolSelectAsync()` + `SymbolIsSynchronizedAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static async Task EnsureSelected(
        this MT5Service svc,
        string symbol,
        int timeoutSec = 10,
        CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                                     |
| ------------ | ------------------- | ----------------------------------------------- |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)          |
| `symbol`     | `string`            | Symbol name (e.g., `"EURUSD"`, `"XAUUSD"`)      |
| `timeoutSec` | `int`               | Timeout in seconds (default: 10)                |
| `ct`         | `CancellationToken` | Cancellation token                              |

---

## ⬆️ Output

| Type   | Description                                |
| ------ | ------------------------------------------ |
| `Task` | Completes successfully if symbol is ready |

**Throws:**

* `InvalidOperationException` — If symbol is not synchronized after selection

---

## 💬 Just the essentials

* **What it is.** Helper that ensures symbol is visible in MarketWatch and fully synchronized with server data before you use it.
* **Why you need it.** Prevents errors when accessing symbol data (ticks, properties, DOM) by ensuring symbol is ready first.
* **Sanity check.** If method completes without exception → symbol is ready. If throws → symbol not available or not synchronized.

---

## 🎯 Purpose

Use it before symbol operations:

* Before reading symbol properties (tick, spread, etc.).
* Before placing orders on a symbol.
* Before subscribing to MarketBook (DOM).
* As safety check in automated strategies.

---

## 🧩 Notes & Tips

* **Two-step process:** First selects symbol (makes it visible in MarketWatch), then verifies synchronization.
* **Synchronization:** Ensures symbol data is up-to-date from server. Critical for accurate prices.
* **Throws on failure:** Unlike low-level methods, this throws exception if symbol can't be synchronized.
* **Timeout:** 10 seconds is usually enough. Increase for slow connections or exotic symbols.
* **Idempotent:** Safe to call multiple times. Won't fail if symbol already selected.
* **Used internally:** Many other sugar methods call this automatically (e.g., `PlaceMarket`, `GetSymbolSnapshot`).

---

## 🔧 Under the Hood

This sugar method combines two low-level calls:

```csharp
// Step 1: Select symbol (make it visible in MarketWatch)
await svc.SymbolSelectAsync(symbol, selected: true, deadline, ct);

// Step 2: Check if symbol is synchronized with server
var sync = await svc.SymbolIsSynchronizedAsync(symbol, deadline, ct);

// Step 3: Throw exception if not synchronized
if (!sync)
    throw new InvalidOperationException($"Symbol '{symbol}' is not synchronized in terminal.");
```

**What it improves:**

* **Combines 2 calls into 1** - simpler API
* **Automatic validation** - throws if symbol not ready
* **Shared deadline** - both calls use same timeout
* **Clear error message** - tells you exactly what's wrong

---

## 📊 Low-Level Alternative

**WITHOUT sugar (manual approach):**
```csharp
// You have to do this manually:
var deadline = DateTime.UtcNow.AddSeconds(10);

// Step 1: Select symbol
await svc.SymbolSelectAsync("EURUSD", selected: true, deadline, ct);

// Step 2: Check synchronization
var sync = await svc.SymbolIsSynchronizedAsync("EURUSD", deadline, ct);

// Step 3: Validate manually
if (!sync)
{
    throw new InvalidOperationException("Symbol 'EURUSD' is not synchronized in terminal.");
}

// Now you can use the symbol
var tick = await svc.SymbolInfoTickAsync("EURUSD", deadline, ct);
```

**WITH sugar (one-liner):**
```csharp
// Sugar method does all of the above:
await svc.EnsureSelected("EURUSD");

// Now you can use the symbol
var tick = await svc.SymbolInfoTickAsync("EURUSD", ...);
```

**Benefits:**

* ✅ **3 lines → 1 line**
* ✅ **Automatic deadline management**
* ✅ **Automatic validation**
* ✅ **Clearer intent**

---

## 🔗 Usage Examples

### 1) Basic usage - ensure symbol ready

```csharp
// svc — MT5Service instance

await svc.EnsureSelected("EURUSD");

// Now safe to use symbol
var tick = await svc.SymbolInfoTickAsync("EURUSD");
Console.WriteLine($"EURUSD Bid: {tick.Bid}");
```

---

### 2) With custom timeout

```csharp
// Use longer timeout for slow connection
await svc.EnsureSelected("BTCUSD", timeoutSec: 30);

Console.WriteLine("BTCUSD is ready");
```

---

### 3) With error handling

```csharp
var symbol = "XAUUSD";

try
{
    await svc.EnsureSelected(symbol);
    Console.WriteLine($"✓ {symbol} is ready");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"✗ Failed to synchronize {symbol}: {ex.Message}");
    return;
}

// Safe to proceed with symbol operations
var spread = await svc.GetSpreadPointsAsync(symbol);
Console.WriteLine($"Spread: {spread} points");
```

---

### 4) Ensure multiple symbols before trading

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };

foreach (var symbol in symbols)
{
    try
    {
        await svc.EnsureSelected(symbol);
        Console.WriteLine($"✓ {symbol} ready");
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine($"✗ {symbol} not available");
    }
}
```

---

### 5) Before placing order

```csharp
var symbol = "EURUSD";

// Ensure symbol ready first
await svc.EnsureSelected(symbol);

// Now safe to place order
var result = await svc.BuyMarket(
    symbol: symbol,
    volume: 0.01,
    sl: 1.0800,
    tp: 1.0900
);

Console.WriteLine($"Order placed: #{result.Order}");
```

---

### 6) Before accessing MarketBook (DOM)

```csharp
var symbol = "BTCUSD";

// Must ensure symbol ready before DOM subscription
await svc.EnsureSelected(symbol, timeoutSec: 15);

// Now can subscribe to DOM
var subResult = await svc.MarketBookAddAsync(symbol);

if (subResult.OpenedSuccessfully)
{
    Console.WriteLine($"DOM subscription opened for {symbol}");
}
```

---

### 7) Parallel symbol preparation

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };

// Prepare all symbols in parallel
var tasks = symbols.Select(symbol =>
    svc.EnsureSelected(symbol).ContinueWith(t =>
        new { Symbol = symbol, Success = !t.IsFaulted }
    )
);

var results = await Task.WhenAll(tasks);

foreach (var result in results)
{
    var status = result.Success ? "✓ Ready" : "✗ Failed";
    Console.WriteLine($"{result.Symbol}: {status}");
}

var readySymbols = results.Where(r => r.Success).Select(r => r.Symbol).ToList();
Console.WriteLine($"\n{readySymbols.Count} symbols ready for trading");
```

---

### 8) With cancellation token

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5)); // Cancel after 5 seconds

try
{
    await svc.EnsureSelected("EURUSD", timeoutSec: 10, ct: cts.Token);
    Console.WriteLine("Symbol ready");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled (timeout)");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Synchronization failed: {ex.Message}");
}
```

---

### 9) Retry pattern for unreliable symbols

```csharp
var symbol = "EXOTIC_PAIR";
int maxRetries = 3;
int retryCount = 0;
bool success = false;

while (retryCount < maxRetries && !success)
{
    try
    {
        await svc.EnsureSelected(symbol, timeoutSec: 15);
        success = true;
        Console.WriteLine($"✓ {symbol} ready after {retryCount + 1} attempt(s)");
    }
    catch (InvalidOperationException)
    {
        retryCount++;
        Console.WriteLine($"Retry {retryCount}/{maxRetries}...");
        await Task.Delay(2000); // Wait 2 seconds before retry
    }
}

if (!success)
{
    Console.WriteLine($"✗ Failed to prepare {symbol} after {maxRetries} attempts");
}
```

---

### 10) Integration with strategy initialization

```csharp
public class TradingStrategy
{
    private readonly MT5Service _svc;
    private readonly string[] _symbols;

    public TradingStrategy(MT5Service svc, string[] symbols)
    {
        _svc = svc;
        _symbols = symbols;
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("Initializing strategy...");

        // Ensure all symbols ready
        foreach (var symbol in _symbols)
        {
            await _svc.EnsureSelected(symbol, timeoutSec: 20);
            Console.WriteLine($"  ✓ {symbol} ready");
        }

        Console.WriteLine("Strategy initialized successfully");
    }

    public async Task ExecuteAsync()
    {
        // All symbols guaranteed to be ready here
        foreach (var symbol in _symbols)
        {
            var tick = await _svc.SymbolInfoTickAsync(symbol);
            Console.WriteLine($"{symbol}: Bid={tick.Bid}");
        }
    }
}

// Usage
var strategy = new TradingStrategy(svc, new[] { "EURUSD", "GBPUSD" });
await strategy.InitializeAsync();
await strategy.ExecuteAsync();
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `SymbolSelectAsync()` - Select/deselect symbol in MarketWatch (step 1)
* `SymbolIsSynchronizedAsync()` - Check if symbol synchronized with server (step 2)

**🍬 Sugar methods that use `EnsureSelected` internally:**

* `GetSymbolSnapshot()` - Calls this before getting symbol data
* `PlaceMarket()` / `BuyMarket()` / `SellMarket()` - Ensures symbol ready before placing order
* `PlacePending()` - Ensures symbol ready before placing pending order
* All `*LimitPoints()` and `*StopPoints()` methods - Ensure symbol ready first
* All `*ByRisk()` methods - Ensure symbol ready before calculating volume
* `NormalizePriceAsync()` / `NormalizeVolumeAsync()` - Can optionally call this first

---

## ⚠️ Common Pitfalls

1. **Symbol doesn't exist on broker:**
   ```csharp
   // Will throw InvalidOperationException
   await svc.EnsureSelected("NONEXISTENT_SYMBOL");
   ```

2. **Symbol exists but not tradeable:**
   ```csharp
   // May fail if symbol is not synchronized (e.g., closed market hours)
   await svc.EnsureSelected("SYMBOL");
   ```

3. **Too short timeout:**
   ```csharp
   // May fail on slow connection
   await svc.EnsureSelected("SYMBOL", timeoutSec: 1);
   ```

**Solution:** Use reasonable timeout (10-30 seconds) and handle exceptions properly.
