// Login page behaviour (vanilla JS + Bootstrap validation styles).
// Client-side checks are for convenience only; the server validates everything again.
(() => {
    'use strict';

    const form = document.getElementById('loginForm');
    if (!form) return;

    const passwordInput = document.getElementById('Password');
    const toggleButton = document.getElementById('togglePassword');
    const submitButton = document.getElementById('loginButton');

    // Show / hide password
    toggleButton?.addEventListener('click', () => {
        const show = passwordInput.type === 'password';
        passwordInput.type = show ? 'text' : 'password';
        toggleButton.textContent = show ? 'Hide' : 'Show';
        toggleButton.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
    });

    // Validate before submit, then prevent double submission
    form.addEventListener('submit', (event) => {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        submitButton.disabled = true;
        submitButton.querySelector('.spinner-border').classList.remove('d-none');
        submitButton.querySelector('.btn-text').textContent = 'Signing in...';
    });
})();
