# 🔍 Interactive Protobuf Types Inspector

Interactive utility for exploring MT5 gRPC API protobuf types.

## 📌 Why You Need This Tool

When working with MT5 gRPC API, you constantly face questions like:

- *"What fields does `AccountSummaryData` have?"*
- *"Where can I find the `Ticket` field?"*
- *"What values does the `ORDER_TYPE` enum contain?"*
- *"What types are available for working with orders/positions?"*

**Yes, all this information is already in the documentation**, but without this inspector you would have to:

-  Open multiple documentation files and scroll through pages
-  Remember which doc file contains which type
-  Manually search through text (Ctrl+F) in each file
-  Constantly switch between IDE and documentation browser
-  Interrupt your coding flow to look up references

**ProtobufInspector solves all these problems in one command!** 🎯

### ✨ Convenience Features:

-  **Instant launch** — just `dotnet run inspect`
-  **Smart search** — find types by name, field, or keyword
-  **Clear output** — structured display with emojis and formatting
-  **Case insensitive** — no need to remember exact casing
-  **Interactive** — explore API in real-time without recompiling code
-  **Time saver** — answers in seconds instead of minutes of searching

**Real-world benefit:** Instead of spending 5-10 minutes searching documentation or proto files, you get the answer in 5 seconds. For daily MT5 API development, this tool becomes indispensable.

## 🚀 Launch

```bash
dotnet run inspect
```

## 📖 Available Commands

| Command | Description | Example |
|---------|-------------|---------|
| `list` or `ls` | Show all available types | `list` |
| `<TypeName>` | Inspect specific type | `OpenedOrdersData` |
| `search <text>` or `find <text>` | Find types containing text | `search Order` |
| `field <name>` | Find types with specific field | `field Balance` |
| `enum <name>` | Show enum values | `enum BMT5_ENUM_ORDER_TYPE` |
| `help` or `?` | Show help | `help` |
| `exit`, `quit` or `q` | Exit inspector | `exit` |

## 💡 Practical Examples

### Example 1: Discover AccountSummaryData fields
```
> AccountSummaryData
```
**Result:**
```
📋 Properties (15):
  • AccountBalance    : double
  • AccountEquity     : double
  • AccountProfit     : double
  • AccountMargin     : double
  ...
```

### Example 2: Find type with "Ticket" field
```
> field Ticket
```
**Result:**
```
✅ Found in 5 type(s):

📦 OpenedOrderInfo:
   └─ Ticket: ulong

📦 PositionInfo:
   └─ Ticket: ulong

📦 OrderCloseRequest:
   └─ Ticket: ulong
...
```

### Example 3: View ORDER_TYPE values
```
> enum ENUM_ORDER_TYPE
```
**Result:**
```
════════════════════════════════
    ENUM: ENUM_ORDER_TYPE                      
════════════════════════════════

  ORDER_TYPE_BUY           = 0
  ORDER_TYPE_SELL          = 1
  ORDER_TYPE_BUY_LIMIT     = 2
  ORDER_TYPE_SELL_LIMIT    = 3
  ORDER_TYPE_BUY_STOP      = 4
  ORDER_TYPE_SELL_STOP     = 5
  ...
```

### Example 4: Find all "Position" related types
```
> search Position
```
**Result:**
```
✅ Found 8 type(s):

  [Class]  PositionInfo
  [Class]  PositionsTotalData
  [Class]  PositionsHistoryData
  [Enum]   ENUM_POSITION_TYPE
  ...
```

### Example 5: View all available types
```
> list
```
**Result:**
```
📦 Found 156 types:

  [Data]    AccountSummaryData
  [Data]    BalanceData
  [Data]    OpenedOrdersData
  [Request] OrderSendRequest
  [Request] OrderCloseRequest
  ...
```

## 🎯 Common Use Cases

### Scenario 1: "Getting 'field not found' error"
**Problem:** Writing `summary.FreeMargin` but getting compilation error.

**Solution:**
```
> field FreeMargin
# or
> AccountSummaryData
```
**Result:** Discover the correct field name is `AccountBalance` or `AccountEquity`.

---

### Scenario 2: "Don't know what properties a type has"
**Problem:** Working with `OpenedOrdersData` but unsure what's inside.

**Solution:**
```
> OpenedOrdersData
```
**Result:**
```
  📚 OpenedOrders  : List<OpenedOrderInfo>
  📚 PositionInfos : List<PositionInfo>
```

---

### Scenario 3: "Need enum values for ORDER_TYPE"
**Problem:** Want to use enum but don't know exact values.

**Solution:**
```
> enum BMT5_ENUM_ORDER_TYPE
```
**Result:** All values with numeric codes.

---

### Scenario 4: "Looking for order-related types"
**Problem:** Need to find all types related to orders.

**Solution:**
```
> search Order
```
**Result:** `OrderSendRequest`, `OrderCloseRequest`, `OpenedOrderInfo`, etc.

---

### Scenario 5: "Want to understand what's available in API"
**Problem:** New to the API, want to see what's available.

**Solution:**
```
> list
```
**Result:** Complete list of all types (Data, Request, Reply).

## 🔧 Code Integration

After finding needed information:

```csharp
// Found via inspector that correct field is AccountBalance
> AccountSummaryData
  • AccountBalance : double  ← here it is!

// Use in code:
var summary = await service.GetAccountSnapshot();
Console.WriteLine(summary.Summary.AccountBalance);  // ✅ Works!
```

## 📝 Notes

- **Case insensitive**: can write `openeordersdata` or `OpenedOrdersData`
- **Partial search**: `search Pos` finds `Position`, `PositionInfo`, etc.
- **Lists marked** 📚: `List<T>` or `RepeatedField<T>`
- **Classes marked** 📦: regular types
- **Enums marked** as `[Enum]`

## 🚨 Troubleshooting

**Q: Command doesn't find type**
```
> MyType
❌ Type 'MyType' not found
```
**A:** Try search:
```
> search MyType
```

**Q: Need to find all enums**
```
> list
# Look for lines with [Enum]
```

**Q: Want to exit inspector**
```
> exit
# or
> quit
# or
> q
# or just Ctrl+C
```

## 🎓 Additional Information

- File location: `Examples/Helpers/ProtobufInspector.cs`
- Invoked from Program.cs: `case "inspect"` or `case "types"`
- Used for: Exploring MT5 gRPC API types from `mt5_term_api`

## 💻 Technical Details

**What is inspected:**

- All types from namespace `mt5_term_api`
- Only public types (public classes/enums)
- Types with suffixes: `Data`, `Request`, `Reply`
- All public properties and their types

**Supported types:**

- ✅ Classes (protobuf messages)
- ✅ Enums
- ✅ Generic types (List<T>, etc.)
- ✅ Primitive types (int, double, string, etc.)
