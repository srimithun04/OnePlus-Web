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

    const hideScrollElement = (element) => {
        element.classList.remove("is-visible");
    };

    const handleScrollAnimation = () => {
        scrollElements.forEach((el) => {
            if (elementInView(el, 1.25)) {
                displayScrollElement(el);
            }
        });
    };

    window.addEventListener("scroll", () => {
        handleScrollAnimation();
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
});