# CSharpMT5 — Documentation BASE (C#)

No magic. Just MT5, C#, and clear recipes: get a quote, open/modify/close safely, and stream ticks.

<div class="grid cards" markdown>

-   :material-rocket-launch: **Start Here**
    ---
    First run, profiles, quick commands.
    <br>
    [:octicons-play-16: Getting Started](Getting_Started.md){ .md-button }
    [:material-form-textbox-password: Command Index](Command_Index.md){ .md-button-outline }

-   :material-source-branch: **Architecture & Reliability**
    ---
    Data flow, health checks, timeouts & retries.
    <br>
    [:material-topology: Data Flow](Architecture_DataFlow.md){ .md-button }
    [:material-clock-alert: Timeouts & Retries](Timeouts_RetriesPolicy.md){ .md-button-outline }
    [:material-heart-pulse: Health](Diagnostics/Health.md){ .md-button-outline }

-   :material-chart-line: **Market Data**
    ---
    Quotes, symbols, limits, ensure visible.
    <br>
    [:material-currency-usd: Quote](Market_Data/Quote.md){ .md-button }
    [:material-eye: Ensure Visible](Market_Data/Ensure_Symbol_Visible.md){ .md-button-outline }
    [:material-shield-half-full: Limits](Market_Data/Limits.md){ .md-button-outline }

-   :material-cart: **Orders & Positions**
    ---
    Place, modify, partial closes, trails.
    <br>
    [:material-cart-plus: Place](Orders_Positions/Place.md){ .md-button }
    [:material-pencil: Modify](Orders_Positions/Modify.md){ .md-button-outline }
    [:material-select-group: Close %](Orders_Positions/Close.percent.md){ .md-button-outline }

-   :material-broadcast: **Streaming**
    ---
    Subscribe to live tick events.
    <br>
    [:material-rss: Subscribe](Streaming/Subscribe.md){ .md-button }

-   :material-account: **Account**
    ---
    Profiles and account info.
    <br>
    [:material-badge-account: Overview](Account/Overview.md){ .md-button }
    [:material-card-account-details: Show](Account/Show.md){ .md-button-outline }
    [:material-account-cog: Profiles](Account/Profiles.md){ .md-button-outline }

-   :material-api: **API Reference**
    ---
    Types, messages, and streaming (C#).
    <br>
    [:material-view-list: Overview](API_Reference/Overview.md){ .md-button }
    [:material-format-list-bulleted-type: Enums](API_Reference/Enums.md){ .md-button-outline }
    [:material-email: Messages](API_Reference/Messages.md){ .md-button-outline }
    [:material-email-fast: Streaming](API_Reference/Streaming.md){ .md-button-outline }

-   :material-hammer-wrench: **Shortcasts & Tools**
    ---
    Live examples, ticket toolbox, logging formats.
    <br>
    [:material-video-outline: Live Examples](Shortcasts_LiveExamples.md){ .md-button }
    [:material-ticket-account: Ticket Toolbox](Ticket_Toolbox.md){ .md-button-outline }
    [:material-xml: Logging Formats](Logging_OutputFormats.md){ .md-button-outline }

-   :material-shield-lock: **Ops**
    ---
    Smart stops, risk tools, troubleshooting.
    <br>
    [:material-shield: Smart Stops](SymbolRules_SmartStops.md){ .md-button }
    [:material-shield-search: Lot Calc](Risk_Tools/Lot.calc.md){ .md-button-outline }
    [:material-frequently-asked-questions: Troubleshooting](Troubleshooting%28FAQ%29.md){ .md-button-outline }
</div>

---

## 📑 Table of Contents

### Core Guides
- [Getting Started](Getting_Started.md)
- [Architecture & Data Flow](Architecture_DataFlow.md)
- [Timeouts & Retries Policy](Timeouts_RetriesPolicy.md)
- [Command Index](Command_Index.md)
- [Glossary](Glossary.md)
- [Shortcasts: Live Examples](Shortcasts_LiveExamples.md)
- [Ticket Toolbox](Ticket_Toolbox.md)
- [Logging: Output Formats](Logging_OutputFormats.md)

### Account
- [Overview](Account/Overview.md)
- [Show](Account/Show.md)
- [Info](Account/Info.md)
- [Profiles](Account/Profiles.md)

### Market Data
- [Overview](Market_Data/Market_Data_Overview.md)
- [Quote](Market_Data/Quote.md)
- [Symbol](Market_Data/Symbol.md)
- [Limits](Market_Data/Limits.md)
- [Ensure Symbol Visible](Market_Data/Ensure_Symbol_Visible.md)
- [Reverse](Market_Data/Reverse.md)
- [Close (generic)](Market_Data/Close.md)
- [Close (by symbol)](Market_Data/Close-symbol.md)
- [Close (all)](Market_Data/Close-all.md)
- [Panic](Market_Data/Panic.md)
- [Pending: move](Market_Data/Pending.move.md)
- [Pending: modify](Market_Data/Pending.modify.md)

### Orders & Positions
- [Overview](Orders_Positions/Orders_Positions_Overview.md)
- [Orders](Orders_Positions/Orders.md)
- [Positions](Orders_Positions/Positions.md)
- [Place](Orders_Positions/Place.md)
- [Buy](Orders_Positions/Buy.md)
- [Sell](Orders_Positions/Sell.md)
- [Pending](Orders_Positions/Pending.md)
- [Modify](Orders_Positions/Modify.md)
- [Position.modify](Orders_Positions/Position.modify.md)
- [Position.modify.points](Orders_Positions/Position.modify.points.md)
- [CloseBy](Orders_Positions/CloseBy.md)
- [Cancel](Orders_Positions/Cancel.md)
- [Cancel All](Orders_Positions/Cancel_All.md)
- [Partial-close (how-to)](Orders_Positions/Partial-close.md)
- [Close.half](Orders_Positions/Close.half.md)
- [Close.partial](Orders_Positions/Close.partial.md)
- [Close.percent](Orders_Positions/Close.percent.md)
- [Trail.start](Orders_Positions/Trail.start.md)
- [Trail.stop](Orders_Positions/Trail.stop.md)

### Streaming
- [Subscribe](Streaming/Subscribe.md)

### History
- [Overview](History/History_Overview.md)
- [History](History/History.md)
- [Export](History/History_export.md)
- [History Expor (legacy)](History_Expor.md)

### Diagnostics / Ops
- [Health](Diagnostics/Health.md)
- [Troubleshooting & FAQ](Troubleshooting%28FAQ%29.md)
- [Symbol Rules & Smart Stops](SymbolRules_SmartStops.md)

### API Reference
- [Overview](API_Reference/Overview.md)
- [Enums](API_Reference/Enums.md)
- [Messages](API_Reference/Messages.md)
- [Streaming](API_Reference/Streaming.md)

### Misc
- [Overview](Misc/Misc_Overview.md)
- [List](Misc/List.md)
- [Ticket Show](Misc/Ticket_Show.md)
- [Specific Ticket](Misc/Specific_Ticket.md)
- [Pending List](Misc/Pending_List.md)
- [Reverse by Ticket](Misc/Reverse_Ticket.md)

---

## ⚡ In 10 minutes you can
- Run a demo profile and fetch a quote.
- Subscribe to multi-symbol ticks.
- Place a market order with safe stops.
- Modify a position respecting symbol rules.

**Quick links:**  
👉 [Quote](Market_Data/Quote.md) · 👉 [Place](Orders_Positions/Place.md) · 👉 [Subscribe](Streaming/Subscribe.md)
