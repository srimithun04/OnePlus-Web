// === NEW & IMPROVED wwwroot/js/Home/All_Products.js ===
// This script is now cleaner and uses a more robust filtering method.

document.addEventListener('DOMContentLoaded', () => {

    // --- Helper function for professional, non-blocking notifications ---
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

    // --- Function to dispatch the global cart update event ---
    const dispatchCartUpdate = (newItemCount) => {
        document.dispatchEvent(new CustomEvent('cartUpdated', {
            detail: { newItemCount }
        }));
    };


    // --- CATEGORY FILTERING LOGIC (REWRITTEN FOR SMOOTH ANIMATIONS) ---
    const filterButtons = document.querySelectorAll('.filter-btn');
    const productCards = document.querySelectorAll('.product-card');

    filterButtons.forEach(button => {
        button.addEventListener('click', () => {
            document.querySelector('.filter-btn.active').classList.remove('active');
            button.classList.add('active');

            const filterValue = button.dataset.filter;

            productCards.forEach(card => {
                const categoryIds = card.dataset.categories.split(' ');
                const shouldShow = (filterValue === 'all' || categoryIds.includes(filterValue));

                // We now simply add or remove the '.hidden' class.
                // The CSS handles the animation and collapsing the space.
                if (shouldShow) {
                    card.classList.remove('hidden');
                } else {
                    card.classList.add('hidden');
                }
            });
        });
    });


    // --- ADD TO CART FUNCTIONALITY ---
    document.querySelectorAll('.btn-add-cart').forEach(button => {
        button.addEventListener('click', async (event) => {
            event.preventDefault();

            const productId = button.dataset.productId;
            if (!productId) return;

            button.textContent = 'Adding...';
            button.disabled = true;

            try {
                const response = await fetch('/Cart/Add', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ productId: parseInt(productId), quantity: 1 })
                });

                if (response.status === 401) {
                    Swal.fire({
                        title: 'Please Log In',
                        text: 'You must be logged in to add items to your cart.',
                        icon: 'info',
                        confirmButtonText: 'Log In',
                        confirmButtonColor: '#d1000b'
                    }).then(() => window.location.href = '/Uam/Login');
                    return;
                }

                if (!response.ok) throw new Error('API request failed');

                const result = await response.json();

                if (result.success) {
                    showToast('Product added to cart!');
                    dispatchCartUpdate(result.newItemCount);
                } else {
                    showToast(result.message || 'Failed to add product.', 'error');
                }
            } catch (error) {
                console.error('Add to cart error:', error);
                showToast('An unexpected error occurred.', 'error');
            } finally {
                button.textContent = 'Add to Cart';
                button.disabled = false;
            }
        });
    });
});
