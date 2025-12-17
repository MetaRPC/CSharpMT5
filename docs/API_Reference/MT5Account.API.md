Parsing MT5Account.cs...
Found 71 methods
Generated docs/API_Reference/MT5Account.API.md
---

## Table of Contents

1. [ConnectByHostPortAsync](#connectbyhostportasync)
2. [Connect](#connect)
3. [ConnectByServerNameAsync](#connectbyservernameasync)
4. [ConnectByServerName](#connectbyservername)
5. [AccountSummaryAsync](#accountsummaryasync)
6. [AccountSummary](#accountsummary)
7. [AccountInfoDoubleAsync](#accountinfodoubleasync)
8. [AccountInfoDouble](#accountinfodouble)
9. [AccountInfoIntegerAsync](#accountinfointegerasync)
10. [AccountInfoInteger](#accountinfointeger)
11. [AccountInfoStringAsync](#accountinfostringasync)
12. [AccountInfoString](#accountinfostring)
13. [SymbolsTotalAsync](#symbolstotalasync)
14. [SymbolsTotal](#symbolstotal)
15. [SymbolExistAsync](#symbolexistasync)
16. [SymbolExist](#symbolexist)
17. [SymbolNameAsync](#symbolnameasync)
18. [SymbolName](#symbolname)
19. [SymbolSelectAsync](#symbolselectasync)
20. [SymbolSelect](#symbolselect)
21. [SymbolIsSynchronizedAsync](#symbolissynchronizedasync)
22. [SymbolIsSynchronized](#symbolissynchronized)
23. [SymbolInfoDoubleAsync](#symbolinfodoubleasync)
24. [SymbolInfoDouble](#symbolinfodouble)
25. [SymbolInfoIntegerAsync](#symbolinfointegerasync)
26. [SymbolInfoInteger](#symbolinfointeger)
27. [SymbolInfoStringAsync](#symbolinfostringasync)
28. [SymbolInfoString](#symbolinfostring)
29. [SymbolInfoMarginRateAsync](#symbolinfomarginrateasync)
30. [SymbolInfoMarginRate](#symbolinfomarginrate)
31. [SymbolInfoTickAsync](#symbolinfotickasync)
32. [SymbolInfoTick](#symbolinfotick)
33. [SymbolInfoSessionQuoteAsync](#symbolinfosessionquoteasync)
34. [SymbolInfoSessionQuote](#symbolinfosessionquote)
35. [SymbolInfoSessionTradeAsync](#symbolinfosessiontradeasync)
36. [SymbolInfoSessionTrade](#symbolinfosessiontrade)
37. [OpenedOrdersAsync](#openedordersasync)
38. [OpenedOrders](#openedorders)
39. [OrderHistoryAsync](#orderhistoryasync)
40. [OrderHistory](#orderhistory)
41. [OpenedOrdersTicketsAsync](#openedordersticketsasync)
42. [OpenedOrdersTickets](#openedorderstickets)
43. [PositionsHistoryAsync](#positionshistoryasync)
44. [PositionsHistory](#positionshistory)
45. [PositionsTotalAsync](#positionstotalasync)
46. [PositionsTotal](#positionstotal)
47. [MarketBookAddAsync](#marketbookaddasync)
48. [MarketBookAdd](#marketbookadd)
49. [MarketBookGetAsync](#marketbookgetasync)
50. [MarketBookGet](#marketbookget)
51. [MarketBookReleaseAsync](#marketbookreleaseasync)
52. [MarketBookRelease](#marketbookrelease)
53. [TickValueWithSizeAsync](#tickvaluewithsizeasync)
54. [TickValueWithSize](#tickvaluewithsize)
55. [SymbolParamsManyAsync](#symbolparamsmanyasync)
56. [SymbolParamsMany](#symbolparamsmany)
57. [OrderSendAsync](#ordersendasync)
58. [OrderSend](#ordersend)
59. [OrderModifyAsync](#ordermodifyasync)
60. [OrderModify](#ordermodify)
61. [OrderCloseAsync](#ordercloseasync)
62. [OrderClose](#orderclose)
63. [OrderCalcMarginAsync](#ordercalcmarginasync)
64. [OrderCalcMargin](#ordercalcmargin)
65. [OrderCheckAsync](#ordercheckasync)
66. [OrderCheck](#ordercheck)
67. [OnSymbolTickAsync](#onsymboltickasync)
68. [OnTradeAsync](#ontradeasync)
69. [OnPositionProfitAsync](#onpositionprofitasync)
70. [OnPositionsAndPendingOrdersTicketsAsync](#onpositionsandpendingordersticketsasync)
71. [OnTradeTransactionAsync](#ontradetransactionasync)

---

## ConnectByHostPortAsync

Connects to the MT5 terminal using host and port asynchronously.

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
| `host` | `string` | The IP address or domain of the MT5 server. |
| `port` | `int` | The port on which the MT5 server listens (default is 443). |
| `baseChartSymbol` | `string` | The base chart symbol to use (e.g., "EURUSD"). |
| `waitForTerminalIsAlive` | `bool` | Whether to wait for terminal readiness before returning. |
| `timeoutSeconds` | `int` | How long to wait for terminal readiness before timing out. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task` - A task representing the asynchronous connection operation.

---

## Connect

Connects to the MT5 terminal using host and port synchronously.

### Signature

```csharp
public void Connect(
    string host,
    int port = 443,
    string baseChartSymbol = "EURUSD",
    bool waitForTerminalIsAlive = true,
    int timeoutSeconds = 30
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `host` | `string` | The IP address or domain of the MT5 server. |
| `port` | `int` | The port on which the MT5 server listens (default is 443). |
| `baseChartSymbol` | `string` | The base chart symbol to use (e.g., "EURUSD"). |
| `waitForTerminalIsAlive` | `bool` | Whether to wait for terminal readiness before returning. |
| `timeoutSeconds` | `int` | How long to wait for terminal readiness before timing out. |

---

## ConnectByServerNameAsync

Connects to the MT5 terminal using server name asynchronously.

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
| `serverName` | `string` | The MT5 server cluster name. |
| `baseChartSymbol` | `string` | The base chart symbol to use (e.g., "EURUSD"). |
| `waitForTerminalIsAlive` | `bool` | Whether to wait for terminal readiness before returning. |
| `timeoutSeconds` | `int` | How long to wait for terminal readiness before timing out. |
| `deadline` | `DateTime?` | Optional deadline for the gRPC call. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task` - A task representing the asynchronous connection operation.

---

## ConnectByServerName

Connects to the MT5 terminal using server name synchronously.

### Signature

```csharp
public void ConnectByServerName(
    string serverName,
    string baseChartSymbol = "EURUSD",
    bool waitForTerminalIsAlive = true,
    int timeoutSeconds = 30
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `serverName` | `string` | The MT5 server cluster name. |
| `baseChartSymbol` | `string` | The base chart symbol to use (e.g., "EURUSD"). |
| `waitForTerminalIsAlive` | `bool` | Whether to wait for terminal readiness before returning. |
| `timeoutSeconds` | `int` | How long to wait for terminal readiness before timing out. |

---

## AccountSummaryAsync

Gets the complete summary of the trading account in a single call. Returns all essential account information including balance, equity, margin, profit, leverage, and currency. This is the recommended method for retrieving account data as it minimizes network calls.

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
| `deadline` | `DateTime?` | Optional deadline after which the request will be canceled if not completed. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the request. |

### Returns

`Task<AccountSummaryData>` - The server's response containing account summary data.

---

## AccountSummary

Gets the complete summary of the trading account synchronously.

### Signature

```csharp
public AccountSummaryData AccountSummary(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional deadline after which the request will be canceled if not completed. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the request. |

### Returns

`AccountSummaryData` - The server's response containing account summary data.

---

## AccountInfoDoubleAsync

Retrieves a specific double-precision property of the trading account. Use this to get individual numeric values such as BALANCE, EQUITY, MARGIN, PROFIT, etc.

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
| `property` | `AccountInfoDoublePropertyType` | The account double property to retrieve. |
| `deadline` | `DateTime?` | Optional deadline after which the call will be cancelled. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the operation. |

### Returns

`Task<double>` - The double value of the requested account property.

---

## AccountInfoDouble

Retrieves a specific double-precision property of the trading account synchronously.

### Signature

```csharp
public double AccountInfoDouble(
    AccountInfoDoublePropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `property` | `AccountInfoDoublePropertyType` | The account double property to retrieve. |
| `deadline` | `DateTime?` | Optional deadline after which the call will be cancelled. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the operation. |

### Returns

`double` - The double value of the requested account property.

---

## AccountInfoIntegerAsync

Retrieves a specific integer property of the trading account. Use this to get values such as LOGIN, LEVERAGE, TRADE_MODE, LIMIT_ORDERS, etc.

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
| `property` | `AccountInfoIntegerPropertyType` | The account integer property to retrieve. |
| `deadline` | `DateTime?` | Optional deadline after which the call will be cancelled. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the operation. |

### Returns

`Task<long>` - The integer value of the requested account property.

---

## AccountInfoInteger

Retrieves a specific integer property of the trading account synchronously.

### Signature

```csharp
public long AccountInfoInteger(
    AccountInfoIntegerPropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `property` | `AccountInfoIntegerPropertyType` | The account integer property to retrieve. |
| `deadline` | `DateTime?` | Optional deadline after which the call will be cancelled. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the operation. |

### Returns

`long` - The integer value of the requested account property.

---

## AccountInfoStringAsync

Retrieves a specific string property of the trading account. Use this to get textual information such as account NAME, SERVER, CURRENCY, or COMPANY.

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
| `property` | `AccountInfoStringPropertyType` | The account string property to retrieve. |
| `deadline` | `DateTime?` | Optional deadline after which the call will be cancelled. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the operation. |

### Returns

`Task<string>` - The string value of the requested account property.

---

## AccountInfoString

Retrieves a specific string property of the trading account synchronously.

### Signature

```csharp
public string AccountInfoString(
    AccountInfoStringPropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `property` | `AccountInfoStringPropertyType` | The account string property to retrieve. |
| `deadline` | `DateTime?` | Optional deadline after which the call will be cancelled. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the operation. |

### Returns

`string` - The string value of the requested account property.

---

## SymbolsTotalAsync

Gets the total count of available symbols on the MT5 server. Returns either all symbols known to the server or only those currently shown in the MarketWatch window. Use this to determine how many symbols are available before requesting detailed symbol information.

### Signature

```csharp
public Task<SymbolsTotalData> SymbolsTotalAsync(
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `selectedOnly` | `bool` | If true, returns only symbols visible in MarketWatch; if false, returns all available symbols. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolsTotalData>` - Total number of symbols matching the filter criteria.

---

## SymbolsTotal

Gets the total count of available symbols on the MT5 server synchronously.

### Signature

```csharp
public SymbolsTotalData SymbolsTotal(
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `selectedOnly` | `bool` | If true, returns only symbols visible in MarketWatch; if false, returns all available symbols. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolsTotalData` - Total number of symbols matching the filter criteria.

---

## SymbolExistAsync

Checks if a symbol exists on the MT5 server. Returns whether the specified symbol name is available for trading.

### Signature

```csharp
public Task<SymbolExistData> SymbolExistAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to check (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolExistData>` - Reply with existence status.

---

## SymbolExist

Checks if a symbol exists on the MT5 server synchronously.

### Signature

```csharp
public SymbolExistData SymbolExist(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name to check (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolExistData` - Reply with existence status.

---

## SymbolNameAsync

Gets the symbol name by its position in the symbols list. Returns the name of the symbol at the specified index.

### Signature

```csharp
public Task<SymbolNameData> SymbolNameAsync(
    int index,
    bool selected,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `index` | `int` | Position in the symbols list (0-based index). |
| `selected` | `bool` | If true, searches only in Market Watch; if false, searches all symbols. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolNameData>` - Reply with symbol name.

---

## SymbolName

Gets the symbol name by its position in the symbols list synchronously.

### Signature

```csharp
public SymbolNameData SymbolName(
    int index,
    bool selected,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `index` | `int` | Position in the symbols list (0-based index). |
| `selected` | `bool` | If true, searches only in Market Watch; if false, searches all symbols. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolNameData` - Reply with symbol name.

---

## SymbolSelectAsync

Selects or deselects a symbol in the Market Watch window. Symbols must be selected in Market Watch to receive price updates and place trades.

### Signature

```csharp
public Task<SymbolSelectData> SymbolSelectAsync(
    string symbol,
    bool select,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `select` | `bool` | True to select symbol, false to remove from Market Watch. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolSelectData>` - Reply with success status.

---

## SymbolSelect

Selects or deselects a symbol in the Market Watch window synchronously.

### Signature

```csharp
public SymbolSelectData SymbolSelect(
    string symbol,
    bool select,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `select` | `bool` | True to select symbol, false to remove from Market Watch. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolSelectData` - Reply with success status.

---

## SymbolIsSynchronizedAsync

Checks if symbol data is synchronized with the trade server. Returns whether the symbol's price and market data is currently up to date.

### Signature

```csharp
public Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolIsSynchronizedData>` - Reply with synchronization status.

---

## SymbolIsSynchronized

Checks if symbol data is synchronized with the trade server synchronously.

### Signature

```csharp
public SymbolIsSynchronizedData SymbolIsSynchronized(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolIsSynchronizedData` - Reply with synchronization status.

---

## SymbolInfoDoubleAsync

Gets a double property value for a specified symbol. Used to retrieve numeric properties like point size, spread, volume limits, etc.

### Signature

```csharp
public Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(
    string symbol,
    SymbolInfoDoubleProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoDoubleProperty` | Property type (SYMBOL_POINT, SYMBOL_VOLUME_MIN, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolInfoDoubleData>` - Reply with the property value.

---

## SymbolInfoDouble

Gets a double property value for a specified symbol synchronously.

### Signature

```csharp
public SymbolInfoDoubleData SymbolInfoDouble(
    string symbol,
    SymbolInfoDoubleProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoDoubleProperty` | Property type (SYMBOL_POINT, SYMBOL_VOLUME_MIN, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolInfoDoubleData` - Reply with the property value.

---

## SymbolInfoIntegerAsync

Gets an integer property value for a specified symbol. Used to retrieve integer properties like digits, spread in points, trade mode, etc.

### Signature

```csharp
public Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
    string symbol,
    SymbolInfoIntegerProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoIntegerProperty` | Property type (SYMBOL_DIGITS, SYMBOL_SPREAD, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolInfoIntegerData>` - Reply with the property value.

---

## SymbolInfoInteger

Gets an integer property value for a specified symbol synchronously.

### Signature

```csharp
public SymbolInfoIntegerData SymbolInfoInteger(
    string symbol,
    SymbolInfoIntegerProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoIntegerProperty` | Property type (SYMBOL_DIGITS, SYMBOL_SPREAD, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolInfoIntegerData` - Reply with the property value.

---

## SymbolInfoStringAsync

Gets a string property value for a specified symbol. Used to retrieve string properties like base currency, description, path, etc.

### Signature

```csharp
public Task<SymbolInfoStringData> SymbolInfoStringAsync(
    string symbol,
    SymbolInfoStringProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoStringProperty` | Property type (SYMBOL_CURRENCY_BASE, SYMBOL_DESCRIPTION, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolInfoStringData>` - Reply with the property value.

---

## SymbolInfoString

Gets a string property value for a specified symbol synchronously.

### Signature

```csharp
public SymbolInfoStringData SymbolInfoString(
    string symbol,
    SymbolInfoStringProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `property` | `SymbolInfoStringProperty` | Property type (SYMBOL_CURRENCY_BASE, SYMBOL_DESCRIPTION, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolInfoStringData` - Reply with the property value.

---

## SymbolInfoMarginRateAsync

Gets the margin rate required for opening positions on a specified symbol. Returns the margin multiplier applied for buy and sell orders, which varies based on order type. Use this to calculate the exact margin requirement before placing orders.

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
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `orderType` | `ENUM_ORDER_TYPE` | Type of order (BUY, SELL, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolInfoMarginRateData>` - Margin rate information including initial and maintenance margin multipliers.

---

## SymbolInfoMarginRate

Gets the margin rate required for opening positions on a specified symbol synchronously.

### Signature

```csharp
public SymbolInfoMarginRateData SymbolInfoMarginRate(
    string symbol,
    ENUM_ORDER_TYPE orderType,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `orderType` | `ENUM_ORDER_TYPE` | Type of order (BUY, SELL, etc.). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolInfoMarginRateData` - Margin rate information including initial and maintenance margin multipliers.

---

## SymbolInfoTickAsync

Gets the latest tick data for a specified symbol. Returns real-time market information including current bid/ask prices, last trade price, volume, and timestamp. This is the primary method for retrieving current market prices for trading decisions.

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
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "GBPUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<MrpcMqlTick>` - Latest tick data with bid/ask prices, last price, volume, and time.

---

## SymbolInfoTick

Gets the latest tick data for a specified symbol synchronously.

### Signature

```csharp
public MrpcMqlTick SymbolInfoTick(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "GBPUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`MrpcMqlTick` - Latest tick data with bid/ask prices, last price, volume, and time.

---

## SymbolInfoSessionQuoteAsync

Gets the quote (pricing) session schedule for a symbol on a specific day. Returns the start and end times when price quotes are available for the symbol. Use this to determine when you can expect to receive price updates for market data.

### Signature

```csharp
public Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6). |
| `sessionIndex` | `uint` | Session index (0 for first session, 1 for second session if multiple sessions exist). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolInfoSessionQuoteData>` - Quote session times including start and end times in seconds since midnight.

---

## SymbolInfoSessionQuote

Gets the quote (pricing) session schedule for a symbol on a specific day synchronously.

### Signature

```csharp
public SymbolInfoSessionQuoteData SymbolInfoSessionQuote(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6). |
| `sessionIndex` | `uint` | Session index (0 for first session, 1 for second session if multiple sessions exist). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolInfoSessionQuoteData` - Quote session times including start and end times in seconds since midnight.

---

## SymbolInfoSessionTradeAsync

Gets the trading session schedule for a symbol on a specific day. Returns the start and end times when trading operations are allowed for the symbol. Use this to determine when you can open/close positions and place orders.

### Signature

```csharp
public Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6). |
| `sessionIndex` | `uint` | Session index (0 for first session, 1 for second session if multiple sessions exist). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolInfoSessionTradeData>` - Trade session times including start and end times in seconds since midnight.

---

## SymbolInfoSessionTrade

Gets the trading session schedule for a symbol on a specific day synchronously.

### Signature

```csharp
public SymbolInfoSessionTradeData SymbolInfoSessionTrade(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6). |
| `sessionIndex` | `uint` | Session index (0 for first session, 1 for second session if multiple sessions exist). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolInfoSessionTradeData` - Trade session times including start and end times in seconds since midnight.

---

## OpenedOrdersAsync

Gets the currently opened orders and positions for the connected account asynchronously.

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
| `sortMode` | `BMT5_ENUM_OPENED_ORDER_SORT_TYPE` | The sort mode for the opened orders. |
| `deadline` | `DateTime?` | Optional deadline after which the request will be canceled if not completed. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the request. |

### Returns

`Task<OpenedOrdersData>` - A task representing the asynchronous operation. The result contains opened orders and positions.

---

## OpenedOrders

Gets the currently opened orders and positions for the connected account synchronously.

### Signature

```csharp
public OpenedOrdersData OpenedOrders(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `sortMode` | `BMT5_ENUM_OPENED_ORDER_SORT_TYPE` | The sort mode for the opened orders. |
| `deadline` | `DateTime?` | Optional deadline after which the request will be canceled if not completed. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the request. |

### Returns

`OpenedOrdersData` - The server's response containing opened orders and positions.

---

## OrderHistoryAsync

Gets the historical orders for the connected trading account within the specified time range asynchronously.

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
| `from` | `DateTime` | The start time for the history query (server time). |
| `to` | `DateTime` | The end time for the history query (server time). |
| `sortMode` | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` | The sort mode: by open time, close time, or ticket ID. |
| `pageNumber` | `int` | The page number for paginated results (default 0). |
| `itemsPerPage` | `int` | The number of items per page (default 0 = all). |
| `deadline` | `DateTime?` | Optional deadline after which the request will be canceled if not completed. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the request. |

### Returns

`Task<OrdersHistoryData>` - A task representing the asynchronous operation. The result contains paged historical order data.

---

## OrderHistory

Gets the historical orders for the connected trading account within the specified time range synchronously.

### Signature

```csharp
public OrdersHistoryData OrderHistory(
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
| `from` | `DateTime` | The start time for the history query (server time). |
| `to` | `DateTime` | The end time for the history query (server time). |
| `sortMode` | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` | The sort mode: by open time, close time, or ticket ID. |
| `pageNumber` | `int` | The page number for paginated results (default 0). |
| `itemsPerPage` | `int` | The number of items per page (default 0 = all). |
| `deadline` | `DateTime?` | Optional deadline after which the request will be canceled if not completed. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token to cancel the request. |

### Returns

`OrdersHistoryData` - The server's response containing paged historical order data.

---

## OpenedOrdersTicketsAsync

Gets ticket IDs of all currently opened orders and positions asynchronously.

### Signature

```csharp
public Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<OpenedOrdersTicketsData>` - Task containing collection of opened order and position tickets.

---

## OpenedOrdersTickets

Gets ticket IDs of all currently opened orders and positions synchronously.

### Signature

```csharp
public OpenedOrdersTicketsData OpenedOrdersTickets(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`OpenedOrdersTicketsData` - Collection of opened order and position tickets.

---

## PositionsHistoryAsync

Retrieves historical positions based on filter and time range asynchronously.

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
| `sortType` | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE` | Sorting type for historical positions. |
| `openFrom` | `DateTime?` | Optional start of open time filter (UTC). |
| `openTo` | `DateTime?` | Optional end of open time filter (UTC). |
| `page` | `int` | Optional page number. |
| `size` | `int` | Optional items per page. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<PositionsHistoryData>` - Task containing historical position records.

---

## PositionsHistory

Retrieves historical positions based on filter and time range synchronously.

### Signature

```csharp
public PositionsHistoryData PositionsHistory(
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
| `sortType` | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE` | Sorting type for historical positions. |
| `openFrom` | `DateTime?` | Optional start of open time filter (UTC). |
| `openTo` | `DateTime?` | Optional end of open time filter (UTC). |
| `page` | `int` | Optional page number. |
| `size` | `int` | Optional items per page. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`PositionsHistoryData` - Historical position records.

---

## PositionsTotalAsync

Gets the total count of currently open positions on the account. Returns a simple count of all active positions regardless of symbol. Use this for quick checks of position count before retrieving detailed position information.

### Signature

```csharp
public Task<PositionsTotalData> PositionsTotalAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<PositionsTotalData>` - Total number of open positions.

---

## PositionsTotal

Gets the total count of currently open positions on the account synchronously.

### Signature

```csharp
public PositionsTotalData PositionsTotal(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`PositionsTotalData` - Total number of open positions.

---

## MarketBookAddAsync

Subscribes to Market Depth (DOM/Level II) updates for a specified symbol. After subscription, you can retrieve current order book data showing pending buy and sell orders. Use this to access liquidity information and see the market depth before placing large orders.

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
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<MarketBookAddData>` - Subscription confirmation response.

---

## MarketBookAdd

Subscribes to Market Depth (DOM/Level II) updates for a specified symbol synchronously.

### Signature

```csharp
public MarketBookAddData MarketBookAdd(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`MarketBookAddData` - Subscription confirmation response.

---

## MarketBookGetAsync

Gets the current Market Depth (order book) data for a subscribed symbol. Returns pending buy and sell orders with prices and volumes from the order book. Use this to analyze liquidity, identify support/resistance levels, or optimize order placement.

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
| `symbol` | `string` | Symbol name (must be subscribed via MarketBookAdd first). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<MarketBookGetData>` - Market book data containing arrays of buy and sell orders with prices and volumes.

---

## MarketBookGet

Gets the current Market Depth (order book) data for a subscribed symbol synchronously.

### Signature

```csharp
public MarketBookGetData MarketBookGet(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (must be subscribed via MarketBookAdd first). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`MarketBookGetData` - Market book data containing arrays of buy and sell orders with prices and volumes.

---

## MarketBookReleaseAsync

Unsubscribes from Market Depth updates for a specified symbol. Stops receiving order book data and releases associated resources. Use this when you no longer need DOM data for a symbol to free up resources.

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
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<MarketBookReleaseData>` - Unsubscription confirmation response.

---

## MarketBookRelease

Unsubscribes from Market Depth updates for a specified symbol synchronously.

### Signature

```csharp
public MarketBookReleaseData MarketBookRelease(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`MarketBookReleaseData` - Unsubscription confirmation response.

---

## TickValueWithSizeAsync

Gets tick value and tick size data for the given symbols asynchronously. Returns tick values, contract size, and tick size for trading calculations.

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
| `symbols` | `IEnumerable<string>` | List of symbol names. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<TickValueWithSizeData>` - Task containing tick value and contract size info per symbol.

---

## TickValueWithSize

Gets tick value and tick size data for the given symbols synchronously.

### Signature

```csharp
public TickValueWithSizeData TickValueWithSize(
    IEnumerable<string> symbols,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbols` | `IEnumerable<string>` | List of symbol names. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`TickValueWithSizeData` - Tick value and contract size info per symbol.

---

## SymbolParamsManyAsync

Retrieves symbol parameters for multiple instruments asynchronously. Gets comprehensive parameter details for one or more symbols with pagination support. Returns extensive symbol information including contract specifications, trading conditions, and current state.

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
| `request` | `SymbolParamsManyRequest` | The request containing filters and pagination. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<SymbolParamsManyData>` - Task containing symbol parameter details.

---

## SymbolParamsMany

Retrieves symbol parameters for multiple instruments synchronously.

### Signature

```csharp
public SymbolParamsManyData SymbolParamsMany(
    SymbolParamsManyRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `SymbolParamsManyRequest` | The request containing filters and pagination. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`SymbolParamsManyData` - Symbol parameter details.

---

## OrderSendAsync

Sends a trading order to the MT5 server (market or pending order). Use this method to open new positions or place pending orders with specified parameters including symbol, volume, price, stop loss, and take profit levels.

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
| `request` | `OrderSendRequest` | The order request to send. |
| `deadline` | `DateTime?` | Optional deadline for the operation. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<OrderSendData>` - Task containing response with deal/order confirmation data.

---

## OrderSend

Sends a trading order to the MT5 server synchronously.

### Signature

```csharp
public OrderSendData OrderSend(
    OrderSendRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderSendRequest` | The order request to send. |
| `deadline` | `DateTime?` | Optional deadline for the operation. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`OrderSendData` - Response containing deal/order confirmation data.

---

## OrderModifyAsync

Modifies an existing order or position parameters. Use this to update stop loss, take profit, price, or other parameters of an open position or pending order.

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
| `request` | `OrderModifyRequest` | The modification request (SL, TP, price, expiration, etc.). |
| `deadline` | `DateTime?` | Optional deadline for the operation. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<OrderModifyData>` - Task containing updated order/deal info.

---

## OrderModify

Modifies an existing order or position synchronously.

### Signature

```csharp
public OrderModifyData OrderModify(
    OrderModifyRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderModifyRequest` | The modification request (SL, TP, price, expiration, etc.). |
| `deadline` | `DateTime?` | Optional deadline for the operation. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`OrderModifyData` - Response containing updated order/deal info.

---

## OrderCloseAsync

Closes an open position or deletes a pending order. For positions, you can specify partial closure by providing a volume less than the total position size.

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
| `request` | `OrderCloseRequest` | The close request including ticket, volume, and slippage. |
| `deadline` | `DateTime?` | Optional deadline for the operation. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<OrderCloseData>` - Task containing the close result and return codes.

---

## OrderClose

Closes an open position or deletes a pending order synchronously.

### Signature

```csharp
public OrderCloseData OrderClose(
    OrderCloseRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderCloseRequest` | The close request including ticket, volume, and slippage. |
| `deadline` | `DateTime?` | Optional deadline for the operation. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`OrderCloseData` - Response describing the close result and return codes.

---

## OrderCalcMarginAsync

Calculates the margin required to open a position with specified parameters. Returns the amount of funds needed in account currency to maintain the position. Use this before placing orders to verify sufficient margin and avoid margin call risks.

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
| `request` | `OrderCalcMarginRequest` | The request containing symbol, order type, volume, and price. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<OrderCalcMarginData>` - The required margin in account currency.

---

## OrderCalcMargin

Calculates the margin required for a planned trade operation synchronously.

### Signature

```csharp
public OrderCalcMarginData OrderCalcMargin(
    OrderCalcMarginRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderCalcMarginRequest` | The request containing symbol, order type, volume, and price. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`OrderCalcMarginData` - The required margin in account currency.

---

## OrderCheckAsync

Validates a trading request and checks if there are sufficient funds to execute it. Returns detailed calculations including margin requirements, expected profit, and resulting balance. Use this to verify order validity before sending, preventing rejected orders due to insufficient funds.

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
| `request` | `OrderCheckRequest` | The order check request. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`Task<OrderCheckData>` - Validation result with margin, profit, balance calculations, and any error codes.

---

## OrderCheck

Validates a trading request and checks if there are sufficient funds to execute it synchronously.

### Signature

```csharp
public OrderCheckData OrderCheck(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `request` | `OrderCheckRequest` | The order check request. |
| `deadline` | `DateTime?` | Optional gRPC deadline. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`OrderCheckData` - Validation result with margin, profit, balance calculations, and any error codes.

---

## OnSymbolTickAsync

Subscribes to real-time tick updates for one or more symbols. Receives a continuous stream of price updates (bid, ask, last, volume) whenever prices change. Use this for real-time price monitoring, tick-based trading strategies, or market data feeds.

### Signature

```csharp
public IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
    IEnumerable<string> symbols,
    [EnumeratorCancellation] CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `symbols` | `IEnumerable<string>` | The symbol names to subscribe to. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`IAsyncEnumerable<OnSymbolTickData>` - Async stream of tick data responses.

---

## OnTradeAsync

Subscribes to trade events whenever a trading operation occurs. Receives notifications when orders are opened, closed, modified, or deleted. Use this to track all trading activity in real-time and react to order execution events.

### Signature

```csharp
public IAsyncEnumerable<OnTradeData> OnTradeAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`IAsyncEnumerable<OnTradeData>` - Async stream of trade event data.

---

## OnPositionProfitAsync

Subscribes to periodic updates of position profit/loss values. Receives profit updates at regular intervals for all open positions. Use this to monitor unrealized PnL in real-time and implement profit-based exit strategies.

### Signature

```csharp
public IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
    int intervalMs,
    bool ignoreEmpty = true,
    [EnumeratorCancellation] CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `intervalMs` | `int` | Interval in milliseconds to poll server. |
| `ignoreEmpty` | `bool` | Skip frames with no change. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`IAsyncEnumerable<OnPositionProfitData>` - Async stream of profit updates.

---

## OnPositionsAndPendingOrdersTicketsAsync

Subscribes to periodic updates of position and pending order ticket numbers. Receives lists of currently open position tickets and pending order tickets at regular intervals. Use this to efficiently track which positions/orders exist without retrieving full details.

### Signature

```csharp
public IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
    int intervalMs,
    [EnumeratorCancellation] CancellationToken cancellationToken = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `intervalMs` | `int` | Polling interval in milliseconds. |
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData>` - Async stream of ticket ID snapshots.

---

## OnTradeTransactionAsync

Subscribes to detailed trade transaction events. Receives comprehensive information about every trade operation including request, result, and execution details. Use this for detailed trade auditing, debugging order execution, or building advanced trading analytics.

### Signature

```csharp
public IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `cancellationToken` | `CancellationToken` | Optional cancellation token. |

### Returns

`IAsyncEnumerable<OnTradeTransactionData>` - Async stream of trade transaction replies.

---

