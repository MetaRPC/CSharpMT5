# Pending Modify (`pending.modify`) ✏️

## What it Does

Modifies an existing **pending order** (Limit / Stop / Stop‑Limit): entry/trigger/limit price, SL/TP, and TIF/expiration.

> A similar command: **`pending.move`** (`pmove') — shifts prices **by ±N points**. The exact setting of the values is shown here.

---
## Method Signature

```csharp
public Task<bool> ModifyPendingOrderAsync(
    ulong ticket,
    string? type,               // "buylimit"|"selllimit"|"buystop"|"sellstop"|"buystoplimit"|"sellstoplimit"|null
    double? price,              // for limit/stop
    double? stop,               // for stop/stop-limit (trigger)
    double? limit,              // for stop-limit (limit price)
    double? sl,
    double? tp,
    string? tif,                // "GTC"|"DAY"|"GTD"|null
    DateTimeOffset? expire,
    CancellationToken ct);

## Input Parameters ⬇️

| Parameter       | Type           | Required | Description                                                                 |           |         |          |              |                                              |
| --------------- | -------------- | -------- | --------------------------------------------------------------------------- | --------- | ------- | -------- | ------------ | -------------------------------------------- |
| `--profile, -p` | string         | yes      | Which profile to use (from `profiles.json`).                                |           |         |          |              |                                              |
| `--ticket, -t`  | ulong          | yes      | Pending order ticket.                                                       |           |         |          |              |                                              |
| `--type`        | string         | no       | \`buylimit                                                                  | selllimit | buystop | sellstop | buystoplimit | sellstoplimit\` (для валидации инвариантов). |
| `--price`       | double         | no       | Новый **entry price** (для Limit/Stop).                                     |           |         |          |              |                                              |
| `--stop`        | double         | no       | Новый **trigger price** (для Stop/Stop‑Limit).                              |           |         |          |              |                                              |
| `--limit`       | double         | no       | Новый **limit price** (для Stop‑Limit).                                     |           |         |          |              |                                              |
| `--sl`          | double         | no       | New Stop Loss (absolute).                                                   |           |         |          |              |                                              |
| `--tp`          | double         | no       | New Take Profit (absolute).                                                 |           |         |          |              |                                              |
| `--tif`         | string         | no       | \`GTC                                                                       | DAY       | GTD\`.  |          |              |                                              |
| `--expire`      | DateTimeOffset | no       | ISO‑8601, **используется только** при `--tif=GTD` (Specified/SpecifiedDay). |           |         |          |              |                                              |
| `--symbol, -s`  | string         | no       | Для best‑effort `ensure-visible` (необязательный).                          |           |         |          |              |                                              |
| `--timeout-ms`  | int            | no       | Per‑RPC timeout (default `30000`).                                          |           |         |          |              |                                              |
| `--dry-run`     | flag           | no       | Показать изменения, **не** отправляя запрос.                                |           |         |          |              |                                              |

> Параметра `--output` **нет** — команда печатает текст.

---

## Rules & Validation ✅

* **Stop‑Limit**: requires **both** `--stop` and `--limit'. '--price` is not allowed for this type of **.

  * `buystoplimit`: `limit ≤ stop`
  * `sellstoplimit`: `limit ≥ stop`
* **Limit/Stop**: Requires `--price'.
* **TIF**: `GTC` / `DAY' / `GTD' (=`Specified`/`SpecifiedDay'); for `GTD`, you can/should set `--expire` (UTC/ISO‑8601).

---

## How to Use 🛠️

```powershell
# Change SL/TP only
dotnet run -- pending.modify -p demo -t 123456 --sl 1.0950 --tp 1.1050

# Change entry price (Buy Limit)
dotnet run -- pending.modify -p demo -t 123456 --type buylimit --price 1.1000

# Stop‑Limit: set trigger & limit (no --price)
dotnet run -- pending.modify -p demo -t 123456 --type buystoplimit --stop 1.1010 --limit 1.1005

# TIF=GTD with expiry
dotnet run -- pending.modify -p demo -t 123456 --tif GTD --expire "2025-09-01T12:00:00Z"

# Dry‑run preview
dotnet run -- pending.modify -p demo -t 123456 --price 1.1000 --dry-run
```

---

## Code Reference 🧩

```csharp
// (optional) ensure visibility
if (!string.IsNullOrWhiteSpace(symbol))
    await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3));

// apply changes
var ok = await _mt5Account.ModifyPendingOrderAsync(
    ticket: ticket,
    type: typeStr,
    price: price,
    stop: stop,
    limit: limit,
    sl: sl,
    tp: tp,
    tif: tifStr,
    expire: expire,
    ct: CancellationToken.None
);
```


```

---

📌 In short: точечная модификация параметров pending‑ордера с жёсткой валидацией инвариантов типа, поддержкой `TIF` и dry‑run превью.
