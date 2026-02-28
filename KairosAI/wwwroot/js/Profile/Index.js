
        (function () {
            'use strict';

            if (typeof lucide !== 'undefined') lucide.createIcons();

            /* ── Fade in orchestration ── */
            document.addEventListener('DOMContentLoaded', function () {
                document.querySelectorAll('.k-fade').forEach(function (el, i) {
                    setTimeout(function () { el.classList.add('visible'); }, 55 + i * 60);
                });
                /* Risk bar */
                setTimeout(function () {
                    var fill = document.getElementById('riskFill');
                    if (fill) fill.style.width = fill.dataset.target + '%';
                }, 400);
            });

            /* ── Tabs ── */
            window.switchTab = function (btn, panelId) {
                document.querySelectorAll('.tab-btn').forEach(function (b) {
                    b.classList.remove('active');
                    b.setAttribute('aria-selected', 'false');
                });
                document.querySelectorAll('.tab-panel').forEach(function (p) {
                    p.classList.remove('active');
                });

                btn.classList.add('active');
                btn.setAttribute('aria-selected', 'true');

                var panel = document.getElementById(panelId);
                if (!panel) return;
                panel.classList.add('active');
                panel.style.opacity   = '0';
                panel.style.transform = 'translateY(6px)';
                requestAnimationFrame(function () {
                    panel.style.transition = 'opacity .3s ease, transform .3s cubic-bezier(0.22,1,0.36,1)';
                    panel.style.opacity    = '1';
                    panel.style.transform  = 'translateY(0)';
                });
            };

            /* ═══════════════════════════════════════════════════
               SHIELD MODAL — SVG progress ring countdown
               r=24 → circumference = 2π × 24 ≈ 150.8
            ═══════════════════════════════════════════════════ */
            var CIRC     = 150.796;   /* 2π × 24 */
            var DURATION = 10;
            var shieldEl = null;
            var ticker   = null;
            var remaining;

            window.handleShieldToggle = function (el) {
                if (el.classList.contains('on')) {
                    shieldEl = el;
                    openShieldModal();
                } else {
                    el.classList.add('on');
                    el.setAttribute('aria-checked', 'true');
                    showToast('Escudo KairosAI activado. Tu portafolio vuelve a estar protegido.', 'success');
                }
            };

            function openShieldModal () {
                remaining = DURATION;

                /* DOM refs */
                var ring   = document.getElementById('ringFill');
                var numEl  = document.getElementById('ringNumber');
                var secEl  = document.getElementById('timerSec');
                var dot    = document.getElementById('timerDot');
                var status = document.getElementById('timerStatus');
                var btn    = document.getElementById('confirmDisableBtn');

                /* Reset visuals */
                ring.style.strokeDashoffset = '0';
                ring.classList.remove('expired');
                numEl.textContent = remaining;
                numEl.classList.remove('expired');
                secEl.textContent  = remaining;
                dot.classList.remove('done');
                status.classList.remove('hidden');
                btn.disabled = true;
                btn.setAttribute('aria-disabled', 'true');
                btn.classList.remove('ready');

                /* Show modal */
                var modal = document.getElementById('legalModal');
                modal.classList.add('open');
                setTimeout(function () {
                    modal.querySelector('.btn-shield-cancel').focus();
                }, 60);

                /* Tick every second */
                ticker = setInterval(function () {
                    remaining--;
                    numEl.textContent = remaining;
                    secEl.textContent = remaining;

                    /* Shrink ring proportionally: full → empty */
                    var progress = remaining / DURATION;            /* 1 → 0 */
                    ring.style.strokeDashoffset = String(CIRC * (1 - progress));

                    if (remaining <= 0) {
                        clearInterval(ticker);
                        ticker = null;
                        ring.classList.add('expired');
                        numEl.classList.add('expired');
                        dot.classList.add('done');

                        /* Update status text */
                        secEl.parentElement.innerHTML =
                            '<span style="color:var(--text-main);font-weight:600;">Puedes confirmar ahora</span>';

                        /* Unlock confirm button */
                        btn.disabled = false;
                        btn.setAttribute('aria-disabled', 'false');
                        btn.classList.add('ready');
                        btn.focus();
                    }
                }, 1000);
            }

            window.cancelShieldDisable = function () {
                if (ticker) { clearInterval(ticker); ticker = null; }
                document.getElementById('legalModal').classList.remove('open');
                if (shieldEl) {
                    shieldEl.classList.add('on');
                    shieldEl.setAttribute('aria-checked', 'true');
                }
            };

            window.confirmShieldDisable = function () {
                if (ticker) { clearInterval(ticker); ticker = null; }
                document.getElementById('legalModal').classList.remove('open');
                if (shieldEl) {
                    shieldEl.classList.remove('on');
                    shieldEl.setAttribute('aria-checked', 'false');
                }
                showToast('Intervención IA desactivada. Operando bajo tu propio riesgo.', 'error');
            };

            /* Close on backdrop */
            document.getElementById('legalModal').addEventListener('click', function (e) {
                if (e.target === this) window.cancelShieldDisable();
            });

            /* Close on Esc */
            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape' && document.getElementById('legalModal').classList.contains('open')) {
                    window.cancelShieldDisable();
                }
            });

            /* ── Generic toggle ── */
            window.toggleSwitch = function (track) {
                if (track.id === 'kairos-shield-toggle') return;
                track.classList.toggle('on');
                var on = track.classList.contains('on');
                track.setAttribute('aria-checked', String(on));
                track.style.transform = 'scale(0.9)';
                setTimeout(function () {
                    track.style.transition = 'transform .22s cubic-bezier(0.34,1.56,0.64,1)';
                    track.style.transform  = 'scale(1)';
                }, 70);
            };

            /* ── Password strength ── */
            window.evalStrength = function (input) {
                var val   = input.value;
                var bars  = document.querySelectorAll('#strengthBars .sb');
                var label = document.getElementById('strengthLabel');
                var score = 0;
                if (val.length >= 8)           score++;
                if (/[A-Z]/.test(val))         score++;
                if (/[0-9]/.test(val))         score++;
                if (/[^A-Za-z0-9]/.test(val))  score++;
                var colors = ['','#ef4444','#f59e0b','#3b82f6','#10b981'];
                var labels = ['','Débil','Regular','Buena','Fuerte'];
                bars.forEach(function (bar, i) {
                    bar.style.background = i < score ? colors[score] : 'var(--border-color)';
                });
                if (label) {
                    label.textContent = val.length ? labels[score] : '';
                    label.style.color = colors[score] || 'var(--text-muted)';
                }
            };

            /* ── Delete confirm ── */
            window.confirmDelete = function () {
                if (confirm('¿Estás seguro? Esta acción eliminará tu cuenta de forma permanente y no se puede deshacer.')) {
                    /* submit delete form */
                }
            };

            /* ── Toast ── */
            window.showToast = function (message, type) {
                var toast = document.getElementById('kairos-toast');
                var msg   = document.getElementById('toast-message');
                var icon  = document.getElementById('toast-icon');
                msg.textContent = message;
                if (type === 'error') {
                    toast.style.borderColor = 'rgba(239,68,68,0.3)';
                    toast.style.color       = '#ef4444';
                    icon.innerHTML          = '<i data-lucide="shield-off" style="width:14px;height:14px;flex-shrink:0;"></i>';
                } else {
                    toast.style.borderColor = 'rgba(16,185,129,0.3)';
                    toast.style.color       = '#10b981';
                    icon.innerHTML          = '<i data-lucide="shield-check" style="width:14px;height:14px;flex-shrink:0;"></i>';
                }
                if (typeof lucide !== 'undefined') lucide.createIcons();
                toast.classList.add('show');
                setTimeout(function () { toast.classList.remove('show'); }, 4200);
            };

        })();
