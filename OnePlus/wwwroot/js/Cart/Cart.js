// --- wwwroot/js/Cart/Cart.js ---
// This is a modern, professional script for handling all cart interactions.
// It uses async/await for clean API calls and provides instant user feedback.

document.addEventListener('DOMContentLoaded', function () {
    // --- Element Selectors ---
    const dynamicContent = document.getElementById('cart-dynamic-content');
    if (!dynamicContent) return; // Exit if the main container isn't on the page

    // --- Configuration ---
    const shippingCosts = {
        'store-pickup': 0,
        'delivery': 750.00 // Standard delivery fee in INR
    };
    const currencyLocale = 'en-IN';
    const currencyOptions = { style: 'currency', currency: 'INR' };

    // --- Utility Functions ---
    const formatCurrency = (value) => value.toLocaleString(currencyLocale, currencyOptions);

    const showToast = (title, icon = 'success') => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
        Toast.fire({ icon, title });
    };

    // --- Core UI Update Functions ---
    const updateCartTotals = () => {
        const itemRows = dynamicContent.querySelectorAll('.cart-item-row:not(.removing)');
        let subtotal = 0;
        itemRows.forEach(row => {
            const price = parseFloat(row.dataset.productPrice);
            const quantity = parseInt(row.querySelector('.quantity-input').value);
            subtotal += price * quantity;
            row.querySelector('.item-total').textContent = formatCurrency(price * quantity);
        });

        const selectedShipping = dynamicContent.querySelector('input[name="shipping-mode"]:checked')?.value || 'store-pickup';
        const shippingCost = shippingCosts[selectedShipping];

        const overallTotal = subtotal + shippingCost;

        // Update all summary fields
        document.getElementById('subtotal-ttc').textContent = formatCurrency(subtotal);
        document.getElementById('shipping-cost').textContent = shippingCost === 0 ? 'Free' : formatCurrency(shippingCost);
        document.getElementById('overall-total').textContent = formatCurrency(overallTotal);
        document.getElementById('checkout-button-total').textContent = formatCurrency(overallTotal);

        // If cart becomes empty, show the message
        if (itemRows.length === 0) {
            displayEmptyCartMessage();
        }
    };

    const updateGlobalCartCount = (count) => {
        const cartCountElement = document.getElementById('cartItemCount');
        if (cartCountElement) {
            cartCountElement.textContent = count;
            cartCountElement.style.display = count > 0 ? 'block' : 'none'; // Show/hide badge
        }
    };

    const displayEmptyCartMessage = () => {
        dynamicContent.innerHTML = `
            <div class="empty-cart-message animate-fade-in">
                <p>Your cart is empty. Let's find something for you!</p>
                <a href="/Home/All_Products" class="btn btn-primary-hero">Shop Now</a>
            </div>`;
    };

    // --- API Call Functions ---
    const apiCall = async (endpoint, body) => {
        try {
            const response = await fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!response.ok) {
                throw new Error(`Network response was not ok, status: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error(`API call to ${endpoint} failed:`, error);
            Swal.fire('Error!', 'An unexpected error occurred. Please try again.', 'error');
            return null; // Return null on failure
        }
    };

    const handleQuantityUpdate = async (cartItemId, quantity, itemRow) => {
        itemRow.classList.add('processing'); // Visual feedback
        const result = await apiCall('/Cart/Update', { cartItemId, quantity });
        itemRow.classList.remove('processing');

        if (result && result.success) {
            showToast(result.message);
            updateGlobalCartCount(result.newItemCount);

            if (quantity <= 0) {
                itemRow.classList.add('removing');
                itemRow.addEventListener('animationend', () => {
                    itemRow.remove();
                    updateCartTotals(); // Update totals after the row is visually gone
                }, { once: true });
            } else {
                itemRow.querySelector('.quantity-input').value = quantity;
                updateCartTotals();
            }
        } else {
            // Revert the input value on failure
            const originalQty = itemRow.querySelector('.quantity-input').defaultValue;
            itemRow.querySelector('.quantity-input').value = originalQty;
        }
    };

    // --- Event Listeners ---
    dynamicContent.addEventListener('click', async (e) => {
        const itemRow = e.target.closest('.cart-item-row');
        if (itemRow) {
            const cartItemId = parseInt(itemRow.dataset.cartItemId);
            const quantityInput = itemRow.querySelector('.quantity-input');
            let quantity = parseInt(quantityInput.value);

            // Quantity buttons
            if (e.target.closest('.quantity-plus')) {
                handleQuantityUpdate(cartItemId, quantity + 1, itemRow);
            } else if (e.target.closest('.quantity-minus')) {
                handleQuantityUpdate(cartItemId, quantity - 1, itemRow);
            }
            // Remove button
            else if (e.target.closest('.btn-remove-item')) {
                const result = await Swal.fire({
                    title: 'Remove Item?',
                    text: "Are you sure you want to remove this item?",
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d1000b',
                    confirmButtonText: 'Yes, remove it!'
                });
                if (result.isConfirmed) {
                    handleQuantityUpdate(cartItemId, 0, itemRow); // Removing is just updating quantity to 0
                }
            }
        }

        // Clear Cart button
        if (e.target.closest('.btn-clear-cart')) {
            const result = await Swal.fire({
                title: 'Clear Entire Cart?',
                text: "This will remove all items. This action cannot be undone.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d1000b',
                confirmButtonText: 'Yes, Clear Cart!'
            });
            if (result.isConfirmed) {
                const apiResult = await apiCall('/Cart/Clear', {});
                if (apiResult && apiResult.success) {
                    showToast('Cart has been cleared.');
                    updateGlobalCartCount(0);
                    document.querySelectorAll('.cart-item-row').forEach(row => {
                        row.classList.add('removing');
                        row.addEventListener('animationend', () => row.remove(), { once: true });
                    });
                    // Wait for animations to finish before showing the empty message
                    setTimeout(displayEmptyCartMessage, 500);
                }
            }
        }
    });

    // Listener for manual quantity input changes
    dynamicContent.addEventListener('change', (e) => {
        if (e.target.classList.contains('quantity-input')) {
            const itemRow = e.target.closest('.cart-item-row');
            const cartItemId = parseInt(itemRow.dataset.cartItemId);
            let quantity = parseInt(e.target.value);
            if (isNaN(quantity) || quantity < 0) {
                quantity = 0; // Default to 0 if input is invalid
            }
            handleQuantityUpdate(cartItemId, quantity, itemRow);
        }
        // Listener for shipping option changes
        else if (e.target.name === 'shipping-mode') {
            updateCartTotals();
        }
    });
});
