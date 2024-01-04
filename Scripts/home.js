
$(document).ready(function () {
    // Initialize the Swiper for Túi Xách
    var swiperTuiXach = new Swiper("#slide-tuixach .slide-content", {
        slidesPerView: 3,
        spaceBetween: 25,
        loop: true,
        centeredSlides: true,
        fadeEffect: {
            crossFade: true
        },
        grabCursor: true,
        pagination: {
            el: "#slide-tuixach .swiper-pagination",
            clickable: true,
            dynamicBullets: true,
        },
        navigation: {
            nextEl: "#slide-tuixach .swiper-button-next",
            prevEl: "#slide-tuixach .swiper-button-prev",
        },
        breakpoints: {
            0: {
                slidesPerView: 2,
            },
            520: {
                slidesPerView: 3,
            },
            950: {
                slidesPerView: 4,
            },
        },
    });

    // Manual Bootstrap carousel navigation for Túi Xách
    $("#slide-tuixach .swiper-button-next").on("click", function () {
        $("#slide-tuixach .slide-content").carousel("next");
    });

    $("#slide-tuixach .swiper-button-prev").on("click", function () {
        $("#slide-tuixach .slide-content").carousel("prev");
    });
    // khuyến mãi
    var swiperKhuyenmai = new Swiper("#slide-khuyenmai .slide-content", {
        slidesPerView: 3,
        spaceBetween: 25,
        loop: true,
        centeredSlides: true,
        fadeEffect: {
            crossFade: true
        },
        grabCursor: true,
        pagination: {
            el: "#slide-khuyenmai .swiper-pagination",
            clickable: true,
            dynamicBullets: true,
        },
        navigation: {
            nextEl: "#slide-khuyenmai .swiper-button-next",
            prevEl: "#slide-khuyenmai .swiper-button-prev",
        },
        breakpoints: {
            0: {
                slidesPerView: 2,
            },
            520: {
                slidesPerView: 3,
            },
            950: {
                slidesPerView: 4,
            },
        },
    });

    // Manual Bootstrap carousel navigation for Túi Xách
    $("#slide-khuyenmai .swiper-button-next").on("click", function () {
        $("#slide-khuyenmai .slide-content").carousel("next");
    });

    $("#slide-khuyenmai .swiper-button-prev").on("click", function () {
        $("#slide-khuyenmai .slide-content").carousel("prev");
    });

    // Initialize the Swiper for Giày Dép
    var swiperGiayDep = new Swiper("#slider-giaydep .slide-content", {
        slidesPerView: 3,
        spaceBetween: 25,
        loop: true,
        centeredSlides: true,
        fadeEffect: {
            crossFade: true
        },
        grabCursor: true,
        pagination: {
            el: "#slider-giaydep .swiper-pagination",
            clickable: true,
            dynamicBullets: true,
        },
        navigation: {
            nextEl: "#slider-giaydep .swiper-button-next",
            prevEl: "#slider-giaydep .swiper-button-prev",
        },
        breakpoints: {
            0: {
                slidesPerView: 2,
            },
            520: {
                slidesPerView: 3,
            },
            950: {
                slidesPerView: 4,
            },
        },
    });

    // Manual Bootstrap carousel navigation for Giày Dép
    $("#slider-giaydep .swiper-button-next").on("click", function () {
        $("#slider-giaydep .slide-content").carousel("next");
    });

    $("#slider-giaydep .swiper-button-prev").on("click", function () {
        $("#slider-giaydep .slide-content").carousel("prev");
    });
});