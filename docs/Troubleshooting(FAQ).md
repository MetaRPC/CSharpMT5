# Diagnostics & Troubleshooting (FAQ) 🛠️

This section lists **common errors, warnings, and edge cases** you may encounter when running `CSharpMT5`, with explanations tied to our code and retry/validation logic.

---

## 1. Profiles & Connection

**❌ Error:**
`Set Host or MtClusterName before connect`

**Cause:**
Your profile is missing either `Host/Port` or `ServerName`.
Our code (`Validators.EnsureProfile`) enforces that at least one connect method is defined.

**✅ Fix:**
Check `profiles.json`. Example:

```json
"default": {
  "AccountId": 111111,
  "Password": "YOUR_PASSWORD",
  "Host": "95.217.147.61",
  "Port": 443
}
```

or

```json
"demo": {
  "AccountId": 95591860,
  "Password": "YOUR_PASSWORD",
  "ServerName": "MetaQuotes-Demo"
}
```

---

## 2. Stale Quotes / Invalid Stops

**❌ Error:**
`Invalid TP for BUY: must be > Ask`
or
`[STALE >5s]` on quotes.

**Cause:**
Our preflight (`PreflightStops`) validates SL/TP against latest **Bid/Ask**.
If your SL/TP is on the wrong side of market, or quotes are stale, validation fails.

**✅ Fix:**

* Adjust SL/TP relative to Bid/Ask.
* If using `--dry-run`, values are not checked.
* For stale feed: ensure terminal is connected, or switch to another broker server.

---

## 3. Timeouts & Retries

**❌ Error:**
`OperationCanceledException`

**Cause:**

* The gRPC call exceeded `--timeout-ms`.
* Network hiccup.
* MetaTrader terminal busy.

**✅ Fix:**

* Increase `--timeout-ms` (default 30000).
* Use `--trace` for detailed logs.
* Our code retries most calls (`CallWithRetry`), but some failures propagate if persistent.

---

## 4. Symbol Visibility

**⚠️ Warning:**
`EnsureSymbolVisibleAsync failed: Symbol hidden`

**Cause:**
MT5 sometimes hides symbols until manually added to Market Watch.
Our code attempts best-effort auto-ensure, but not all servers allow it.

**✅ Fix:**
Run:

```powershell
dotnet run -- symbol ensure -p demo -s EURUSD
```

---

## 5. Partial Close / Reverse

**❌ Error:**
`Position with ticket #123456 not found`

**Cause:**

* Ticket not in `OpenedOrdersAsync` response.
* Wrong account/profile.
* Already closed by another action.

**✅ Fix:**

* Double-check ticket with `positions` or `orders`.
* Ensure correct profile with `-p demo`.
* For `reverse`, ensure symbol exposure exists.

---

## 6. History Export

**❌ Error:**
`file path invalid or not writable`

**Cause:**
`history.export` uses `--file` to save CSV/JSON. If path invalid, exception thrown.

**✅ Fix:**

* Use full path (e.g., `C:\temp\hist.csv`).
* Ensure write permissions.
* Example:

```powershell
dotnet run -- history.export -p demo -d 30 --to csv --file C:\temp\hist.csv
```

---

## 7. Dry-Run Mode

**ℹ️ Behavior:**
With `--dry-run`, nothing is sent to broker.
Our code prints JSON or text simulation.

Use it to **test commands safely** before risking real money.

---

## 8. Panic Mode

**⚠️ Note:**
`panic` closes all positions and cancels pendings.
This calls `CloseOrderByTicketAsync(volume=0)` convention for deletions.

**✅ Best practice:**
Use only in controlled scripts (CI/CD, emergency exit).
