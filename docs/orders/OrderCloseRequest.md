## Closing an existing order

> **Requesting** to close an open order in MT5.

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

✨**Method Signature:** Task<OrderCloseResponse> OrderCloseAsync(OrderCloseRequest request);

* **Input:**
  * **request (OrderCloseRequest)** — object with properties:
    * **Ticket (ulong)** — ticket ID of the order to close.
    * **Volume (double)** — volume to close (in lots).

* **Output:**
  * **OrderCloseResponse** — object with properties:
    * **ReturnedCode (int)** — result code of the close operation.
    * **ReturnedCodeDescription (string)** — human-readable description of the result.

**Purpose:**
Allows you to close an existing order (partially or fully) by ticket in a single call and receive the operation’s return code and description for logging and handling errors. 🚀
  
