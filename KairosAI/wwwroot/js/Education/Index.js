
    if (typeof lucide !== 'undefined') lucide.createIcons();
        document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.k-fade').forEach((el, i) => setTimeout(() => el.classList.add('visible'), 60 + i * 80));
            setTimeout(() => { const xp = document.getElementById('xpFill'); if (xp) xp.style.width = xp.dataset.target + '%'; }, 350);
            setTimeout(() => {document.querySelectorAll('.mod-progress-fill').forEach(bar => { bar.style.width = (bar.dataset.target || 0) + '%'; }); }, 500);
            document.querySelectorAll('.step-line.done').forEach((line, i) => {line.style.animationDelay = (i * 120) + 'ms'; });
        });
    function filterModules(btn, filter) {
        document.querySelectorAll('.filter-pill').forEach(p => p.classList.remove('active')); btn.classList.add('active');
    const cards = document.querySelectorAll('.module-card');
    let visible = 0;
            cards.forEach(card => {
        let show = false;
    if (filter === 'all') show = true;
                else if (filter === 'progress') show = parseInt(card.dataset.progress) > 0 && parseInt(card.dataset.progress) < 100;
    else show = card.dataset.level === filter;
    card.style.transition = 'opacity 0.22s ease, transform 0.22s ease';
    if (show) {card.style.opacity = '1'; card.style.transform = 'scale(1)'; card.style.display = ''; card.style.pointerEvents = 'auto'; visible++; }
    else {card.style.opacity = '0'; card.style.transform = 'scale(0.96)'; card.style.pointerEvents = 'none'; setTimeout(() => { if (card.style.opacity === '0') card.style.display = 'none'; }, 220); }
            });
    const counter = document.getElementById('moduleCount'); if (counter) counter.textContent = visible + ' módulos';
        }
