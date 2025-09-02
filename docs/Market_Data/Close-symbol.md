# Close by Symbol (`close-symbol`) 🎯

## What it Does

Closes **all open positions for one symbol** on the current MT5 account. Shows a preview unless confirmed with `--yes`. `--dry-run` prints the plan and exits without sending requests.

---
## Method Signatures

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task CloseOrderByTicketAsync(
    ulong ticket,
    string symbol,
    double volume,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                            |
| --------------- | ------ | -------- | ------------------------------------------------------ |
| `--profile, -p` | string | yes      | Which profile to use (from `profiles.json`).           |
| `--symbol, -s`  | string | no       | Target symbol (defaults to profile’s `DefaultSymbol`). |
| `--yes, -y`     | flag   | no       | Execute without interactive confirmation.              |
| `--dry-run`     | flag   | no       | Print intended actions and exit (no network calls).    |
| `--timeout-ms`  | int    | no       | Per-RPC timeout in milliseconds (default: `30000`).    |

Aliases: `cs`, `flatten-symbol`.

---

## Output ⬆️

**Text only.**

* No offers → "No positions to close by <SYMBOL>".
* Without "-yes" → Previous (first 10 seconds) + "Pass" - yes for execution.` (exit=2).
* `--intermediate run" → "[INTERMEDIATE RUN] Will close all positions by <CHARACTER>.`
* Execution → "Closed normally: X; Error: Y" (output=0/1).

---

## How to Use 🛠️

```powershell
# Preview
dotnet run -- close-symbol -p demo -s EURUSD

# Execute
dotnet run -- close-symbol -p demo -s EURUSD --yes

# Dry-run
dotnet run -- close-symbol -p demo -s EURUSD --dry-run
```

---

## Code Reference 🧩

```csharp
// Preview / dry-run
Console.WriteLine($"[DRY-RUN] Would close all positions for {symbol}.");

// Fetch open positions
var opened = await _mt5Account.OpenedOrdersAsync();

// Close by ticket
await _mt5Account.CloseOrderByTicketAsync(ticket, symbol, volume);
```


