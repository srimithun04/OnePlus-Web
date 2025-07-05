$(document).ready(function () {

    // --- Hero Image Scroller Logic ---
    const scroller = $('.hero-image-scroller');
    if (scroller.length) {
        const images = scroller.children('img');
        images.clone().appendTo(scroller); // Duplicate images for a seamless loop
    }

    // --- On-Load Animations ---
    $('.animate-on-load').each(function () {
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

    // --- Category Filtering Logic ---
    $('.filter-btn').on('click', function () {
        // Handle active button style
        $('.filter-btn').removeClass('active');
        $(this).addClass('active');

        const filter = $(this).data('filter');

        $('.product-card').each(function () {
            const product = $(this);
            product.hide(); // Hide all cards initially

            if (filter === 'all') {
                product.fadeIn('fast');
            } else {
                const categories = product.data('categories').toString().split(' ');
                if (categories.includes(filter.toString())) {
                    product.fadeIn('fast');
                }
            }
        });
    });

    // Initial check for animations
    handleScrollAnimation();
    handleVideoScale();
});
