# ✅ Get Detailed Symbol Parameters (`SymbolParamsManyAsync`)

> **Request:** Get full information about symbol and its group for one or multiple symbols on **MT5**. Returns comprehensive symbol properties.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolParamsManyAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `SymbolParamsMany` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `SymbolParamsMany(SymbolParamsManyRequest) → SymbolParamsManyReply`
* **Low‑level client (generated):** `AccountHelper.AccountHelperClient.SymbolParamsMany(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolParamsManyData> SymbolParamsManyAsync(
            SymbolParamsManyRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolParamsManyRequest { symbol_name, sort_type, page_number, items_per_page }`


**Reply message:**

`SymbolParamsManyReply { data: SymbolParamsManyData }`

---

## 🔽 Input

| Parameter           | Type                       | Description                                               |
| ------------------- | -------------------------- | --------------------------------------------------------- |
| `request`           | `SymbolParamsManyRequest`  | Protobuf request with filtering and pagination            |
| `deadline`          | `DateTime?`                | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`        | Cooperative cancel for the call/retry loop                |

### `SymbolParamsManyRequest`

| Field           | Type                                 | Description                                                   |
| --------------- | ------------------------------------ | ------------------------------------------------------------- |
| `SymbolName`    | `string` (optional)                  | Filter by symbol name (e.g., `"EURUSD"`). Omit for all symbols |
| `SortType`      | `AH_SYMBOL_PARAMS_MANY_SORT_TYPE`    | Sort order (optional)                                         |
| `PageNumber`    | `int32` (optional)                   | Page number for pagination (0-based)                          |
| `ItemsPerPage`  | `int32` (optional)                   | Number of items per page (0 = all items)                      |

---

## ⬆️ Output — `SymbolParamsManyData`

| Field           | Type                         | Description                                |
| --------------- | ---------------------------- | ------------------------------------------ |
| `SymbolInfos`   | `List<SymbolParameters>`     | Array of detailed symbol parameters        |
| `SymbolsTotal`  | `int32`                      | Total number of symbols matching filter    |
| `PageNumber`    | `int32` (optional)           | Current page number                        |
| `ItemsPerPage`  | `int32` (optional)           | Items per page                             |

### `SymbolParameters` — Detailed symbol information (112 fields)

**Price fields:**

| Field          | Type     | Description                  |
| -------------- | -------- | ---------------------------- |
| `Name`         | `string` | Symbol name                  |
| `Bid`          | `double` | Current Bid price            |
| `BidHigh`      | `double` | Highest Bid of the day       |
| `BidLow`       | `double` | Lowest Bid of the day        |
| `Ask`          | `double` | Current Ask price            |
| `AskHigh`      | `double` | Highest Ask of the day       |
| `AskLow`       | `double` | Lowest Ask of the day        |
| `Last`         | `double` | Last deal price              |
| `LastHigh`     | `double` | Highest Last price of day    |
| `LastLow`      | `double` | Lowest Last price of day     |

**Volume fields:**

| Field             | Type     | Description                      |
| ----------------- | -------- | -------------------------------- |
| `VolumeReal`      | `double` | Real volume                      |
| `VolumeHighReal`  | `double` | Maximum real volume of the day   |
| `VolumeLowReal`   | `double` | Minimum real volume of the day   |
| `Volume`          | `int64`  | Volume (integer)                 |
| `VolumeHigh`      | `int64`  | Maximum volume of the day        |
| `VolumeLow`       | `int64`  | Minimum volume of the day        |

**Trade parameters:**

| Field                  | Type     | Description                        |
| ---------------------- | -------- | ---------------------------------- |
| `Point`                | `double` | Point size (minimum price change)  |
| `TradeTickValue`       | `double` | Tick value                         |
| `TradeTickValueProfit` | `double` | Tick value for profit calculation  |
| `TradeTickValueLoss`   | `double` | Tick value for loss calculation    |
| `TradeTickSize`        | `double` | Tick size                          |
| `TradeContractSize`    | `double` | Contract size                      |
| `VolumeMin`            | `double` | Minimum volume for a deal          |
| `VolumeMax`            | `double` | Maximum volume for a deal          |
| `VolumeStep`           | `double` | Volume step                        |
| `VolumeLimit`          | `double` | Maximum allowed aggregate volume   |

**Swap fields:**

| Field          | Type     | Description                      |
| -------------- | -------- | -------------------------------- |
| `SwapLong`     | `double` | Long swap value                  |
| `SwapShort`    | `double` | Short swap value                 |
| `SwapSunday`   | `double` | Sunday swap                      |
| `SwapMonday`   | `double` | Monday swap                      |
| `SwapTuesday`  | `double` | Tuesday swap                     |
| `SwapWednesday`| `double` | Wednesday swap (often triple)    |
| `SwapThursday` | `double` | Thursday swap                    |
| `SwapFriday`   | `double` | Friday swap                      |
| `SwapSaturday` | `double` | Saturday swap                    |

**Margin fields:**

| Field                 | Type     | Description                         |
| --------------------- | -------- | ----------------------------------- |
| `MarginInitial`       | `double` | Initial margin                      |
| `MarginMaintenance`   | `double` | Maintenance margin                  |
| `MarginHedged`        | `double` | Hedged margin                       |

**Session fields:**

| Field                      | Type     | Description                           |
| -------------------------- | -------- | ------------------------------------- |
| `SessionVolume`            | `double` | Total volume for current session      |
| `SessionTurnover`          | `double` | Total turnover for current session    |
| `SessionInterest`          | `double` | Total interest for current session    |
| `SessionBuyOrdersVolume`   | `double` | Buy orders volume in session          |
| `SessionSellOrdersVolume`  | `double` | Sell orders volume in session         |
| `SessionOpen`              | `double` | Session open price                    |
| `SessionClose`             | `double` | Session close price                   |
| `SessionAw`                | `double` | Average weighted price                |
| `SessionPriceSettlement`   | `double` | Settlement price                      |
| `SessionPriceLimitMin`     | `double` | Minimum price limit                   |
| `SessionPriceLimitMax`     | `double` | Maximum price limit                   |
| `SessionDeals`             | `int64`  | Number of deals in session            |
| `SessionBuyOrders`         | `int64`  | Number of buy orders in session       |
| `SessionSellOrders`        | `int64`  | Number of sell orders in session      |

**Options/Greeks fields:**

| Field               | Type     | Description                    |
| ------------------- | -------- | ------------------------------ |
| `OptionStrike`      | `double` | Strike price for options       |
| `PriceChange`       | `double` | Price change                   |
| `PriceVolatility`   | `double` | Volatility                     |
| `PriceTheoretical`  | `double` | Theoretical option price       |
| `PriceDelta`        | `double` | Option Delta                   |
| `PriceTheta`        | `double` | Option Theta                   |
| `PriceGamma`        | `double` | Option Gamma                   |
| `PriceVega`         | `double` | Option Vega                    |
| `PriceRho`          | `double` | Option Rho                     |
| `PriceOmega`        | `double` | Option Omega                   |
| `PriceSensitivity`  | `double` | Price sensitivity              |

**Symbol properties:**

| Field                  | Type                                  | Description                          |
| ---------------------- | ------------------------------------- | ------------------------------------ |
| `Sector`               | `BMT5_ENUM_SYMBOL_SECTOR`             | Sector (Basic Materials, Energy, etc) |
| `Industry`             | `BMT5_ENUM_SYMBOL_INDUSTRY`           | Industry classification               |
| `Custom`               | `bool`                                | Is custom symbol                      |
| `BackgroundColor`      | `string`                              | Background color                      |
| `ChartMode`            | `BMT5_ENUM_SYMBOL_CHART_MODE`         | Chart mode (Bid or Last)              |
| `Exist`                | `bool`                                | Symbol exists                         |
| `Select`               | `bool`                                | Symbol selected in MarketWatch        |
| `SubscriptionDelay`    | `int32`                               | Subscription delay in seconds         |
| `Visible`              | `bool`                                | Symbol visible in MarketWatch         |
| `Digits`               | `int32`                               | Digits after decimal point            |
| `SpreadFloat`          | `bool`                                | Floating spread                       |
| `Spread`               | `int32`                               | Current spread in points              |
| `TicksBookDepth`       | `int32`                               | Market depth                          |

**Time fields:**

| Field            | Type        | Description                     |
| ---------------- | ----------- | ------------------------------- |
| `Time`           | `Timestamp` | Last quote time                 |
| `TimeMsc`        | `int64`     | Last quote time in milliseconds |
| `StartTime`      | `Timestamp` | Symbol start time               |
| `ExpirationTime` | `Timestamp` | Symbol expiration time          |

**Trading modes:**

| Field                | Type                                  | Description                          |
| -------------------- | ------------------------------------- | ------------------------------------ |
| `TradeCalcMode`      | `BMT5_ENUM_SYMBOL_CALC_MODE`          | Profit/margin calculation mode        |
| `TradeMode`          | `BMT5_ENUM_SYMBOL_TRADE_MODE`         | Trading mode (Full, LongOnly, etc)    |
| `TradeStopsLevel`    | `int32`                               | Minimum distance for stops (points)   |
| `TradeFreezeLevel`   | `int32`                               | Freeze level (points)                 |
| `TradeExeMode`       | `BMT5_ENUM_SYMBOL_TRADE_EXECUTION`    | Execution mode                        |
| `SwapMode`           | `BMT5_ENUM_SYMBOL_SWAP_MODE`          | Swap calculation mode                 |
| `SwapRollover3days`  | `BMT5_ENUM_DAY_OF_WEEK`               | Day of triple swap rollover           |

**Order/Fill modes:**

| Field                  | Type                                  | Description                          |
| ---------------------- | ------------------------------------- | ------------------------------------ |
| `MarginHedgedUseLeg`   | `bool`                                | Use hedged margin for legs            |
| `ExpirationMode`       | `int32`                               | Allowed expiration modes (flags)      |
| `FillingMode`          | `List<BMT5_ENUM_ORDER_TYPE_FILLING>`  | Allowed filling modes                 |
| `OrderMode`            | `BMT5_ENUM_ORDER_TYPE`                | Allowed order types                   |
| `OrderGtcMode`         | `BMT5_ENUM_SYMBOL_ORDER_GTC_MODE`     | GTC mode                              |
| `OptionMode`           | `BMT5_ENUM_SYMBOL_OPTION_MODE`        | Option type (European/American)       |
| `OptionRight`          | `BMT5_ENUM_SYMBOL_OPTION_RIGHT`       | Option right (Call/Put)               |

**String properties:**

| Field             | Type     | Description                        |
| ----------------- | -------- | ---------------------------------- |
| `Basis`           | `string` | Underlying asset for derivatives   |
| `Category`        | `string` | Symbol category                    |
| `Country`         | `string` | Country                            |
| `SectorName`      | `string` | Sector name                        |
| `IndustryName`    | `string` | Industry name                      |
| `CurrencyBase`    | `string` | Base currency                      |
| `CurrencyProfit`  | `string` | Profit currency                    |
| `CurrencyMargin`  | `string` | Margin currency                    |
| `Bank`            | `string` | Liquidity provider                 |
| `SymDescription`  | `string` | Symbol description                 |
| `Exchange`        | `string` | Exchange name                      |
| `Formula`         | `string` | Custom symbol formula              |
| `Isin`            | `string` | ISIN code                          |
| `Page`            | `string` | Web page URL                       |
| `Path`            | `string` | Path in symbol tree                |

---

## 🧱 Related enums (from proto)

### `AH_SYMBOL_PARAMS_MANY_SORT_TYPE` (Sort Order)

| Enum Value | Description |
|------------|-------------|
| `AhParamsManySortTypeSymbolNameAsc` | Sort by symbol name ascending (A-Z) |
| `AhParamsManySortTypeSymbolNameDesc` | Sort by symbol name descending (Z-A) |
| `AhParamsManySortTypeMqlIndexAsc` | Sort by MQL index ascending |
| `AhParamsManySortTypeMqlIndexDesc` | Sort by MQL index descending |

---

### `BMT5_ENUM_SYMBOL_SECTOR` (Market Sectors)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SECTOR_UNDEFINED` | Undefined sector |
| `BMT5_SECTOR_BASIC_MATERIALS` | Basic materials |
| `BMT5_SECTOR_COMMUNICATION_SERVICES` | Communication services |
| `BMT5_SECTOR_CONSUMER_CYCLICAL` | Consumer cyclical |
| `BMT5_SECTOR_CONSUMER_DEFENSIVE` | Consumer defensive |
| `BMT5_SECTOR_CURRENCY` | Currencies (Forex) |
| `BMT5_SECTOR_CURRENCY_CRYPTO` | Cryptocurrencies |
| `BMT5_SECTOR_ENERGY` | Energy |
| `BMT5_SECTOR_FINANCIAL` | Finance |
| `BMT5_SECTOR_HEALTHCARE` | Healthcare |
| `BMT5_SECTOR_INDUSTRIALS` | Industrials |
| `BMT5_SECTOR_REAL_ESTATE` | Real estate |
| `BMT5_SECTOR_TECHNOLOGY` | Technology |
| `BMT5_SECTOR_UTILITIES` | Utilities |

---

### `BMT5_ENUM_SYMBOL_INDUSTRY` (Industry Classifications - 152 values)

#### Basic Materials (15 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_UNDEFINED` | Undefined |
| `BMT5_INDUSTRY_AGRICULTURAL_INPUTS` | Agricultural inputs |
| `BMT5_INDUSTRY_ALUMINIUM` | Aluminium |
| `BMT5_INDUSTRY_BUILDING_MATERIALS` | Building materials |
| `BMT5_INDUSTRY_CHEMICALS` | Chemicals |
| `BMT5_INDUSTRY_COKING_COAL` | Coking coal |
| `BMT5_INDUSTRY_COPPER` | Copper |
| `BMT5_INDUSTRY_GOLD` | Gold |
| `BMT5_INDUSTRY_LUMBER_WOOD` | Lumber and wood production |
| `BMT5_INDUSTRY_INDUSTRIAL_METALS` | Other industrial metals and mining |
| `BMT5_INDUSTRY_PRECIOUS_METALS` | Other precious metals and mining |
| `BMT5_INDUSTRY_PAPER` | Paper and paper products |
| `BMT5_INDUSTRY_SILVER` | Silver |
| `BMT5_INDUSTRY_SPECIALTY_CHEMICALS` | Specialty chemicals |
| `BMT5_INDUSTRY_STEEL` | Steel |

#### Communication Services (7 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_ADVERTISING` | Advertising agencies |
| `BMT5_INDUSTRY_BROADCASTING` | Broadcasting |
| `BMT5_INDUSTRY_GAMING_MULTIMEDIA` | Electronic gaming and multimedia |
| `BMT5_INDUSTRY_ENTERTAINMENT` | Entertainment |
| `BMT5_INDUSTRY_INTERNET_CONTENT` | Internet content and information |
| `BMT5_INDUSTRY_PUBLISHING` | Publishing |
| `BMT5_INDUSTRY_TELECOM` | Telecom services |

#### Consumer Cyclical (23 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_APPAREL_MANUFACTURING` | Apparel manufacturing |
| `BMT5_INDUSTRY_APPAREL_RETAIL` | Apparel retail |
| `BMT5_INDUSTRY_AUTO_MANUFACTURERS` | Auto manufacturers |
| `BMT5_INDUSTRY_AUTO_PARTS` | Auto parts |
| `BMT5_INDUSTRY_AUTO_DEALERSHIP` | Auto and truck dealerships |
| `BMT5_INDUSTRY_DEPARTMENT_STORES` | Department stores |
| `BMT5_INDUSTRY_FOOTWEAR_ACCESSORIES` | Footwear and accessories |
| `BMT5_INDUSTRY_FURNISHINGS` | Furnishing, fixtures and appliances |
| `BMT5_INDUSTRY_GAMBLING` | Gambling |
| `BMT5_INDUSTRY_HOME_IMPROV_RETAIL` | Home improvement retail |
| `BMT5_INDUSTRY_INTERNET_RETAIL` | Internet retail |
| `BMT5_INDUSTRY_LEISURE` | Leisure |
| `BMT5_INDUSTRY_LODGING` | Lodging |
| `BMT5_INDUSTRY_LUXURY_GOODS` | Luxury goods |
| `BMT5_INDUSTRY_PACKAGING_CONTAINERS` | Packaging and containers |
| `BMT5_INDUSTRY_PERSONAL_SERVICES` | Personal services |
| `BMT5_INDUSTRY_RECREATIONAL_VEHICLES` | Recreational vehicles |
| `BMT5_INDUSTRY_RESIDENT_CONSTRUCTION` | Residential construction |
| `BMT5_INDUSTRY_RESORTS_CASINOS` | Resorts and casinos |
| `BMT5_INDUSTRY_RESTAURANTS` | Restaurants |
| `BMT5_INDUSTRY_SPECIALTY_RETAIL` | Specialty retail |
| `BMT5_INDUSTRY_TEXTILE_MANUFACTURING` | Textile manufacturing |
| `BMT5_INDUSTRY_TRAVEL_SERVICES` | Travel services |

#### Consumer Defensive (12 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_BEVERAGES_BREWERS` | Beverages - Brewers |
| `BMT5_INDUSTRY_BEVERAGES_NON_ALCO` | Beverages - Non-alcoholic |
| `BMT5_INDUSTRY_BEVERAGES_WINERIES` | Beverages - Wineries and distilleries |
| `BMT5_INDUSTRY_CONFECTIONERS` | Confectioners |
| `BMT5_INDUSTRY_DISCOUNT_STORES` | Discount stores |
| `BMT5_INDUSTRY_EDUCATION_TRAINIG` | Education and training services |
| `BMT5_INDUSTRY_FARM_PRODUCTS` | Farm products |
| `BMT5_INDUSTRY_FOOD_DISTRIBUTION` | Food distribution |
| `BMT5_INDUSTRY_GROCERY_STORES` | Grocery stores |
| `BMT5_INDUSTRY_HOUSEHOLD_PRODUCTS` | Household and personal products |
| `BMT5_INDUSTRY_PACKAGED_FOODS` | Packaged foods |
| `BMT5_INDUSTRY_TOBACCO` | Tobacco |

#### Energy (8 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_OIL_GAS_DRILLING` | Oil and gas drilling |
| `BMT5_INDUSTRY_OIL_GAS_EP` | Oil and gas extraction and processing |
| `BMT5_INDUSTRY_OIL_GAS_EQUIPMENT` | Oil and gas equipment and services |
| `BMT5_INDUSTRY_OIL_GAS_INTEGRATED` | Oil and gas integrated |
| `BMT5_INDUSTRY_OIL_GAS_MIDSTREAM` | Oil and gas midstream |
| `BMT5_INDUSTRY_OIL_GAS_REFINING` | Oil and gas refining and marketing |
| `BMT5_INDUSTRY_THERMAL_COAL` | Thermal coal |
| `BMT5_INDUSTRY_URANIUM` | Uranium |

#### Financial (19 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_EXCHANGE_TRADED_FUND` | Exchange traded fund |
| `BMT5_INDUSTRY_ASSETS_MANAGEMENT` | Assets management |
| `BMT5_INDUSTRY_BANKS_DIVERSIFIED` | Banks - Diversified |
| `BMT5_INDUSTRY_BANKS_REGIONAL` | Banks - Regional |
| `BMT5_INDUSTRY_CAPITAL_MARKETS` | Capital markets |
| `BMT5_INDUSTRY_CLOSE_END_FUND_DEBT` | Closed-End fund - Debt |
| `BMT5_INDUSTRY_CLOSE_END_FUND_EQUITY` | Closed-end fund - Equity |
| `BMT5_INDUSTRY_CLOSE_END_FUND_FOREIGN` | Closed-end fund - Foreign |
| `BMT5_INDUSTRY_CREDIT_SERVICES` | Credit services |
| `BMT5_INDUSTRY_FINANCIAL_CONGLOMERATE` | Financial conglomerates |
| `BMT5_INDUSTRY_FINANCIAL_DATA_EXCHANGE` | Financial data and stock exchange |
| `BMT5_INDUSTRY_INSURANCE_BROKERS` | Insurance brokers |
| `BMT5_INDUSTRY_INSURANCE_DIVERSIFIED` | Insurance - Diversified |
| `BMT5_INDUSTRY_INSURANCE_LIFE` | Insurance - Life |
| `BMT5_INDUSTRY_INSURANCE_PROPERTY` | Insurance - Property and casualty |
| `BMT5_INDUSTRY_INSURANCE_REINSURANCE` | Insurance - Reinsurance |
| `BMT5_INDUSTRY_INSURANCE_SPECIALTY` | Insurance - Specialty |
| `BMT5_INDUSTRY_MORTGAGE_FINANCE` | Mortgage finance |
| `BMT5_INDUSTRY_SHELL_COMPANIES` | Shell companies |

#### Healthcare (11 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_BIOTECHNOLOGY` | Biotechnology |
| `BMT5_INDUSTRY_DIAGNOSTICS_RESEARCH` | Diagnostics and research |
| `BMT5_INDUSTRY_DRUGS_MANUFACTURERS` | Drugs manufacturers - general |
| `BMT5_INDUSTRY_DRUGS_MANUFACTURERS_SPEC` | Drugs manufacturers - Specialty and generic |
| `BMT5_INDUSTRY_HEALTHCARE_PLANS` | Healthcare plans |
| `BMT5_INDUSTRY_HEALTH_INFORMATION` | Health information services |
| `BMT5_INDUSTRY_MEDICAL_FACILITIES` | Medical care facilities |
| `BMT5_INDUSTRY_MEDICAL_DEVICES` | Medical devices |
| `BMT5_INDUSTRY_MEDICAL_DISTRIBUTION` | Medical distribution |
| `BMT5_INDUSTRY_MEDICAL_INSTRUMENTS` | Medical instruments and supplies |
| `BMT5_INDUSTRY_PHARM_RETAILERS` | Pharmaceutical retailers |

#### Industrials (25 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_AEROSPACE_DEFENSE` | Aerospace and defense |
| `BMT5_INDUSTRY_AIRLINES` | Airlines |
| `BMT5_INDUSTRY_AIRPORTS_SERVICES` | Airports and air services |
| `BMT5_INDUSTRY_BUILDING_PRODUCTS` | Building products and equipment |
| `BMT5_INDUSTRY_BUSINESS_EQUIPMENT` | Business equipment and supplies |
| `BMT5_INDUSTRY_CONGLOMERATES` | Conglomerates |
| `BMT5_INDUSTRY_CONSULTING_SERVICES` | Consulting services |
| `BMT5_INDUSTRY_ELECTRICAL_EQUIPMENT` | Electrical equipment and parts |
| `BMT5_INDUSTRY_ENGINEERING_CONSTRUCTION` | Engineering and construction |
| `BMT5_INDUSTRY_FARM_HEAVY_MACHINERY` | Farm and heavy construction machinery |
| `BMT5_INDUSTRY_INDUSTRIAL_DISTRIBUTION` | Industrial distribution |
| `BMT5_INDUSTRY_INFRASTRUCTURE_OPERATIONS` | Infrastructure operations |
| `BMT5_INDUSTRY_FREIGHT_LOGISTICS` | Integrated freight and logistics |
| `BMT5_INDUSTRY_MARINE_SHIPPING` | Marine shipping |
| `BMT5_INDUSTRY_METAL_FABRICATION` | Metal fabrication |
| `BMT5_INDUSTRY_POLLUTION_CONTROL` | Pollution and treatment controls |
| `BMT5_INDUSTRY_RAILROADS` | Railroads |
| `BMT5_INDUSTRY_RENTAL_LEASING` | Rental and leasing services |
| `BMT5_INDUSTRY_SECURITY_PROTECTION` | Security and protection services |
| `BMT5_INDUSTRY_SPEALITY_BUSINESS_SERVICES` | Specialty business services |
| `BMT5_INDUSTRY_SPEALITY_MACHINERY` | Specialty industrial machinery |
| `BMT5_INDUSTRY_STUFFING_EMPLOYMENT` | Staffing and employment services |
| `BMT5_INDUSTRY_TOOLS_ACCESSORIES` | Tools and accessories |
| `BMT5_INDUSTRY_TRUCKING` | Trucking |
| `BMT5_INDUSTRY_WASTE_MANAGEMENT` | Waste management |

#### Real Estate (12 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_REAL_ESTATE_DEVELOPMENT` | Real estate - Development |
| `BMT5_INDUSTRY_REAL_ESTATE_DIVERSIFIED` | Real estate - Diversified |
| `BMT5_INDUSTRY_REAL_ESTATE_SERVICES` | Real estate services |
| `BMT5_INDUSTRY_REIT_DIVERSIFIED` | REIT - Diversified |
| `BMT5_INDUSTRY_REIT_HEALTCARE` | REIT - Healthcare facilities |
| `BMT5_INDUSTRY_REIT_HOTEL_MOTEL` | REIT - Hotel and motel |
| `BMT5_INDUSTRY_REIT_INDUSTRIAL` | REIT - Industrial |
| `BMT5_INDUSTRY_REIT_MORTAGE` | REIT - Mortgage |
| `BMT5_INDUSTRY_REIT_OFFICE` | REIT - Office |
| `BMT5_INDUSTRY_REIT_RESIDENTAL` | REIT - Residential |
| `BMT5_INDUSTRY_REIT_RETAIL` | REIT - Retail |
| `BMT5_INDUSTRY_REIT_SPECIALITY` | REIT - Specialty |

#### Technology (12 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_COMMUNICATION_EQUIPMENT` | Communication equipment |
| `BMT5_INDUSTRY_COMPUTER_HARDWARE` | Computer hardware |
| `BMT5_INDUSTRY_CONSUMER_ELECTRONICS` | Consumer electronics |
| `BMT5_INDUSTRY_ELECTRONIC_COMPONENTS` | Electronic components |
| `BMT5_INDUSTRY_ELECTRONIC_DISTRIBUTION` | Electronics and computer distribution |
| `BMT5_INDUSTRY_IT_SERVICES` | Information technology services |
| `BMT5_INDUSTRY_SCIENTIFIC_INSTRUMENTS` | Scientific and technical instruments |
| `BMT5_INDUSTRY_SEMICONDUCTOR_EQUIPMENT` | Semiconductor equipment and materials |
| `BMT5_INDUSTRY_SEMICONDUCTORS` | Semiconductors |
| `BMT5_INDUSTRY_SOFTWARE_APPLICATION` | Software - Application |
| `BMT5_INDUSTRY_SOFTWARE_INFRASTRUCTURE` | Software - Infrastructure |
| `BMT5_INDUSTRY_SOLAR` | Solar |

#### Utilities (6 values)

| Enum Value | Description |
|------------|-------------|
| `BMT5_INDUSTRY_UTILITIES_DIVERSIFIED` | Utilities - Diversified |
| `BMT5_INDUSTRY_UTILITIES_POWERPRODUCERS` | Utilities - Independent power producers |
| `BMT5_INDUSTRY_UTILITIES_RENEWABLE` | Utilities - Renewable |
| `BMT5_INDUSTRY_UTILITIES_REGULATED_ELECTRIC` | Utilities - Regulated electric |
| `BMT5_INDUSTRY_UTILITIES_REGULATED_GAS` | Utilities - Regulated gas |
| `BMT5_INDUSTRY_UTILITIES_REGULATED_WATER` | Utilities - Regulated water |

---

### `BMT5_ENUM_SYMBOL_CHART_MODE` (Chart Display Mode)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SYMBOL_CHART_MODE_BID` | Chart based on Bid prices |
| `BMT5_SYMBOL_CHART_MODE_LAST` | Chart based on Last deal prices |

---

### `BMT5_ENUM_SYMBOL_CALC_MODE` (Profit/Margin Calculation Mode)

| Enum Value | Description | Formula |
|------------|-------------|---------|
| `BMT5_SYMBOL_CALC_MODE_FOREX` | Forex mode | Margin: `Lots * Contract_Size / Leverage * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `BMT5_SYMBOL_CALC_MODE_FOREX_NO_LEVERAGE` | Forex without leverage | Margin: `Lots * Contract_Size * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `BMT5_SYMBOL_CALC_MODE_FUTURES` | Futures | Margin: `Lots * InitialMargin * Margin_Rate`<br>Profit: `(close_price - open_price) * TickPrice / TickSize * Lots` |
| `BMT5_SYMBOL_CALC_MODE_CFD` | CFD | Margin: `Lots * ContractSize * MarketPrice * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `BMT5_SYMBOL_CALC_MODE_CFDINDEX` | CFD index | Margin: `(Lots * ContractSize * MarketPrice) * TickPrice / TickSize * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `BMT5_SYMBOL_CALC_MODE_CFDLEVERAGE` | CFD with leverage | Margin: `(Lots * ContractSize * MarketPrice) / Leverage * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `BMT5_SYMBOL_CALC_MODE_EXCH_STOCKS` | Exchange stocks | Margin: `Lots * ContractSize * LastPrice * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `BMT5_SYMBOL_CALC_MODE_EXCH_FUTURES` | Exchange futures | Margin: `Lots * InitialMargin * Margin_Rate` or `Lots * MaintenanceMargin * Margin_Rate`<br>Profit: `(close_price - open_price) * Lots * TickPrice / TickSize` |
| `BMT5_SYMBOL_CALC_MODE_EXCH_FUTURES_FORTS` | FORTS futures | Complex formula with MarginDiscount |
| `BMT5_SYMBOL_CALC_MODE_EXCH_BONDS` | Exchange bonds | Margin: `Lots * ContractSize * FaceValue * open_price / 100`<br>Profit: `Lots * close_price * FaceValue * Contract_Size + AccruedInterest * Lots * ContractSize` |
| `BMT5_SYMBOL_CALC_MODE_EXCH_STOCKS_MOEX` | MOEX stocks | Margin: `Lots * ContractSize * LastPrice * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `BMT5_SYMBOL_CALC_MODE_EXCH_BONDS_MOEX` | MOEX bonds | Margin: `Lots * ContractSize * FaceValue * open_price / 100`<br>Profit: `Lots * close_price * FaceValue * Contract_Size + AccruedInterest * Lots * ContractSize` |

---

### `BMT5_ENUM_SYMBOL_TRADE_MODE` (Trading Permissions)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SYMBOL_TRADE_MODE_DISABLED` | Trade is disabled for the symbol |
| `BMT5_SYMBOL_TRADE_MODE_LONGONLY` | Allowed only long positions (BUY) |
| `BMT5_SYMBOL_TRADE_MODE_SHORTONLY` | Allowed only short positions (SELL) |
| `BMT5_SYMBOL_TRADE_MODE_CLOSEONLY` | Allowed only position close operations |
| `BMT5_SYMBOL_TRADE_MODE_FULL` | No trade restrictions |

---

### `BMT5_ENUM_SYMBOL_TRADE_EXECUTION` (Order Execution Mode)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SYMBOL_TRADE_EXECUTION_REQUEST` | Execution by request |
| `BMT5_SYMBOL_TRADE_EXECUTION_INSTANT` | Instant execution |
| `BMT5_SYMBOL_TRADE_EXECUTION_MARKET` | Market execution |
| `BMT5_SYMBOL_TRADE_EXECUTION_EXCHANGE` | Exchange execution |

---

### `BMT5_ENUM_SYMBOL_SWAP_MODE` (Swap Calculation Mode)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SYMBOL_SWAP_MODE_DISABLED` | Swaps disabled (no swaps) |
| `BMT5_SYMBOL_SWAP_MODE_POINTS` | Swaps charged in points |
| `BMT5_SYMBOL_SWAP_MODE_CURRENCY_SYMBOL` | Swaps charged in money in base currency of the symbol |
| `BMT5_SYMBOL_SWAP_MODE_CURRENCY_MARGIN` | Swaps charged in money in margin currency of the symbol |
| `BMT5_SYMBOL_SWAP_MODE_CURRENCY_DEPOSIT` | Swaps charged in money, in client deposit currency |
| `BMT5_SYMBOL_SWAP_MODE_CURRENCY_PROFIT` | Swaps charged in money in profit calculation currency |
| `BMT5_SYMBOL_SWAP_MODE_INTEREST_CURRENT` | Swaps charged as annual interest from current price (360 days) |
| `BMT5_SYMBOL_SWAP_MODE_INTEREST_OPEN` | Swaps charged as annual interest from open price (360 days) |
| `BMT5_SYMBOL_SWAP_MODE_REOPEN_CURRENT` | Swaps by reopening positions at close price +/- points |
| `BMT5_SYMBOL_SWAP_MODE_REOPEN_BID` | Swaps by reopening positions at Bid price +/- points |

---

### `BMT5_ENUM_DAY_OF_WEEK` (Day of Week)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SUNDAY` | Sunday |
| `BMT5_MONDAY` | Monday |
| `BMT5_TUESDAY` | Tuesday |
| `BMT5_WEDNESDAY` | Wednesday (often triple swap day) |
| `BMT5_THURSDAY` | Thursday |
| `BMT5_FRIDAY` | Friday |
| `BMT5_SATURDAY` | Saturday |

---

### `BMT5_ENUM_SYMBOL_ORDER_GTC_MODE` (Order Lifetime Mode)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SYMBOL_ORDERS_GTC` | Pending orders and SL/TP valid until explicit cancellation (Good-Till-Cancelled) |
| `BMT5_SYMBOL_ORDERS_DAILY` | Orders valid during one trading day. All SL/TP and pending orders deleted at day end |
| `BMT5_SYMBOL_ORDERS_DAILY_EXCLUDING_STOPS` | Only pending orders deleted at day end. SL/TP levels preserved |

---

### `BMT5_ENUM_SYMBOL_OPTION_MODE` (Option Exercise Type)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SYMBOL_OPTION_MODE_EUROPEAN` | European option - can only be exercised on expiration date |
| `BMT5_SYMBOL_OPTION_MODE_AMERICAN` | American option - can be exercised any trading day before expiry |

---

### `BMT5_ENUM_SYMBOL_OPTION_RIGHT` (Option Type)

| Enum Value | Description |
|------------|-------------|
| `BMT5_SYMBOL_OPTION_RIGHT_CALL` | Call option - right to BUY at strike price |
| `BMT5_SYMBOL_OPTION_RIGHT_PUT` | Put option - right to SELL at strike price |

---

### `BMT5_ENUM_ORDER_TYPE_FILLING` (Order Filling Policy)

| Enum Value | Description |
|------------|-------------|
| `BMT5_ORDER_FILLING_FOK` | **Fill or Kill** - order must be executed in full or not at all |
| `BMT5_ORDER_FILLING_IOC` | **Immediate or Cancel** - execute max available volume, cancel remainder |
| `BMT5_ORDER_FILLING_BOC` | **Book or Cancel** - place order in order book (not implemented in most brokers) |
| `BMT5_ORDER_FILLING_RETURN` | **Return** - partial fills allowed, remainder stays as pending order |

---

### `BMT5_ENUM_ORDER_TYPE` (Order Types)

| Enum Value | Description |
|------------|-------------|
| `BMT5_ORDER_TYPE_BUY` | Market Buy order |
| `BMT5_ORDER_TYPE_SELL` | Market Sell order |
| `BMT5_ORDER_TYPE_BUY_LIMIT` | Buy Limit pending order (buy below current price) |
| `BMT5_ORDER_TYPE_SELL_LIMIT` | Sell Limit pending order (sell above current price) |
| `BMT5_ORDER_TYPE_BUY_STOP` | Buy Stop pending order (buy above current price, breakout) |
| `BMT5_ORDER_TYPE_SELL_STOP` | Sell Stop pending order (sell below current price, breakdown) |
| `BMT5_ORDER_TYPE_BUY_STOP_LIMIT` | Buy Stop Limit - at stop price, place Buy Limit at StopLimit price |
| `BMT5_ORDER_TYPE_SELL_STOP_LIMIT` | Sell Stop Limit - at stop price, place Sell Limit at StopLimit price |
| `BMT5_ORDER_TYPE_CLOSE_BY` | Close By - close position by opposite position |

---

## 💬 Just the essentials

* **What it is.** Returns comprehensive symbol information (112 fields) for one or multiple symbols. Single endpoint for all symbol properties.
* **Why you need it.** Get complete symbol specifications, analyze trading conditions, compare multiple symbols, access all symbol metadata in one call.
* **Sanity check.** If `SymbolsTotal > 0` → data available. Check `SymbolInfos.Count` for actual returned symbols. Use pagination for large result sets.

---

## 🎯 Purpose

Use it to get comprehensive symbol data:

* Get all symbol properties in one call.
* Compare trading conditions across symbols.
* Analyze contract specifications.
* Filter and sort symbols by various criteria.
* Access swap schedules, margins, session data.

---

## 🧩 Notes & Tips

* **Comprehensive data:** Returns 112 fields per symbol. Use only what you need to reduce processing overhead.
* **Pagination:** Use `PageNumber` and `ItemsPerPage` for large symbol lists to reduce memory usage.
* **Filtering:** Set `SymbolName` to get specific symbol. Omit to get all symbols.
* **Sorting:** Use `SortType` to order results by name or MQL index.
* **Performance:** Requesting all symbols can be slow. Filter when possible.
* **Null values:** Some fields may be 0 or empty if broker doesn't provide that data.

---

## 🔗 Usage Examples

### 1) Get all parameters for single symbol

```csharp
// acc — connected MT5Account

var result = await acc.SymbolParamsManyAsync(new SymbolParamsManyRequest
{
    SymbolName = "EURUSD"
});

if (result.SymbolInfos.Count > 0)
{
    var symbol = result.SymbolInfos[0];

    Console.WriteLine($"Symbol: {symbol.Name}");
    Console.WriteLine($"Bid: {symbol.Bid}, Ask: {symbol.Ask}");
    Console.WriteLine($"Spread: {symbol.Spread} points");
    Console.WriteLine($"Contract size: {symbol.TradeContractSize}");
    Console.WriteLine($"Min volume: {symbol.VolumeMin}, Max: {symbol.VolumeMax}, Step: {symbol.VolumeStep}");
    Console.WriteLine($"Stops level: {symbol.TradeStopsLevel} points");
    Console.WriteLine($"Swap long: {symbol.SwapLong}, short: {symbol.SwapShort}");
    Console.WriteLine($"Digits: {symbol.Digits}");
}
```

---

### 2) Get all available symbols (paginated)

```csharp
int pageSize = 50;
int currentPage = 0;

while (true)
{
    var result = await acc.SymbolParamsManyAsync(new SymbolParamsManyRequest
    {
        PageNumber = currentPage,
        ItemsPerPage = pageSize,
        SortType = AH_SYMBOL_PARAMS_MANY_SORT_TYPE.AhParamsManySortTypeSymbolNameAsc
    });

    Console.WriteLine($"\nPage {currentPage + 1} ({result.SymbolInfos.Count} symbols):");

    foreach (var symbol in result.SymbolInfos)
    {
        Console.WriteLine($"  {symbol.Name,-15} Bid: {symbol.Bid,10:F5}  Ask: {symbol.Ask,10:F5}  Spread: {symbol.Spread,3}");
    }

    // Check if more pages
    if ((currentPage + 1) * pageSize >= result.SymbolsTotal)
        break;

    currentPage++;
}

Console.WriteLine($"\nTotal symbols: {result.SymbolsTotal}");
```

---

### 3) Find symbols with lowest spreads

```csharp
var result = await acc.SymbolParamsManyAsync(new SymbolParamsManyRequest());

var lowSpreadSymbols = result.SymbolInfos
    .Where(s => s.Spread > 0) // Exclude symbols with no spread data
    .OrderBy(s => s.Spread)
    .Take(10);

Console.WriteLine("Top 10 symbols with lowest spreads:\n");

foreach (var symbol in lowSpreadSymbols)
{
    Console.WriteLine($"{symbol.Name,-15} Spread: {symbol.Spread,3} points ({symbol.Spread * symbol.Point:F5})");
}
```

---

### 4) Analyze swap conditions

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };

Console.WriteLine("Swap analysis:\n");

foreach (var symbolName in symbols)
{
    var result = await acc.SymbolParamsManyAsync(new SymbolParamsManyRequest
    {
        SymbolName = symbolName
    });

    if (result.SymbolInfos.Count > 0)
    {
        var s = result.SymbolInfos[0];

        Console.WriteLine($"{s.Name}:");
        Console.WriteLine($"  Long swap:  {s.SwapLong:F2}");
        Console.WriteLine($"  Short swap: {s.SwapShort:F2}");
        Console.WriteLine($"  Triple swap day: {s.SwapRollover3days}");
        Console.WriteLine($"  Swap mode: {s.SwapMode}");
        Console.WriteLine();
    }
}
```

---

### 5) Compare trading conditions across symbols

```csharp
var result = await acc.SymbolParamsManyAsync(new SymbolParamsManyRequest());

// Filter major Forex pairs
var forexPairs = result.SymbolInfos
    .Where(s => s.Name.Contains("USD") && s.Name.Length == 6)
    .ToList();

Console.WriteLine("Forex pairs trading conditions:\n");
Console.WriteLine($"{"Symbol",-10} {"Spread",-8} {"Min Lot",-10} {"Max Lot",-10} {"Stop Level",-12}");
Console.WriteLine(new string('-', 60));

foreach (var symbol in forexPairs.OrderBy(s => s.Name))
{
    Console.WriteLine($"{symbol.Name,-10} {symbol.Spread,-8} {symbol.VolumeMin,-10:F2} {symbol.VolumeMax,-10:F2} {symbol.TradeStopsLevel,-12}");
}
```

---

### 6) Get session information for symbol

```csharp
var result = await acc.SymbolParamsManyAsync(new SymbolParamsManyRequest
{
    SymbolName = "BTCUSD"
});

if (result.SymbolInfos.Count > 0)
{
    var s = result.SymbolInfos[0];

    Console.WriteLine($"Session info for {s.Name}:\n");
    Console.WriteLine($"Session volume: {s.SessionVolume}");
    Console.WriteLine($"Session turnover: {s.SessionTurnover}");
    Console.WriteLine($"Session deals: {s.SessionDeals}");
    Console.WriteLine($"Session buy orders: {s.SessionBuyOrders}");
    Console.WriteLine($"Session sell orders: {s.SessionSellOrders}");
    Console.WriteLine($"Session open: {s.SessionOpen}");
    Console.WriteLine($"Session close: {s.SessionClose}");
    Console.WriteLine($"Buy volume: {s.SessionBuyOrdersVolume}");
    Console.WriteLine($"Sell volume: {s.SessionSellOrdersVolume}");
}
```
