## Retrieving Multiple Symbol Parameters

> **Request:** fetch parameters for multiple symbols from MT5
> Retrieve detailed parameter data for a set of symbols (with optional paging).

---

### Code Example

```csharp
var request = new SymbolParamsManyRequest
{
    SymbolName   = "EURUSD",
    SortType     = AH_SYMBOL_PARAMS_MANY_SORT_TYPE.AhParamsManySortTypeSymbolNameAsc,
    PageNumber   = 0,
    ItemsPerPage = 50
};
var symbols = await _mt5Account.SymbolParamsManyAsync(request);
_logger.LogInformation(
    "SymbolParamsManyAsync: Count={Count}",
    symbols.SymbolInfos.Count);
```

---

### Method Signature

```csharp
Task<SymbolParamsManyData> SymbolParamsManyAsync(
    SymbolParamsManyRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

**`SymbolParamsManyRequest`** — structure with the following fields:

| Field               | Type                              | Description                                        |
| ------------------- | --------------------------------- | -------------------------------------------------- |
| `SymbolName`        | `string`                          | Optional filter to return matching symbol names    |
| `SortType`          | `AH_SYMBOL_PARAMS_MANY_SORT_TYPE` | Enum for sorting results (e.g., by name or spread) |
| `PageNumber`        | `int`                             | Zero-based page number                             |
| `ItemsPerPage`      | `int`                             | Number of records per page                         |
| `deadline`          | `DateTime?`                       | Optional timeout                                   |
| `cancellationToken` | `CancellationToken`               | Optional cancel token                              |

### Sort Type Enum Values (Examples)

| Enum Value                           | Description                    |
| ------------------------------------ | ------------------------------ |
| `AhParamsManySortTypeSymbolNameAsc`  | Sort by symbol name A→Z        |
| `AhParamsManySortTypeSymbolNameDesc` | Sort by symbol name Z→A        |
| `AhParamsManySortTypeSpreadAsc`      | Sort by spread (lowest first)  |
| `AhParamsManySortTypeSpreadDesc`     | Sort by spread (highest first) |

---

## ⬆️ Output

Returns a **SymbolParamsManyData** object:

| Field          | Type                              | Description                                      |
| -------------- | --------------------------------- | ------------------------------------------------ |
| `SymbolInfos`  | `RepeatedField<SymbolParameters>` | List of parameter data per symbol (current page) |
| `SymbolsTotal` | `int`                             | Total number of matched symbols                  |
| `PageNumber`   | `int`                             | Page number returned                             |
| `ItemsPerPage` | `int`                             | Number of items per page                         |

### `SymbolParameters` Structure (partial)

Each `SymbolInfo` includes fields like:

| Field               | Type        | Description                  |
| ------------------- | ----------- | ---------------------------- |
| `Symbol`            | `string`    | Symbol name (e.g., "EURUSD") |
| `Digits`            | `int`       | Number of decimal places     |
| `Point`             | `double`    | Minimum price change         |
| `Spread`            | `int`       | Current spread in points     |
| `VolumeMin`         | `double`    | Minimum trading volume       |
| `VolumeMax`         | `double`    | Maximum trading volume       |
| `VolumeStep`        | `double`    | Step for volume changes      |
| `SwapLong`          | `double`    | Swap for long positions      |
| `SwapShort`         | `double`    | Swap for short positions     |
| `SessionTradeFrom`  | `Timestamp` | Session start (UTC)          |
| `SessionTradeTo`    | `Timestamp` | Session end (UTC)            |
| `MarginInitial`     | `double`    | Initial margin per lot       |
| `MarginMaintenance` | `double`    | Maintenance margin per lot   |

*For the full list, refer to the original gRPC schema or generated model.*

---

## 🎯 Purpose

Use this endpoint to retrieve **full trading parameters** for multiple symbols in a single query.

Perfect for:

* Building symbol configuration panels
* Trading UI setup
* Dynamic filtering and analysis
