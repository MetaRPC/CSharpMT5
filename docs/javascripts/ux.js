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

  // Initialize Contact Panel
  initContactPanel();
});

// ============================================================================
// PROGRESS TRACKER - Track documentation reading progress
// ============================================================================

const PROGRESS_STORAGE_KEY = 'csharpmt5_docs_progress';

// Documentation structure: 119 total files
const DOC_STRUCTURE = {
  'guides': {
    name: '📘 Guides',
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
  if (document.getElementById('progress-float-btn')) return;

  const progressHTML = `
    <!-- Floating Button -->
    <button id="progress-float-btn" class="progress-float-btn" title="Learning Progress">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"></circle>
        <path d="M12 6v6l4 2"></path>
      </svg>
      <span class="progress-badge" id="progress-badge">0%</span>
    </button>

    <!-- Side Panel -->
    <div id="progress-panel" class="progress-panel">
      <div class="progress-panel-header">
        <h3>📊 Learning Progress</h3>
        <button id="progress-panel-close" class="progress-panel-close" title="Close">&times;</button>
      </div>

      <div class="progress-panel-content">
        <div class="progress-overall-section">
          <div class="progress-stats">
            <span class="progress-count" id="overall-progress-text">0 / 119</span>
            <span class="progress-percentage" id="overall-progress-pct">0%</span>
          </div>
          <div class="progress-bar-wrapper">
            <div class="progress-bar-fill" id="overall-progress-fill"></div>
          </div>
          <p class="progress-subtitle">Total Documentation Pages</p>
        </div>

        <div class="progress-categories-section">
          <h4>By Category</h4>
          <div id="progress-categories"></div>
        </div>

        <button id="progress-reset-btn" class="progress-reset-btn-panel">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8"></path>
            <path d="M21 3v5h-5"></path>
            <path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16"></path>
            <path d="M3 21v-5h5"></path>
          </svg>
          Reset Progress
        </button>
      </div>
    </div>

    <!-- Overlay -->
    <div id="progress-overlay" class="progress-overlay"></div>
  `;

  // Insert at end of body
  document.body.insertAdjacentHTML('beforeend', progressHTML);

  // Add event listeners
  document.getElementById('progress-float-btn').addEventListener('click', openProgressPanel);
  document.getElementById('progress-panel-close').addEventListener('click', closeProgressPanel);
  document.getElementById('progress-overlay').addEventListener('click', closeProgressPanel);
  document.getElementById('progress-reset-btn').addEventListener('click', resetProgress);
}

function openProgressPanel() {
  document.getElementById('progress-panel').classList.add('open');
  document.getElementById('progress-overlay').classList.add('visible');
  document.body.style.overflow = 'hidden';
}

function closeProgressPanel() {
  document.getElementById('progress-panel').classList.remove('open');
  document.getElementById('progress-overlay').classList.remove('visible');
  document.body.style.overflow = '';
}

function updateProgressDisplay() {
  const progress = getProgress();
  const stats = calculateProgress(progress);

  const percentage = Math.round(stats.overall.percentage);

  // Update floating button badge
  const badge = document.getElementById('progress-badge');
  if (badge) {
    badge.textContent = percentage + '%';
  }

  // Update overall progress in panel
  const overallFill = document.getElementById('overall-progress-fill');
  const overallText = document.getElementById('overall-progress-text');
  const overallPct = document.getElementById('overall-progress-pct');

  if (overallFill) {
    overallFill.style.width = percentage + '%';
  }

  if (overallText) {
    overallText.textContent = `${stats.overall.completed} / ${stats.overall.total}`;
  }

  if (overallPct) {
    overallPct.textContent = percentage + '%';
  }

  // Update category progress
  const categoriesContainer = document.getElementById('progress-categories');
  if (categoriesContainer) {
    categoriesContainer.innerHTML = '';

    Object.entries(stats.categories).forEach(([key, cat]) => {
      const catPercentage = Math.round(cat.percentage);
      const categoryHTML = `
        <div class="progress-category">
          <div class="progress-category-header">
            <span class="progress-category-name">${cat.name}</span>
            <span class="progress-category-count">${cat.completed}/${cat.total}</span>
          </div>
          <div class="progress-bar-wrapper small">
            <div class="progress-bar-fill" style="width: ${catPercentage}%"></div>
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
  if (confirm('Are you sure you want to reset all progress?')) {
    localStorage.removeItem(PROGRESS_STORAGE_KEY);
    updateProgressDisplay();
    console.log('Progress reset');
  }
}

// ============================================================================
// CONTACT PANEL - Quick access to support channels
// ============================================================================

function initContactPanel() {
  // Create and inject contact button and panel
  createContactPanel();
}

function createContactPanel() {
  // Check if contact panel already exists
  if (document.getElementById('contact-float-btn')) return;

  const contactHTML = `
    <!-- Floating Contact Button -->
    <button id="contact-float-btn" class="contact-float-btn" title="Contact & Support">
      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
      </svg>
    </button>

    <!-- Contact Side Panel -->
    <div id="contact-panel" class="contact-panel">
      <div class="contact-panel-header">
        <h3>💬 Contact & Support</h3>
        <button id="contact-panel-close" class="contact-panel-close" title="Close">&times;</button>
      </div>

      <div class="contact-panel-content">
        <div class="contact-intro">
          <h4>Need Help?</h4>
          <p>Have questions about CSharpMT5 SDK? Reach out to us through your preferred messenger!</p>
        </div>

        <div class="contact-buttons">
          <!-- Telegram Button -->
          <a href="https://t.me/YOUR_TELEGRAM_USERNAME" target="_blank" class="contact-btn contact-btn-telegram">
            <div class="contact-btn-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 0C5.373 0 0 5.373 0 12s5.373 12 12 12 12-5.373 12-12S18.627 0 12 0zm5.562 8.161l-1.84 8.673c-.139.622-.502.775-.998.483l-2.764-2.037-1.332 1.282c-.147.147-.271.271-.556.271l.199-2.815 5.139-4.643c.224-.199-.048-.31-.347-.111l-6.355 4.003-2.737-.856c-.595-.187-.607-.595.125-.881l10.703-4.124c.496-.182.93.114.762.877z"/>
              </svg>
            </div>
            <div class="contact-btn-content">
              <div class="contact-btn-title">Telegram</div>
              <div class="contact-btn-desc">Quick responses & community chat</div>
            </div>
          </a>

          <!-- WhatsApp Button -->
          <a href="https://wa.me/YOUR_PHONE_NUMBER" target="_blank" class="contact-btn contact-btn-whatsapp">
            <div class="contact-btn-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z"/>
              </svg>
            </div>
            <div class="contact-btn-content">
              <div class="contact-btn-title">WhatsApp</div>
              <div class="contact-btn-desc">Direct messaging & voice calls</div>
            </div>
          </a>

          <!-- GitHub Discussions Button -->
          <a href="YOUR_GITHUB_DISCUSSIONS_URL" target="_blank" class="contact-btn contact-btn-github">
            <div class="contact-btn-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/>
              </svg>
            </div>
            <div class="contact-btn-content">
              <div class="contact-btn-title">GitHub Discussions</div>
              <div class="contact-btn-desc">Community support & Q&A</div>
            </div>
          </a>

          <!-- Email Button -->
          <a href="mailto:YOUR_EMAIL_ADDRESS" class="contact-btn contact-btn-email">
            <div class="contact-btn-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
                <polyline points="22,6 12,13 2,6"></polyline>
              </svg>
            </div>
            <div class="contact-btn-content">
              <div class="contact-btn-title">Email</div>
              <div class="contact-btn-desc">Business inquiries & partnerships</div>
            </div>
          </a>
        </div>
      </div>
    </div>

    <!-- Contact Overlay -->
    <div id="contact-overlay" class="progress-overlay"></div>
  `;

  // Insert at end of body
  document.body.insertAdjacentHTML('beforeend', contactHTML);

  // Add event listeners
  document.getElementById('contact-float-btn').addEventListener('click', openContactPanel);
  document.getElementById('contact-panel-close').addEventListener('click', closeContactPanel);
  document.getElementById('contact-overlay').addEventListener('click', closeContactPanel);
}

function openContactPanel() {
  document.getElementById('contact-panel').classList.add('open');
  document.getElementById('contact-overlay').classList.add('visible');
  document.body.style.overflow = 'hidden';
}

function closeContactPanel() {
  document.getElementById('contact-panel').classList.remove('open');
  document.getElementById('contact-overlay').classList.remove('visible');
  document.body.style.overflow = '';
}
