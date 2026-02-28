
    document.addEventListener('DOMContentLoaded', function() {
        lucide.createIcons();

    const form = document.getElementById('loginForm');
    const submitBtn = document.getElementById('submitBtn');
    const btnText = document.getElementById('btnText');
    const btnLoader = document.getElementById('btnLoader');

    window.togglePassword = function() {
                const input = document.getElementById('passwordInput');
    const icon = document.getElementById('eyeIcon');

    if (input.type === 'password') {
        input.type = 'text';
    icon.setAttribute('data-lucide', 'eye-off');
                } else {
        input.type = 'password';
    icon.setAttribute('data-lucide', 'eye');
                }
    lucide.createIcons();
            };

    form.addEventListener('submit', function(e) {
                if ($(form).valid()) {
        submitBtn.disabled = true;
    submitBtn.classList.add('opacity-80', 'cursor-not-allowed');
    btnText.classList.add('invisible');
    btnLoader.classList.remove('hidden');
                }
            });

    const validator = $(form).data('validator');
    if (validator) {
                 const originalHighlight = validator.settings.highlight;
    const originalUnhighlight = validator.settings.unhighlight;

    validator.settings.highlight = function (element, errorClass, validClass) {
        originalHighlight.call(this, element, errorClass, validClass);
    $(element).addClass('border-red-500/50 focus:ring-red-500 bg-red-900/10').removeClass('border-[#2d2d42] focus:ring-kairos-primary');
                 };

    validator.settings.unhighlight = function (element, errorClass, validClass) {
        originalUnhighlight.call(this, element, errorClass, validClass);
    $(element).removeClass('border-red-500/50 focus:ring-red-500 bg-red-900/10').addClass('border-[#2d2d42] focus:ring-kairos-primary');
                 };
            }
        });
