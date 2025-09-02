# Close (`close`) 🔒

## What it Does

Closes an **open position** or **order** in MT5 by ticket ID. Supports full or partial volume and `--dry-run` preview.

---
## Method Signatures

```csharp
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    CancellationToken cancellationToken = default);

public Task CloseOrderByTicketAsync(
    ulong ticket,
    string symbol,
    double volume,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                      |
| --------------- | ------ | -------- | ------------------------------------------------ |
| `--profile, -p` | string | yes      | Which profile to use (from `profiles.json`).     |
| `--ticket, -t`  | ulong  | yes      | Ticket ID of the order/position to close.        |
| `--symbol, -s`  | string | no       | Symbol (defaults to profile’s `DefaultSymbol`).  |
| `--volume, -v`  | double | yes      | Volume to close (lots).                          |
| `--timeout-ms`  | int    | no       | Per-RPC timeout in ms (default: 30000).          |
| `--dry-run`     | flag   | no       | Print what would happen, but don’t send request. |

Alias: `c`.

---

## Output ⬆️

**Text only.**

* `--dry-run`: `[DRY-RUN] CLOSE #<ticket> <symbol> vol=<volume>`
* Successful closing: `CLOSE done: ticket=<ticket>`
* Errors are printed via the `ErrorPrinter`, exit code = 1.

---

## How to Use 🛠️

```powershell
# Close position completely
dotnet run -- close -p demo -t 123456 -s EURUSD -v 0.10 --yes

# Dry-run preview
dotnet run -- close -p demo -t 123456 -s EURUSD -v 0.10 --dry-run
```

---

## Code Reference 🧩

```csharp
// Dry-run
Console.WriteLine($"[DRY-RUN] CLOSE #{ticket} {symbol} vol={volume}");

// Ensure symbol visible
await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3), ct);

// Close by ticket
await _mt5Account.CloseOrderByTicketAsync(ticket, symbol, volume, deadline: null, cancellationToken: ct);
```
