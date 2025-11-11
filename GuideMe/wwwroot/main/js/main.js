(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($('#spinner').length > 0) {
                $('#spinner').removeClass('show');
            }
        }, 1);
    };
    spinner();
    
    
    // Initiate the wowjs
    new WOW().init();


    // Sticky Navbar
    $(window).scroll(function () {
        if ($(this).scrollTop() > 45) {
            $('.navbar').addClass('sticky-top shadow-sm');
        } else {
            $('.navbar').removeClass('sticky-top shadow-sm');
        }
    });
    
    
    // Dropdown on mouse hover
    const $dropdown = $(".dropdown");
    const $dropdownToggle = $(".dropdown-toggle");
    const $dropdownMenu = $(".dropdown-menu");
    const showClass = "show";
  
    $(document).ready(function () {
        const showClass = "show"; // define your show class
        const $dropdown = $(".dropdown"); // adjust selector if needed
        const $dropdownToggle = ".dropdown-toggle";
        const $dropdownMenu = ".dropdown-menu";

        // Function to close all dropdowns
        function closeAllDropdowns() {
            $dropdown.removeClass(showClass);
            $dropdown.find($dropdownToggle).attr("aria-expanded", "false");
            $dropdown.find($dropdownMenu).removeClass(showClass);
        }

        // Click to toggle dropdown
        $dropdown.on("click", function (e) {
            e.stopPropagation(); // prevent click from bubbling to document
            const $this = $(this);
            const isOpen = $this.hasClass(showClass);

            closeAllDropdowns(); // close other dropdowns

            if (!isOpen) {
                $this.addClass(showClass);
                $this.find($dropdownToggle).attr("aria-expanded", "true");
                $this.find($dropdownMenu).addClass(showClass);
            }
        });

        // Click outside closes dropdown
        $(document).on("click", function () {
            closeAllDropdowns();
        });

        // Optional: handle window resize if you have responsive behavior
        $(window).on("resize", function () {
            if ($(window).width() < 992) {
                closeAllDropdowns(); // close dropdowns on small screens
            }
        });
    });



    
    
    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({scrollTop: 0}, 1500, 'easeInOutExpo');
        return false;
    });


    // Testimonials carousel
    $(".testimonial-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1000,
        center: true,
        margin: 24,
        dots: true,
        loop: true,
        nav : false,
        responsive: {
            0:{
                items:1
            },
            768:{
                items:2
            },
            992:{
                items:3
            }
        }
    });
    
})(jQuery);

const navbar = document.getElementById('mainNavbar');
const userDropdown = document.getElementById('userDropdown');
const dropdownMenu = userDropdown.querySelector('.dropdown-menu');

// Navbar scroll color
window.addEventListener('scroll', () => {
    if (window.scrollY > 50) {
        navbar.classList.add('scrolled');
    } else {
        navbar.classList.remove('scrolled');
    }
});

// Click-to-toggle dropdown
userDropdown.addEventListener('click', function (e) {
    e.stopPropagation(); // prevent closing immediately
    dropdownMenu.classList.toggle('show');
});

// Close dropdown when clicking outside
window.addEventListener('click', function (e) {
    if (!userDropdown.contains(e.target)) {
        dropdownMenu.classList.remove('show');
    }
});