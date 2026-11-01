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
| `SwapRollover3Days`  | `BMT5_ENUM_DAY_OF_WEEK`               | Day of triple swap rollover           |

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
| `Bmt5SectorUndefined` | Undefined sector |
| `Bmt5SectorBasicMaterials` | Basic materials |
| `Bmt5SectorCommunicationServices` | Communication services |
| `Bmt5SectorConsumerCyclical` | Consumer cyclical |
| `Bmt5SectorConsumerDefensive` | Consumer defensive |
| `Bmt5SectorCurrency` | Currencies (Forex) |
| `Bmt5SectorCurrencyCrypto` | Cryptocurrencies |
| `Bmt5SectorEnergy` | Energy |
| `Bmt5SectorFinancial` | Finance |
| `Bmt5SectorHealthcare` | Healthcare |
| `Bmt5SectorIndustrials` | Industrials |
| `Bmt5SectorRealEstate` | Real estate |
| `Bmt5SectorTechnology` | Technology |
| `Bmt5SectorUtilities` | Utilities |

---

### `BMT5_ENUM_SYMBOL_INDUSTRY` (Industry Classifications - 152 values)

#### Basic Materials (15 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryUndefined` | Undefined |
| `Bmt5IndustryAgriculturalInputs` | Agricultural inputs |
| `Bmt5IndustryAluminium` | Aluminium |
| `Bmt5IndustryBuildingMaterials` | Building materials |
| `Bmt5IndustryChemicals` | Chemicals |
| `Bmt5IndustryCokingCoal` | Coking coal |
| `Bmt5IndustryCopper` | Copper |
| `Bmt5IndustryGold` | Gold |
| `Bmt5IndustryLumberWood` | Lumber and wood production |
| `Bmt5IndustryIndustrialMetals` | Other industrial metals and mining |
| `Bmt5IndustryPreciousMetals` | Other precious metals and mining |
| `Bmt5IndustryPaper` | Paper and paper products |
| `Bmt5IndustrySilver` | Silver |
| `Bmt5IndustrySpecialtyChemicals` | Specialty chemicals |
| `Bmt5IndustrySteel` | Steel |

#### Communication Services (7 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryAdvertising` | Advertising agencies |
| `Bmt5IndustryBroadcasting` | Broadcasting |
| `Bmt5IndustryGamingMultimedia` | Electronic gaming and multimedia |
| `Bmt5IndustryEntertainment` | Entertainment |
| `Bmt5IndustryInternetContent` | Internet content and information |
| `Bmt5IndustryPublishing` | Publishing |
| `Bmt5IndustryTelecom` | Telecom services |

#### Consumer Cyclical (23 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryApparelManufacturing` | Apparel manufacturing |
| `Bmt5IndustryApparelRetail` | Apparel retail |
| `Bmt5IndustryAutoManufacturers` | Auto manufacturers |
| `Bmt5IndustryAutoParts` | Auto parts |
| `Bmt5IndustryAutoDealership` | Auto and truck dealerships |
| `Bmt5IndustryDepartmentStores` | Department stores |
| `Bmt5IndustryFootwearAccessories` | Footwear and accessories |
| `Bmt5IndustryFurnishings` | Furnishing, fixtures and appliances |
| `Bmt5IndustryGambling` | Gambling |
| `Bmt5IndustryHomeImprovRetail` | Home improvement retail |
| `Bmt5IndustryInternetRetail` | Internet retail |
| `Bmt5IndustryLeisure` | Leisure |
| `Bmt5IndustryLodging` | Lodging |
| `Bmt5IndustryLuxuryGoods` | Luxury goods |
| `Bmt5IndustryPackagingContainers` | Packaging and containers |
| `Bmt5IndustryPersonalServices` | Personal services |
| `Bmt5IndustryRecreationalVehicles` | Recreational vehicles |
| `Bmt5IndustryResidentConstruction` | Residential construction |
| `Bmt5IndustryResortsCasinos` | Resorts and casinos |
| `Bmt5IndustryRestaurants` | Restaurants |
| `Bmt5IndustrySpecialtyRetail` | Specialty retail |
| `Bmt5IndustryTextileManufacturing` | Textile manufacturing |
| `Bmt5IndustryTravelServices` | Travel services |

#### Consumer Defensive (12 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryBeveragesBrewers` | Beverages - Brewers |
| `Bmt5IndustryBeveragesNonAlco` | Beverages - Non-alcoholic |
| `Bmt5IndustryBeveragesWineries` | Beverages - Wineries and distilleries |
| `Bmt5IndustryConfectioners` | Confectioners |
| `Bmt5IndustryDiscountStores` | Discount stores |
| `Bmt5IndustryEducationTrainig` | Education and training services |
| `Bmt5IndustryFarmProducts` | Farm products |
| `Bmt5IndustryFoodDistribution` | Food distribution |
| `Bmt5IndustryGroceryStores` | Grocery stores |
| `Bmt5IndustryHouseholdProducts` | Household and personal products |
| `Bmt5IndustryPackagedFoods` | Packaged foods |
| `Bmt5IndustryTobacco` | Tobacco |

#### Energy (8 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryOilGasDrilling` | Oil and gas drilling |
| `Bmt5IndustryOilGasEp` | Oil and gas extraction and processing |
| `Bmt5IndustryOilGasEquipment` | Oil and gas equipment and services |
| `Bmt5IndustryOilGasIntegrated` | Oil and gas integrated |
| `Bmt5IndustryOilGasMidstream` | Oil and gas midstream |
| `Bmt5IndustryOilGasRefining` | Oil and gas refining and marketing |
| `Bmt5IndustryThermalCoal` | Thermal coal |
| `Bmt5IndustryUranium` | Uranium |

#### Financial (19 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryExchangeTradedFund` | Exchange traded fund |
| `Bmt5IndustryAssetsManagement` | Assets management |
| `Bmt5IndustryBanksDiversified` | Banks - Diversified |
| `Bmt5IndustryBanksRegional` | Banks - Regional |
| `Bmt5IndustryCapitalMarkets` | Capital markets |
| `Bmt5IndustryCloseEndFundDebt` | Closed-End fund - Debt |
| `Bmt5IndustryCloseEndFundEquity` | Closed-end fund - Equity |
| `Bmt5IndustryCloseEndFundForeign` | Closed-end fund - Foreign |
| `Bmt5IndustryCreditServices` | Credit services |
| `Bmt5IndustryFinancialConglomerate` | Financial conglomerates |
| `Bmt5IndustryFinancialDataExchange` | Financial data and stock exchange |
| `Bmt5IndustryInsuranceBrokers` | Insurance brokers |
| `Bmt5IndustryInsuranceDiversified` | Insurance - Diversified |
| `Bmt5IndustryInsuranceLife` | Insurance - Life |
| `Bmt5IndustryInsuranceProperty` | Insurance - Property and casualty |
| `Bmt5IndustryInsuranceReinsurance` | Insurance - Reinsurance |
| `Bmt5IndustryInsuranceSpecialty` | Insurance - Specialty |
| `Bmt5IndustryMortgageFinance` | Mortgage finance |
| `Bmt5IndustryShellCompanies` | Shell companies |

#### Healthcare (11 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryBiotechnology` | Biotechnology |
| `Bmt5IndustryDiagnosticsResearch` | Diagnostics and research |
| `Bmt5IndustryDrugsManufacturers` | Drugs manufacturers - general |
| `Bmt5IndustryDrugsManufacturersSpec` | Drugs manufacturers - Specialty and generic |
| `Bmt5IndustryHealthcarePlans` | Healthcare plans |
| `Bmt5IndustryHealthInformation` | Health information services |
| `Bmt5IndustryMedicalFacilities` | Medical care facilities |
| `Bmt5IndustryMedicalDevices` | Medical devices |
| `Bmt5IndustryMedicalDistribution` | Medical distribution |
| `Bmt5IndustryMedicalInstruments` | Medical instruments and supplies |
| `Bmt5IndustryPharmRetailers` | Pharmaceutical retailers |

#### Industrials (25 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryAerospaceDefense` | Aerospace and defense |
| `Bmt5IndustryAirlines` | Airlines |
| `Bmt5IndustryAirportsServices` | Airports and air services |
| `Bmt5IndustryBuildingProducts` | Building products and equipment |
| `Bmt5IndustryBusinessEquipment` | Business equipment and supplies |
| `Bmt5IndustryConglomerates` | Conglomerates |
| `Bmt5IndustryConsultingServices` | Consulting services |
| `Bmt5IndustryElectricalEquipment` | Electrical equipment and parts |
| `Bmt5IndustryEngineeringConstruction` | Engineering and construction |
| `Bmt5IndustryFarmHeavyMachinery` | Farm and heavy construction machinery |
| `Bmt5IndustryIndustrialDistribution` | Industrial distribution |
| `Bmt5IndustryInfrastructureOperations` | Infrastructure operations |
| `Bmt5IndustryFreightLogistics` | Integrated freight and logistics |
| `Bmt5IndustryMarineShipping` | Marine shipping |
| `Bmt5IndustryMetalFabrication` | Metal fabrication |
| `Bmt5IndustryPollutionControl` | Pollution and treatment controls |
| `Bmt5IndustryRailroads` | Railroads |
| `Bmt5IndustryRentalLeasing` | Rental and leasing services |
| `Bmt5IndustrySecurityProtection` | Security and protection services |
| `Bmt5IndustrySpealityBusinessServices` | Specialty business services |
| `Bmt5IndustrySpealityMachinery` | Specialty industrial machinery |
| `Bmt5IndustryStuffingEmployment` | Staffing and employment services |
| `Bmt5IndustryToolsAccessories` | Tools and accessories |
| `Bmt5IndustryTrucking` | Trucking |
| `Bmt5IndustryWasteManagement` | Waste management |

#### Real Estate (12 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryRealEstateDevelopment` | Real estate - Development |
| `Bmt5IndustryRealEstateDiversified` | Real estate - Diversified |
| `Bmt5IndustryRealEstateServices` | Real estate services |
| `Bmt5IndustryReitDiversified` | REIT - Diversified |
| `Bmt5IndustryReitHealtcare` | REIT - Healthcare facilities |
| `Bmt5IndustryReitHotelMotel` | REIT - Hotel and motel |
| `Bmt5IndustryReitIndustrial` | REIT - Industrial |
| `Bmt5IndustryReitMortage` | REIT - Mortgage |
| `Bmt5IndustryReitOffice` | REIT - Office |
| `Bmt5IndustryReitResidental` | REIT - Residential |
| `Bmt5IndustryReitRetail` | REIT - Retail |
| `Bmt5IndustryReitSpeciality` | REIT - Specialty |

#### Technology (12 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryCommunicationEquipment` | Communication equipment |
| `Bmt5IndustryComputerHardware` | Computer hardware |
| `Bmt5IndustryConsumerElectronics` | Consumer electronics |
| `Bmt5IndustryElectronicComponents` | Electronic components |
| `Bmt5IndustryElectronicDistribution` | Electronics and computer distribution |
| `Bmt5IndustryItServices` | Information technology services |
| `Bmt5IndustryScientificInstruments` | Scientific and technical instruments |
| `Bmt5IndustrySemiconductorEquipment` | Semiconductor equipment and materials |
| `Bmt5IndustrySemiconductors` | Semiconductors |
| `Bmt5IndustrySoftwareApplication` | Software - Application |
| `Bmt5IndustrySoftwareInfrastructure` | Software - Infrastructure |
| `Bmt5IndustrySolar` | Solar |

#### Utilities (6 values)

| Enum Value | Description |
|------------|-------------|
| `Bmt5IndustryUtilitiesDiversified` | Utilities - Diversified |
| `Bmt5IndustryUtilitiesPowerproducers` | Utilities - Independent power producers |
| `Bmt5IndustryUtilitiesRenewable` | Utilities - Renewable |
| `Bmt5IndustryUtilitiesRegulatedElectric` | Utilities - Regulated electric |
| `Bmt5IndustryUtilitiesRegulatedGas` | Utilities - Regulated gas |
| `Bmt5IndustryUtilitiesRegulatedWater` | Utilities - Regulated water |

---

### `BMT5_ENUM_SYMBOL_CHART_MODE` (Chart Display Mode)

| Enum Value | Description |
|------------|-------------|
| `Bmt5SymbolChartModeBid` | Chart based on Bid prices |
| `Bmt5SymbolChartModeLast` | Chart based on Last deal prices |

---

### `BMT5_ENUM_SYMBOL_CALC_MODE` (Profit/Margin Calculation Mode)

| Enum Value | Description | Formula |
|------------|-------------|---------|
| `Bmt5SymbolCalcModeForex` | Forex mode | Margin: `Lots * Contract_Size / Leverage * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `Bmt5SymbolCalcModeForexNoLeverage` | Forex without leverage | Margin: `Lots * Contract_Size * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `Bmt5SymbolCalcModeFutures` | Futures | Margin: `Lots * InitialMargin * Margin_Rate`<br>Profit: `(close_price - open_price) * TickPrice / TickSize * Lots` |
| `Bmt5SymbolCalcModeCfd` | CFD | Margin: `Lots * ContractSize * MarketPrice * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `Bmt5SymbolCalcModeCfdindex` | CFD index | Margin: `(Lots * ContractSize * MarketPrice) * TickPrice / TickSize * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `Bmt5SymbolCalcModeCfdleverage` | CFD with leverage | Margin: `(Lots * ContractSize * MarketPrice) / Leverage * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `Bmt5SymbolCalcModeExchStocks` | Exchange stocks | Margin: `Lots * ContractSize * LastPrice * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `Bmt5SymbolCalcModeExchFutures` | Exchange futures | Margin: `Lots * InitialMargin * Margin_Rate` or `Lots * MaintenanceMargin * Margin_Rate`<br>Profit: `(close_price - open_price) * Lots * TickPrice / TickSize` |
| `Bmt5SymbolCalcModeExchFuturesForts` | FORTS futures | Complex formula with MarginDiscount |
| `Bmt5SymbolCalcModeExchBonds` | Exchange bonds | Margin: `Lots * ContractSize * FaceValue * open_price / 100`<br>Profit: `Lots * close_price * FaceValue * Contract_Size + AccruedInterest * Lots * ContractSize` |
| `Bmt5SymbolCalcModeExchStocksMoex` | MOEX stocks | Margin: `Lots * ContractSize * LastPrice * Margin_Rate`<br>Profit: `(close_price - open_price) * Contract_Size * Lots` |
| `Bmt5SymbolCalcModeExchBondsMoex` | MOEX bonds | Margin: `Lots * ContractSize * FaceValue * open_price / 100`<br>Profit: `Lots * close_price * FaceValue * Contract_Size + AccruedInterest * Lots * ContractSize` |
| `Bmt5SymbolCalcModeServCollateral` | Collateral | Instrument used as collateral; no margin charged and no profit/loss calculated for the position |

---

### `BMT5_ENUM_SYMBOL_TRADE_MODE` (Trading Permissions)

| Enum Value | Description |
|------------|-------------|
| `Bmt5SymbolTradeModeDisabled` | Trade is disabled for the symbol |
| `Bmt5SymbolTradeModeLongonly` | Allowed only long positions (BUY) |
| `Bmt5SymbolTradeModeShortonly` | Allowed only short positions (SELL) |
| `Bmt5SymbolTradeModeCloseonly` | Allowed only position close operations |
| `Bmt5SymbolTradeModeFull` | No trade restrictions |

---

### `BMT5_ENUM_SYMBOL_TRADE_EXECUTION` (Order Execution Mode)

| Enum Value | Description |
|------------|-------------|
| `Bmt5SymbolTradeExecutionRequest` | Execution by request |
| `Bmt5SymbolTradeExecutionInstant` | Instant execution |
| `Bmt5SymbolTradeExecutionMarket` | Market execution |
| `Bmt5SymbolTradeExecutionExchange` | Exchange execution |

---

### `BMT5_ENUM_SYMBOL_SWAP_MODE` (Swap Calculation Mode)

| Enum Value | Description |
|------------|-------------|
| `Bmt5SymbolSwapModeDisabled` | Swaps disabled (no swaps) |
| `Bmt5SymbolSwapModePoints` | Swaps charged in points |
| `Bmt5SymbolSwapModeCurrencySymbol` | Swaps charged in money in base currency of the symbol |
| `Bmt5SymbolSwapModeCurrencyMargin` | Swaps charged in money in margin currency of the symbol |
| `Bmt5SymbolSwapModeCurrencyDeposit` | Swaps charged in money, in client deposit currency |
| `Bmt5SymbolSwapModeCurrencyProfit` | Swaps charged in money in profit calculation currency |
| `Bmt5SymbolSwapModeInterestCurrent` | Swaps charged as annual interest from current price (360 days) |
| `Bmt5SymbolSwapModeInterestOpen` | Swaps charged as annual interest from open price (360 days) |
| `Bmt5SymbolSwapModeReopenCurrent` | Swaps by reopening positions at close price +/- points |
| `Bmt5SymbolSwapModeReopenBid` | Swaps by reopening positions at Bid price +/- points |

---

### `BMT5_ENUM_DAY_OF_WEEK` (Day of Week)

| Enum Value | Description |
|------------|-------------|
| `Bmt5Sunday` | Sunday |
| `Bmt5Monday` | Monday |
| `Bmt5Tuesday` | Tuesday |
| `Bmt5Wednesday` | Wednesday (often triple swap day) |
| `Bmt5Thursday` | Thursday |
| `Bmt5Friday` | Friday |
| `Bmt5Saturday` | Saturday |

---

### `BMT5_ENUM_SYMBOL_ORDER_GTC_MODE` (Order Lifetime Mode)

| Enum Value | Description |
|------------|-------------|
| `Bmt5SymbolOrdersGtc` | Pending orders and SL/TP valid until explicit cancellation (Good-Till-Cancelled) |
| `Bmt5SymbolOrdersDaily` | Orders valid during one trading day. All SL/TP and pending orders deleted at day end |
| `Bmt5SymbolOrdersDailyExcludingStops` | Only pending orders deleted at day end. SL/TP levels preserved |

---

### `BMT5_ENUM_SYMBOL_OPTION_MODE` (Option Exercise Type)

| Enum Value | Description |
|------------|-------------|
| `Bmt5SymbolOptionModeEuropean` | European option - can only be exercised on expiration date |
| `Bmt5SymbolOptionModeAmerican` | American option - can be exercised any trading day before expiry |

---

### `BMT5_ENUM_SYMBOL_OPTION_RIGHT` (Option Type)

| Enum Value | Description |
|------------|-------------|
| `Bmt5SymbolOptionRightCall` | Call option - right to BUY at strike price |
| `Bmt5SymbolOptionRightPut` | Put option - right to SELL at strike price |

---

### `BMT5_ENUM_ORDER_TYPE_FILLING` (Order Filling Policy)

| Enum Value | Description |
|------------|-------------|
| `Bmt5OrderFillingFok` | **Fill or Kill** - order must be executed in full or not at all |
| `Bmt5OrderFillingIoc` | **Immediate or Cancel** - execute max available volume, cancel remainder |
| `Bmt5OrderFillingBoc` | **Book or Cancel** - place order in order book (not implemented in most brokers) |
| `Bmt5OrderFillingReturn` | **Return** - partial fills allowed, remainder stays as pending order |

---

### `BMT5_ENUM_ORDER_TYPE` (Order Types)

| Enum Value | Description |
|------------|-------------|
| `Bmt5OrderTypeBuy` | Market Buy order |
| `Bmt5OrderTypeSell` | Market Sell order |
| `Bmt5OrderTypeBuyLimit` | Buy Limit pending order (buy below current price) |
| `Bmt5OrderTypeSellLimit` | Sell Limit pending order (sell above current price) |
| `Bmt5OrderTypeBuyStop` | Buy Stop pending order (buy above current price, breakout) |
| `Bmt5OrderTypeSellStop` | Sell Stop pending order (sell below current price, breakdown) |
| `Bmt5OrderTypeBuyStopLimit` | Buy Stop Limit - at stop price, place Buy Limit at StopLimit price |
| `Bmt5OrderTypeSellStopLimit` | Sell Stop Limit - at stop price, place Sell Limit at StopLimit price |
| `Bmt5OrderTypeCloseBy` | Close By - close position by opposite position |

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
        Console.WriteLine($"  Triple swap day: {s.SwapRollover3Days}");
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
