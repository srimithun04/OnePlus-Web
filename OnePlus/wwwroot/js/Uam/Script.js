document.addEventListener('DOMContentLoaded', function () {
    const signUpButton = document.getElementById('signUpBtn');
    const signInButton = document.getElementById('signInBtn');
    const container = document.getElementById('authContainer');

    if (signUpButton && container) {
        signUpButton.addEventListener('click', () => {
            container.classList.add('signup-active');
        });
    }

    if (signInButton && container) {
        signInButton.addEventListener('click', () => {
            container.classList.remove('signup-active');
        });
    }
});
