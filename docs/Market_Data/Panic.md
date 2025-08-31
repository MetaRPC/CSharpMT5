# Panic (`panic`) 🚨

## What it Does

Runs an **emergency flatten** routine: quickly brings the account to **flat exposure**.
Typical behavior: close all open positions (and optionally pending orders), then report a summary.

> This is intended for **oh‑no** moments: unexpected behavior, news spikes, or manual kill‑switch.

---

## Input Parameters ⬇️

| Parameter         | Type   |Description                                          |
| ----------------- | ------ |---------------------------------------------------- |
| `--profile`, `-p` | string | Which profile to use (from `profiles.json`).         |
| `--output`, `-o`  | string |`text` (default) or `json`.                          |
| `--timeout-ms`    | int    | RPC timeout in ms (default: 30000).                  |
| `--dry-run`       | flag   | Print intended actions but do **not** send requests. |
---

## Output Fields ⬆️

| Field     | Type  | Description                                                   |
| --------- | ----- | ------------------------------------------------------------- |
| `Closed`  | int   | Number of positions successfully closed.                      |
| `Errors`  | int   | Number of positions that failed to close.                     |
| `Items[]` | array | Per-ticket results (ticket, symbol, volume, status, message). |
---

## How to Use 🛠️

### CLI

```powershell
# Emergency flatten (no filters)
dotnet run -- panic -p demo

# Dry-run preview (JSON)
dotnet run -- panic -p demo --dry-run -o json --timeout-ms 60000
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
panic
```

---

## When to Use ❓

* **Emergency exit** — flatten exposure during spikes or incidents.
* **Guardrail** — manual kill switch for algos.
* **Operational reset** — clean up before redeploying a strategy.

---

## Notes & Safety 🛡️

* Market conditions may cause slippage; the routine should use reasonable defaults for `deviation`.
* If the market is closed or trading is disabled, items will be reported under `Errors`.
* `--dry-run` is safe and recommended for validation in tests/CI.

---

## Code Reference 🧩

```csharp
var panicSymbolOpt = new Option<string?>(new[] { "--symbol", "-s" }, "Limit to symbol (optional)");
var panicDevOpt    = new Option<int>(new[] { "--deviation" }, () => 10, "Max slippage for closes");

var panic = new Command("panic", "Close ALL positions and cancel ALL pendings (optionally by symbol)");
panic.AddOption(profileOpt);
panic.AddOption(panicSymbolOpt);
panic.AddOption(panicDevOpt);
panic.AddOption(timeoutOpt);
panic.AddOption(dryRunOpt);

panic.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbol    = ctx.ParseResult.GetValueForOption(panicSymbolOpt);
    var deviation = ctx.ParseResult.GetValueForOption(panicDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol!);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PANIC Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol} Dev:{Dev}", symbol ?? "<any>", deviation))
    {
        try
        {
            await ConnectAsync();
```
