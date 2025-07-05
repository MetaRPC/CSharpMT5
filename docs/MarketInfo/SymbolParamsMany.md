# Retrieving Multiple Symbol Parameters

> **Request:** fetch parameters for multiple symbols from MT5

Retrieve detailed parameter data for a set of symbols (with optional paging).

### Code Example

```csharp
var request = new SymbolParamsManyRequest();
// (optionally set filters/page properties on request)
var symbols = await _mt5Account.SymbolParamsManyAsync(request);
_logger.LogInformation(
    "SymbolParamsManyAsync: Count={Count}",
    symbols.SymbolInfos.Count);
```

✨**Method Signature:**

```csharp
Task<SymbolParamsManyData> SymbolParamsManyAsync(
    SymbolParamsManyRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
 ```

* **Input:**
    * **request (SymbolParamsManyRequest):**
       * Optional fields: paging (`PageNumber`, `ItemsPerPage`), symbol filters, etc.

* **Output:**
    * **SymbolParamsManyData** with properties:
      * `SymbolInfos` (`RepeatedField<SymbolInfoData>`) — list of symbol parameter records.
      * `SymbolsTotal` (`int`) — total number of symbols matching the request.
      * `PageNumber` (`int`) — current page index (if paging used).
      * `ItemsPerPage` (`int`) — number of items per page (if paging used).

**Purpose:** Use a single call to fetch bulk symbol information with optional pagination and filtering, keeping your code concise and flexible. 🚀


