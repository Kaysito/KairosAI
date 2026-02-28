
    document.addEventListener('DOMContentLoaded', function() {
        lucide.createIcons();

    // Función genérica para alternar visibilidad de contraseña
    // Acepta el ID del input y el ID del icono como parámetros
    window.togglePass = function(inputId, iconId) {
                const input = document.getElementById(inputId);
    const icon = document.getElementById(iconId);

    if (input.type === 'password') {
        input.type = 'text';
    icon.setAttribute('data-lucide', 'eye-off');
                } else {
        input.type = 'password';
    icon.setAttribute('data-lucide', 'eye');
                }
    lucide.createIcons();
            };

    // Efecto de carga
    const form = document.getElementById('registerForm');
    const btn = document.getElementById('submitBtn');
    const btnText = document.getElementById('btnText');
    const btnLoader = document.getElementById('btnLoader');
    const btnIcon = document.getElementById('btnIcon');

    form.addEventListener('submit', function() {
                if ($(this).valid()) {
        btn.disabled = true;
    btnText.innerText = 'Creando cuenta...';
    btnLoader.classList.remove('hidden');
    btnIcon.classList.add('hidden');
    btn.classList.add('opacity-75');
                }
            });
        });
