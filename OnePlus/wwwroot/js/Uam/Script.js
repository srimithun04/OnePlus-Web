document.addEventListener('DOMContentLoaded', function () {
    const signUpButton = document.getElementById('signUpBtn');
    const signInButton = document.getElementById('signInBtn');
    const formContainer = document.getElementById('formContainer');

    if (signUpButton && formContainer) {
        signUpButton.addEventListener('click', (e) => {
            e.preventDefault(); // Prevent form submission if it's inside a form
            formContainer.classList.add('left-panel-active');
        });
    }

    if (signInButton && formContainer) {
        signInButton.addEventListener('click', (e) => {
            e.preventDefault();
            formContainer.classList.remove('left-panel-active');
        });
    }
});
