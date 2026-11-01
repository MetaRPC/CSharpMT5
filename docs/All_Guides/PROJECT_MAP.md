# 🗺️ CSharpMT5 Project Map

> The whole project as one annotated tree, with links. **Links point to section *overviews*, never to individual method pages** — open an overview to drill down into its methods.

---

## 🧱 The three layers

CSharpMT5 is built in three decoupled layers. Your code can enter at any level.

| Layer | Source | What it does | Overview |
| ----- | ------ | ------------ | -------- |
| **3 · MT5Sugar** | `MT5Sugar.cs` | Convenience on top of Service (extension methods): points-based SL/TP, risk-based sizing, price/volume normalization, snapshots, batch close/cancel, market-data & stats helpers. | [MT5Sugar overview](../MT5Sugar/MT5Sugar.API_Overview.md) |
| **2 · MT5Service** | `MT5Service.cs` | Typed facade: friendly signatures, unwrapped C# primitives (no raw proto), convenience shortcuts (`GetBidAsync`, `BuyMarketAsync`). What most code uses. | [MT5Service overview](../MT5Service/MT5Service.Overview.md) |
| **1 · MT5Account** | `package/Helpers/MT5Account.cs` | Low-level gRPC: proto request/reply, connect/session, auto-reconnect, streaming. Everything else builds on it. | [MT5Account overview](../MT5Account/MT5Account.Master.Overview.md) |

```
Your code  →  MT5Sugar  →  MT5Service  →  MT5Account  →  gRPC  →  MetaRPC gateway → MT5 terminal
```

---

## 🌳 Repository tree

- **`CSharpMT5/`** — repo root (SDK layers + runnable demo app)
  - `MT5Service.cs` — **Layer 2** source → [MT5Service overview](../MT5Service/MT5Service.Overview.md)
  - `MT5Sugar.cs` — **Layer 3** source (extension methods on `MT5Service`) → [MT5Sugar overview](../MT5Sugar/MT5Sugar.API_Overview.md)
  - `Program.cs` — CLI entry / command dispatcher (`lowlevel`, `service`, `sugar`, `trading`, `inspect`, `usercode`, …) → [Getting Started](./GETTING_STARTED.md)
  - **`package/`** — low-level gRPC layer (the publishable NuGet package `MetaRPC.MT5`)
    - `Helpers/MT5Account.cs` — **Layer 1** source → [MT5Account overview](../MT5Account/MT5Account.Master.Overview.md)
    - `*Grpc.cs` · `Mt5TermApi*.cs` · `MrpcMt5Error.cs` — generated gRPC / protobuf stubs (the raw contract)
    - `MetaRPC.MT5.csproj` — the publishable package project
  - **`Errors/`** — domain exceptions: `ApiExceptionMT5.cs`, `ConnectExceptionMT5.cs`
  - **`Examples/`** — runnable demos (dispatched by `Program.cs`)
    - **`Helpers/`** — shared helpers (`ConnectionHelper`, `ConsoleHelper`, `ProgressBarHelper`) + `ProtobufInspector` → [Protobuf Inspector](./PROTOBUF_INSPECTOR_GUIDE.md)
    - **`LowLevel/`** — `Program.LowLevel` / `.Trading` / `.Streaming` → [MT5Account overview](../MT5Account/MT5Account.Master.Overview.md)
    - **`Services/`** — `Program.Service` → [MT5Service overview](../MT5Service/MT5Service.Overview.md)
    - **`Sugar/`** — `Program.Sugar` (+ Monitor / PendingOrders / Scalper) → [MT5Sugar overview](../MT5Sugar/MT5Sugar.API_Overview.md)
    - **`Orchestrators/`** + **`Presets/`** — the strategy library → [Strategies overview](../Strategies/Strategies.Master.Overview.md)
    - **`MoreExamples/`** — explained walkthroughs (`01`–`10`)
    - **`UserCode/`** — `ProgramUserCode` sandbox → [UserCode Sandbox](./USERCODE_SANDBOX_GUIDE.md)
  - **`Config/`** — `appsettings.json` (your MT5 credentials + API key) → [Getting Started](./GETTING_STARTED.md)
  - **`tests/`** — test project
  - **`docs/`** — this documentation site (see the map below)
  - `mkdocs.yml` — docs site config
  - `MetaRPC.CSharpMT5.csproj` · `MetaRPC.CSharpMT5.sln` — the demo app project / solution

---

## 📚 Documentation tree

- **`docs/`**
  - [Home](../index.md) — landing page
  - **`All_Guides/`** — the guides (Getting Started, Your First Project, Glossary, MT5 for Beginners, Sync vs Async, gRPC Streaming, ENUMs, Return Codes, Protobuf Inspector, UserCode Sandbox); browse them in the left nav
  - **`MT5Account/`** — low-level gRPC docs → [overview](../MT5Account/MT5Account.Master.Overview.md) *(per-method pages inside)*
  - **`MT5Service/`** — typed facade docs → [overview](../MT5Service/MT5Service.Overview.md) *(per-group pages inside)*
  - **`MT5Sugar/`** — convenience API docs → [overview](../MT5Sugar/MT5Sugar.API_Overview.md) *(per-group pages inside)*
  - **`Strategies/`** — the orchestrators & presets → [overview](../Strategies/Strategies.Master.Overview.md) *(per-strategy pages inside)*
  - `Guide_Images/` · `styles/` · `javascripts/` · `includes/` — assets & theme

> The `MT5Account/`, `MT5Service/`, `MT5Sugar/` and `Strategies/` folders hold the detail pages. By design this map links only their **overviews** — each overview is the index into its own pages.

---

## 🎯 Which layer should you use?

- **Most trading apps → [MT5Sugar](../MT5Sugar/MT5Sugar.API_Overview.md).** Points-based SL/TP, risk sizing and auto-normalization protect you from off-quote and invalid-volume rejections.
- **Custom execution → [MT5Service](../MT5Service/MT5Service.Overview.md).** Typed calls when you manage your own sizing/normalization.
- **Protocol-level control → [MT5Account](../MT5Account/MT5Account.Master.Overview.md).** Raw proto structures and streams.
