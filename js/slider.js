const myCarousel = document.querySelector('#heroSlider');
    
        const carousel = new bootstrap.Carousel(myCarousel, {
            interval: 3000,
            ride: 'carousel',
            pause: false,
            wrap: true,
            touch: true
        });
    
        // khi quay lại tab hoặc cuộn lại thì chạy tiếp
        document.addEventListener("visibilitychange", function () {
            if (!document.hidden) {
                carousel.cycle();
            }
        });