# Ваш первый проект с нуля

> **Быстрый старт** - создайте свой собственный MT5 торговый проект за 10 минут, используя только NuGet пакет MetaRPC.MT5

---

## Для кого этот гайд?

Этот документ предназначен для тех, кто хочет:

- **Быстро начать** писать код для MT5 в своем проекте
- **Не клонировать** весь репозиторий CSharpMT5
- **Создать проект с нуля** и подключить минимальные зависимости
- **Написать первый метод** и увидеть результат немедленно

**Разница между этим гайдом и Getting_Started.md:**

| Getting Started | Your First Project (этот гайд) |
|----------------|--------------------------------|
| Клонируете готовый репозиторий | Создаете проект с нуля |
| Изучаете архитектуру и примеры | Сразу пишете работающий код |
| Долгий путь обучения | Быстрый результат |
| Для глубокого погружения | Для быстрого старта |

> После того как вы пройдете этот гайд и получите первый результат, рекомендуем изучить [Getting Started](Getting_Started.md) для понимания полной архитектуры SDK.

---

## Что мы будем делать?

В этом гайде мы создадим минималистичный проект, который:

1. Подключится к MT5 терминалу через gRPC шлюз
2. Получит баланс счета
3. Выведет результат в консоль

**Это займет 10 минут и требует минимум кода.**

---

## Шаг 1: Установите .NET 8 SDK

Если у вас еще не установлен .NET 8 SDK:

**Скачайте и установите:**

- [.NET 8 SDK Download](https://dotnet.microsoft.com/download/dotnet/8.0)

**Проверьте установку:**

```bash
dotnet --version
# Должно показать: 8.0.x или выше
```

---

## Шаг 2: Создайте новый консольный проект

Откройте терминал (командную строку) и выполните:

```bash
# Создаем папку для проекта
mkdir MyMT5Project
cd MyMT5Project

# Создаем новый консольный проект
dotnet new console -n MyMT5Project

# Переходим в папку проекта
cd MyMT5Project
```

**Что произошло:**

- Создана папка `MyMT5Project`
- Внутри создан .NET проект с файлами:
  - `MyMT5Project.csproj` - файл проекта
  - `Program.cs` - главный файл кода

---

## Шаг 3: Установите NuGet пакет MetaRPC.MT5

Это самый важный шаг - устанавливаем пакет, который содержит все необходимое:

```bash
dotnet add package MetaRPC.MT5
```

**Что включает этот пакет:**

- Прото-файлы (Protocol Buffers схемы для gRPC)
- `MT5Account` класс для низкоуровневого взаимодействия с MT5
- Все необходимые зависимости (Grpc.Net.Client, Grpc.Core и т.д.)

> **Важно:** Этот пакет - это ВСЁ что вам нужно для работы с MT5. Никаких дополнительных файлов клонировать не требуется.

---

## Шаг 4: Установите пакеты для работы с конфигурацией

Нам понадобятся пакеты для чтения `appsettings.json`:

```bash
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.Binder
```

---

## Шаг 5: Создайте файл конфигурации appsettings.json

Создайте файл `appsettings.json` в корне проекта (рядом с `Program.cs`):

```json
{
  "MT5": {
    "User": 591129415,
    "Password": "IpoHj17tYu67@",
    "ServerName": "FxPro-MT5 Demo",
    "Host": "mt5.mrpc.pro",
    "Port": 443,
    "GrpcServer": "https://mt5.mrpc.pro:443",
    "BaseChartSymbol": "EURUSD",
    "InstanceId": null,
    "ConnectTimeoutSeconds": 120
  }
}
```

**Объяснение параметров:**

| Параметр | Описание | Пример |
|----------|----------|--------|
| **User** | Номер вашего MT5 счета (логин) | `591129415` |
| **Password** | Мастер-пароль от MT5 счета | `"IpoHj17tYu67@"` |
| **ServerName** | Название сервера вашего брокера | `"FxPro-MT5 Demo"` |
| **Host** | Адрес gRPC шлюза (предоставляется MetaRPC) | `"mt5.mrpc.pro"` |
| **Port** | Порт шлюза | `443` |
| **GrpcServer** | Полный URL шлюза | `"https://mt5.mrpc.pro:443"` |
| **BaseChartSymbol** | Торговый символ по умолчанию | `"EURUSD"` |
| **InstanceId** | ID инстанса (оставьте `null` для авто) | `null` |
| **ConnectTimeoutSeconds** | Таймаут подключения в секундах | `120` |

**Замените:**

- `User`, `Password`, `ServerName` - на данные вашего MT5 демо-счета
- `Host`, `Port` - оставьте как есть (это адрес публичного шлюза MetaRPC)

> **Нет MT5 аккаунта?** Прочитайте [MT5 для начинающих](MT5_For_Beginners.md) - там пошагово показано как создать демо-счет.

---

## Шаг 6: Настройте копирование appsettings.json в bin

Откройте файл `MyMT5Project.csproj` и добавьте эту секцию внутри `<Project>`:

```xml
<ItemGroup>
  <None Remove="appsettings.json" />
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

**Полный пример csproj файла:**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MetaRPC.MT5" Version="1.0.942" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.2" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
  </ItemGroup>

  <!-- Копирование appsettings.json в bin -->
  <ItemGroup>
    <None Remove="appsettings.json" />
    <Content Include="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

---

## Шаг 7: Напишите код для подключения и получения баланса

Откройте `Program.cs` и замените его содержимое на следующий код:

```csharp
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using mt5_term_api;

// ============================================================================
// КОНФИГУРАЦИЯ - Загружаем настройки из appsettings.json
// ============================================================================

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var mt5Config = configuration.GetSection("MT5");

int user = mt5Config.GetValue<int>("User");
string password = mt5Config.GetValue<string>("Password") ?? "";
string serverName = mt5Config.GetValue<string>("ServerName") ?? "";
string grpcServer = mt5Config.GetValue<string>("GrpcServer") ?? "";
string baseSymbol = mt5Config.GetValue<string>("BaseChartSymbol") ?? "EURUSD";
int connectTimeout = mt5Config.GetValue<int>("ConnectTimeoutSeconds", 120);

Console.WriteLine("=== MT5 Connection Configuration ===");
Console.WriteLine($"User: {user}");
Console.WriteLine($"Server: {serverName}");
Console.WriteLine($"gRPC: {grpcServer}");
Console.WriteLine($"Symbol: {baseSymbol}");
Console.WriteLine("====================================\n");

// ============================================================================
// ПОДКЛЮЧЕНИЕ - Создаем gRPC канал и MT5Account
// ============================================================================

Console.WriteLine("Connecting to MT5 gateway...");

// Создаем gRPC канал
var channel = GrpcChannel.ForAddress(grpcServer, new GrpcChannelOptions
{
    Credentials = ChannelCredentials.SecureSsl,
    MaxReceiveMessageSize = 100 * 1024 * 1024, // 100 MB
    MaxSendMessageSize = 100 * 1024 * 1024     // 100 MB
});

// Создаем MT5Account - это главный объект для работы с MT5
var mt5Account = new MT5Account(channel);

// Подключаемся к MT5 терминалу
var connectRequest = new ConnectRequest
{
    User = user,
    Password = password,
    ServerName = serverName,
    BaseChartSymbol = baseSymbol
};

var connectResponse = await mt5Account.ConnectAsync(connectRequest, deadline: DateTime.UtcNow.AddSeconds(connectTimeout));

if (connectResponse.RetCode != 0)
{
    Console.WriteLine($"ERROR: Connection failed with code {connectResponse.RetCode}");
    Console.WriteLine($"Message: {connectResponse.RetCodeMessage}");
    return;
}

Console.WriteLine("✓ Connected successfully!");
Console.WriteLine($"Instance ID: {connectResponse.InstanceId}\n");

// ============================================================================
// ПОЛУЧЕНИЕ БАЛАНСА - Вызываем метод GetAccountInfo
// ============================================================================

Console.WriteLine("Fetching account balance...");

var accountInfoRequest = new GetAccountInfoRequest
{
    InstanceId = connectResponse.InstanceId
};

var accountInfoResponse = await mt5Account.GetAccountInfoAsync(accountInfoRequest);

if (accountInfoResponse.RetCode != 0)
{
    Console.WriteLine($"ERROR: Failed to get account info with code {accountInfoResponse.RetCode}");
    Console.WriteLine($"Message: {accountInfoResponse.RetCodeMessage}");
    return;
}

// Выводим информацию о счете
Console.WriteLine("=== Account Information ===");
Console.WriteLine($"Balance: {accountInfoResponse.Balance:F2}");
Console.WriteLine($"Equity: {accountInfoResponse.Equity:F2}");
Console.WriteLine($"Margin: {accountInfoResponse.Margin:F2}");
Console.WriteLine($"Free Margin: {accountInfoResponse.FreeMargin:F2}");
Console.WriteLine($"Currency: {accountInfoResponse.Currency}");
Console.WriteLine($"Leverage: 1:{accountInfoResponse.Leverage}");
Console.WriteLine("===========================\n");

Console.WriteLine("✓ Success! Your first MT5 connection is complete.");

// ============================================================================
// ОТКЛЮЧЕНИЕ - Закрываем соединение
// ============================================================================

var disconnectRequest = new DisconnectRequest
{
    InstanceId = connectResponse.InstanceId
};

await mt5Account.DisconnectAsync(disconnectRequest);
Console.WriteLine("✓ Disconnected from MT5.");
```

---

## Шаг 8: Запустите проект

Сохраните все файлы и выполните:

```bash
dotnet run
```

**Ожидаемый результат:**

```
=== MT5 Connection Configuration ===
User: 591129415
Server: FxPro-MT5 Demo
gRPC: https://mt5.mrpc.pro:443
Symbol: EURUSD
====================================

Connecting to MT5 gateway...
✓ Connected successfully!
Instance ID: abc123-def456-...

Fetching account balance...
=== Account Information ===
Balance: 10000.00
Equity: 10000.00
Margin: 0.00
Free Margin: 10000.00
Currency: USD
Leverage: 1:100
===========================

✓ Success! Your first MT5 connection is complete.
✓ Disconnected from MT5.
```

---

## Поздравляем! Вы сделали это!

Вы только что:

✅ Создали новый .NET проект с нуля
✅ Подключили NuGet пакет MetaRPC.MT5
✅ Настроили конфигурацию подключения
✅ Подключились к MT5 терминалу через gRPC
✅ Получили баланс счета программно

**Это был низкоуровневый (Low-Level) подход** с прямым использованием `MT5Account` и gRPC.

---

## Что дальше?

Теперь, когда у вас есть рабочий проект, вы можете:

### 1. Изучить полную архитектуру SDK

Прочитайте [Getting Started](Getting_Started.md) чтобы узнать о:

- **MT5Account** (Low-Level) - то что вы только что использовали
- **MT5Service** (Wrappers) - удобные обертки над MT5Account
- **MT5Sugar** (High-Level) - синтаксический сахар для быстрой разработки

### 2. Добавить больше функциональности

**Примеры того что можно сделать:**

```csharp
// Получить все открытые позиции
var positionsRequest = new GetPositionsRequest { InstanceId = instanceId };
var positionsResponse = await mt5Account.GetPositionsAsync(positionsRequest);

// Открыть рыночный ордер
var orderRequest = new MarketOrderRequest
{
    InstanceId = instanceId,
    Symbol = "EURUSD",
    Volume = 0.01,
    OrderType = OrderType.Buy
};
var orderResponse = await mt5Account.MarketOrderAsync(orderRequest);

// Получить котировки в реальном времени (streaming)
var tickRequest = new TickRequest { InstanceId = instanceId, Symbol = "EURUSD" };
var tickStream = mt5Account.SubscribeToTicks(tickRequest);
await foreach (var tick in tickStream.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"Bid: {tick.Bid}, Ask: {tick.Ask}");
}
```

### 3. Скопировать готовые классы из репозитория

Если вы хотите использовать **MT5Service** или **MT5Sugar** в своем проекте:

1. Склонируйте репозиторий CSharpMT5
2. Скопируйте файлы `MT5Service.cs` и/или `MT5Sugar.cs` в свой проект
3. Используйте удобные методы высокого уровня

**Пример с MT5Sugar:**

```csharp
var sugar = new MT5Sugar(mt5Account, instanceId);

// Открыть Buy позицию
await sugar.Buy("EURUSD", 0.01);

// Закрыть все позиции по символу
await sugar.CloseAllPositions("EURUSD");

// Получить баланс одной строкой
var balance = await sugar.GetBalance();
```

### 4. Изучить готовые примеры

В репозитории CSharpMT5 есть множество примеров:

- [Orchestrators](Strategies/Strategies.Master.Overview.md) - готовые торговые стратегии
- [Adaptive Preset](Strategies/Presets/AdaptiveMarketModePreset.md) - умная мультистратегия
- [User Code Sandbox](UserCode_Sandbox_Guide.md) - шаблон для ваших стратегий

### 5. Прочитать дополнительные гайды

- [Sync vs Async](Sync_vs_Async.md) - когда использовать синхронные/асинхронные методы
- [gRPC Stream Management](GRPC_STREAM_MANAGEMENT.md) - работа с потоковыми данными
- [Return Codes Reference](ReturnCodes_Reference_EN.md) - коды возврата операций
- [Protobuf Inspector](ProtobufInspector.README.EN.md) - инструмент для изучения protobuf структур

---

## Частые вопросы (FAQ)

### Где взять доступ к gRPC шлюзу?

В примере используется публичный шлюз MetaRPC:

```
Host: mt5.mrpc.pro
Port: 443
```

Этот шлюз доступен всем для тестирования.

> Если у вас есть вопросы по работе шлюза, используйте кнопку "Contact & Support" на сайте документации или посетите [GitHub Discussions](https://github.com/MetaRPC/CSharpMT5/discussions).

### Могу ли я использовать свой собственный шлюз?

Да! Если у вас есть собственная инстанция шлюза, просто измените параметры `Host`, `Port` и `GrpcServer` в `appsettings.json`.

### Как получить MT5 демо-счет?

Прочитайте [MT5 для начинающих](MT5_For_Beginners.md) - там пошаговая инструкция по установке MT5 и созданию демо-счета.

### Что если я получаю ошибку подключения?

Проверьте:

1. Правильность логина/пароля/сервера в `appsettings.json`
2. Что MT5 терминал не запущен локально (шлюз сам подключается к MT5)
3. Интернет-соединение
4. Таймаут подключения (увеличьте `ConnectTimeoutSeconds` если медленный интернет)

### Нужно ли устанавливать MT5 терминал?

**Нет!** Шлюз MetaRPC сам подключается к серверам MT5. Вам нужны только:

- Логин/пароль от MT5 счета
- Название сервера брокера
- Доступ к gRPC шлюзу

---

## Структура вашего проекта

После завершения всех шагов ваша структура проекта должна выглядеть так:

```
MyMT5Project/
├── appsettings.json          # Конфигурация подключения
├── MyMT5Project.csproj       # Файл проекта с зависимостями
├── Program.cs                # Главный код приложения
└── bin/                      # Собранные файлы (создается автоматически)
    └── Debug/
        └── net8.0/
            ├── MyMT5Project.exe
            └── appsettings.json
```

---

## Резюме: Что мы сделали

В этом гайде вы создали минималистичный проект, который:

1. **Использует только NuGet пакет** - не требует клонирования репозитория
2. **Подключается к MT5** через gRPC шлюз
3. **Читает конфигурацию** из `appsettings.json`
4. **Выполняет низкоуровневые gRPC вызовы** напрямую через `MT5Account`
5. **Получает баланс счета** и выводит в консоль

**Это основа** для любого вашего MT5 проекта на C#.

---

## Следующие шаги

Теперь вы готовы к:

- 📖 [Getting Started](Getting_Started.md) - Полное изучение архитектуры SDK
- 📖 [MT5Account API](API_Reference/MT5Account.API.md) - Низкоуровневый API справочник
- 📖 [MT5Service API](API_Reference/MT5Service.API.md) - Удобные обертки
- 📖 [MT5Sugar API](API_Reference/MT5Sugar.API.md) - Высокоуровневый API
- 🎯 [Orchestrators](Strategies/Strategies.Master.Overview.md) - Готовые торговые стратегии

---

**Удачи в разработке ваших торговых систем!**

> "Лучший способ изучить что-то - это построить это с нуля. Теперь у вас есть фундамент. Стройте."
>
> — MetaRPC Team
