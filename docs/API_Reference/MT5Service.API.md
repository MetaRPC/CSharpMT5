Parsing MT5Service.cs...
Found 66 methods
Generated docs/API_Reference/MT5Service.API.md
---

## Table of Contents

1. [ConnectByHostPortAsync](#connectbyhostportasync)
2. [ConnectByServerNameAsync](#connectbyservernameasync)
3. [AccountSummaryAsync](#accountsummaryasync)
4. [AccountInfoDoubleAsync](#accountinfodoubleasync)
5. [AccountInfoIntegerAsync](#accountinfointegerasync)
6. [AccountInfoStringAsync](#accountinfostringasync)
7. [GetBalanceAsync](#getbalanceasync)
8. [GetEquityAsync](#getequityasync)
9. [GetMarginAsync](#getmarginasync)
10. [GetFreeMarginAsync](#getfreemarginasync)
11. [GetProfitAsync](#getprofitasync)
12. [GetLoginAsync](#getloginasync)
13. [GetLeverageAsync](#getleverageasync)
14. [GetAccountNameAsync](#getaccountnameasync)
15. [GetServerNameAsync](#getservernameasync)
16. [GetCurrencyAsync](#getcurrencyasync)
17. [SymbolsTotalAsync](#symbolstotalasync)
18. [SymbolExistAsync](#symbolexistasync)
19. [SymbolNameAsync](#symbolnameasync)
20. [SymbolSelectAsync](#symbolselectasync)
21. [SymbolIsSynchronizedAsync](#symbolissynchronizedasync)
22. [SymbolInfoDoubleAsync](#symbolinfodoubleasync)
23. [SymbolInfoIntegerAsync](#symbolinfointegerasync)
24. [SymbolInfoStringAsync](#symbolinfostringasync)
25. [SymbolInfoMarginRateAsync](#symbolinfomarginrateasync)
26. [GetBidAsync](#getbidasync)
27. [GetAskAsync](#getaskasync)
28. [GetSpreadAsync](#getspreadasync)
29. [GetVolumeMinAsync](#getvolumeminasync)
30. [GetVolumeMaxAsync](#getvolumemaxasync)
31. [GetVolumeStepAsync](#getvolumestepasync)
32. [GetPointAsync](#getpointasync)
33. [IsSymbolAvailableAsync](#issymbolavailableasync)
34. [IsTradingAllowedAsync](#istradingallowedasync)
35. [SymbolInfoTickAsync](#symbolinfotickasync)
36. [QuoteAsync](#quoteasync)
37. [SymbolInfoSessionQuoteAsync](#symbolinfosessionquoteasync)
38. [SymbolInfoSessionTradeAsync](#symbolinfosessiontradeasync)
39. [TickValueWithSizeAsync](#tickvaluewithsizeasync)
40. [SymbolParamsManyAsync](#symbolparamsmanyasync)
41. [MarketBookAddAsync](#marketbookaddasync)
42. [MarketBookReleaseAsync](#marketbookreleaseasync)
43. [MarketBookGetAsync](#marketbookgetasync)
44. [OpenedOrdersAsync](#openedordersasync)
45. [OpenedOrdersTicketsAsync](#openedordersticketsasync)
46. [OrderHistoryAsync](#orderhistoryasync)
47. [PositionsHistoryAsync](#positionshistoryasync)
48. [PositionsTotalAsync](#positionstotalasync)
49. [OrderCalcMarginAsync](#ordercalcmarginasync)
50. [OrderCheckAsync](#ordercheckasync)
51. [OrderSendAsync](#ordersendasync)
52. [OrderModifyAsync](#ordermodifyasync)
53. [OrderCloseAsync](#ordercloseasync)
54. [BuyMarketAsync](#buymarketasync)
55. [SellMarketAsync](#sellmarketasync)
56. [BuyLimitAsync](#buylimitasync)
57. [SellLimitAsync](#selllimitasync)
58. [BuyStopAsync](#buystopasync)
59. [SellStopAsync](#sellstopasync)
60. [GetRecentOrdersAsync](#getrecentordersasync)
61. [GetTodayOrdersAsync](#gettodayordersasync)
62. [OnSymbolTickAsync](#onsymboltickasync)
63. [OnTradeAsync](#ontradeasync)
64. [OnPositionProfitAsync](#onpositionprofitasync)
65. [OnPositionsAndPendingOrdersTicketsAsync](#onpositionsandpendingordersticketsasync)
66. [OnTradeTransactionAsync](#ontradetransactionasync)

---

## ConnectByHostPortAsync

Establishes connection to MT5 terminal via gRPC using direct host and port.

### Signature

```csharp
public Task ConnectByHostPortAsync(
    string host,
    int port = 443,
    string baseChartSymbol = "EURUSD",
    bool waitForTerminalIsAlive = true,
    int timeoutSeconds = 30,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `host` | `string` | gRPC server hostname or IP address (e.g., "mt5.mrpc.pro"). |
| `port` | `int` | gRPC server port number (default: 443 for HTTPS). |
| `baseChartSymbol` | `string` | Symbol to use for initial chart/market watch (default: "EURUSD"). |
| `waitForTerminalIsAlive` | `bool` | If true, waits for terminal to be ready before returning (default: true). |
| `timeoutSeconds` | `int` | Connection timeout in seconds (default: 30). |
| `deadline` | `DateTime?` | Optional RPC deadline for this call. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task` - Task that completes when connection is established.

---

## ConnectByServerNameAsync

Establishes connection to MT5 terminal via gRPC using server name lookup.

### Signature

```csharp
public Task ConnectByServerNameAsync(
    string serverName,
    string baseChartSymbol = "EURUSD",
    bool waitForTerminalIsAlive = true,
    int timeoutSeconds = 30,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `serverName` | `string` | MT5 server name (e.g., "FxPro-MT5 Demo") - will be resolved to host:port. |
| `baseChartSymbol` | `string` | Symbol to use for initial chart/market watch (default: "EURUSD"). |
| `waitForTerminalIsAlive` | `bool` | If true, waits for terminal to be ready before returning (default: true). |
| `timeoutSeconds` | `int` | Connection timeout in seconds (default: 30). |
| `deadline` | `DateTime?` | Optional RPC deadline for this call. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task` - Task that completes when connection is established.

---

## AccountSummaryAsync

Retrieves comprehensive account summary including balance, equity, margin, and key ratios.

### Signature

```csharp
public Task<AccountSummaryData> AccountSummaryAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline; if null, uses default timeout. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<AccountSummaryData>` - <see cref="AccountSummaryData"/> containing balance, equity, margin, free margin, profit, margin level, and other account metrics.

### Remarks

This is a rich object that provides multiple account fields in one call. For single field access, use GetBalanceAsync(), GetEquityAsync(), etc.

---

## AccountInfoDoubleAsync

Retrieves a double-precision numeric account property (e.g., Balance, Equity, Margin).

### Signature

```csharp
public Task<double> AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `property` | `AccountInfoDoublePropertyType` | The account property type to query. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Unwrapped double value of the requested property (no .Value needed).

### Remarks

UNWRAPPED: Returns primitive double directly instead of Data wrapper. Common properties: AccountBalance, AccountEquity, AccountMargin, AccountMarginFree, AccountProfit.

---

## AccountInfoIntegerAsync

Retrieves an integer account property (e.g., Login, Leverage, TradeMode).

### Signature

```csharp
public Task<long> AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `property` | `AccountInfoIntegerPropertyType` | The account property type to query. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<long>` - Unwrapped long value of the requested property (no .Value needed).

### Remarks

UNWRAPPED: Returns primitive long directly instead of Data wrapper. Common properties: AccountLogin, AccountLeverage, AccountTradeAllowed, AccountTradeMode.

---

## AccountInfoStringAsync

Retrieves a string account property (e.g., Name, Server, Currency).

### Signature

```csharp
public Task<string> AccountInfoStringAsync(
    AccountInfoStringPropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `property` | `AccountInfoStringPropertyType` | The account property type to query. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<string>` - Unwrapped string value of the requested property (no .Value needed).

### Remarks

UNWRAPPED: Returns primitive string directly instead of Data wrapper. Common properties: AccountName, AccountServer, AccountCurrency, AccountCompany.

---

## GetBalanceAsync

Gets current account balance (total funds deposited minus withdrawals).

### Signature

```csharp
public Task<double> GetBalanceAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Account balance in account currency.

### Remarks

Convenience shortcut for AccountInfoDoubleAsync(AccountBalance).

---

## GetEquityAsync

Gets current account equity (balance + floating profit/loss).

### Signature

```csharp
public Task<double> GetEquityAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Account equity in account currency.

### Remarks

Convenience shortcut for AccountInfoDoubleAsync(AccountEquity). Equity = Balance + Profit from open positions.

---

## GetMarginAsync

Gets currently used margin for open positions.

### Signature

```csharp
public Task<double> GetMarginAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Used margin in account currency.

### Remarks

Convenience shortcut for AccountInfoDoubleAsync(AccountMargin).

---

## GetFreeMarginAsync

Gets current free margin available for opening new positions.

### Signature

```csharp
public Task<double> GetFreeMarginAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Free margin in account currency.

### Remarks

Convenience shortcut for AccountInfoDoubleAsync(AccountMarginFree). Free Margin = Equity - Used Margin.

---

## GetProfitAsync

Gets current floating profit/loss from all open positions.

### Signature

```csharp
public Task<double> GetProfitAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Total floating profit in account currency (can be negative for loss).

### Remarks

Convenience shortcut for AccountInfoDoubleAsync(AccountProfit).

---

## GetLoginAsync

Gets the account login/identification number.

### Signature

```csharp
public Task<long> GetLoginAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<long>` - Account login number as long.

### Remarks

Convenience shortcut for AccountInfoIntegerAsync(AccountLogin).

---

## GetLeverageAsync

Gets the account leverage ratio (e.g., 100 for 1:100, 500 for 1:500).

### Signature

```csharp
public Task<long> GetLeverageAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<long>` - Leverage as long (e.g., 100, 500, 1000).

### Remarks

Convenience shortcut for AccountInfoIntegerAsync(AccountLeverage).

---

## GetAccountNameAsync

Gets the account owner's name.

### Signature

```csharp
public Task<string> GetAccountNameAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<string>` - Account holder name as string.

### Remarks

Convenience shortcut for AccountInfoStringAsync(AccountName).

---

## GetServerNameAsync

Gets the MT5 server name this account is connected to.

### Signature

```csharp
public Task<string> GetServerNameAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<string>` - Server name as string (e.g., "FxPro-MT5 Demo").

### Remarks

Convenience shortcut for AccountInfoStringAsync(AccountServer).

---

## GetCurrencyAsync

Gets the account's base currency.

### Signature

```csharp
public Task<string> GetCurrencyAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<string>` - Currency code as string (e.g., "USD", "EUR", "GBP").

### Remarks

Convenience shortcut for AccountInfoStringAsync(AccountCurrency).

---

## SymbolsTotalAsync

Returns the total number of symbols available in the terminal.

### Signature

```csharp
public Task<int> SymbolsTotalAsync(
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `selectedOnly` | `bool` | If true, counts only symbols selected in Market Watch; if false, counts all symbols. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<int>` - Total symbol count as int.

### Remarks

UNWRAPPED: Returns primitive int directly (was Data.Total).

---

## SymbolExistAsync

Checks whether the specified symbol exists in the terminal's symbol list.

### Signature

```csharp
public Task<bool> SymbolExistAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to check (e.g., "EURUSD", "XAUUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<bool>` - True if symbol exists, false otherwise.

### Remarks

UNWRAPPED: Returns primitive bool directly (was Data.Exists). Note: Existence doesn't guarantee the symbol is synchronized - use IsSymbolAvailableAsync() for that.

---

## SymbolNameAsync

Gets the symbol name by its index position in the symbols list.

### Signature

```csharp
public Task<string> SymbolNameAsync(
    int index,
    bool selected,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `index` | `int` | Zero-based symbol index in the list. |
| `selected` | `bool` | If true, searches only among selected symbols in Market Watch; if false, searches all symbols. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<string>` - Symbol name at the specified index, or empty string if index is out of range.

### Remarks

UNWRAPPED: Returns primitive string directly (was Data.Name).

---

## SymbolSelectAsync

Selects or deselects a symbol in the Market Watch window.

### Signature

```csharp
public Task<bool> SymbolSelectAsync(
    string symbol,
    bool selected = true,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to modify (e.g., "EURUSD"). |
| `selected` | `bool` | True to add symbol to Market Watch, false to remove it (default: true). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<bool>` - True if operation succeeded, false otherwise.

### Remarks

UNWRAPPED: Returns primitive bool directly (was Data.Success). Symbols must be selected in Market Watch to receive real-time quotes and trade them.

---

## SymbolIsSynchronizedAsync

Checks whether the specified symbol is synchronized with the server (receiving live quotes).

### Signature

```csharp
public Task<bool> SymbolIsSynchronizedAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to verify (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<bool>` - True if symbol is synchronized and ready for trading, false otherwise.

### Remarks

UNWRAPPED: Returns primitive bool directly (was Data.Synchronized). A symbol must be both Exist and Synchronized to be tradeable. Use IsSymbolAvailableAsync() for combined check.

---

## SymbolInfoDoubleAsync

Retrieves a double-precision numeric property of the specified symbol.

### Signature

```csharp
public Task<double> SymbolInfoDoubleAsync(
    string symbol,
    SymbolInfoDoubleProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD", "XAUUSD"). |
| `property` | `SymbolInfoDoubleProperty` | Numeric property to query (e.g., SymbolBid, SymbolAsk, SymbolPoint, SymbolVolumeMin). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Unwrapped double value of the requested property.

### Remarks

UNWRAPPED: Returns primitive double directly (was Data.Value). Common properties: SymbolBid, SymbolAsk, SymbolPoint, SymbolVolumeMin, SymbolVolumeMax, SymbolVolumeStep.

---

## SymbolInfoIntegerAsync

Retrieves an integer property of the specified symbol.

### Signature

```csharp
public Task<long> SymbolInfoIntegerAsync(
    string symbol,
    SymbolInfoIntegerProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoIntegerProperty` | Integer property to query (e.g., SymbolDigits, SymbolSpread, SymbolTradeMode). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<long>` - Unwrapped long value of the requested property.

### Remarks

UNWRAPPED: Returns primitive long directly (was Data.Value). Common properties: SymbolDigits, SymbolSpread, SymbolTradeMode, SymbolStopsLevel.

---

## SymbolInfoStringAsync

Retrieves a string property of the specified symbol.

### Signature

```csharp
public Task<string> SymbolInfoStringAsync(
    string symbol,
    SymbolInfoStringProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoStringProperty` | String property to query (e.g., SymbolDescription, SymbolBaseCurrency, SymbolPath). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<string>` - Unwrapped string value of the requested property.

### Remarks

UNWRAPPED: Returns primitive string directly (was Data.Value). Common properties: SymbolDescription, SymbolBaseCurrency, SymbolProfitCurrency, SymbolPath.

---

## SymbolInfoMarginRateAsync

Retrieves margin rate information for the specified symbol and order type.

### Signature

```csharp
public Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
    string symbol,
    ENUM_ORDER_TYPE orderType,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD"). |
| `orderType` | `ENUM_ORDER_TYPE` | Order type for margin calculation (BUY or SELL). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<SymbolInfoMarginRateData>` - <see cref="SymbolInfoMarginRateData"/> containing initial and maintenance margin rates.

### Remarks

Thin wrapper - returns rich object. Used for calculating required margin for positions.

---

## GetBidAsync

Gets the current bid price for the specified symbol.

### Signature

```csharp
public Task<double> GetBidAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Current bid price as double.

### Remarks

Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolBid).

---

## GetAskAsync

Gets the current ask price for the specified symbol.

### Signature

```csharp
public Task<double> GetAskAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Current ask price as double.

### Remarks

Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolAsk).

---

## GetSpreadAsync

Gets the current spread in points for the specified symbol.

### Signature

```csharp
public Task<long> GetSpreadAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<long>` - Current spread in points as long.

### Remarks

Convenience shortcut for SymbolInfoIntegerAsync(symbol, SymbolSpread).

---

## GetVolumeMinAsync

Gets the minimum allowed lot size for the specified symbol.

### Signature

```csharp
public Task<double> GetVolumeMinAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Minimum volume in lots as double (e.g., 0.01).

### Remarks

Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolVolumeMin). Use this to validate order volumes before placing trades.

---

## GetVolumeMaxAsync

Gets the maximum allowed lot size for the specified symbol.

### Signature

```csharp
public Task<double> GetVolumeMaxAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Maximum volume in lots as double (e.g., 100.0).

### Remarks

Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolVolumeMax).

---

## GetVolumeStepAsync

Gets the volume step increment for the specified symbol.

### Signature

```csharp
public Task<double> GetVolumeStepAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Volume step in lots as double (e.g., 0.01 means volumes must be multiples of 0.01).

### Remarks

Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolVolumeStep). Use this to round volumes to valid increments (e.g., if step is 0.01, volume 0.015 is invalid).

---

## GetPointAsync

Gets the point size for the specified symbol (minimum price change).

### Signature

```csharp
public Task<double> GetPointAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<double>` - Point value as double (e.g., 0.00001 for EURUSD on 5-digit broker).

### Remarks

Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolPoint). Used for price calculations and converting points to price levels.

---

## IsSymbolAvailableAsync

Checks if a symbol exists in the terminal AND is synchronized with server (fully ready for trading).

### Signature

```csharp
public Task<bool> IsSymbolAvailableAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to check (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<bool>` - True if symbol exists and is synchronized (ready to trade), false otherwise.

### Remarks

NEW CONVENIENCE METHOD: Combines SymbolExistAsync() and SymbolIsSynchronizedAsync() checks. More efficient than calling both methods separately. Use this before attempting to trade a symbol.

---

## IsTradingAllowedAsync

Checks if trading operations are allowed for the current account.

### Signature

```csharp
public Task<bool> IsTradingAllowedAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<bool>` - True if trading is allowed, false if account is read-only or trading is disabled.

### Remarks

NEW CONVENIENCE METHOD: Checks AccountInfoIntegerAsync(AccountTradeAllowed). Returns false for investor/read-only accounts or when trading is globally disabled.

---

## SymbolInfoTickAsync

Gets the latest tick data for the specified symbol.

### Signature

```csharp
public Task<MrpcMqlTick> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<MrpcMqlTick>` - Last known tick containing bid, ask, and related fields.

---

## QuoteAsync

Gets current quote for symbol (convenience alias for SymbolInfoTickAsync).

### Signature

```csharp
public Task<MrpcMqlTick> QuoteAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<MrpcMqlTick>` - Last known tick containing bid, ask, and related fields.

---

## SymbolInfoSessionQuoteAsync

Gets trading session quote information for the specified symbol and day.

### Signature

```csharp
public Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    int sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD"). |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of the week for which session data is requested. |
| `sessionIndex` | `int` | Index of the session on that day. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<SymbolInfoSessionQuoteData>` - Session quote information including open and close times.

---

## SymbolInfoSessionTradeAsync

Gets trading session trade information for the specified symbol and day.

### Signature

```csharp
public Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    int sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Target symbol name (e.g., "EURUSD"). |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of the week for which trade session data is requested. |
| `sessionIndex` | `int` | Index of the session on that day. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<SymbolInfoSessionTradeData>` - Session trade information including open and close times.

---

## TickValueWithSizeAsync

Calculates tick value and size for one or multiple symbols.

### Signature

```csharp
public Task<TickValueWithSizeData> TickValueWithSizeAsync(
    IEnumerable<string> symbols,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbols` | `IEnumerable<string>` | Collection of symbol names (e.g., "EURUSD", "GBPUSD"). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<TickValueWithSizeData>` - Tick value and size data for each specified symbol.

---

## SymbolParamsManyAsync

Retrieves multiple symbol parameters in a single request.

### Signature

```csharp
public Task<SymbolParamsManyData> SymbolParamsManyAsync(
    SymbolParamsManyRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `SymbolParamsManyRequest` | Request object specifying symbols and parameters to fetch. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<SymbolParamsManyData>` - Combined data set containing requested symbol parameters.

---

## MarketBookAddAsync

Subscribes to market depth (order book) updates for the specified symbol.

### Signature

```csharp
public Task<MarketBookAddData> MarketBookAddAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to subscribe to (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<MarketBookAddData>` - Result indicating whether the subscription was successful.

---

## MarketBookReleaseAsync

Unsubscribes from market depth (order book) updates for the specified symbol.

### Signature

```csharp
public Task<MarketBookReleaseData> MarketBookReleaseAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to unsubscribe from (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<MarketBookReleaseData>` - Result indicating whether the unsubscription was successful.

---

## MarketBookGetAsync

Retrieves the current market depth (order book) for the specified symbol.

### Signature

```csharp
public Task<MarketBookGetData> MarketBookGetAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to query (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<MarketBookGetData>` - Current order book data containing bid and ask levels.

---

## OpenedOrdersAsync

Retrieves all currently opened orders with optional sorting.

### Signature

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sortMode` | `BMT5_ENUM_OPENED_ORDER_SORT_TYPE` | Sorting mode for the orders (default: by open time ascending). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OpenedOrdersData>` - Collection of opened orders with full details.

---

## OpenedOrdersTicketsAsync

Retrieves ticket numbers of all currently opened orders.

### Signature

```csharp
public Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Returns

`Task<OpenedOrdersTicketsData>` - List of ticket IDs for opened orders.

---

## OrderHistoryAsync

Retrieves historical orders within a specified date range.

### Signature

```csharp
public Task<OrdersHistoryData> OrderHistoryAsync(
    DateTime from,
    DateTime to,
    BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
    int pageNumber = 0,
    int itemsPerPage = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `from` | `DateTime` | Start date of the history interval. |
| `to` | `DateTime` | End date of the history interval. |
| `sortMode` | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` | Sorting mode for historical orders (default: by close time ascending). |
| `pageNumber` | `int` | Page number for paginated results (0 for all). |
| `itemsPerPage` | `int` | Number of items per page (0 for all). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrdersHistoryData>` - Historical orders data within the requested period.

---

## PositionsHistoryAsync

Retrieves historical positions with sorting, optional time filters, and paging.

### Signature

```csharp
public Task<PositionsHistoryData> PositionsHistoryAsync(
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType,
    DateTime? openFrom = null,
    DateTime? openTo = null,
    int page = 0,
    int size = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sortType` | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE` | Sorting mode for the positions history. |
| `openFrom` | `DateTime?` | Filter: include positions opened on/after this time (optional). |
| `openTo` | `DateTime?` | Filter: include positions opened before/at this time (optional). |
| `page` | `int` | Page index for paginated results (0-based). |
| `size` | `int` | Items per page (0 to return all). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<PositionsHistoryData>` - Positions history matching the specified criteria.

---

## PositionsTotalAsync

Returns the total number of currently open positions.

### Signature

```csharp
public Task<PositionsTotalData> PositionsTotalAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Returns

`Task<PositionsTotalData>` - Total count of open positions.

---

## OrderCalcMarginAsync

Estimates required margin for a prospective order.

### Signature

```csharp
public Task<OrderCalcMarginData> OrderCalcMarginAsync(
    OrderCalcMarginRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderCalcMarginRequest` | Order parameters (e.g., symbol, type, volume, price). |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderCalcMarginData>` - Calculated margin data for the given order parameters.

---

## OrderCheckAsync

Performs a pre-trade check to validate order parameters.

### Signature

```csharp
public Task<OrderCheckData> OrderCheckAsync(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderCheckRequest` | Order parameters to verify before sending. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderCheckData>` - Result containing validation status and possible trade errors.

---

## OrderSendAsync

Sends a trade order to the server for execution.

### Signature

```csharp
public Task<OrderSendData> OrderSendAsync(
    OrderSendRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderSendRequest` | Order details including symbol, type, volume, and price. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderSendData>` - Result containing execution outcome and order ticket.

---

## OrderModifyAsync

Modifies parameters of an existing pending order.

### Signature

```csharp
public Task<OrderModifyData> OrderModifyAsync(
    OrderModifyRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderModifyRequest` | New order parameters such as price, SL, or TP levels. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderModifyData>` - Result indicating success or failure of the modification.

---

## OrderCloseAsync

Closes an existing open order or position.

### Signature

```csharp
public Task<OrderCloseData> OrderCloseAsync(
    OrderCloseRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderCloseRequest` | Order close parameters including ticket, volume, and price. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderCloseData>` - Result containing close status and related trade data.

---

## BuyMarketAsync

Opens a market BUY order at the current ask price.

### Signature

```csharp
public Task<OrderSendData> BuyMarketAsync(
    string symbol,
    double volume,
    double? stopLoss = null,
    double? takeProfit = null,
    string comment = "",
    ulong magic = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots (e.g., 0.01, 0.1, 1.0). |
| `stopLoss` | `double?` | Optional stop loss price level. |
| `takeProfit` | `double?` | Optional take profit price level. |
| `comment` | `string` | Optional comment for the order (max 31 characters). |
| `magic` | `ulong` | Optional magic number for order identification by EA (default: 0). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<OrderSendData>` - <see cref="OrderSendData"/> containing order ticket, result code, and execution details.

### Remarks

NEW CONVENIENCE METHOD: Simplifies market order placement - no need to manually build OrderSendRequest. Executes immediately at current market price (ask for buy). Use for instant order execution.

---

## SellMarketAsync

Opens a market SELL order at the current bid price.

### Signature

```csharp
public Task<OrderSendData> SellMarketAsync(
    string symbol,
    double volume,
    double? stopLoss = null,
    double? takeProfit = null,
    string comment = "",
    ulong magic = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots (e.g., 0.01, 0.1, 1.0). |
| `stopLoss` | `double?` | Optional stop loss price level. |
| `takeProfit` | `double?` | Optional take profit price level. |
| `comment` | `string` | Optional comment for the order (max 31 characters). |
| `magic` | `ulong` | Optional magic number for order identification by EA (default: 0). |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<OrderSendData>` - <see cref="OrderSendData"/> containing order ticket, result code, and execution details.

### Remarks

NEW CONVENIENCE METHOD: Simplifies market order placement - no need to manually build OrderSendRequest. Executes immediately at current market price (bid for sell). Use for instant order execution.

---

## BuyLimitAsync

Places a pending BUY LIMIT order (buy below current price).

### Signature

```csharp
public Task<OrderSendData> BuyLimitAsync(
    string symbol,
    double volume,
    double price,
    double? stopLoss = null,
    double? takeProfit = null,
    string comment = "",
    ulong magic = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `price` | `double` | Entry price level - must be below current ask price. |
| `stopLoss` | `double?` | Optional stop loss price level. |
| `takeProfit` | `double?` | Optional take profit price level. |
| `comment` | `string` | Optional comment for the order. |
| `magic` | `ulong` | Optional magic number for order identification by EA. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<OrderSendData>` - <see cref="OrderSendData"/> containing order ticket and result code.

### Remarks

NEW CONVENIENCE METHOD: Places pending order that triggers when price drops to specified level. Use when expecting price to fall before going up. Order activates automatically when price reaches entry level.

---

## SellLimitAsync

Places a pending SELL LIMIT order (sell above current price).

### Signature

```csharp
public Task<OrderSendData> SellLimitAsync(
    string symbol,
    double volume,
    double price,
    double? stopLoss = null,
    double? takeProfit = null,
    string comment = "",
    ulong magic = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `price` | `double` | Entry price level - must be above current bid price. |
| `stopLoss` | `double?` | Optional stop loss price level. |
| `takeProfit` | `double?` | Optional take profit price level. |
| `comment` | `string` | Optional comment for the order. |
| `magic` | `ulong` | Optional magic number for order identification by EA. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<OrderSendData>` - <see cref="OrderSendData"/> containing order ticket and result code.

### Remarks

NEW CONVENIENCE METHOD: Places pending order that triggers when price rises to specified level. Use when expecting price to rise before going down. Order activates automatically when price reaches entry level.

---

## BuyStopAsync

Places a pending BUY STOP order (buy above current price on breakout).

### Signature

```csharp
public Task<OrderSendData> BuyStopAsync(
    string symbol,
    double volume,
    double price,
    double? stopLoss = null,
    double? takeProfit = null,
    string comment = "",
    ulong magic = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `price` | `double` | Entry price level - must be above current ask price. |
| `stopLoss` | `double?` | Optional stop loss price level. |
| `takeProfit` | `double?` | Optional take profit price level. |
| `comment` | `string` | Optional comment for the order. |
| `magic` | `ulong` | Optional magic number for order identification by EA. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<OrderSendData>` - <see cref="OrderSendData"/> containing order ticket and result code.

### Remarks

NEW CONVENIENCE METHOD: Places pending order that triggers when price breaks above specified level. Use for breakout strategies - buy when price exceeds resistance. Order activates automatically on breakout.

---

## SellStopAsync

Places a pending SELL STOP order (sell below current price on breakout).

### Signature

```csharp
public Task<OrderSendData> SellStopAsync(
    string symbol,
    double volume,
    double price,
    double? stopLoss = null,
    double? takeProfit = null,
    string comment = "",
    ulong magic = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `price` | `double` | Entry price level - must be below current bid price. |
| `stopLoss` | `double?` | Optional stop loss price level. |
| `takeProfit` | `double?` | Optional take profit price level. |
| `comment` | `string` | Optional comment for the order. |
| `magic` | `ulong` | Optional magic number for order identification by EA. |
| `deadline` | `DateTime?` | Optional RPC deadline. |
| `cancellationToken` | `CancellationToken` | Token to cancel the operation. |

### Returns

`Task<OrderSendData>` - <see cref="OrderSendData"/> containing order ticket and result code.

### Remarks

NEW CONVENIENCE METHOD: Places pending order that triggers when price breaks below specified level. Use for breakout strategies - sell when price breaks support. Order activates automatically on breakout.

---

## GetRecentOrdersAsync

Gets recent orders from history (last N days).

### Signature

```csharp
public Task<OrdersHistoryData> GetRecentOrdersAsync(
    int days = 7,
    int limit = 100,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Returns

`Task<OrdersHistoryData>`

---

## GetTodayOrdersAsync

Gets today's orders from history.

### Signature

```csharp
public Task<OrdersHistoryData> GetTodayOrdersAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Returns

`Task<OrdersHistoryData>`

---

## OnSymbolTickAsync

Streams live tick updates for the specified symbols.

### Signature

```csharp
public IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
    IEnumerable<string> symbols,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbols` | `IEnumerable<string>` | Collection of symbol names to subscribe to (e.g., "EURUSD", "GBPUSD"). |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`IAsyncEnumerable<OnSymbolTickData>` - Asynchronous stream of tick data for the subscribed symbols.

---

## OnTradeAsync

Streams real-time trade events such as order executions or closures.

### Signature

```csharp
public IAsyncEnumerable<OnTradeData> OnTradeAsync(CancellationToken cancellationToken = default)
```

### Returns

`IAsyncEnumerable<OnTradeData>` - Asynchronous stream of trade event data.

---

## OnPositionProfitAsync

Streams periodic snapshots of position profit/loss.

### Signature

```csharp
public IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
    int intervalMs,
    bool ignoreEmpty = true,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `intervalMs` | `int` | Polling interval in milliseconds between snapshots. |
| `ignoreEmpty` | `bool` | Skip emissions when there are no positions (true by default). |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`IAsyncEnumerable<OnPositionProfitData>` - Asynchronous stream of profit data per position.

---

## OnPositionsAndPendingOrdersTicketsAsync

Streams periodic snapshots of tickets for open positions and pending orders.

### Signature

```csharp
public IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
    int intervalMs,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `intervalMs` | `int` | Polling interval in milliseconds between snapshots. |
| `cancellationToken` | `CancellationToken` | Cancellation token. |

### Returns

`IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData>` - Asynchronous stream containing ticket lists for positions and pending orders.

---

## OnTradeTransactionAsync

Streams real-time trade transaction events from the terminal.

### Signature

```csharp
public IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync(CancellationToken cancellationToken = default)
```

### Returns

`IAsyncEnumerable<OnTradeTransactionData>` - Asynchronous stream of trade transaction data (order updates, deal executions, etc.).

---

