# Retrieving Symbol Name by Index

> **Request:** symbol name at a given index from MT5
> Fetch the name of a symbol by its index in the Market Watch or the full symbol list.

### Code Example

```csharp
var symbolNameData = await _mt5Account.SymbolNameAsync(
    0,      // index of the first symbol
    false   // include all symbols
);
_logger.LogInformation(
    "SymbolNameAsync: FirstSymbol={FirstSymbol}",
    symbolNameData.Name
);
```

✨ **Method Signature:**

```csharp
Task<SymbolNameData> SymbolNameAsync(
    int index,
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **`index`** (`int`) — zero-based position of the symbol in the list.

  * For example, `0` returns the first symbol.
* **`selectedOnly`** (`bool`) — whether to search only among Market Watch symbols or all symbols:

  * `true`  — search only within symbols currently selected in Market Watch.
  * `false` — search within the full symbol list (including unselected symbols).

Optional parameters:

* **`deadline`** (`DateTime?`) — optional UTC deadline for the request (cancels if not completed).
* **`cancellationToken`** (`CancellationToken`) — optional token to cancel the request prematurely.

---

## Output

**`SymbolNameData`** — structure with the following field:

* **`Name`** (`string`) — the symbol name at the specified index, if available; otherwise empty string.

---

## Purpose

Allows you to dynamically retrieve symbol names by index for list navigation, pagination, or custom symbol ordering in your application. 🚀
