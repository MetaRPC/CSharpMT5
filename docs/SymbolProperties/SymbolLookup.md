# Symbol Lookup

## SymbolExistAsync
>Checks for the presence of the symbol

```csharp
var exists = await _mt5Account.SymbolExistAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolExistAsync: Exists={Exists}",
    exists.Exists);
```
✨**Method Signature:**
```csharp
Task<SymbolExistData> SymbolExistAsync(string symbol, …)
```

* **Input:**
  * **symbol (string)** – the name of the symbol, for example `EURUSD`.
* **Output:**
   * **SymbolExistData.Exists (bool)** – `true`/`false`.


## SymbolNameAsync

>Gets the symbol name by the index

```csharp
var symbolName = await _mt5Account.SymbolNameAsync(0, false);
_logger.LogInformation(
    "SymbolNameAsync: FirstSymbol={FirstSymbol}",
    symbolName.Name);
```

✨**Method Signature:**
```csharp
Task<SymbolNameData> SymbolNameAsync(int index, bool selectedOnly, …)
```

* **Input:** 
  * `index (int)` – positions in the list (0-based).

  * `selectedOnly (bool)` – true for Market Watch, false for all.

* **Output:**

  * `SymbolNameData.Name (string)`.

**Purpose** – Provide quick symbol‐lookup utilities (existence check, name by index) in one place.🚀

