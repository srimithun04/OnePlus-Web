$(document).ready(function () {
    // --- Modal Elements ---
    const categoryModal = $('#categoryModal');
    const formView = $('#categoryFormView');
    const messageView = $('#categoryMessageView');
    const newCategoryNameInput = $('#newCategoryName');
    const saveCategoryBtn = $('#saveCategoryBtn');
    const messageIcon = $('#messageIcon');
    const messageText = $('#messageText');
    const messageOkBtn = $('#messageOkBtn');

    // --- Function to show the modal in its initial state (form view) ---
    function showFormModal() {
        // Reset to form view
        messageView.hide();
        formView.show();
        $('#categoryError').text('');
        newCategoryNameInput.val('');

        // Show the modal
        categoryModal.addClass('show');
        newCategoryNameInput.focus();
    }

    // --- Function to show a message in the modal ---
    function showMessageModal(message, isSuccess) {
        // Switch to message view
        formView.hide();
        messageView.show();

        // Set message and icon
        messageText.text(message);
        messageIcon.html(isSuccess ? '&#10004;' : '&#10008;'); // Checkmark or X
        messageIcon.removeClass('success error').addClass(isSuccess ? 'success' : 'error');

        // Ensure modal is visible
        categoryModal.addClass('show');
    }

    // --- Function to Hide the Modal completely ---
    function hideModal() {
        categoryModal.removeClass('show');
    }

    // --- Event Handlers ---

    // Show modal to ADD a category
    $('#addCategoryBtn').on('click', function (e) {
        e.preventDefault();
        showFormModal();
    });

    // Handle DELETE category button
    $('#deleteCategoryBtn').on('click', function () {
        const selectedCategory = $('#SelectedCategoryIds option:selected');

        if (selectedCategory.length !== 1) {
            alert('Please select exactly one category to delete.'); // Standard alert is fine for this simple check
            return;
        }

        const categoryId = selectedCategory.val();
        const categoryName = selectedCategory.text();

        if (!confirm(`Are you sure you want to delete the category "${categoryName}"?`)) {
            return;
        }

        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            type: "POST",
            url: "/Product/DeleteCategory",
            contentType: "application/json; charset=utf-8",
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify({ categoryId: parseInt(categoryId) }),
            success: function (response) {
                if (response.success) {
                    selectedCategory.remove();
                    // Show success message in modal instead of alert
                    showMessageModal(`Category "${categoryName}" was deleted successfully.`, true);
                }
            },
            error: function (xhr) {
                const errorMessage = xhr.responseJSON ? xhr.responseJSON.message : "Could not delete category.";
                // Show error message in modal
                showMessageModal(errorMessage, false);
            }
        });
    });

    // Hide modal when Cancel, Close, or OK button is clicked
    $('#cancelCategoryBtn, .close-btn, #messageOkBtn').on('click', hideModal);

    categoryModal.on('click', function (e) {
        if ($(e.target).is(categoryModal)) {
            hideModal();
        }
    });

    // Handle the Save button click (for adding)
    saveCategoryBtn.on('click', function () {
        const categoryName = newCategoryNameInput.val().trim();

        if (categoryName === '') {
            $('#categoryError').text('Category name cannot be empty.');
            return;
        } else {
            $('#categoryError').text('');
        }

        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            type: "POST",
            url: "/Product/AddCategory",
            contentType: "application/json; charset=utf-8",
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify({ name: categoryName }),
            success: function (response) {
                var newOption = new Option(response.name, response.id);
                $('#SelectedCategoryIds').append(newOption);
                // Show success message instead of alert
                showMessageModal('Category "' + response.name + '" added successfully!', true);
            },
            error: function (xhr) {
                const errorMessage = xhr.responseJSON ? xhr.responseJSON.message : "An unknown error occurred.";
                // Show error message, but in the form view's error span
                $('#categoryError').text("Error: " + errorMessage);
            }
        });
    });

    // Allow submitting modal with Enter key
    newCategoryNameInput.on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            saveCategoryBtn.click();
        }
    });
});