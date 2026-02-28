
    if (typeof lucide !== 'undefined') lucide.createIcons();

        // ── Fade in ──
        document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.k-fade').forEach((el, i) => {
            setTimeout(() => el.classList.add('visible'), 60 + i * 80);
        });

            // Tier bar
            setTimeout(() => {
                const fill = document.getElementById('tierFill');
    if (fill) fill.style.width = fill.dataset.target + '%';
            }, 350);
        });

    // ── Filter ──
    function filterRewards(btn, category) {
        document.querySelectorAll('.filter-pill').forEach(p => p.classList.remove('active'));
    btn.classList.add('active');

            document.querySelectorAll('.reward-card').forEach(card => {
                const match = category === 'all' || card.dataset.category === category;
    card.style.transition = 'opacity 0.22s ease, transform 0.22s ease';
    if (match) {
        card.style.opacity = '1';
    card.style.transform = 'scale(1)';
    card.style.display   = '';
    card.style.pointerEvents = 'auto';
                } else {
        card.style.opacity = '0';
    card.style.transform = 'scale(0.96)';
    card.style.pointerEvents = 'none';
                    setTimeout(() => { if (card.style.opacity === '0') card.style.display = 'none'; }, 220);
                }
            });
        }
