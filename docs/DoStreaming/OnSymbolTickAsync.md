## Streaming: OnSymbolTickAsync

> **Request:** real-time tick updates for one or more symbols

---

### Code Example

```csharp
await foreach (var tick in _mt5Account.OnSymbolTickAsync(
    new[] { Constants.DefaultSymbol },
    cts.Token))
{
    _logger.LogInformation(
        "OnSymbolTickAsync: Symbol={Symbol} Ask={Ask}",
        tick.SymbolTick.Symbol,
        tick.SymbolTick.Ask);
}
```

---

### Method Signature

```csharp
IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
    IEnumerable<string> symbols,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                  | Description                                    |
| ------------------- | --------------------- | ---------------------------------------------- |
| `symbols`           | `IEnumerable<string>` | List of symbols to subscribe to (e.g., EURUSD) |
| `cancellationToken` | `CancellationToken`   | Token to cancel the streaming session          |

---

## ⬆️ Output

Returns a stream of **OnSymbolTickData** items:

| Field        | Type          | Description                           |
| ------------ | ------------- | ------------------------------------- |
| `SymbolTick` | `MrpcMqlTick` | Tick structure containing market data |

### `MrpcMqlTick` Structure

| Field    | Type       | Description                         |
| -------- | ---------- | ----------------------------------- |
| `Symbol` | `string`   | Symbol name (e.g., EURUSD)          |
| `Bid`    | `double`   | Current bid price                   |
| `Ask`    | `double`   | Current ask price                   |
| `Last`   | `double`   | Last deal price                     |
| `Volume` | `long`     | Volume at the tick                  |
| `Time`   | `DateTime` | UTC time when the tick was recorded |

---

## 🎯 Purpose

This stream enables your application to receive **live tick data for one or more symbols** in real time. Useful for:

* Updating charting interfaces
* Real-time dashboards
* Triggering trades or alerts in algorithmic systems
