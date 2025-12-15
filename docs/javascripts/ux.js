// CSharpMT5 — Enhanced UX
document.addEventListener('DOMContentLoaded', function() {
  console.log('CSharpMT5 Documentation loaded');

  // Smooth scroll for anchor links
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
      e.preventDefault();
      const target = document.querySelector(this.getAttribute('href'));
      if (target) {
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    });
  });

  // Initialize Progress Tracker
  initProgressTracker();
});

// ============================================================================
// PROGRESS TRACKER - Track documentation reading progress
// ============================================================================

const PROGRESS_STORAGE_KEY = 'csharpmt5_docs_progress';

// Documentation structure: 119 total files
const DOC_STRUCTURE = {
  'guides': {
    name: '📘 Основные гайды',
    pages: [
      'Getting_Started',
      'MT5_For_Beginners',
      'GRPC_STREAM_MANAGEMENT',
      'Sync_vs_Async',
      'ReturnCodes_Reference_EN',
      'ProtobufInspector.README.EN',
      'UserCode_Sandbox_Guide',
      'Glossary'
    ]
  },
  'mt5account': {
    name: '📦 MT5Account API',
    pages: [
      'MT5Account/MT5Account.Master.Overview',
      'MT5Account/1. Account_information/Account_Information.Overview',
      'MT5Account/1. Account_information/AccountSummary',
      'MT5Account/1. Account_information/AccountInfoDouble',
      'MT5Account/1. Account_information/AccountInfoInteger',
      'MT5Account/1. Account_information/AccountInfoString',
      'MT5Account/2. Symbol_information/Symbol_Information.Overview',
      'MT5Account/2. Symbol_information/SymbolInfoTick',
      'MT5Account/2. Symbol_information/SymbolsTotal',
      'MT5Account/2. Symbol_information/SymbolName',
      'MT5Account/2. Symbol_information/SymbolSelect',
      'MT5Account/2. Symbol_information/SymbolExist',
      'MT5Account/2. Symbol_information/SymbolIsSynchronized',
      'MT5Account/2. Symbol_information/SymbolInfoDouble',
      'MT5Account/2. Symbol_information/SymbolInfoInteger',
      'MT5Account/2. Symbol_information/SymbolInfoString',
      'MT5Account/3. Position_Orders_Information/Position_Orders_Information.Overview',
      'MT5Account/3. Position_Orders_Information/PositionsTotal',
      'MT5Account/3. Position_Orders_Information/OpenedOrders',
      'MT5Account/3. Position_Orders_Information/OpenedOrdersTickets',
      'MT5Account/3. Position_Orders_Information/OrderHistory',
      'MT5Account/3. Position_Orders_Information/PositionsHistory',
      'MT5Account/4. Trading_Operattons/Trading_Operations.Overview',
      'MT5Account/4. Trading_Operattons/OrderSend',
      'MT5Account/4. Trading_Operattons/OrderModify',
      'MT5Account/4. Trading_Operattons/OrderClose',
      'MT5Account/4. Trading_Operattons/OrderCalcMargin',
      'MT5Account/4. Trading_Operattons/OrderCheck',
      'MT5Account/5. Market_Depth(DOM)/Market_Depth.Overview',
      'MT5Account/5. Market_Depth(DOM)/MarketBookAdd',
      'MT5Account/5. Market_Depth(DOM)/MarketBookGet',
      'MT5Account/5. Market_Depth(DOM)/MarketBookRelease',
      'MT5Account/6. Addittional_Methods/Additional_Methods.Overview',
      'MT5Account/6. Addittional_Methods/SymbolInfoMarginRate',
      'MT5Account/6. Addittional_Methods/SymbolInfoSessionQuote',
      'MT5Account/6. Addittional_Methods/SymbolInfoSessionTrade',
      'MT5Account/6. Addittional_Methods/SymbolParamsMany',
      'MT5Account/6. Addittional_Methods/TickValueWithSize',
      'MT5Account/7. Streaming_Methods/Streaming_Methods.Overview',
      'MT5Account/7. Streaming_Methods/SubscribeToTicks',
      'MT5Account/7. Streaming_Methods/OnTrade',
      'MT5Account/7. Streaming_Methods/SubscribeToTradeTransaction',
      'MT5Account/7. Streaming_Methods/SubscribeToPositionProfit',
      'MT5Account/7. Streaming_Methods/OnPositionsAndPendingOrdersTickets'
    ]
  },
  'mt5service': {
    name: '🔧 MT5Service',
    pages: [
      'MT5Service/MT5Service.Overview',
      'MT5Service/Account_Convenience_Methods',
      'MT5Service/Symbol_Convenience_Methods',
      'MT5Service/Trading_Convenience_Methods',
      'MT5Service/History_Convenience_Methods'
    ]
  },
  'mt5sugar': {
    name: '🍬 MT5Sugar',
    pages: [
      'MT5Sugar/MT5Sugar.API_Overview',
      'MT5Sugar/1. Infrastructure/EnsureSelected',
      'MT5Sugar/2. Snapshots/GetAccountSnapshot',
      'MT5Sugar/2. Snapshots/GetSymbolSnapshot',
      'MT5Sugar/3. Normalization_Utils/GetDigitsAsync',
      'MT5Sugar/3. Normalization_Utils/GetPointAsync',
      'MT5Sugar/3. Normalization_Utils/GetSpreadPointsAsync',
      'MT5Sugar/3. Normalization_Utils/NormalizePriceAsync',
      'MT5Sugar/3. Normalization_Utils/PointsToPipsAsync',
      'MT5Sugar/4. History_Helpers/OrdersHistoryLast',
      'MT5Sugar/4. History_Helpers/PositionsHistoryPaged',
      'MT5Sugar/5. Streams_Helpers/ReadTicks',
      'MT5Sugar/5. Streams_Helpers/ReadTrades',
      'MT5Sugar/6. Trading_Market_Pending/PlaceMarket',
      'MT5Sugar/6. Trading_Market_Pending/PlacePending',
      'MT5Sugar/6. Trading_Market_Pending/CloseByTicket',
      'MT5Sugar/6. Trading_Market_Pending/CloseAll',
      'MT5Sugar/6. Trading_Market_Pending/ModifySlTpAsync',
      'MT5Sugar/8. Volume_Price_Utils/GetVolumeLimitsAsync',
      'MT5Sugar/8. Volume_Price_Utils/NormalizeVolumeAsync',
      'MT5Sugar/8. Volume_Price_Utils/CalcVolumeForRiskAsync',
      'MT5Sugar/8. Volume_Price_Utils/GetTickValueAndSizeAsync',
      'MT5Sugar/8. Volume_Price_Utils/PriceFromOffsetPointsAsync',
      'MT5Sugar/9. Pending_ByPoints/BuyLimitPoints',
      'MT5Sugar/9. Pending_ByPoints/BuyStopPoints',
      'MT5Sugar/9. Pending_ByPoints/SellLimitPoints',
      'MT5Sugar/9. Pending_ByPoints/SellStopPoints',
      'MT5Sugar/10. Market_ByRisk/BuyMarketByRisk',
      'MT5Sugar/10. Market_ByRisk/SellMarketByRisk',
      'MT5Sugar/11. Bulk_Convenience/CancelAll',
      'MT5Sugar/11. Bulk_Convenience/CloseAllPending',
      'MT5Sugar/11. Bulk_Convenience/CloseAllPositions',
      'MT5Sugar/12. Market_Depth_DOM/SubscribeToMarketBookAsync',
      'MT5Sugar/12. Market_Depth_DOM/GetMarketBookSnapshotAsync',
      'MT5Sugar/12. Market_Depth_DOM/GetBestBidAskFromBookAsync',
      'MT5Sugar/12. Market_Depth_DOM/CalculateLiquidityAtLevelAsync',
      'MT5Sugar/13. Order_Validation/ValidateOrderAsync',
      'MT5Sugar/13. Order_Validation/CalculateBuyMarginAsync',
      'MT5Sugar/13. Order_Validation/CalculateSellMarginAsync',
      'MT5Sugar/13. Order_Validation/CheckMarginAvailabilityAsync',
      'MT5Sugar/14. Session_Time/GetQuoteSessionAsync',
      'MT5Sugar/14. Session_Time/GetTradeSessionAsync',
      'MT5Sugar/15. Position_Monitoring/GetPositionCountAsync',
      'MT5Sugar/15. Position_Monitoring/GetTotalProfitLossAsync',
      'MT5Sugar/15. Position_Monitoring/GetProfitablePositionsAsync',
      'MT5Sugar/15. Position_Monitoring/GetLosingPositionsAsync',
      'MT5Sugar/15. Position_Monitoring/GetPositionStatsBySymbolAsync'
    ]
  },
  'strategies': {
    name: '🎯 Strategies',
    pages: [
      'Strategies/Strategies.Master.Overview',
      'Strategies/Orchestrators_EN/SimpleScalpingOrchestrator',
      'Strategies/Orchestrators_EN/SimpleScalpingOrchestrator.HOW_IT_WORKS',
      'Strategies/Orchestrators_EN/PendingBreakoutOrchestrator',
      'Strategies/Orchestrators_EN/PendingBreakoutOrchestrator.HOW_IT_WORKS',
      'Strategies/Orchestrators_EN/GridTradingOrchestrator',
      'Strategies/Orchestrators_EN/GridTradingOrchestrator.HOW_IT_WORKS',
      'Strategies/Orchestrators_EN/NewsStraddleOrchestrator',
      'Strategies/Orchestrators_EN/NewsStraddleOrchestrator.HOW_IT_WORKS',
      'Strategies/Orchestrators_EN/QuickHedgeOrchestrator',
      'Strategies/Orchestrators_EN/QuickHedgeOrchestrator.HOW_IT_WORKS',
      'Strategies/Presets/AdaptiveMarketModePreset'
    ]
  },
  'api_reference': {
    name: '📚 API Reference',
    pages: [
      'API_Reference/MT5Account.API',
      'API_Reference/MT5Service.API',
      'API_Reference/MT5Sugar.API'
    ]
  }
};

function initProgressTracker() {
  // Track current page visit
  trackPageVisit();

  // Create and inject progress bar
  createProgressBar();

  // Update progress display
  updateProgressDisplay();
}

function trackPageVisit() {
  const currentPath = getCurrentPagePath();
  if (!currentPath) return;

  const progress = getProgress();
  if (!progress.visitedPages.includes(currentPath)) {
    progress.visitedPages.push(currentPath);
    progress.lastVisit = new Date().toISOString();
    saveProgress(progress);
  }
}

function getCurrentPagePath() {
  // Get current page path from URL
  const path = window.location.pathname;

  // Extract page identifier from path
  // Example: /CSharpMT5/MT5Account/MT5Account.Master.Overview/ -> MT5Account/MT5Account.Master.Overview
  const match = path.match(/\/CSharpMT5\/(.+?)(?:\/|\.html)?$/);
  if (!match) return null;

  return match[1].replace(/\/$/, ''); // Remove trailing slash
}

function getProgress() {
  const stored = localStorage.getItem(PROGRESS_STORAGE_KEY);
  if (stored) {
    try {
      return JSON.parse(stored);
    } catch (e) {
      console.error('Failed to parse progress data:', e);
    }
  }

  // Default progress structure
  return {
    visitedPages: [],
    lastVisit: new Date().toISOString()
  };
}

function saveProgress(progress) {
  localStorage.setItem(PROGRESS_STORAGE_KEY, JSON.stringify(progress));
}

function createProgressBar() {
  // Check if progress bar already exists
  if (document.getElementById('docs-progress-bar')) return;

  const progressBarHTML = `
    <div id="docs-progress-bar" class="docs-progress-container">
      <div class="docs-progress-header">
        <span class="docs-progress-title">📊 Прогресс изучения</span>
        <button id="progress-reset-btn" class="progress-reset-btn" title="Сбросить прогресс">↻</button>
      </div>
      <div class="docs-progress-overall">
        <div class="progress-bar-wrapper">
          <div class="progress-bar-fill" id="overall-progress-fill"></div>
          <span class="progress-bar-text" id="overall-progress-text">0%</span>
        </div>
      </div>
      <div id="progress-categories" class="docs-progress-categories"></div>
      <button id="progress-toggle-btn" class="progress-toggle-btn">Показать детали ▼</button>
    </div>
  `;

  // Insert progress bar at the top of main content
  const contentArea = document.querySelector('.md-content');
  if (contentArea) {
    contentArea.insertAdjacentHTML('afterbegin', progressBarHTML);

    // Add event listeners
    document.getElementById('progress-reset-btn').addEventListener('click', resetProgress);
    document.getElementById('progress-toggle-btn').addEventListener('click', toggleProgressDetails);
  }
}

function updateProgressDisplay() {
  const progress = getProgress();
  const stats = calculateProgress(progress);

  // Update overall progress bar
  const overallFill = document.getElementById('overall-progress-fill');
  const overallText = document.getElementById('overall-progress-text');

  if (overallFill && overallText) {
    const percentage = Math.round(stats.overall.percentage);
    overallFill.style.width = percentage + '%';
    overallText.textContent = `${stats.overall.completed} / ${stats.overall.total} (${percentage}%)`;
  }

  // Update category progress
  const categoriesContainer = document.getElementById('progress-categories');
  if (categoriesContainer) {
    categoriesContainer.innerHTML = '';

    Object.entries(stats.categories).forEach(([key, cat]) => {
      const percentage = Math.round(cat.percentage);
      const categoryHTML = `
        <div class="progress-category">
          <div class="progress-category-header">
            <span class="progress-category-name">${cat.name}</span>
            <span class="progress-category-count">${cat.completed}/${cat.total}</span>
          </div>
          <div class="progress-bar-wrapper small">
            <div class="progress-bar-fill" style="width: ${percentage}%"></div>
          </div>
        </div>
      `;
      categoriesContainer.insertAdjacentHTML('beforeend', categoryHTML);
    });
  }
}

function calculateProgress(progress) {
  const stats = {
    overall: { completed: 0, total: 0, percentage: 0 },
    categories: {}
  };

  Object.entries(DOC_STRUCTURE).forEach(([key, category]) => {
    const total = category.pages.length;
    const completed = category.pages.filter(page =>
      progress.visitedPages.includes(page)
    ).length;

    stats.categories[key] = {
      name: category.name,
      completed: completed,
      total: total,
      percentage: total > 0 ? (completed / total) * 100 : 0
    };

    stats.overall.completed += completed;
    stats.overall.total += total;
  });

  stats.overall.percentage = stats.overall.total > 0
    ? (stats.overall.completed / stats.overall.total) * 100
    : 0;

  return stats;
}

function resetProgress() {
  if (confirm('Вы уверены, что хотите сбросить весь прогресс?')) {
    localStorage.removeItem(PROGRESS_STORAGE_KEY);
    updateProgressDisplay();
    console.log('Progress reset');
  }
}

function toggleProgressDetails() {
  const categoriesContainer = document.getElementById('progress-categories');
  const toggleBtn = document.getElementById('progress-toggle-btn');

  if (categoriesContainer && toggleBtn) {
    const isHidden = categoriesContainer.style.display === 'none' || !categoriesContainer.style.display;

    if (isHidden) {
      categoriesContainer.style.display = 'block';
      toggleBtn.textContent = 'Скрыть детали ▲';
    } else {
      categoriesContainer.style.display = 'none';
      toggleBtn.textContent = 'Показать детали ▼';
    }
  }
}
