
        if (typeof lucide !== 'undefined') lucide.createIcons();

        // ── Fade inicial y Veredicto Kairós ──
        document.addEventListener('DOMContentLoaded', () => {
            document.querySelectorAll('.k-fade').forEach((el, i) => {
                setTimeout(() => el.classList.add('visible'), 60 + i * 80);
            });

            // Animación de las barras del veredicto
            setTimeout(() => {
                const fillPos = document.getElementById('fillPos');
        const fillNeu = document.getElementById('fillNeu');
        const fillNeg = document.getElementById('fillNeg');

        if (fillPos) fillPos.style.width = fillPos.dataset.target + '%';
        if (fillNeu) fillNeu.style.width = fillNeu.dataset.target + '%';
        if (fillNeg) fillNeg.style.width = fillNeg.dataset.target + '%';
            }, 400);
        });

        // ── Filter tabs ──
        function filterNews(btn, category) {
            // Pill activa
            document.querySelectorAll('.filter-pill').forEach(p => {
                p.classList.remove('active');
                p.setAttribute('aria-selected', 'false');
            });
        btn.classList.add('active');
        btn.setAttribute('aria-selected', 'true');

        // Filtrar cards
        const cards = document.querySelectorAll('#newsGrid article');
            cards.forEach(card => {
                const match = category === 'todo' || card.dataset.category === category;
        card.style.transition = 'opacity 0.25s ease, transform 0.25s ease';
        if (match) {
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
        }
