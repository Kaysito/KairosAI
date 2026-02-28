
// ── MOTOR DE RESCATE DE LOGOS ──
const TICKER_MAP = {
    'BTC': 'https://assets.coingecko.com/coins/images/1/large/bitcoin.png',
    'ETH': 'https://assets.coingecko.com/coins/images/279/large/ethereum.png',
    'USDT': 'https://assets.coingecko.com/coins/images/325/large/Tether.png',
    'BNB': 'https://assets.coingecko.com/coins/images/825/large/bnb-icon2_2x.png',
    'SOL': 'https://assets.coingecko.com/coins/images/4128/large/solana.png',
    'NVDA': 'https://logo.clearbit.com/nvidia.com',
    'AAPL': 'https://logo.clearbit.com/apple.com',
    'TSLA': 'https://logo.clearbit.com/tesla.com',
    'MSFT': 'https://logo.clearbit.com/microsoft.com',
    'AMZN': 'https://logo.clearbit.com/amazon.com'
};

window.handleLogoError = function (img, symbol) {
    if (img.dataset.fallbackTried) {
        img.style.display = 'none';
        img.nextElementSibling.classList.remove('hidden');
        img.nextElementSibling.classList.add('flex');
        return;
    }
    img.dataset.fallbackTried = 'true';
    img.src = TICKER_MAP[symbol.toUpperCase()] || `https://logo.synthfinance.com/ticker/${symbol}`;
};

window.handleListLogoError = function (img, symbol) {
    if (img.dataset.fallbackTried) {
        img.style.display = 'none';
        img.nextElementSibling.classList.remove('hidden');
        img.nextElementSibling.classList.add('flex');
        return;
    }
    img.dataset.fallbackTried = 'true';
    img.src = TICKER_MAP[symbol.toUpperCase()] || `https://logo.synthfinance.com/ticker/${symbol}`;
};

// ── Gráficas e Iconos ──
document.addEventListener('DOMContentLoaded', function () {
    if (typeof lucide !== 'undefined') lucide.createIcons();

    const ctx = document.getElementById('assetChart').getContext('2d');
    const labels = @Json.Serialize(Model.ChartLabels ?? new List < string > ());
    const dataPoints = @Json.Serialize(Model.ChartData ?? new List < decimal > ());

    const isPositive = @(Model.Change24h >= 0 ? "true" : "false");
    const lineColor = isPositive ? '#10b981' : '#ef4444';
    const gradientColor = isPositive ? 'rgba(16, 185, 129, 0.2)' : 'rgba(239, 68, 68, 0.2)';

    let gradient = ctx.createLinearGradient(0, 0, 0, 300);
    gradient.addColorStop(0, gradientColor);
    gradient.addColorStop(1, 'rgba(0,0,0,0)');

    function getTextColor() {
        return document.documentElement.getAttribute('data-theme') === 'light' ? '#6b7280' : '#8b8fa8';
    }
    function getGridColor() {
        return document.documentElement.getAttribute('data-theme') === 'light' ? 'rgba(0,0,0,0.05)' : 'rgba(255,255,255,0.05)';
    }

    let chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels.length > 0 ? labels : ['1h', '2h', '3h', '4h', '5h', '6h'],
            datasets: [{
                label: 'Precio USD',
                data: dataPoints.length > 0 ? dataPoints : [0, 0, 0, 0, 0, 0],
                borderColor: lineColor,
                backgroundColor: gradient,
                borderWidth: 2.5,
                pointRadius: 0,
                pointHoverRadius: 6,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    backgroundColor: 'rgba(15,23,42,0.9)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    bodyFont: { family: "'JetBrains Mono', monospace" },
                    borderColor: 'rgba(255,255,255,0.1)',
                    borderWidth: 1,
                    padding: 10,
                    displayColors: false
                }
            },
            scales: {
                x: { display: false },
                y: {
                    display: true,
                    position: 'right',
                    grid: { color: getGridColor(), drawBorder: false },
                    ticks: {
                        color: getTextColor(),
                        font: { family: "'JetBrains Mono', monospace", size: 10 },
                        callback: function (value) { return '$' + value; }
                    }
                }
            },
            interaction: { mode: 'nearest', axis: 'x', intersect: false }
        }
    });

    // Actualizar colores si cambian de tema
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.attributeName === "data-theme") {
                chart.options.scales.y.ticks.color = getTextColor();
                chart.options.scales.y.grid.color = getGridColor();
                chart.update();
            }
        });
    });
    observer.observe(document.documentElement, { attributes: true });
});
