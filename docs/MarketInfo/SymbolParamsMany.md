## Retrieving Multiple Symbol Parameters

> **Request:** fetch parameters for multiple symbols from MT5
> Retrieve detailed parameter data for a set of symbols (with optional paging).

### Code Example

```csharp
var request = new SymbolParamsManyRequest
{
    // optional filters:
    SymbolName   = "EURUSD",                             // only symbols whose name contains this
    SortType     = AH_SYMBOL_PARAMS_MANY_SORT_TYPE.AhParamsManySortTypeSymbolNameAsc,
    PageNumber   = 0,                                    // zero-based
    ItemsPerPage = 50
};
var symbols = await _mt5Account.SymbolParamsManyAsync(request);
_logger.LogInformation(
    "SymbolParamsManyAsync: Count={Count}",
    symbols.SymbolInfos.Count);
```

✨ **Method Signature:**

```csharp
Task<SymbolParamsManyData> SymbolParamsManyAsync(
    SymbolParamsManyRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

**`SymbolParamsManyRequest`** — structure with the following fields:

* **`SymbolName`** (`string`) — optional filter: only return symbols whose name contains this value.

  * *HasSymbolName* indicates whether it was set.
* **`SortType`** (`AH_SYMBOL_PARAMS_MANY_SORT_TYPE`) — sort order for the returned list. Possible values:

  * **`AhParamsManySortTypeSymbolNameAsc`** — by symbol name ascending (default)
  * **`AhParamsManySortTypeSymbolNameDesc`** — by symbol name descending
  * **`AhParamsManySortTypeSpreadAsc`** — by current spread ascending
  * **`AhParamsManySortTypeSpreadDesc`** — by current spread descending
  * *(…other sort criteria as defined in the enum…)*
  * *HasSortType* indicates whether you explicitly set it.
* **`PageNumber`** (`int`) — zero-based page index for pagination.

  * *HasPageNumber* indicates whether you explicitly set it; default is 0.
* **`ItemsPerPage`** (`int`) — number of items to return per page.

  * *HasItemsPerPage* indicates whether you explicitly set it; default is 0 (no paging).

Optional parameters (on the async call):

* **`deadline`** (`DateTime?`) — optional UTC deadline for the operation.
* **`cancellationToken`** (`CancellationToken`) — optional token to cancel the request.

---

## Output

**`SymbolParamsManyData`** — structure with the following properties:

* **`SymbolInfos`** (`RepeatedField<SymbolParameters>`) — list of symbol parameter records matching your request (one page).
* **`SymbolsTotal`** (`int`) — total number of symbols matching your filter (across all pages).
* **`PageNumber`** (`int`) — the page index returned.
* **`ItemsPerPage`** (`int`) — the items-per-page returned.

---

### `SymbolParameters` Structure

Each item in `SymbolInfos` is a `SymbolParameters` message with many fields. Key examples include:

* **`Symbol`** (`string`) — the symbol name (e.g., "EURUSD").
* **`Digits`** (`int`) — number of decimal places.
* **`Point`** (`double`) — the minimal price change.
* **`Spread`** (`int`) — current spread (in points).
* **`VolumeMin`**, **`VolumeMax`**, **`VolumeStep`** — trading volume limits.
* **`MarginInitial`**, **`MarginMaintenance`** — margin requirements per lot.
* **`SwapLong`**, **`SwapShort`** — swap (rollover) rates.
* **`SessionTradeFrom`**, **`SessionTradeTo`** — trading session windows.
* *…and dozens more: tick values, contract size, option Greeks, session quotes, etc.*

*For the complete list of fields, see the generated `SymbolParameters` class in your MT5 gRPC client (or the original `.proto`).*

---

**Purpose:**
Use one call to retrieve bulk symbol settings (prices, volumes, margins, sessions, etc.) with optional filtering, sorting, and pagination—keeping your code concise and flexible. 🚀
