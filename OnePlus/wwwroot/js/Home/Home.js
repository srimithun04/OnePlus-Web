// wwwroot/js/Home/Home.js
$(document).ready(function () {

    // --- On-Load Animations ---
    $('.animate-on-load').each(function (index) {
        $(this).addClass('is-visible');
    });

    // --- Scroll-Triggered Animations ---
    const scrollElements = document.querySelectorAll(".animate-on-scroll");

    const elementInView = (el, dividend = 1) => {
        const elementTop = el.getBoundingClientRect().top;
        return (
            elementTop <= (window.innerHeight || document.documentElement.clientHeight) / dividend
        );
    };

    const displayScrollElement = (element) => {
        element.classList.add("is-visible");
    };

    const handleScrollAnimation = () => {
        scrollElements.forEach((el) => {
            if (elementInView(el, 1.25)) {
                displayScrollElement(el);
            }
        });
    };

    // --- Video Scaling Animation ---
    const videoContainer = document.querySelector('.video-promo-container');
    let lastScrollY = window.scrollY;

    const handleVideoScale = () => {
        if (!videoContainer) return;

        const top = videoContainer.getBoundingClientRect().top;
        const startScale = 1;
        const endScale = 0.8;
        const scaleRange = startScale - endScale;
        const viewHeight = window.innerHeight;

        if (top < viewHeight && top > -videoContainer.offsetHeight) {
            const currentScrollY = window.scrollY;
            const scrollDirection = currentScrollY > lastScrollY ? 'down' : 'up';
            let progress = (viewHeight - top) / (viewHeight + videoContainer.offsetHeight);
            progress = Math.max(0, Math.min(1, progress)); // Clamp between 0 and 1

            let scale = startScale - (progress * scaleRange);

            // Apply a slight "bounce" effect
            if (scrollDirection === 'up' && scale < startScale) {
                scale = Math.min(startScale, scale + 0.01);
            }

            videoContainer.style.transform = `scale(${scale})`;
            lastScrollY = currentScrollY;
        }
    };

    window.addEventListener("scroll", () => {
        handleScrollAnimation();
        handleVideoScale();
    });

    // --- Category Filtering Logic (Used on All_Products page) ---
    $('.filter-btn').on('click', function () {
        // Handle active button style
        $('.filter-btn').removeClass('active');
        $(this).addClass('active');

        const filter = $(this).data('filter');

        $('.product-card').each(function () {
            const product = $(this);
            const categories = product.data('categories').toString().split(' ');

            if (filter === 'all' || categories.includes(filter.toString())) {
                product.fadeIn('fast');
            } else {
                product.fadeOut('fast');
            }
        });
    });

    // --- Product Scroller Logic ---
    const scroller = $('.product-scroller');
    if (scroller.length) {
        const scrollAmount = 310; // Width of card (290) + margin (20)

        $('#scroll-right').on('click', function () {
            scroller.animate({
                scrollLeft: `+=${scrollAmount}`
            }, 400);
        });

        $('#scroll-left').on('click', function () {
            scroller.animate({
                scrollLeft: `-=${scrollAmount}`
            }, 400);
        });
    }

    // Initial check for animations
    handleScrollAnimation();
    handleVideoScale();

    // --- NEW ADDITION: Add To Cart functionality ---
    // This function will be called from Home/Index.cshtml and Home/All_Products.cshtml
    $('.btn-add-cart').on('click', async function (event) {
        event.preventDefault(); // Prevent default link behavior

        const productId = $(this).data('product-id');
        if (!productId) {
            console.error("Product ID not found for add to cart button.");
            return;
        }

        try {
            const response = await fetch('/Cart/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest' // Standard header for AJAX requests
                },
                body: JSON.stringify({ productId: parseInt(productId), quantity: 1 }) // Always add 1 for initial add
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    alert('Product added to cart successfully!');
                    // Update cart count if an element with cartItemCount ID exists
                    const cartItemCountSpan = document.getElementById('cartItemCount'); // Get the global cart count span
                    if (cartItemCountSpan && result.newItemCount !== undefined) {
                        cartItemCountSpan.textContent = result.newItemCount;
                    }
                } else {
                    alert('Failed to add product to cart: ' + result.message);
                    if (result.message === "User not logged in.") {
                        window.location.href = '/Uam/Login'; // Redirect to login if not authenticated
                    }
                }
            } else {
                alert('Error: ' + response.statusText + ' - Status: ' + response.status);
            }
        } catch (error) {
            console.error('Error adding to cart:', error);
            alert('An error occurred while adding to cart.');
        }
    });

    // Function to fetch and update cart item count on page load (for _Layout.cshtml)
    async function fetchCartItemCount() {
        try {
            const response = await fetch('/Cart/GetCartItemCount');
            if (response.ok) {
                const result = await response.json();
                if (result.success && result.count !== undefined) {
                    const cartItemCountSpan = document.getElementById('cartItemCount');
                    if (cartItemCountSpan) {
                        cartItemCountSpan.textContent = result.count;
                    }
                }
            }
        } catch (error) {
            console.error("Error fetching cart item count:", error);
        }
    }

    // Call fetchCartItemCount when the DOM is fully loaded
    fetchCartItemCount();
});