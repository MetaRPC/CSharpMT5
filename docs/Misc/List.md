# List Profiles (`list`) 📂

## What it Does 🎯

Displays all available **profile names** from your `profiles.json`.
Profiles contain login, server, and password settings for connecting to MT5.

---

## Input Parameters ⬇️

| Parameter        | Type   | Required | Description                                       |
| ---------------- | ------ | -------- | ------------------------------------------------- |
| `--output`, `-o` | string | ❌        | Output format: `text` (default) or `json`.        |
| `--timeout-ms`   | int    | ❌        | Timeout in ms for the operation (default: 30000). |

---

## Output Fields ⬆️

| Field      | Type  | Description                                           |
| ---------- | ----- | ----------------------------------------------------- |
| `Profiles` | array | List of profile names (e.g., `demo`, `live`, `test`). |

---

## How to Use 🛠️

### CLI

```powershell
# Show available profiles
dotnet run -- list

# JSON output
dotnet run -- list -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo   # set default profile to demo
list          # show all available profiles
```

---

## When to Use ❓

* To verify which profiles are configured before connecting.
* To quickly check if `profiles.json` is loaded correctly.
* To confirm names before running `use-pf` or commands requiring `-p`.

---

## Notes & Safety 🛡️

* If no `profiles.json` is present, this will return an empty list or error.
* Use in combination with `info` to confirm credentials for a selected profile.

---

## Code Reference (to be filled by you) 🧩

```csharp
var listCmd = new Command("list", "Show profile names from profiles.json");
listCmd.AddAlias("ls");
listCmd.AddOption(outputOpt);
listCmd.SetHandler((string output) =>
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
}, outputOpt);

profilesCmd.AddCommand(listCmd);
```
