# ✅ Closing Orders and Positions (`OrderCloseAsync`)

> **Request:** Close an open position or cancel a pending order on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderCloseAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradingHelper`
* **Proto definition:** `OrderClose` (defined in `mt5-term-api-trading-helper.proto`)

### RPC

* **Service:** `mt5_term_api.TradingHelper`
* **Method:** `OrderClose(OrderCloseRequest) → OrderCloseReply`
* **Low‑level client (generated):** `TradingHelper.TradingHelperClient.OrderClose(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<OrderCloseData> OrderCloseAsync(
            OrderCloseRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderCloseRequest { ticket, volume, slippage }`


**Reply message:**

`OrderCloseReply { data: OrderCloseData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `request`           | `OrderCloseRequest` | Protobuf request with close parameters                    |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

### `OrderCloseRequest`

| Field      | Type    | Description                                                |
| ---------- | ------- | ---------------------------------------------------------- |
| `Ticket`   | `ulong` | Position or pending order ticket **REQUIRED**               |
| `Volume`   | `double` | Volume to close (for partial close; use `0` for full close) |
| `Slippage` | `int`   | Max price slippage in points (for market positions)         |

---

## ⬆️ Output — `OrderCloseData`

| Field                      | Type                     | Description                                  |
| -------------------------- | ------------------------ | -------------------------------------------- |
| `ReturnedCode`             | `uint`                   | Operation return code (10009 = success)      |
| `ReturnedStringCode`       | `string`                 | String representation of return code         |
| `ReturnedCodeDescription`  | `string`                 | Human-readable description                   |
| `CloseMode`                | `MRPC_ORDER_CLOSE_MODE`  | Close mode (market/partial/pending)          |

---

## 🧱 Related enums (from proto)

### `MRPC_ORDER_CLOSE_MODE`

* `MrpcMarketOrderClose` — Full market position close
* `MrpcMarketOrderPartialClose` — Partial market position close
* `MrpcPendingOrderRemove` — Pending order cancellation

---

## 💬 Just the essentials

* **What it is.** Closes open market positions (full or partial) or cancels pending orders.
* **Why you need it.** Exit trades, take profits, cut losses, cancel unfilled pending orders.
* **Sanity check.** If `ReturnedCode == 10009` → close/cancel successful. Check `CloseMode` for operation type.

---

## 🎯 Purpose

Use it to exit trades:

* Close entire position (full close).
* Close part of position (partial close).
* Cancel pending orders.

---

## 🧩 Notes & Tips

* **Return codes:** 10009 = success. Other codes indicate errors (invalid ticket, market closed, etc.).
* **Full vs Partial:** Set `Volume = 0` for full close. Set specific volume (e.g., `0.01`) for partial close.
* **Pending orders:** For pending orders, `Volume` is ignored. Order is simply cancelled.
* **Slippage:** For market positions, allows price deviation. Higher slippage = higher chance of execution.
* **CloseMode:** Check this field to confirm operation type (full/partial/pending).

---

## 🔗 Usage Examples

### 1) Close entire position

```csharp
// acc — connected MT5Account
// positionTicket — ticket from OrderSendAsync or OpenedOrdersAsync

var result = await acc.OrderCloseAsync(new OrderCloseRequest
{
    Ticket = positionTicket,
    Volume = 0,  // 0 = close entire position
    Slippage = 10
});

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✓ Position closed: {result.CloseMode}");
}
else
{
    Console.WriteLine($"✗ Close failed: {result.ReturnedCodeDescription}");
}
```

---

### 2) Partial position close

```csharp
// Close half of 0.10 lot position (close 0.05 lots)
var result = await acc.OrderCloseAsync(new OrderCloseRequest
{
    Ticket = positionTicket,
    Volume = 0.05,  // Partial close: 0.05 lots
    Slippage = 10
});

if (result.CloseMode == MRPC_ORDER_CLOSE_MODE.MrpcMarketOrderPartialClose)
{
    Console.WriteLine("✓ Partial close successful");
}
```

---

### 3) Cancel pending order

```csharp
// pendingOrderTicket — ticket of pending order

var result = await acc.OrderCloseAsync(new OrderCloseRequest
{
    Ticket = pendingOrderTicket,
    Volume = 0  // Volume ignored for pending orders
});

if (result.CloseMode == MRPC_ORDER_CLOSE_MODE.MrpcPendingOrderRemove)
{
    Console.WriteLine("✓ Pending order cancelled");
}
```

---

### 4) Close all positions for a symbol

```csharp
var openOrders = await acc.OpenedOrdersAsync();
var eurusdPositions = openOrders.Positions.Where(p => p.Symbol == "EURUSD");

foreach (var position in eurusdPositions)
{
    var result = await acc.OrderCloseAsync(new OrderCloseRequest
    {
        Ticket = (ulong)position.Ticket,
        Volume = 0,
        Slippage = 10
    });

    Console.WriteLine($"Position {position.Ticket}: {result.ReturnedCodeDescription}");
}
```

---

### 5) Close position with error handling

```csharp
try
{
    var result = await acc.OrderCloseAsync(
        new OrderCloseRequest
        {
            Ticket = positionTicket,
            Volume = 0,
            Slippage = 20
        },
        deadline: DateTime.UtcNow.AddSeconds(10));

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"✓ Closed: {result.CloseMode}");
    }
    else
    {
        Console.WriteLine($"⚠ Server returned: {result.ReturnedCodeDescription}");
    }
}
catch (Grpc.Core.RpcException ex)
{
    Console.WriteLine($"✗ gRPC error: {ex.Status}");
}
catch (ApiExceptionMT5 ex)
{
    Console.WriteLine($"✗ API error: {ex.Message}");
}
```

---

### 6) Close position with volume validation

```csharp
// Get position info first
var positions = await acc.OpenedOrdersAsync();
var position = positions.Positions.FirstOrDefault(p => p.Ticket == positionTicket);

if (position != null)
{
    var volumeToClose = 0.01; // Want to close 0.01 lots

    if (volumeToClose >= position.Volume)
    {
        // Close entire position if requested volume >= position volume
        volumeToClose = 0;
    }

    var result = await acc.OrderCloseAsync(new OrderCloseRequest
    {
        Ticket = (ulong)position.Ticket,
        Volume = volumeToClose,
        Slippage = 10
    });

    Console.WriteLine($"Close result: {result.ReturnedCodeDescription}");
    Console.WriteLine($"Mode: {result.CloseMode}");
}
```
