# Getting All Open Orders

> **Request:** current list of open orders from MT5
> Fetch all open orders on the account in one call.

### Code Example

```csharp
var openedOrdersData = await _mt5Account.OpenedOrdersAsync();
_logger.LogInformation(
    "OpenedOrdersAsync: Count={Count}",
    openedOrdersData.OpenedOrders.Count
);
```

✨ **Method Signature:**

```csharp
Task<OpenedOrdersResponse> OpenedOrdersAsync()
```

---

## Input

*None* — this method takes no parameters.

---

## Output

**`OpenedOrdersResponse`** — structure with:

* **`OpenedOrders`** (`IReadOnlyList<OrderInfo>`) — list of all current open orders.

### `OrderInfo` Structure

Each item in `OpenedOrders` contains:

* **`Ticket`** (`ulong`) — unique ticket number of the order.
* **`Symbol`** (`string`) — trading symbol (e.g., `"EURUSD"`).
* **`Type`** (`TMT5_ENUM_ORDER_TYPE`) — order type. Possible values:

  * **`Tmt5OrderTypeBuy`** — market Buy order.
  * **`Tmt5OrderTypeSell`** — market Sell order.
  * **`Tmt5OrderTypeBuyLimit`** — pending Buy Limit order.
  * **`Tmt5OrderTypeSellLimit`** — pending Sell Limit order.
  * **`Tmt5OrderTypeBuyStop`** — pending Buy Stop order.
  * **`Tmt5OrderTypeSellStop`** — pending Sell Stop order.
  * **`Tmt5OrderTypeBuyStopLimit`** — pending Buy Stop Limit order.
  * **`Tmt5OrderTypeSellStopLimit`** — pending Sell Stop Limit order.
* **`VolumeInitial`** (`double`) — original volume of the order when placed.
* **`VolumeCurrent`** (`double`) — remaining volume of the order.
* **`PriceOpen`** (`double`) — price at which the order was opened.
* **`PriceCurrent`** (`double`) — current market price for the symbol.
* **`StopLoss`** (`double`) — current stop-loss level.
* **`TakeProfit`** (`double`) — current take-profit level.
* **`Commission`** (`double`) — commission charged so far on the order.
* **`Swap`** (`double`) — swap or rollover charges accrued.
* **`Profit`** (`double`) — the current floating profit or loss of the order.
* **`TimeSetup`** (`DateTime`) — UTC timestamp when the order was placed.
* **`State`** (`TMT5_ENUM_ORDER_STATE`) — current state of the order. Possible values:

  * **`Tmt5OrderStatePlaced`**
  * **`Tmt5OrderStateExecuted`**
  * **`Tmt5OrderStateCancelled`**
  * **`Tmt5OrderStateRejected`**
  * **`Tmt5OrderStateExpired`**

---

## Purpose

Allows you to receive all open orders on the account in one universal call, so that you can log, process, or make decisions on them without juggling multiple methods. 🚀
