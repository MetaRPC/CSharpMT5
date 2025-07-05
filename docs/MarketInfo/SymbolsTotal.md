## Market Info

### SymbolsTotalAsync

```csharp
// Getting the total number of all characters
var total = await _mt5Account.SymbolsTotalAsync(false);

// Getting the number of selected (MarketWatch) characters only
var selectedTotal = await _mt5Account.SymbolsTotalAsync(true);
```

✨**Method Signature:**
```csharp
 Task<SymbolsTotalData> SymbolsTotalAsync(bool selectedOnly, DateTime? deadline = null, CancellationToken cancellationToken = default)
```
* **Input:**
    * **selectedOnly (bool)**
    * **false** — return the total number of all available characters.
    * **true** — return the number of only those characters that are selected in Market Watch.

* **Output:**
    * **SymbolsTotalData** — container with property.
    * **Total (int)** – the number of symbols matching the filter.

**Purpose:** Simplify your code by using one method for both “all” and “selected” symbol counts. No duplicate overloads—just flip the boolean and you’re done! 🚀
