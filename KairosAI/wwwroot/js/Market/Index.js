<script>
    (function () {
            const TICKER_DOMAIN_MAP = {AAPL:'apple.com', MSFT:'microsoft.com', GOOGL:'google.com', AMZN:'amazon.com', NVDA:'nvidia.com' };
    const CDN = 'https://cdn.tickerlogos.com/';

    window.handleLogoError = function(img, symbol, type) {
                if (!img.dataset.triedSynth) {
        img.dataset.triedSynth = '1';
    img.src = 'https://logo.synthfinance.com/ticker/' + (symbol || '');
    return;
                }
    img.style.display = 'none';
    const wrap = img.closest('.asset-logo-wrap');
    if (wrap) {
        let fb = wrap.querySelector('.asset-logo-fallback');
    if (!fb) {
        fb = document.createElement('span');
    fb.className = 'asset-logo-fallback';
    fb.textContent = symbol ? symbol.charAt(0).toUpperCase() : '?';
    wrap.appendChild(fb);
                    }
    fb.style.display = '';
                }
            };

            document.addEventListener('DOMContentLoaded', () => {
                if (typeof lucide !== 'undefined') lucide.createIcons();
                document.querySelectorAll('.k-fade').forEach((el, i) => {
        setTimeout(() => el.classList.add('visible'), 70 + i * 80);
                });

                document.querySelectorAll('img[data-stock-ticker]').forEach(img => {
                    const ticker = img.getAttribute('data-stock-ticker') || '';
    const domain = TICKER_DOMAIN_MAP[ticker.toUpperCase()];
    if (domain) {
        img.src = CDN + domain;
    img.style.display = 'block';
    const fb = img.parentElement.querySelector('.asset-logo-fallback');
    if (fb) fb.style.display = 'none';
                    }
                });
            });
        })();
</script>