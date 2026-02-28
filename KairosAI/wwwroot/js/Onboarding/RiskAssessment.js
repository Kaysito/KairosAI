
            let currentStep = 1;
            const TOTAL_STEPS = 6;

            const scores = { axisA: 0, axisB: 0 };

            const CRYPTOS = [
                { id:'BTC', sym:'BTC', label:'Bitcoin', color:'#f97316' },
                { id:'ETH', sym:'ETH', label:'Ethereum', color:'#818cf8' },
                { id:'SOL', sym:'SOL', label:'Solana', color:'#10b981' },
                { id:'ADA', sym:'ADA', label:'Cardano', color:'#3b82f6' },
                { id:'DOGE', sym:'DOGE', label:'Dogecoin', color:'#eab308' },
                { id:'XRP', sym:'XRP', label:'Ripple', color:'#06b6d4' },
                { id:'DOT', sym:'DOT', label:'Polkadot', color:'#ec4899' },
                { id:'AVAX', sym:'AVAX', label:'Avalanche', color:'#ef4444' },
                { id:'LINK', sym:'LINK', label:'Chainlink', color:'#3b82f6' },
                { id:'MATIC', sym:'MATIC', label:'Polygon', color:'#8b5cf6' },
                { id:'LTC', sym:'LTC', label:'Litecoin', color:'#94a3b8' },
                { id:'SHIB', sym:'SHIB', label:'Shiba Inu',color:'#f59e0b' },
            ];

            let selectedCryptos = new Set(['BTC', 'ETH']);
            let selectedMarkets = new Set(['Cripto']);

            document.addEventListener('DOMContentLoaded', () => {
                lucide.createIcons();
                renderDots();
                renderCandleChart();
                renderCryptoChips();
            });

            // ── Función para asegurar el envío del form ──
            function submitAssessmentForm(e) {
                e.preventDefault(); // Detenemos el comportamiento default
                const form = document.getElementById('riskForm');
                form.submit(); // Disparamos el submit explícitamente por Javascript
            }

            function renderDots() {
                const container = document.getElementById('dots-container');
                container.innerHTML = '';
                for (let i = 1; i <= TOTAL_STEPS; i++) {
                    const dot = document.createElement('div');
                    dot.className = `dot w-2 h-2 rounded-full ${i === 1 ? 'bg-k-purple scale-125' : 'bg-gray-700'}`;
                    dot.id = `dot-${i}`;
                    container.appendChild(dot);
                }
            }

            function updateDots() {
                for (let i = 1; i <= TOTAL_STEPS; i++) {
                    const dot = document.getElementById(`dot-${i}`);
                    if (i < currentStep) {
                        dot.className = 'dot w-2 h-2 rounded-full bg-green-500';
                    } else if (i === currentStep) {
                        dot.className = 'dot w-2 h-2 rounded-full bg-k-purple scale-125';
                    } else {
                        dot.className = 'dot w-2 h-2 rounded-full bg-gray-700';
                    }
                }
            }

            function renderCandleChart() {
                const svg = document.getElementById('candle-chart');
                const candles = [
                    [60,75,50,70], [70,78,62,65], [65,80,60,78], [78,85,72,82],
                    [82,88,75,76], [76,84,70,83], [83,92,80,90],
                ];
                const W = 420, H = 140, PAD = 15;
                const maxP = 95, minP = 48, range = maxP - minP;
                const scaleY = (p) => PAD + ((maxP - p) / range) * (H - PAD * 2);
                const candleW = 32, spacing = (W - PAD * 2) / candles.length;
                let svgContent = '';

                candles.forEach((c, i) => {
                    const [o, h, l, cl] = c;
                    const x = PAD + i * spacing + spacing / 2;
                    const isGreen = cl >= o;
                    const top  = scaleY(Math.max(o, cl)), bot  = scaleY(Math.min(o, cl));
                    const bodyH = Math.max(bot - top, 2);
                    svgContent += `<line x1="${x}" y1="${scaleY(h)}" x2="${x}" y2="${scaleY(l)}" stroke="${isGreen ? '#10b981' : '#ef4444'}" stroke-width="1.5" opacity="0.8"/>`;
                    svgContent += `<rect x="${x - candleW/2 + 2}" y="${top}" width="${candleW - 4}" height="${bodyH}" fill="${isGreen ? '#10b981' : '#ef4444'}" rx="2" opacity="0.9"/>`;
                });

                const maPoints = candles.map((c, i) => `${PAD + i * spacing + spacing / 2},${scaleY((c[0] + c[3]) / 2)}`).join(' ');
                svgContent += `<polyline points="${maPoints}" fill="none" stroke="#7c3aed" stroke-width="1.5" stroke-dasharray="3,3" opacity="0.6"/>`;

                for (let p = 60; p <= 90; p += 10) {
                    const y = scaleY(p);
                    svgContent += `<line x1="${PAD}" y1="${y}" x2="${W - PAD}" y2="${y}" stroke="rgba(255,255,255,0.04)" stroke-width="1"/>`;
                    svgContent += `<text x="${PAD - 2}" y="${y + 4}" fill="rgba(255,255,255,0.2)" font-size="9" text-anchor="end" font-family="DM Mono">${p}K</text>`;
                }
                svg.innerHTML = svgContent;
            }

            function renderCryptoChips() {
                const container = document.getElementById('crypto-chips');
                container.innerHTML = '';
                CRYPTOS.forEach(c => {
                    const chip = document.createElement('div');
                    chip.className = 'crypto-chip rounded-xl px-3 py-2 flex items-center gap-2 text-sm font-mono';
                    chip.dataset.id = c.id;
                    chip.innerHTML = `<div class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold" style="background:${c.color}22;color:${c.color}">${c.sym[0]}</div><span class="text-gray-300">${c.sym}</span><span class="text-gray-600 text-xs hidden sm:inline">${c.label}</span>`;

                    if(c.id === 'BTC' || c.id === 'ETH') chip.classList.add('active');

                    chip.addEventListener('click', () => toggleCrypto(chip, c.id));
                    container.appendChild(chip);
                });
                updateCryptoCount();
            }

            function toggleCrypto(el, id) {
                if (selectedCryptos.has(id)) {
                    selectedCryptos.delete(id);
                    el.classList.remove('active');
                } else {
                    selectedCryptos.add(id);
                    el.classList.add('active');
                }
                document.getElementById('selectedCryptosInput').value = [...selectedCryptos].join(',');
                updateCryptoCount();
                checkStep5Valid();
            }

            function toggleMarket(el) {
                const val = el.dataset.value;
                const icon = el.querySelector('.check-market i');
                if (selectedMarkets.has(val)) {
                    selectedMarkets.delete(val);
                    el.classList.remove('selected');
                    icon.classList.add('hidden');
                } else {
                    selectedMarkets.add(val);
                    el.classList.add('selected');
                    icon.classList.remove('hidden');
                }
                document.getElementById('selectedMarketsInput').value = [...selectedMarkets].join(',');
                lucide.createIcons();
            }

            function updateCryptoCount() {
                const btn = document.getElementById('btn-next');
                if (currentStep === 5) btn.disabled = selectedCryptos.size < 2;
            }

            function checkStep5Valid() {
                document.getElementById('btn-next').disabled = selectedCryptos.size < 2;
            }

            function selectOption(el) {
                const group = el.dataset.group;
                document.querySelectorAll(`[data-group="${group}"]`).forEach(card => {
                    card.classList.remove('selected');
                    card.querySelector('.check-icon').style.opacity = '0';
                });
                el.classList.add('selected');
                el.querySelector('.check-icon').style.opacity = '1';

                const axisA = parseInt(el.dataset.axisA || 0);
                const axisB = parseInt(el.dataset.axisB || 0);

                if (['s1','s2'].includes(group)) {
                    scores['axisA'] = (scores['axisA'] || 0);
                    if (group === 's1') { scores.s1a = axisA; document.getElementById('h-b1').value = el.dataset.value; }
                    if (group === 's2') { scores.s2a = axisA; document.getElementById('h-b2').value = el.dataset.value; }
                }
                if (group === 's3') { scores.s3b = axisB; document.getElementById('h-b3').value = el.dataset.value; }
                if (group === 's4') { scores.s4b = axisB; document.getElementById('h-b4').value = el.dataset.value; }

                scores.axisA = (scores.s1a || 0) + (scores.s2a || 0);
                scores.axisB = (scores.s3b || 0) + (scores.s4b || 0);

                document.getElementById('btn-next').disabled = false;
            }

            function changeStep(direction) {
                if (direction === 1 && !canProceed()) return;

                const current = document.querySelector(`.step-container[data-step="${currentStep}"]`);
                current.classList.remove('active');

                currentStep += direction;

                const next = document.querySelector(`.step-container[data-step="${currentStep}"]`);
                setTimeout(() => { next.classList.add('active'); lucide.createIcons(); }, 80);

                document.getElementById('step-counter').textContent = `0${currentStep < 10 ? currentStep : currentStep}/0${TOTAL_STEPS}`;
                document.getElementById('progress-bar').style.width = `${(currentStep / TOTAL_STEPS) * 100}%`;

                updateDots();

                document.getElementById('btn-prev').classList.toggle('hidden', currentStep === 1);

                const btnNext = document.getElementById('btn-next');

                if (currentStep === 6) {
                    document.getElementById('nav-buttons').classList.add('hidden');
                    startAIAnalysis();
                } else if (currentStep === 5) {
                    btnNext.disabled = selectedCryptos.size < 2;
                    btnNext.innerHTML = 'Finalizar <i data-lucide="check" style="width:18px;height:18px"></i>';
                    lucide.createIcons();
                } else {
                    const stepEl = document.querySelector(`.step-container[data-step="${currentStep}"]`);
                    const checked = stepEl ? stepEl.querySelector('.opt-card.selected') : null;
                    btnNext.disabled = !checked;
                    btnNext.innerHTML = 'Siguiente <i data-lucide="arrow-right" style="width:18px;height:18px"></i>';
                    lucide.createIcons();
                }
            }

            function canProceed() {
                if (currentStep === 5) return selectedCryptos.size >= 2;
                const stepEl = document.querySelector(`.step-container[data-step="${currentStep}"]`);
                return stepEl ? !!stepEl.querySelector('.opt-card.selected') : true;
            }

            function startAIAnalysis() {
                const texts = ["Procesando eje conductual...", "Cruzando respuestas con matrices de riesgo...", "Evaluando nivel de conocimiento técnico...", "Calibrando personalidad de la IA...", "Generando perfil final..."];
                const loadingBar = document.getElementById('loading-bar');
                loadingBar.style.width = '100%';

                let i = 0;
                const textEl = document.getElementById('loading-text');
                const interval = setInterval(() => {
                    i++;
                    if (i < texts.length) {
                        textEl.textContent = texts[i];
                    } else {
                        clearInterval(interval);
                        showResult();
                    }
                }, 640);
            }

            function showResult() {
                const axisA = scores.axisA || 2, axisB = scores.axisB || 2;
                const percA = ((axisA - 2) / 4) * 100, percB = ((axisB - 2) / 4) * 100;
                const labelA = axisA <= 2 ? 'Pasivo' : axisA <= 4 ? 'Moderado' : 'Agresivo';
                const labelB = axisB <= 2 ? 'Principiante' : axisB <= 4 ? 'Intermedio' : 'Experto';

                let riskTitle, riskPerc, riskColor, riskBarColor, aiMessage, riskLabel;

                if (axisA <= 2) {
                    riskTitle = "Inversor Cauteloso"; riskPerc = 20; riskColor = "text-blue-400";
                    riskBarColor = "bg-blue-500"; riskLabel = "BAJO";
                    aiMessage = `Eres ${labelB === 'Principiante' ? 'nuevo en el mundo de las inversiones y' : 'alguien que'} prefiere la seguridad sobre el rendimiento. Te hablaré con calma, protegeré tu capital de decisiones impulsivas y te guiaré paso a paso. Activo el Escudo Anti-Pánico en modo máximo.`;
                } else if (axisA <= 4) {
                    if (axisB <= 2) {
                        riskTitle = "Explorador Prudente"; riskPerc = 45; riskColor = "text-k-purple";
                        riskBarColor = "from-blue-500 to-k-purple bg-gradient-to-r"; riskLabel = "MODERADO";
                        aiMessage = `Tienes curiosidad e instinto, pero aún estás construyendo tu base de conocimiento. Te daré contexto educativo en cada recomendación y te protegeré de los sesgos más comunes como el FOMO. El camino correcto es aprender mientras inviertes.`;
                    } else {
                        riskTitle = "Estratega Equilibrado"; riskPerc = 55; riskColor = "text-k-purple";
                        riskBarColor = "from-k-purple to-k-cyan bg-gradient-to-r"; riskLabel = "MODERADO-ALTO";
                        aiMessage = `Tienes base sólida y control emocional. Te presentaré análisis balanceados, sin sobreprotección pero sí con alertas precisas cuando el mercado muestre señales de peligro. Trabajaremos como socios estratégicos.`;
                    }
                } else {
                    if (axisB <= 2) {
                        riskTitle = "Principiante Agresivo"; riskPerc = 80; riskColor = "text-red-400";
                        riskBarColor = "from-k-pink to-red-500 bg-gradient-to-r"; riskLabel = "ALTO ⚠️";
                        aiMessage = `Tu energía es admirable, pero tu conocimiento aún está en construcción — y eso en mercados volátiles puede costar caro. Activaré el Escudo Anti-Pánico con Fricción Positiva reforzada para protegerte de ti mismo mientras aprendes. Serás agresivo con inteligencia.`;
                    } else {
                        riskTitle = "Operador de Alto Vuelo"; riskPerc = 90; riskColor = "text-k-pink";
                        riskBarColor = "from-k-purple via-k-pink to-red-500 bg-gradient-to-r"; riskLabel = "ELEVADO";
                        aiMessage = `Sé lo que buscas. Te hablaré directo, con jerga técnica, datos duros y sin filtros paternalistas. Conoces el juego y sus reglas. Mis intervenciones serán quirúrgicas, solo cuando el riesgo sistémico lo justifique. De tú a tú.`;
                    }
                }

                document.getElementById('result-title').textContent = riskTitle;
                document.getElementById('result-title').className = `text-3xl sm:text-4xl font-extrabold mb-1 ${riskColor}`;
                document.getElementById('result-subtitle').textContent = `${labelA} · ${labelB} · Riesgo ${riskLabel}`;
                document.getElementById('axis-a-label').textContent = `${labelA} (${Math.round(percA)}%)`;
                document.getElementById('axis-b-label').textContent = `${labelB} (${Math.round(percB)}%)`;
                document.getElementById('risk-label').textContent = riskLabel;
                document.getElementById('risk-label').className = `font-mono text-xs font-bold ${riskColor}`;
                document.getElementById('risk-bar').className = `axis-bar h-full rounded-full ${riskBarColor}`;
                document.getElementById('result-desc').textContent = aiMessage;

                document.getElementById('loading-state').classList.add('hidden');
                document.getElementById('result-state').classList.remove('hidden');
                lucide.createIcons();

                setTimeout(() => {
                    document.getElementById('axis-a-bar').style.width = `${Math.max(percA, 5)}%`;
                    document.getElementById('axis-b-bar').style.width = `${Math.max(percB, 5)}%`;
                    document.getElementById('risk-bar').style.width   = `${riskPerc}%`;
                }, 200);
            }

tailwind.config = {
    theme: {
        extend: {
            fontFamily: {
                sans: ['Space Grotesk', 'system-ui', 'sans-serif'],
                mono: ['JetBrains Mono', 'monospace'],
            },
            colors: {
                'k-purple': '#7c3aed',
                'k-cyan': '#06b6d4',
                'k-pink': '#ec4899',
                'k-dark': '#080811',
                'k-card': '#10101e',
            }
        }
    }
}