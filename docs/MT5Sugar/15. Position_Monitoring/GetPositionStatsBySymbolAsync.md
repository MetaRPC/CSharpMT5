# Get Position Stats By Symbol (`GetPositionStatsBySymbolAsync`)

> **Portfolio method:** Aggregate position statistics per symbol - count, volume, and P&L.

**API:** `MT5Service.GetPositionStatsBySymbolAsync(...)`
**Region:** [15] POSITION MONITORING

## Signature

```csharp
public static async Task<Dictionary<string, (int count, double totalVolume, double totalPnL)>>
    GetPositionStatsBySymbolAsync(
        this MT5Service svc,
        int timeoutSec = 20,
        CancellationToken ct = default)
```

## Returns

**Dictionary<string, (int count, double totalVolume, double totalPnL)>**

- **Key**: Symbol name (e.g., "EURUSD", "XAUUSD")
- **Value Tuple**:
  - `count` - Number of open positions
  - `totalVolume` - Sum of all position volumes (lots)
  - `totalPnL` - Total profit/loss for this symbol

## Examples

```csharp
// Get portfolio breakdown
var stats = await svc.GetPositionStatsBySymbolAsync();

Console.WriteLine("📊 Portfolio Statistics:");
foreach (var (symbol, (count, volume, pnl)) in stats)
{
    string plEmoji = pnl >= 0 ? "🟢" : "🔴";
    Console.WriteLine($"{plEmoji} {symbol}: {count} positions, {volume:F2} lots, ${pnl:F2} P&L");
}

// Example output:
// 📊 Portfolio Statistics:
// 🟢 EURUSD: 3 positions, 0.30 lots, $45.50 P&L
// 🔴 GBPUSD: 2 positions, 0.20 lots, -$12.30 P&L
// 🟢 XAUUSD: 1 positions, 0.05 lots, $89.00 P&L

// Find worst-performing symbol
var stats = await svc.GetPositionStatsBySymbolAsync();
var worstSymbol = stats
    .OrderBy(kvp => kvp.Value.totalPnL)
    .FirstOrDefault();

if (worstSymbol.Key != null)
{
    Console.WriteLine($"⚠️ Worst performer: {worstSymbol.Key} with ${worstSymbol.Value.totalPnL:F2}");

    if (worstSymbol.Value.totalPnL < -100)
    {
        Console.WriteLine($"🚨 Closing all {worstSymbol.Key} positions!");
        // Close all positions for this symbol
        var positions = await svc.GetLosingPositionsAsync(worstSymbol.Key);
        // ... close logic
    }
}

// Portfolio diversification check
var stats = await svc.GetPositionStatsBySymbolAsync();
int symbolCount = stats.Count;
double totalVolume = stats.Sum(kvp => kvp.Value.totalVolume);

Console.WriteLine($"📈 Trading {symbolCount} symbols with total {totalVolume:F2} lots");

foreach (var (symbol, (count, volume, pnl)) in stats)
{
    double volumePercent = (volume / totalVolume) * 100;
    Console.WriteLine($"  {symbol}: {volumePercent:F1}% of portfolio");

    if (volumePercent > 40)
    {
        Console.WriteLine($"  ⚠️ Over-concentrated in {symbol}!");
    }
}

// Symbol exposure limits
var maxPositionsPerSymbol = 5;
var stats = await svc.GetPositionStatsBySymbolAsync();

bool canTrade = true;
if (stats.TryGetValue("EURUSD", out var eurusdStats))
{
    if (eurusdStats.count >= maxPositionsPerSymbol)
    {
        Console.WriteLine($"⚠️ EURUSD limit reached: {eurusdStats.count}/{maxPositionsPerSymbol}");
        canTrade = false;
    }
}

if (canTrade)
{
    await svc.BuyMarketByRisk("EURUSD", 50, 100);
}
```

## Summary

- ✅ Returns dictionary grouped by symbol
- ✅ Includes count, total volume, and total P&L
- ✅ Perfect for portfolio analytics and risk management
