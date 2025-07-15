document.addEventListener('DOMContentLoaded', function () {
    const cartItemsContainer = document.querySelector('.cart-items');
    const subtotalTtcSpan = document.getElementById('subtotal-ttc');
    const shippingCostSpan = document.getElementById('shipping-cost');
    const overallTotalSpan = document.getElementById('overall-total');
    const checkoutButtonTotalSpan = document.getElementById('checkout-button-total');
    const clearCartButton = document.querySelector('.btn-clear-cart');
    const shippingModeRadios = document.querySelectorAll('input[name="shipping-mode"]');
    const cartItemCountSpan = document.getElementById('cartItemCount'); // Assuming this ID exists in _Layout.cshtml

    const shippingCosts = {
        'store-pickup': 0,
        'delivery': 9.00 // Assuming this is in Euro, adjust currency formatting if needed
    };

    function formatCurrency(value) {
        return value.toLocaleString('en-IN', { style: 'currency', currency: 'INR', minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function updateCartTotals() {
        let currentSubtotal = 0;
        document.querySelectorAll('.cart-item-row').forEach(itemRow => {
            // Ensure the itemRow is not currently animating out (has the 'removing' class)
            if (!itemRow.classList.contains('removing')) {
                const price = parseFloat(itemRow.dataset.productPrice);
                const quantity = parseInt(itemRow.querySelector('.quantity-input').value);
                const itemTotal = price * quantity;
                itemRow.querySelector('.item-total').textContent = formatCurrency(itemTotal);
                currentSubtotal += itemTotal;
            }
        });

        subtotalTtcSpan.textContent = formatCurrency(currentSubtotal);

        const selectedShippingMode = document.querySelector('input[name="shipping-mode"]:checked')?.value || 'store-pickup';
        const currentShippingCost = shippingCosts[selectedShippingMode];

        shippingCostSpan.textContent = currentShippingCost === 0 ? 'Free' : formatCurrency(currentShippingCost);

        const overallTotal = currentSubtotal + currentShippingCost;
        overallTotalSpan.textContent = formatCurrency(overallTotal);
        checkoutButtonTotalSpan.textContent = formatCurrency(overallTotal);

        let totalQuantity = 0;
        document.querySelectorAll('.quantity-input').forEach(input => {
            const itemRow = input.closest('.cart-item-row');
            if (itemRow && !itemRow.classList.contains('removing')) {
                totalQuantity += parseInt(input.value);
            }
        });
        updateGlobalCartCount(totalQuantity);
        checkAndDisplayEmptyCartMessage();
    }

    function updateGlobalCartCount(count) {
        if (cartItemCountSpan) {
            cartItemCountSpan.textContent = count;
        }
    }

    function checkAndDisplayEmptyCartMessage() {
        // Count only visible, non-removing items
        if (document.querySelectorAll('.cart-item-row:not(.removing)').length === 0) {
            const cartContent = document.querySelector('.cart-content');
            if (cartContent) {
                cartContent.innerHTML = `
                    <div class="empty-cart-message animate-fade-in">
                        <p>Your cart is empty. Start adding some awesome OnePlus products!</p>
                        <a href="/Home/All_Products" class="btn btn-primary-hero">Shop Now</a>
                    </div>
                `;
                document.querySelector('.cart-header').style.marginBottom = '20px';
                subtotalTtcSpan.textContent = formatCurrency(0);
                shippingCostSpan.textContent = 'Free';
                overallTotalSpan.textContent = formatCurrency(0);
                checkoutButtonTotalSpan.textContent = formatCurrency(0);
            }
        }
    }

    // Event listener for quantity changes and item removal
    if (cartItemsContainer) {
        cartItemsContainer.addEventListener('click', async function (event) {
            let target = event.target;
            let cartItemRow = target.closest('.cart-item-row');

            // Find the button (or its icon if clicked directly)
            const quantityMinusBtn = target.closest('.quantity-minus');
            const quantityPlusBtn = target.closest('.quantity-plus');
            const removeItemBtn = target.closest('.btn-remove-item');

            if (!cartItemRow && !quantityMinusBtn && !quantityPlusBtn && !removeItemBtn) {
                return;
            }

            const cartItemId = parseInt(cartItemRow?.dataset.cartItemId || quantityMinusBtn?.dataset.cartItemId || quantityPlusBtn?.dataset.cartItemId || removeItemBtn?.dataset.cartItemId);
            const quantityInput = cartItemRow?.querySelector('.quantity-input');
            let currentQuantity = quantityInput ? parseInt(quantityInput.value) : 0;

            if (quantityMinusBtn) {
                currentQuantity--;
                await sendQuantityUpdate(cartItemId, currentQuantity, quantityInput, cartItemRow);
            } else if (quantityPlusBtn) {
                currentQuantity++;
                await sendQuantityUpdate(cartItemId, currentQuantity, quantityInput, cartItemRow);
            } else if (removeItemBtn) {
                // Use SweetAlert2 for confirmation
                const result = await Swal.fire({
                    title: 'Are you sure?',
                    text: "You won't be able to revert this!",
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#810100', // Your primary red
                    cancelButtonColor: '#666', // Your text-grey
                    confirmButtonText: 'Yes, remove it!',
                    customClass: {
                        popup: 'swal2-custom-popup' // Add custom class for more styling if needed
                    }
                });

                if (result.isConfirmed) {
                    // --- THIS IS THE FIX ---
                    // Instead of a separate remove function, we call the existing
                    // update function with a quantity of 0.
                    const quantityInput = cartItemRow?.querySelector('.quantity-input');
                    await sendQuantityUpdate(cartItemId, 0, quantityInput, cartItemRow);
                }
            }
        });

        // Event listener for direct input changes (e.g., user types a number)
        cartItemsContainer.addEventListener('change', async function (event) {
            let target = event.target;
            if (target.classList.contains('quantity-input')) {
                const cartItemRow = target.closest('.cart-item-row');
                const cartItemId = parseInt(cartItemRow.dataset.cartItemId);
                let newQuantity = parseInt(target.value);

                if (isNaN(newQuantity) || newQuantity < 0) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Invalid Quantity',
                        text: 'Please enter a positive number for the quantity.',
                        confirmButtonColor: '#810100'
                    });
                    target.value = cartItemRow.dataset.originalQuantity || 1;
                    return;
                }
                await sendQuantityUpdate(cartItemId, newQuantity, target, cartItemRow);
            }
        });
    }

    async function sendQuantityUpdate(cartItemId, quantity, quantityInput, itemRowElement) {
        itemRowElement.dataset.originalQuantity = quantityInput ? quantityInput.value : quantity;

        try {
            const response = await fetch('/Cart/UpdateQuantity', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ cartItemId: cartItemId, quantity: quantity })
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    if (quantityInput) {
                        quantityInput.value = quantity;
                    }
                    if (quantity <= 0) {
                        itemRowElement.classList.add('removing'); // Add class for animation
                        itemRowElement.addEventListener('animationend', () => {
                            itemRowElement.remove(); // Remove only after animation
                            updateCartTotals(); // Recalculate after removal
                        }, { once: true }); // Ensure listener runs once
                    } else {
                        updateCartTotals();
                    }
                    updateGlobalCartCount(result.newItemCount);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: 'Error updating quantity: ' + result.message,
                        confirmButtonColor: '#810100'
                    });
                    if (quantityInput) {
                        quantityInput.value = itemRowElement.dataset.originalQuantity;
                    }
                }
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Network Error',
                    text: 'Unable to connect to the server. Status: ' + response.status,
                    confirmButtonColor: '#810100'
                });
                if (quantityInput) {
                    quantityInput.value = itemRowElement.dataset.originalQuantity;
                }
            }
        } catch (error) {
            console.error('Error in sendQuantityUpdate:', error);
            Swal.fire({
                icon: 'error',
                title: 'An Error Occurred',
                text: 'Please try again later.',
                confirmButtonColor: '#810100'
            });
            if (quantityInput) {
                quantityInput.value = itemRowElement.dataset.originalQuantity;
            }
        }
    }

    // This function is no longer needed since we are using sendQuantityUpdate
    // async function removeItemFromCart(cartItemId, itemRowElement) { ... }


    // Event listener for clear cart button
    if (clearCartButton) {
        clearCartButton.addEventListener('click', async function () {
            const result = await Swal.fire({
                title: 'Clear entire cart?',
                text: "This will remove all items from your cart. Are you sure?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#810100', // Your primary red
                cancelButtonColor: '#666', // Your text-grey
                confirmButtonText: 'Yes, clear it!',
                customClass: {
                    popup: 'swal2-custom-popup'
                }
            });

            if (result.isConfirmed) {
                try {
                    const response = await fetch('/Cart/ClearCart', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success) {
                            // Add 'removing' class to all items for animation
                            const itemRows = document.querySelectorAll('.cart-item-row');
                            let itemsRemovedCount = 0;
                            if (itemRows.length > 0) {
                                itemRows.forEach(row => {
                                    row.classList.add('removing');
                                    row.addEventListener('animationend', () => {
                                        row.remove();
                                        itemsRemovedCount++;
                                        if (itemsRemovedCount === itemRows.length) {
                                            // Only update totals/count and display empty message AFTER all animations complete
                                            updateCartTotals();
                                            updateGlobalCartCount(0);
                                            Swal.fire(
                                                'Cleared!',
                                                'Your cart has been emptied.',
                                                'success'
                                            );
                                        }
                                    }, { once: true });
                                });
                            } else {
                                // If cart was already empty client-side, just confirm
                                updateCartTotals();
                                updateGlobalCartCount(0);
                                Swal.fire(
                                    'Cleared!',
                                    'Your cart is already empty.',
                                    'success'
                                );
                            }

                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Failed to Clear',
                                text: 'Error clearing cart: ' + result.message,
                                confirmButtonColor: '#810100'
                            });
                        }
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Network Error',
                            text: 'Unable to connect to the server. Status: ' + response.status,
                            confirmButtonColor: '#810100'
                        });
                    }
                } catch (error) {
                    console.error('Error in clearCartButton handler:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'An Error Occurred',
                        text: 'Please try again later.',
                        confirmButtonColor: '#810100'
                    });
                }
            }
        });
    }

    // Event listeners for shipping mode radios
    shippingModeRadios.forEach(radio => {
        radio.addEventListener('change', updateCartTotals);
    });

    // Initial setup: store original quantity and update totals when page loads
    document.querySelectorAll('.cart-item-row').forEach(itemRow => {
        const quantityInput = itemRow.querySelector('.quantity-input');
        if (quantityInput) {
            itemRow.dataset.originalQuantity = quantityInput.value; // Store original for revert
        }
    });
    updateCartTotals();
});