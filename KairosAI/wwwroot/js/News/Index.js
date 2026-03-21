// ═══════════════════════════════════════════
//  KairosAI — News Intelligence Module
// ═══════════════════════════════════════════

if (typeof lucide !== 'undefined') lucide.createIcons();

document.addEventListener('DOMContentLoaded', () => {

    // ── 1. Fade-in orquestado ──
    document.querySelectorAll('.k-fade').forEach((el, i) => {
        setTimeout(() => el.classList.add('visible'), 60 + i * 80);
    });

    // ── 2. Animación de barras del veredicto ──
    setTimeout(() => {
        const fillPos = document.getElementById('fillPos');
        const fillNeu = document.getElementById('fillNeu');
        const fillNeg = document.getElementById('fillNeg');
        if (fillPos) fillPos.style.width = fillPos.dataset.target + '%';
        if (fillNeu) fillNeu.style.width = fillNeu.dataset.target + '%';
        if (fillNeg) fillNeg.style.width = fillNeg.dataset.target + '%';
    }, 400);

    // ── 3. Renderizar sparklines ──
    renderAllSparklines();

    // ── 4. Auto-refresh de precios cada 60 segundos ──
    setInterval(refreshLivePrices, 60000);

    // ── 5. Reinicializar Lucide icons ──
    setTimeout(() => {
        if (typeof lucide !== 'undefined') lucide.createIcons();
    }, 200);
});

// ═══════════════════════════════════════════
//  SPARKLINE RENDERING
// ═══════════════════════════════════════════

function renderAllSparklines() {
    document.querySelectorAll('.sparkline-canvas, .sparkline-mini').forEach(canvas => {
        const dataStr = canvas.dataset.sparkline;
        if (!dataStr) return;

        const isPositive = canvas.dataset.positive === 'true';
        const data = dataStr.split(',').map(Number).filter(n => !isNaN(n));

        if (data.length < 2) return;

        const ctx = canvas.getContext('2d');
        const w = canvas.width;
        const h = canvas.height;
        const dpr = window.devicePixelRatio || 1;

        // Ajustar para retina
        canvas.width = w * dpr;
        canvas.height = h * dpr;
        canvas.style.width = w + 'px';
        canvas.style.height = h + 'px';
        ctx.scale(dpr, dpr);

        const min = Math.min(...data);
        const max = Math.max(...data);
        const range = max - min || 1;
        const stepX = w / (data.length - 1);
        const padding = 2;

        // Dibujar línea
        ctx.beginPath();
        ctx.strokeStyle = isPositive ? '#10b981' : '#ef4444';
        ctx.lineWidth = 1.5;
        ctx.lineJoin = 'round';
        ctx.lineCap = 'round';

        data.forEach((val, i) => {
            const x = i * stepX;
            const y = padding + ((max - val) / range) * (h - padding * 2);

            if (i === 0) ctx.moveTo(x, y);
            else ctx.lineTo(x, y);
        });

        ctx.stroke();

        // Gradiente debajo de la línea
        const gradient = ctx.createLinearGradient(0, 0, 0, h);
        gradient.addColorStop(0, isPositive ? 'rgba(16,185,129,0.15)' : 'rgba(239,68,68,0.15)');
        gradient.addColorStop(1, 'rgba(0,0,0,0)');

        ctx.lineTo(w, h);
        ctx.lineTo(0, h);
        ctx.closePath();
        ctx.fillStyle = gradient;
        ctx.fill();
    });
}

// ═══════════════════════════════════════════
//  LIVE PRICE REFRESH (AJAX)
// ═══════════════════════════════════════════

async function refreshLivePrices() {
    try {
        const response = await fetch('/News/GetLivePrices');
        if (!response.ok) return;

        const prices = await response.json();

        prices.forEach(item => {
            // Actualizar ticker
            const priceEls = document.querySelectorAll(`#price-${item.symbol}`);
            const changeEls = document.querySelectorAll(`#change-${item.symbol}`);

            priceEls.forEach(el => {
                const formatted = item.price >= 1
                    ? `$${item.price.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                    : `$${item.price.toFixed(6)}`;

                if (el.textContent.trim() !== formatted) {
                    el.textContent = formatted;
                    el.classList.add('price-updated');
                    setTimeout(() => el.classList.remove('price-updated'), 600);
                }
            });

            changeEls.forEach(el => {
                const isPos = item.change24h >= 0;
                const changeText = `${isPos ? '+' : ''}${item.change24h.toFixed(2)}%`;
                el.textContent = changeText;
                el.className = `ticker-change ${isPos ? 'up' : 'down'}`;
            });

            // También actualizar asset chips en las cards
            document.querySelectorAll(`.asset-mini[data-symbol="${item.symbol}"], .asset-chip[data-symbol="${item.symbol}"]`).forEach(chip => {
                const priceEl = chip.querySelector('.asset-price, .asset-mini-price');
                const changeEl = chip.querySelector('.asset-change, .asset-mini-change');

                if (priceEl) {
                    const formatted = item.price >= 1
                        ? `$${item.price.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                        : `$${item.price.toFixed(6)}`;
                    priceEl.textContent = formatted;
                }

                if (changeEl) {
                    const isPos = item.change24h >= 0;
                    changeEl.textContent = `${isPos ? '+' : ''}${item.change24h.toFixed(2)}%`;
                    changeEl.className = changeEl.className.includes('mini')
                        ? `asset-mini-change ${isPos ? 'change-up' : 'change-down'}`
                        : `asset-change ${isPos ? 'change-up' : 'change-down'}`;
                }
            });
        });

        console.log(`[KairosAI] Precios actualizados: ${prices.length} activos`);

    } catch (err) {
        console.warn('[KairosAI] Error al refrescar precios:', err);
    }
}

// ═══════════════════════════════════════════
//  FILTER TABS
// ═══════════════════════════════════════════

function filterNews(btn, category) {
    // Pill activa
    document.querySelectorAll('.filter-pill').forEach(p => {
        p.classList.remove('active');
        p.setAttribute('aria-selected', 'false');
    });
    btn.classList.add('active');
    btn.setAttribute('aria-selected', 'true');

    // Filtrar cards con animación
    const cards = document.querySelectorAll('#newsGrid article');
    let visibleCount = 0;

    cards.forEach((card, idx) => {
        const match = category === 'todo' || card.dataset.category === category;
        card.style.transition = 'opacity 0.25s ease, transform 0.25s ease';

        if (match) {
            visibleCount++;
            card.style.opacity = '1';
            card.style.transform = 'scale(1)';
            card.style.pointerEvents = 'auto';
            card.style.display = '';
        } else {
            card.style.opacity = '0';
            card.style.transform = 'scale(0.97)';
            card.style.pointerEvents = 'none';
            setTimeout(() => {
                if (card.style.opacity === '0') card.style.display = 'none';
            }, 250);
        }
    });

    // Mostrar empty state si no hay resultados
    let emptyState = document.querySelector('.filter-empty-state');
    if (visibleCount === 0) {
        if (!emptyState) {
            emptyState = document.createElement('div');
            emptyState.className = 'filter-empty-state empty-state';
            emptyState.style.gridColumn = '1/-1';
            emptyState.innerHTML = `
                <div style="opacity:0.3;margin-bottom:8px;">📭</div>
                <p style="font-size:0.85rem;">No hay noticias en esta categoría.</p>
            `;
            document.getElementById('newsGrid').appendChild(emptyState);
        }
        emptyState.style.display = '';
    } else if (emptyState) {
        emptyState.style.display = 'none';
    }
}

// ═══════════════════════════════════════════
//  UTILITY: Format numbers
// ═══════════════════════════════════════════

function formatPrice(price) {
    if (price >= 1) {
        return '$' + price.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }
    return '$' + price.toFixed(6);
}

function formatVolume(vol) {
    if (vol >= 1e9) return (vol / 1e9).toFixed(1) + 'B';
    if (vol >= 1e6) return (vol / 1e6).toFixed(1) + 'M';
    if (vol >= 1e3) return (vol / 1e3).toFixed(1) + 'K';
    return vol.toString();
}