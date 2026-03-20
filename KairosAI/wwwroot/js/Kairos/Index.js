// =====================================================================
// KairosAI — Chat Client
// =====================================================================

(function () {
    'use strict';

    // ── Configuración ──
    const CONFIG = {
        maxInputLength: 2000,
        sendEndpoint: '/Kairos/SendApi',
        appVersion: 'v2.4'
    };

    // ── Referencias DOM ──
    const chatContainer = document.getElementById('chatContainer');
    const chatInput = document.getElementById('chatInput');
    const chatForm = document.getElementById('chatForm');
    const sendBtn = document.getElementById('sendBtn');
    const stopBtn = document.getElementById('stopBtn');
    const typingInd = document.getElementById('typingIndicator');

    // ── Estado ──
    let fetchController = null;
    let scrollRafId = null;
    let isGenerating = false;

    // ================================================================
    // INICIALIZACIÓN
    // ================================================================

    function init() {
        // Renderizar iconos Lucide
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }

        // Configurar marked.js
        if (typeof marked !== 'undefined') {
            marked.setOptions({
                breaks: true,
                gfm: true,
                headerIds: false,
                mangle: false
            });
        }

        // Renderizar Markdown de mensajes pre-existentes
        document.querySelectorAll('.raw-markdown').forEach(function (el) {
            el.innerHTML = marked.parse(el.textContent);
            el.classList.remove('raw-markdown');
        });

        // Bind event listeners
        bindEvents();

        // Scroll al fondo y enfocar input
        scrollToBottom();
        if (chatInput) {
            chatInput.focus();
        }
    }

    // ================================================================
    // EVENT BINDING
    // ================================================================

    function bindEvents() {
        // Textarea: auto resize
        if (chatInput) {
            chatInput.addEventListener('input', function () {
                autoResize(this);
            });

            // Enter para enviar, Shift+Enter para nueva línea
            chatInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    chatForm.dispatchEvent(new Event('submit'));
                }
            });
        }

        // Suggestion chips
        document.querySelectorAll('.suggestion-chip[data-prompt]')
            .forEach(function (btn) {
                btn.addEventListener('click', function () {
                    fillInput(this.dataset.prompt);
                });
            });

        // Form submit
        if (chatForm) {
            chatForm.addEventListener('submit', handleFormSubmit);
        }
    }

    // ================================================================
    // UTILIDADES DOM
    // ================================================================

    function scrollToBottom() {
        if (!chatContainer) return;
        chatContainer.scrollTo({
            top: chatContainer.scrollHeight,
            behavior: 'smooth'
        });
    }

    function throttledScroll() {
        if (scrollRafId) return;
        scrollRafId = requestAnimationFrame(function () {
            scrollToBottom();
            scrollRafId = null;
        });
    }

    function autoResize(el) {
        el.style.height = 'auto';
        el.style.height = Math.min(el.scrollHeight, 140) + 'px';
    }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function fillInput(text) {
        if (!chatInput) return;
        chatInput.value = text;
        chatInput.focus();
        autoResize(chatInput);
    }

    function stopGeneration() {
        if (fetchController) {
            fetchController.abort();
            resetUI();
        }
    }

    // ================================================================
    // UI STATE
    // ================================================================

    function setGeneratingState() {
        isGenerating = true;
        typingInd.classList.remove('hidden');
        sendBtn.classList.add('hidden');
        stopBtn.classList.remove('hidden');
        chatInput.disabled = true;
    }

    function resetUI() {
        isGenerating = false;
        typingInd.classList.add('hidden');
        stopBtn.classList.add('hidden');
        sendBtn.classList.remove('hidden');
        chatInput.disabled = false;
        chatInput.focus();
    }

    // ================================================================
    // MESSAGE CREATION
    // ================================================================

    function getOrCreateWrapper() {
        var wrapper = document.getElementById('messagesWrapper');
        if (!wrapper) {
            var emptyState = document.getElementById('emptyState');
            if (emptyState) {
                emptyState.style.display = 'none';
            }

            wrapper = document.createElement('div');
            wrapper.id = 'messagesWrapper';
            wrapper.className = 'k-messages';
            chatContainer.insertBefore(wrapper, typingInd);
        }
        return wrapper;
    }

    function appendUserMessage(text) {
        var wrapper = getOrCreateWrapper();
        var safe = escapeHtml(text);
        wrapper.insertAdjacentHTML('beforeend',
            '<div class="msg-enter k-msg k-msg--user">' +
            '<div class="avatar-user">KH</div>' +
            '<div class="k-msg__body--user">' +
            '<div class="bubble-user">' + safe + '</div>' +
            '</div>' +
            '</div>'
        );
    }

    function appendAiMessage() {
        var wrapper = getOrCreateWrapper();
        var id = 'ai-msg-' + Date.now();
        wrapper.insertAdjacentHTML('beforeend',
            '<div class="msg-enter k-msg">' +
            '<div class="avatar-ai">' +
            '<i data-lucide="bot"></i>' +
            '</div>' +
            '<div class="k-msg__body">' +
            '<div class="k-msg__label">' +
            'KAIROS AI ' +
            '<span class="k-msg__label-dot"></span>' +
            '<span class="k-msg__label-version">' + CONFIG.appVersion + '</span>' +
            '</div>' +
            '<div id="' + id + '" class="ai-prose"></div>' +
            '</div>' +
            '</div>'
        );
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
        return document.getElementById(id);
    }

    function appendErrorMessage(errorText) {
        var wrapper = getOrCreateWrapper();
        var message = errorText || '⚠️ Error de conexión. Intenta de nuevo.';
        wrapper.insertAdjacentHTML('beforeend',
            '<div class="msg-enter k-msg">' +
            '<div class="avatar-ai">' +
            '<i data-lucide="bot"></i>' +
            '</div>' +
            '<div class="k-msg__body">' +
            '<div class="ai-prose" style="color:#ef4444;font-size:0.82rem;">' +
            escapeHtml(message) +
            '</div>' +
            '</div>' +
            '</div>'
        );
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }

    // ================================================================
    // CSRF TOKEN
    // ================================================================

    function getCsrfToken() {
        var input = chatForm.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : null;
    }

    // ================================================================
    // STREAM PROCESSING
    // ================================================================

    async function processStream(response, aiDiv) {
        var reader = response.body.getReader();
        var decoder = new TextDecoder();
        var buffer = '';
        var rawText = '';

        try {
            while (true) {
                var result = await reader.read();
                if (result.done) break;

                buffer += decoder.decode(result.value, { stream: true });

                // Procesar líneas completas
                var lines = buffer.split('\n');
                buffer = lines.pop(); // Guardar línea incompleta

                for (var i = 0; i < lines.length; i++) {
                    var line = lines[i];
                    if (!line.startsWith('data:')) continue;

                    var payload = line.slice(5).trim();
                    if (payload === '[DONE]') return rawText;

                    try {
                        var chunk = JSON.parse(payload);
                        rawText += chunk;
                        aiDiv.innerHTML = marked.parse(rawText);
                        throttledScroll();
                    } catch (parseError) {
                        // Si no es JSON válido, usar como texto plano
                        rawText += payload;
                        aiDiv.innerHTML = marked.parse(rawText);
                        throttledScroll();
                    }
                }
            }
        } finally {
            reader.releaseLock();
        }

        return rawText;
    }

    // ================================================================
    // FORM SUBMIT HANDLER
    // ================================================================

    async function handleFormSubmit(e) {
        e.preventDefault();

        // Prevenir doble envío
        if (isGenerating) return;

        var text = chatInput.value.trim();
        if (!text) return;

        // Validación de longitud
        if (text.length > CONFIG.maxInputLength) {
            alert('El mensaje es demasiado largo (máximo ' +
                CONFIG.maxInputLength + ' caracteres).');
            return;
        }

        // Agregar mensaje del usuario al DOM
        appendUserMessage(text);
        chatInput.value = '';
        autoResize(chatInput);

        // Cambiar estado de UI
        setGeneratingState();
        scrollToBottom();

        // Crear AbortController
        fetchController = new AbortController();

        try {
            // Construir FormData
            var formData = new FormData();
            formData.append('CurrentInput', text);

            // Incluir CSRF token
            var csrfToken = getCsrfToken();
            if (csrfToken) {
                formData.append('__RequestVerificationToken', csrfToken);
            }

            // Enviar request
            var response = await fetch(CONFIG.sendEndpoint, {
                method: 'POST',
                body: formData,
                signal: fetchController.signal
            });

            if (!response.ok) {
                throw new Error('HTTP ' + response.status);
            }

            // Ocultar typing y crear burbuja de respuesta
            typingInd.classList.add('hidden');
            var aiDiv = appendAiMessage();

            // Procesar stream
            await processStream(response, aiDiv);

        } catch (error) {
            if (error.name !== 'AbortError') {
                console.error('KairosAI error:', error);
                appendErrorMessage('⚠️ Error de conexión. Intenta de nuevo.');
            }
        } finally {
            fetchController = null;
            resetUI();
            scrollToBottom();
        }
    }

    // ================================================================
    // API PÚBLICA (Mínima)
    // ================================================================

    window.fillInput = fillInput;
    window.stopGeneration = stopGeneration;

    // ================================================================
    // INICIAR
    // ================================================================

    // Esperar a que el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();