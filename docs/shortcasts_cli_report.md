# CLI Shortcasts — extracted from Program.cs

## `profiles` — Working with profiles


---
## `list` — Show profile names from profiles.json


**Handler preview (excerpt):**
```csharp
{
    var dict = ReadProfiles();

    if (IsJson(output))
    {
        Console.WriteLine(ToJson(dict.Keys.ToArray())); // expect: ["default","demo",...]
    }
    else
    {
        if (dict.Count == 0)
        {
            Console.WriteLine("profiles.json not found or empty.");
            return;
        }

        Console.WriteLine("Profiles:");
        foreach (var name in dict.Keys) // consider: foreach (var name in dict.Keys.OrderBy(x => x))
            Console.WriteLine($"- {name}");
    }
}
```

---
## `show` — Show the parameters of the selected profile (taking into account MT5_PASSWORD)


**Handler preview (excerpt):**
```csharp
{
    _selectedProfile = profile; // remember last chosen profile for subsequent ops

    var eff = GetEffectiveOptions(profile); // profiles.json + env override
    if (IsJson(output))
    {
        Console.WriteLine(ToJson(eff)); // sensitive: includes password in JSON
    }
    else
    {
        var envSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MT5_PASSWORD"));
        Console.WriteLine($"Profile: {profile}");
        Console.WriteLine($"  AccountId:     {eff.AccountId}");
        Console.WriteLine($"  ServerName:    {eff.ServerName}");
        Console.WriteLine($"  Host:          {eff.Host}");
        Console.WriteLine($"  Port:          {eff.Port}");
        Console.WriteLine($"  DefaultSymbol: {eff.DefaultSymbol}");
        Console.WriteLine($"  DefaultVolume: {eff.DefaultVolume}");
        Console.WriteLine($"  Password:      {(string.IsNullOrEmpty(eff.Password) ? "<empty>" : "*** (hidden)")}");
        Console.WriteLine($"  Password source: {(envSet ? "env MT5_PASSWORD" : "profiles/appsettings")}");
```

---
## `info` — Show account summary


**Handler preview (excerpt):**
```csharp
{
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:INFO Profile:{Profile}", profile))
        {
            try
            {
                await ConnectAsync();

                using var opCts = StartOpCts();
                var summary = await CallWithRetry(
                    ct => _mt5Account.AccountSummaryAsync(deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(summary));
                else
                {
                    _logger.LogInformation("=== Account Info ===");
```

---
## `quote` — Get a snapshot price (Bid/Ask/Time)

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
    Validators.EnsureProfile(profile);
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:QUOTE Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        try
        {
            await ConnectAsync();

            // best-effort: ensure visibility
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
```

---
## `buy` — Market buy

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
            Validators.EnsureProfile(profile);
            Validators.EnsureVolume(volume);
            Validators.EnsureDeviation(deviation);

            var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
            _selectedProfile = profile;

            using (UseOpTimeout(timeoutMs))
            using (_logger.BeginScope("Cmd:BUY Profile:{Profile}", profile))
            using (_logger.BeginScope("Symbol:{Symbol}", s))
            using (_logger.BeginScope("OrderParams Vol:{Vol} Dev:{Dev} SL:{SL} TP:{TP}", volume, deviation, sl, tp))
            {
                if (dryRun)
                {
                    Console.WriteLine($"[DRY-RUN] BUY {s} vol={volume} dev={deviation} SL={sl} TP={tp}");
                    return;
                }

                try
```

---
## `trail.start` — Start local trailing stop for a position


**Handler preview (excerpt):**
```csharp
{
    var profile  = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket   = ctx.ParseResult.GetValueForOption(trTicketOpt);
    var distance = ctx.ParseResult.GetValueForOption(trDistOpt);
    var step     = ctx.ParseResult.GetValueForOption(trStepOpt);
    var modestr  = ctx.ParseResult.GetValueForOption(trModeOpt);
    var timeoutMs= ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun   = ctx.ParseResult.GetValueForOption(dryRunOpt);

Validators.EnsureProfile(profile);
Validators.EnsureTicket(ticket);
if (distance <= 0) throw new ArgumentOutOfRangeException(nameof(distance));
if (step <= 0)     throw new ArgumentOutOfRangeException(nameof(step));

var modeText = (modestr ?? "classic").Trim();
if (!System.Enum.TryParse<MT5Account.TrailMode>(modeText, ignoreCase: true, out var mode))
    throw new ArgumentException("Invalid --mode. Use classic|chandelier.");

using (UseOpTimeout(timeoutMs))
using (_logger.BeginScope("Cmd:TRAIL.START Profile:{Profile}", profile))
```

---
## `close.percent` — Close a percentage of a position by ticket


**Handler preview (excerpt):**
```csharp
{
                var profile = ctx.ParseResult.GetValueForOption(profileOpt)!;
                var ticket = ctx.ParseResult.GetValueForOption(cpTicketOpt);
                var pct = ctx.ParseResult.GetValueForOption(cpPctOpt);
                var deviation = ctx.ParseResult.GetValueForOption(cpDevOpt);
                var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
                var dryRun = ctx.ParseResult.GetValueForOption(dryRunOpt);

                Validators.EnsureProfile(profile);
                Validators.EnsureTicket(ticket);
                if (pct <= 0 || pct > 100) throw new ArgumentOutOfRangeException(nameof(pct), "Percent must be in (0;100].");

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:CLOSE.PERCENT Profile:{Profile}", profile))
                using (_logger.BeginScope("Ticket:{Ticket} Pct:{Pct} Dev:{Dev}", ticket, pct, deviation))
                {
                    try
                    {
                        await ConnectAsync();
                        using var opCts = StartOpCts();
```

---
## `close.half` — Close half of a position by ticket


**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(chTicketOpt);
    var deviation = ctx.ParseResult.GetValueForOption(cpDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    ctx.Console.WriteLine("(alias) close.half -> close.percent --pct 50");

    await closePercent.InvokeAsync(new[]
    {
        "--profile",    profile,
        "--ticket",     ticket.ToString(System.Globalization.CultureInfo.InvariantCulture),
        "--pct",        "50",
        "--deviation",  deviation.ToString(System.Globalization.CultureInfo.InvariantCulture),
        "--timeout-ms", timeoutMs.ToString(System.Globalization.CultureInfo.InvariantCulture),
        // if desired, we throw dry-run:
        // dryRun ? "--dry-run" : null
    }
        // if you add conditional elements, finish .Where(s => s != null)!.ToArray()
```

---
## `trail.stop` — Stop local trailing stop


**Handler preview (excerpt):**
```csharp
{
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    _mt5Account.StopTrailing(ticket);
    Console.WriteLine($"✔ trailing stopped for #{ticket}");
}
```

---
## `sell` — Market sell

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
            Validators.EnsureProfile(profile);
            Validators.EnsureVolume(volume);
            Validators.EnsureDeviation(deviation);

            var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
            _selectedProfile = profile;

            using (UseOpTimeout(timeoutMs))
            using (_logger.BeginScope("Cmd:SELL Profile:{Profile}", profile))
            using (_logger.BeginScope("Symbol:{Symbol}", s))
            using (_logger.BeginScope("OrderParams Vol:{Vol} Dev:{Dev} SL:{SL} TP:{TP}", volume, deviation, sl, tp))
            {
                if (dryRun)
                {
                    Console.WriteLine($"[DRY-RUN] SELL {s} vol={volume} dev={deviation} SL={sl} TP={tp}");
                    return;
                }

                try
```

---
## `close` — Close by ticket (volume normalized by symbol rules)

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
            Validators.EnsureProfile(profile);
            Validators.EnsureTicket(ticket);
            Validators.EnsureVolume(volume);

            var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
            _selectedProfile = profile;

            using (UseOpTimeout(timeoutMs))
            using (_logger.BeginScope("Cmd:CLOSE Profile:{Profile}", profile))
            using (_logger.BeginScope("Ticket:{Ticket}", ticket))
            using (_logger.BeginScope("Symbol:{Symbol} Vol:{Vol}", s, volume))
            {
                if (dryRun)
                {
                    Console.WriteLine($"[DRY-RUN] CLOSE #{ticket} {s} vol={volume}");
                    return;
                }

                try
```

---
## `place` — Place a pending order

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`
- `TMT5_ENUM_ORDER_TYPE...`
- `TMT5_ENUM_ORDER_TYPE_TIME...`
- `TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit or`
- `TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStopLimit;`
- `TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit or`
- `TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit or`
- `TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStop or`

**Handler preview (excerpt):**
```csharp
{
    var profile    = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolArg  = ctx.ParseResult.GetValueForOption(symbolOpt);
    var volume     = ctx.ParseResult.GetValueForOption(volumeOpt);
    var typeStr    = ctx.ParseResult.GetValueForOption(placeTypeOpt)!;   // buylimit|...|sellstoplimit
    var priceOptV  = ctx.ParseResult.GetValueForOption(placePriceOpt);
    var stopV      = ctx.ParseResult.GetValueForOption(placeStopOpt);
    var limitV     = ctx.ParseResult.GetValueForOption(placeLimitOpt);
    var tifStr     = ctx.ParseResult.GetValueForOption(placeTifOpt);
    var expireV    = ctx.ParseResult.GetValueForOption(placeExpireOpt);
    var sl         = ctx.ParseResult.GetValueForOption(slOpt);
    var tp         = ctx.ParseResult.GetValueForOption(tpOpt);
    var timeoutMs  = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun     = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(volume);

    var s = Validators.EnsureSymbol(symbolArg ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;
```

---
## `symbol` — Symbol utilities (ensure-visible, limits, show)


---
## `ensure` — Ensure the symbol is visible in terminal

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-ENSURE Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    var report = new Dictionary<string, object?>();
                    try
                    {
                        await ConnectAsync();

                        try
                        {
                            using var visCts = StartOpCts();
                            await _mt5Account.EnsureSymbolVisibleAsync(
                                symbolName,
                                maxWait: TimeSpan.FromSeconds(3),
```

---
## `limits` — Show min/step/max volume for the symbol

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-LIMITS Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    try
                    {
                        await ConnectAsync();

                        // Best-effort visibility (some servers require it)
                        try
                        {
                            using var visCts = StartOpCts();
                            await _mt5Account.EnsureSymbolVisibleAsync(
                                symbolName, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                        }
```

---
## `show` — Short card: Quote + Limits

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-SHOW Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    try
                    {
                        await ConnectAsync();

                        // Ensure visible
                        try
                        {
                            using var visCts = StartOpCts();
                            await _mt5Account.EnsureSymbolVisibleAsync(
                                symbolName, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                        }
```

---
## `ticket` — Work with a specific ticket


---
## `show` — Show info for the ticket (open or from recent history)


**Handler preview (excerpt):**
```csharp
{
    Validators.EnsureProfile(profile);
    if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be > 0.");
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:TICKET-SHOW Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket}", ticket))
    {
        try
        {
            await ConnectAsync();

            // 1) check open sets (orders/positions)
            using var opCts = StartOpCts();
            var openTickets = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            bool isOpenOrder    = openTickets.OpenedOrdersTickets.Contains((long)ticket);
```

---
## `cancel` — Cancel (delete) pending order by ticket

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    symbol = Validators.EnsureSymbol(symbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CANCEL Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Symbol:{Symbol}", ticket, symbol))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CANCEL pending #{ticket} {symbol}");
            return;
        }

        try
        {
            await ConnectAsync();

```

---
## `close-all` — Close ALL open positions (optionally filtered by symbol)


**Handler preview (excerpt):**
```csharp
{
    var profile      = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var filterSymbol = ctx.ParseResult.GetValueForOption(caSymbolOpt);
    var yes          = ctx.ParseResult.GetValueForOption(caYesOpt);
    var deviation    = ctx.ParseResult.GetValueForOption(caDevOpt);
    var timeoutMs    = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun       = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(filterSymbol)) _ = Validators.EnsureSymbol(filterSymbol);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE-ALL Profile:{Profile}", profile))
    using (_logger.BeginScope("FilterSymbol:{Symbol} Dev:{Dev}", filterSymbol ?? "<any>", deviation))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

```

---
## `cancel.all` — Cancel all pending orders (optionally filtered)


**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbol    = ctx.ParseResult.GetValueForOption(pendSymbolOpt);
    var typeStr   = (ctx.ParseResult.GetValueForOption(pendTypeOpt) ?? "any").Trim().ToLowerInvariant();
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol!);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CANCEL.ALL Profile:{Profile}", profile))
    using (_logger.BeginScope("Filter Symbol:{Symbol} Type:{Type}", symbol ?? "<any>", typeStr))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var tickets = await CallWithRetry(
```

---
## `close.partial` — Partially close a position by ticket


**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(cpTicketOpt);
    var volume    = ctx.ParseResult.GetValueForOption(cpVolumeOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
    if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE.PARTIAL Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Vol:{Vol} Dev:{Dev}", ticket, volume, deviation))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSE.PARTIAL ticket={ticket} volume={volume} deviation={deviation}");
            return;
```

---
## `closeby` — Close a position by the opposite position (emulated with two closes)


**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var a         = ctx.ParseResult.GetValueForOption(cbATicketOpt);
    var b         = ctx.ParseResult.GetValueForOption(cbBTicketOpt);
    var volume    = ctx.ParseResult.GetValueForOption(cbVolOpt);
    var deviation = ctx.ParseResult.GetValueForOption(cbDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (a == 0 || b == 0) throw new ArgumentOutOfRangeException("tickets", "Tickets must be > 0.");
    if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSEBY Profile:{Profile}", profile))
    using (_logger.BeginScope("A:{A} B:{B} Vol:{Vol} Dev:{Dev}", a, b, volume, deviation))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSEBY a={a} b={b} volume={volume} deviation={deviation}");
```

---
## `reverse` — Reverse net position for a symbol

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{


    // -------------------------- validate --------------------------

    Validators.EnsureProfile(profile);
    Validators.EnsureDeviation(deviation);
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:REVERSE Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol} Mode:{Mode}", s, mode))
    {
        try
        {
            await ConnectAsync();

            // Ensure visibility
            try
```

---
## `reverse.ticket` — Reverse a single position by ticket

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3), null, null, visCts.Token);`

**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(rvTicketOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:REVERSE.TICKET Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Dev:{Dev}", ticket, deviation))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();
```

---
## `breakeven` — Move SL to entry ± offset (breakeven) for a position

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{

    // -------------------- validation --------------------

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    if (offsetPrice is not null && offsetPoints is not null)
        throw new ArgumentException("Use either --offset (price) OR --offset-points, not both.");
    if (offsetPrice is not null && offsetPrice < 0)
        throw new ArgumentOutOfRangeException(nameof(offsetPrice), "Offset must be >= 0.");
    if (offsetPoints is not null && offsetPoints < 0)
        throw new ArgumentOutOfRangeException(nameof(offsetPoints), "Offset points must be >= 0.");

    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:BREAKEVEN Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} OffsetPrice:{OffsetPrice} OffsetPoints:{OffsetPoints} Force:{Force}", ticket, offsetPrice, offsetPoints, force))
    {
```

---
## `close-symbol` — Close ALL open positions for a given symbol


**Handler preview (excerpt):**
```csharp
{
    Validators.EnsureProfile(profile);

    // Default to profile's default symbol if not provided
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE-SYMBOL Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        try
        {
            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] Would close all positions for {s}.");
                return;
            }

            await ConnectAsync();
```

---
## `partial-close` — Partially close a position by ticket

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{

    // --------------------- validation ---------------------

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    if ((percent is null && volume is null) || (percent is not null && volume is not null))
        throw new ArgumentException("Specify exactly one: --percent or --volume.");

    if (percent is not null)
    {
        if (percent <= 0 || percent > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be in 1..100.");
    }
    if (volume is not null)
    {
        if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");
    }

```

---
## `position.modify` — Modify SL/TP for a position by ticket


**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(posModTicketOpt);
    var sl        = ctx.ParseResult.GetValueForOption(posModSlOpt);
    var tp        = ctx.ParseResult.GetValueForOption(posModTpOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
    if (sl is null && tp is null) throw new ArgumentException("Specify at least one of --sl or --tp.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITION.MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SL:{SL} TP:{TP}", ticket, sl, tp))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] POSITION.MODIFY ticket={ticket} SL={(sl?.ToString() ?? "-")} TP={(tp?.ToString() ?? "-")}");
            return;
```

---
## `panic` — Close ALL positions and cancel ALL pendings (optionally by symbol)


**Handler preview (excerpt):**
```csharp
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
            using var opCts = StartOpCts();

            var pos = await CallWithRetry(ct => _mt5Account.ListPositionVolumesAsync(symbol, ct), opCts.Token);
```

---
## `position.modify.points` — Set SL/TP by distance in points from entry/market


**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(pmpTicketOpt);
    var slPts     = ctx.ParseResult.GetValueForOption(pmpSlPtsOpt);
    var tpPts     = ctx.ParseResult.GetValueForOption(pmpTpPtsOpt);
    var fromStr   = (ctx.ParseResult.GetValueForOption(pmpFromOpt) ?? "entry").Trim().ToLowerInvariant();
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (fromStr != "entry" && fromStr != "market")
        throw new ArgumentException("Invalid --from. Use entry|market.");
    if (slPts is null && tpPts is null)
        throw new ArgumentException("Specify at least one of --sl-points or --tp-points.");
    if (slPts is not null && slPts < 0) throw new ArgumentOutOfRangeException(nameof(slPts));
    if (tpPts is not null && tpPts < 0) throw new ArgumentOutOfRangeException(nameof(tpPts));

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITION.MODIFY.POINTS Profile:{Profile}", profile))
```

---
## `modify` — Modify StopLoss / TakeProfit by ticket

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(`

**Handler preview (excerpt):**
```csharp
{
    // ------------------- fast validation -------------------
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (sl is null && tp is null)
        throw new ArgumentException("At least one of --sl or --tp must be provided.");

    string? s = symbol;
    if (!string.IsNullOrWhiteSpace(s))
        s = Validators.EnsureSymbol(s);

    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SL:{SL} TP:{TP}", ticket, sl, tp))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        if (dryRun)
        {
```

---
## `pending.modify` — Modify a pending order (price/stop-limit/SL/TP/expiry)

**Underlying calls (detected):**
- `await _mt5Account.EnsureSymbolVisibleAsync(s!, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);`

**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolArg = ctx.ParseResult.GetValueForOption(symbolOpt);
    var ticket    = ctx.ParseResult.GetValueForOption(pmTicketOpt);
    var typeStr   = ctx.ParseResult.GetValueForOption(pmTypeOpt);
    var price     = ctx.ParseResult.GetValueForOption(pmPriceOpt);
    var stop      = ctx.ParseResult.GetValueForOption(pmStopOpt);
    var limit     = ctx.ParseResult.GetValueForOption(pmLimitOpt);
    var sl        = ctx.ParseResult.GetValueForOption(pmSlOpt);
    var tp        = ctx.ParseResult.GetValueForOption(pmTpOpt);
    var tifStr    = ctx.ParseResult.GetValueForOption(pmTifOpt);
    var expire    = ctx.ParseResult.GetValueForOption(pmExpireOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    _ = ticket;

    var s = symbolArg ?? GetOptions().DefaultSymbol;
    using (UseOpTimeout(timeoutMs))
```

---
## `pending.move` — Move a pending order price(s) by ±N points


**Handler preview (excerpt):**
```csharp
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(pmTicketOpt);
    var byPoints  = ctx.ParseResult.GetValueForOption(pmByPtsOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (byPoints == 0) { Console.WriteLine("Nothing to do: by-points is 0."); return; }

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING.MOVE Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} ByPoints:{By}", ticket, byPoints))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

```

---
## `stream` — Subscribe to trading events/ticks (auto-reconnect)


**Handler preview (excerpt):**
```csharp
{
    Validators.EnsureProfile(profile);
    if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be > 0.");

    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:STREAM Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol} Seconds:{Seconds}", s, seconds))
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            await ConnectAsync();

            using var streamCts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            _logger.LogInformation("Streaming started (auto-reconnect enabled).");

```

---
## `positions` — List active positions


**Handler preview (excerpt):**
```csharp
{
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:POSITIONS Profile:{Profile}", profile))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var opened = await CallWithRetry(
                    ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(opened));
                else
                {
                    var list = opened.PositionInfos;
```

---
## `orders` — List open orders and positions tickets


**Handler preview (excerpt):**
```csharp
{
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:ORDERS Profile:{Profile}", profile))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var tickets = await CallWithRetry(
                    ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(tickets));
                else
                {
                    var o = tickets.OpenedOrdersTickets;
```

---
## `pending` — Pending orders utilities


---
## `list` — List pending order tickets


**Handler preview (excerpt):**
```csharp
{
    Validators.EnsureProfile(profile);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING/LIST Profile:{Profile}", profile))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            // This call returns both pending-order tickets and open-position tickets
            var tickets = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pendingTickets  = tickets.OpenedOrdersTickets;     // pending orders
            // var positionTickets = tickets.OpenedPositionTickets; // not used here

```

---
## `history` — Orders/deals history for the last N days


**Handler preview (excerpt):**
```csharp
{
        Validators.EnsureProfile(profile);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be > 0.");
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:HISTORY Profile:{Profile}", profile))
        using (_logger.BeginScope("Days:{Days}", days))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
                var to   = DateTime.UtcNow;

                var res = await CallWithRetry(
                    ct => _mt5Account.OrderHistoryAsync(from, to, deadline: null, cancellationToken: ct),
                    opCts.Token);
```

---
## `lot.calc` — Calculate position volume by risk % and SL distance (points)


**Handler preview (excerpt):**
```csharp
{
    var symbol   = Validators.EnsureSymbol(ctx.ParseResult.GetValueForOption(lcSymbolOpt)!);
    var riskPct  = ctx.ParseResult.GetValueForOption(lcRiskPctOpt);
    var slPoints = ctx.ParseResult.GetValueForOption(lcSlPtsOpt);
    var balance  = ctx.ParseResult.GetValueForOption(lcBalanceOpt);
    var minLot   = ctx.ParseResult.GetValueForOption(lcMinLotOpt);
    var lotStep  = ctx.ParseResult.GetValueForOption(lcStepLotOpt);
    var maxLot   = ctx.ParseResult.GetValueForOption(lcMaxLotOpt);
    var timeoutMs= ctx.ParseResult.GetValueForOption(timeoutOpt);
    var output   = (ctx.ParseResult.GetValueForOption(outputOpt) ?? "text").Trim().ToLowerInvariant();

    if (riskPct <= 0 || riskPct > 100) throw new ArgumentOutOfRangeException(nameof(riskPct), "Use (0;100].");
    if (slPoints <= 0) throw new ArgumentOutOfRangeException(nameof(slPoints));
    if (balance <= 0) throw new ArgumentOutOfRangeException(nameof(balance));
    if (minLot <= 0) throw new ArgumentOutOfRangeException(nameof(minLot));
    if (lotStep <= 0) throw new ArgumentOutOfRangeException(nameof(lotStep));
    if (maxLot is not null && maxLot <= 0) throw new ArgumentOutOfRangeException(nameof(maxLot));

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:LOT.CALC Symbol:{Symbol} Risk%:{Risk} SLpts:{SL} Bal:{Bal}", symbol, riskPct, slPoints, balance))
```

---
## `history.export` — Export trading history (deals/orders) to CSV/JSON


**Handler preview (excerpt):**
```csharp
{
    var profile = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var days    = ctx.ParseResult.GetValueForOption(heDaysOpt);
    var symbol  = ctx.ParseResult.GetValueForOption(heSymbolOpt);
    var to      = (ctx.ParseResult.GetValueForOption(heToOpt) ?? "csv").Trim().ToLowerInvariant();
    var file    = ctx.ParseResult.GetValueForOption(heFileOpt)!;
    var timeout = ctx.ParseResult.GetValueForOption(timeoutOpt);

    if (to != "csv" && to != "json") throw new ArgumentException("Use --to csv|json");

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol);

    using (UseOpTimeout(timeout))
    using (_logger.BeginScope("Cmd:HISTORY.EXPORT Profile:{Profile}", profile))
    using (_logger.BeginScope("Days:{Days} Symbol:{Symbol} To:{To} File:{File}", days, symbol ?? "<any>", to, file))
    {
        try
        {
            await ConnectAsync();
```

---
## `health` — Quick connectivity and account diagnostics


**Handler preview (excerpt):**
```csharp
{
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:HEALTH Profile:{Profile}", profile))
        {
            var report = new Dictionary<string, object?>();

            try
            {
                var opt = GetEffectiveOptions(profile);
                report["profile"]    = profile;
                report["accountId"]  = opt.AccountId;
                report["serverName"] = opt.ServerName;
                report["host"]       = opt.Host;
                report["port"]       = opt.Port;

                if (!string.IsNullOrWhiteSpace(opt.Host) && opt.Port > 0)
                {
```

---
