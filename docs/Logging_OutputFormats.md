# Logging & Output Formats 📝

## Logging levels 🔍

The CLI provides two extended logging modes in addition to the default (`info`):

* `--verbose` — enables **debug-level** logging.
  ℹ️ Useful for tracing account IDs, symbol visibility, ticket numbers.

* `--trace` — enables **very detailed** logs (wire/stream-level).
  ⚠️ Warning: very noisy, mostly for troubleshooting connectivity or gRPC issues.

By default only important `info` / `warn` / `error` messages are printed.

---

## Output formats 📤

Many commands support an `-o, --output` option:

* `text` (default) — human-readable, concise output.
* `json` — structured JSON, suitable for parsing by scripts and external systems.

**Examples:**

```powershell
# Positions in JSON
📊 dotnet run -- positions -p demo -o json

# Dry-run in JSON
🤖 dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --dry-run -o json
```

---

## Dry-run mode 🛑

`--dry-run` shows what would be executed **without sending anything** to the server.
This is especially useful for testing and debugging scripts.

```powershell
# Preview a reverse order without sending
👀 dotnet run -- reverse -p demo -s EURUSD --mode net --sl 1.0700 --tp 1.0850 --dry-run
```

---

## Related 🔗

* [Timeouts & Retries Policy](Timeouts_RetriesPolicy.md)
* [Shortcasts & Live Examples](Shortcasts_LiveExamples.md)
