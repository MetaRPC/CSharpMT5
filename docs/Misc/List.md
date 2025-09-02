# List Profiles (`profiles list`) 📂

## What it Does

Displays all available **profile names** from `profiles.json` (login/server/password settings for connecting to MT5).

> Subcommand of the **profiles** group. Invoke as `profiles list` (alias: `ls`).

---

## Input Parameters ⬇️

| Parameter      | Type   | Required | Description                 |
| -------------- | ------ | -------- | --------------------------- |
| `--output, -o` | string | no       | `text` (default) or `json`. |

> `--timeout-ms` is **not** supported for this subcommand.

---

## Output ⬆️

**Text mode**

```
Profiles:
- default
- demo
- live
```

**JSON mode**

```json
["default","demo","live"]
```

---

## How to Use 🛠️

```powershell
# Text
dotnet run -- profiles list

# JSON
dotnet run -- profiles list -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo   # set current profile to demo
pf list       # same as: profiles list
```

---

## Notes & Safety 🛡️

* File path: `profiles.json` is read from **AppContext.BaseDirectory**.
* If file is missing/empty or parsing fails, the command prints `profiles.json not found or empty.` in text mode, or returns `[]` in JSON.
* Iteration order of profile names is not guaranteed. Sort if you need stable output.

---

## Code Reference 🧷

```csharp
// Read & print
var dict = ReadProfiles();
if (IsJson(output))
    Console.WriteLine(ToJson(dict.Keys.ToArray()));
else
{
    if (dict.Count == 0) { Console.WriteLine("profiles.json not found or empty."); return; }
    Console.WriteLine("Profiles:");
    foreach (var name in dict.Keys) Console.WriteLine($"- {name}");
}
```

### Helper

```csharp
private Dictionary<string, MT5Options> ReadProfiles()
{
    var path = Path.Combine(AppContext.BaseDirectory, "profiles.json");
    if (!File.Exists(path)) return new();
    var json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<Dictionary<string, MT5Options>>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
}
```
