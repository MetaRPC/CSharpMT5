# Closing an Existing Order

> **Request:** to close an open order in MT5.

---

### Code Example

```csharp
var closeRequest = new OrderCloseRequest
{
    Ticket = ticket,
    Volume = Constants.DefaultVolume
};
var closeResult = await _mt5Account.OrderCloseAsync(closeRequest);
_logger.LogInformation(
    "OrderCloseAsync: Ticket={Ticket} ReturnCode={Code} Description={Desc}",
    ticket,
    closeResult.ReturnedCode,
    closeResult.ReturnedCodeDescription
);
```

---

### Method Signature

```csharp
Task<OrderCloseResponse> OrderCloseAsync(OrderCloseRequest request)
```

---

## 🔽 Input

**OrderCloseRequest** — object with the following fields:

| Field    | Type     | Description               |
| -------- | -------- | ------------------------- |
| `Ticket` | `ulong`  | ID of the order to close  |
| `Volume` | `double` | Volume to close (in lots) |

---

## ⬆️ Output

Returns an **OrderCloseResponse** object with:

| Field                     | Type     | Description                                |
| ------------------------- | -------- | ------------------------------------------ |
| `ReturnedCode`            | `int`    | Code indicating result of the operation    |
| `ReturnedCodeDescription` | `string` | Description of the result (human-readable) |

---

## 🎯 Purpose

Use this method to **partially or fully close** an existing trade in MT5 by specifying the order ticket and volume.

It provides a simple response with a result code and message, useful for:

* Logging success/failure of close operations
* Error handling in trade workflows
* Manual or automated trade exits
