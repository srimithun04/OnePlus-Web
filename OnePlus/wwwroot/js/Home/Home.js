// wwwroot/js/Home/Home.js
// This script contains all the logic for the home page, including animations
// and the now-fixed "Add to Cart" functionality.

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

    // --- Function to update the cart count in the navbar ---
    const updateGlobalCartCount = (count) => {
        const cartCountElement = document.getElementById('cartItemCount');
        if (cartCountElement) {
            cartCountElement.textContent = count;
            // Pro Tip: Show/hide the count badge based on whether the cart is empty
            cartCountElement.style.display = count > 0 ? 'block' : 'none';
        }
    };

    // --- ADD TO CART FUNCTIONALITY (REWRITTEN) ---
    document.querySelectorAll('.btn-add-cart').forEach(button => {
        button.addEventListener('click', async (event) => {
            event.preventDefault(); // Prevent the link from navigating

            const productId = button.dataset.productId;
            if (!productId) {
                console.error("Product ID not found on button.");
                return;
            }

            // Provide immediate visual feedback to the user
            button.textContent = 'Adding...';
            button.disabled = true;

            try {
                // Calling the new, correct API endpoint: /Cart/Add
                const response = await fetch('/Cart/Add', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        productId: parseInt(productId),
                        quantity: 1 // Default to adding 1 item
                    })
                });

                // Handle case where user is not logged in
                if (response.status === 401) {
                    Swal.fire({
                        title: 'Please Log In',
                        text: 'You need to be logged in to add items to your cart.',
                        icon: 'info',
                        confirmButtonText: 'Log In',
                        confirmButtonColor: '#d1000b'
                    }).then(() => {
                        window.location.href = '/Uam/Login'; // Redirect to login page
                    });
                    return; // Stop execution
                }

                if (!response.ok) {
                    throw new Error('Network response was not ok.');
                }

                const result = await response.json();

                if (result.success) {
                    showToast('Product added to cart!');
                    updateGlobalCartCount(result.newItemCount);
                } else {
                    showToast(result.message || 'Failed to add product.', 'error');
                }

            } catch (error) {
                console.error('Error adding to cart:', error);
                showToast('An unexpected error occurred.', 'error');
            } finally {
                // Always revert the button back to its original state
                button.textContent = 'Add to Cart';
                button.disabled = false;
            }
        });
    });


    // --- All other existing animations and scroller logic remain unchanged ---

    // On-Load Animations
    document.querySelectorAll('.animate-on-load').forEach(el => el.classList.add('is-visible'));

    // Scroll-Triggered Animations
    const scrollElements = document.querySelectorAll(".animate-on-scroll");
    const elementInView = (el, dividend = 1) => {
        const elementTop = el.getBoundingClientRect().top;
        return (elementTop <= (window.innerHeight || document.documentElement.clientHeight) / dividend);
    };
    const displayScrollElement = (element) => element.classList.add("is-visible");
    const handleScrollAnimation = () => {
        scrollElements.forEach((el) => {
            if (elementInView(el, 1.25)) {
                displayScrollElement(el);
            }
        });
    };

    // Video Scaling Animation
    const videoContainer = document.querySelector('.video-promo-container');
    const handleVideoScale = () => {
        if (!videoContainer) return;
        const top = videoContainer.getBoundingClientRect().top;
        const scale = 1 - Math.max(0, Math.min(1, (window.innerHeight - top) / (window.innerHeight + videoContainer.offsetHeight))) * 0.2;
        videoContainer.style.transform = `scale(${scale})`;
    };

    window.addEventListener("scroll", () => {
        handleScrollAnimation();
        handleVideoScale();
    });

    // Product Scroller Logic
    const scroller = document.querySelector('.product-scroller');
    if (scroller) {
        const scrollAmount = 310;
        document.getElementById('scroll-right').addEventListener('click', () => {
            scroller.scrollBy({ left: scrollAmount, behavior: 'smooth' });
        });
        document.getElementById('scroll-left').addEventListener('click', () => {
            scroller.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
        });
    }

    // Initial setup calls
    handleScrollAnimation();
    handleVideoScale();
});
