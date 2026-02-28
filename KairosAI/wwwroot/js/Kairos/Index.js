
    if (typeof lucide !== 'undefined') lucide.createIcons();

    const chatContainer = document.getElementById('chatContainer');
    const chatInput     = document.getElementById('chatInput');
    const chatForm      = document.getElementById('chatForm');
    const sendBtn       = document.getElementById('sendBtn');
    const stopBtn       = document.getElementById('stopBtn');
    const typingInd     = document.getElementById('typingIndicator');

    let fetchController = null;

        // Renderizar Markdown de mensajes pre-existentes (cargados desde el servidor)
        document.querySelectorAll('.raw-markdown').forEach(el => {
        el.innerHTML = marked.parse(el.textContent);
    el.classList.remove('raw-markdown');
        });

    function scrollToBottom() {
            if (chatContainer) chatContainer.scrollTop = chatContainer.scrollHeight;
        }

    function autoResize(el) {
        el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 140) + 'px';
        }

    function handleEnter(e) {
            if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
    chatForm.dispatchEvent(new Event('submit'));
            }
        }

    window.fillInput = function(text) {
            if (chatInput) {
        chatInput.value = text;
    chatInput.focus();
    autoResize(chatInput);
            }
        };

    window.stopGeneration = function() {
            if (fetchController) {
        fetchController.abort();
    resetUI();
            }
        };

    function resetUI() {
        typingInd.classList.add('hidden');
    stopBtn.classList.add('hidden');
    sendBtn.classList.remove('hidden');
    chatInput.disabled = false;
    chatInput.focus();
        }

    // Obtiene o crea el wrapper de mensajes, ocultando la pantalla vacía si es necesario
    function getOrCreateWrapper() {
        let wrapper = document.getElementById('messagesWrapper');
    if (!wrapper) {
                const emptyState = document.getElementById('emptyState');
    if (emptyState) emptyState.style.display = 'none';

    wrapper = document.createElement('div');
    wrapper.id = 'messagesWrapper';
    wrapper.style.cssText = 'max-width:740px;margin:0 auto;display:flex;flex-direction:column;gap:22px;';
    chatContainer.insertBefore(wrapper, typingInd);
            }
    return wrapper;
        }

    function appendUserMessage(text) {
            const wrapper = getOrCreateWrapper();
    const safe = text.replace(/&/g,'&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
wrapper.insertAdjacentHTML('beforeend', `
                <div class="msg-enter" style="display:flex;gap:11px;align-items:flex-start;flex-direction:row-reverse;">
                    <div class="avatar-user">KH</div>
                    <div style="max-width:76%;flex-shrink:0;">
                        <div class="bubble-user" style="padding:11px 15px;border-radius:18px;border-top-right-radius:4px;font-size:0.875rem;line-height:1.6;word-break:break-word;">${safe}</div>
                    </div>
                </div>`);
        }

// Crea la burbuja de respuesta de Kairos y devuelve el div donde escribir el texto
function appendAiMessage() {
    const wrapper = getOrCreateWrapper();
    const id = 'ai-msg-' + Date.now();
    wrapper.insertAdjacentHTML('beforeend', `
                <div class="msg-enter" style="display:flex;gap:11px;align-items:flex-start;">
                    <div class="avatar-ai"><i data-lucide="bot" style="width:15px;height:15px;"></i></div>
                    <div style="flex:1;min-width:0;">
                        <div style="font-size:0.58rem;font-weight:800;color:#7c3aed;letter-spacing:0.12em;text-transform:uppercase;margin-bottom:7px;display:flex;align-items:center;gap:5px;">
                            KAIROS AI <span style="width:3px;height:3px;border-radius:50%;background:rgba(124,58,237,0.35);"></span>
                            <span style="font-weight:500;color:var(--text-muted);letter-spacing:0.03em;text-transform:none;font-size:0.56rem;">v2.4</span>
                        </div>
                        <div id="${id}" class="ai-prose" style="min-height:1em;"></div>
                    </div>
                </div>`);
    if (typeof lucide !== 'undefined') lucide.createIcons();
    return document.getElementById(id);
}

chatForm.addEventListener('submit', async function (e) {
    e.preventDefault();
    const text = chatInput.value.trim();
    if (!text) return;

    appendUserMessage(text);
    chatInput.value = '';
    autoResize(chatInput);

    typingInd.classList.remove('hidden');
    sendBtn.classList.add('hidden');
    stopBtn.classList.remove('hidden');
    chatInput.disabled = true;
    scrollToBottom();

    fetchController = new AbortController();

    try {
        const formData = new FormData();
        formData.append('CurrentInput', text);

        const response = await fetch('/Kairos/SendApi', {
            method: 'POST',
            body: formData,
            signal: fetchController.signal
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        // Ocultar typing indicator y crear burbuja de respuesta
        typingInd.classList.add('hidden');
        const aiDiv = appendAiMessage();
        let rawText = '';

        // ✅ LECTURA REAL DEL STREAM SSE
        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = '';

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            buffer += decoder.decode(value, { stream: true });

            // Procesar líneas completas del buffer
            const lines = buffer.split('\n');
            buffer = lines.pop(); // guardar línea incompleta

            for (const line of lines) {
                if (!line.startsWith('data:')) continue;
                const payload = line.slice(5).trim();
                if (payload === '[DONE]') break;

                try {
                    // Los chunks vienen serializados como JSON string (para escapar \n)
                    const chunk = JSON.parse(payload);
                    rawText += chunk;
                    // Renderizar Markdown en tiempo real
                    aiDiv.innerHTML = marked.parse(rawText);
                    scrollToBottom();
                } catch {
                    // Si por alguna razón no es JSON, usarlo directo
                    rawText += payload;
                    aiDiv.innerHTML = marked.parse(rawText);
                    scrollToBottom();
                }
            }
        }

    } catch (error) {
        if (error.name !== 'AbortError') {
            const wrapper = getOrCreateWrapper();
            wrapper.insertAdjacentHTML('beforeend', `
                        <div style="display:flex;gap:11px;align-items:flex-start;">
                            <div class="avatar-ai"><i data-lucide="bot" style="width:15px;height:15px;"></i></div>
                            <div class="ai-prose" style="color:#ef4444;font-size:0.82rem;">
                                ⚠️ Error de conexión. Por favor intenta de nuevo.
                            </div>
                        </div>`);
        }
    } finally {
        resetUI();
        scrollToBottom();
    }
});

window.onload = () => {
    scrollToBottom();
    chatInput?.focus();
};
