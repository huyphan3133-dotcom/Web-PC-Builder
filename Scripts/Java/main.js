document.querySelectorAll(".slider").forEach(function (slider) {
    var slides = slider.querySelectorAll("img");
    var vt = 0;

    // Đổi banner: n = 1 (tiếp), -1 (lùi)
    function doiBanner(n) {
        if (slides.length === 0) return;

        slides[vt].classList.remove("on");
        vt = (vt + n + slides.length) % slides.length;
        slides[vt].classList.add("on");
    }

    // Tự chuyển ảnh riêng cho từng slider
    setInterval(function () {
        doiBanner(1);
    }, 3000);

    // Gắn hàm vào slider để nút bấm có thể gọi riêng
    slider.doiBanner = doiBanner;
});

// Cập nhật số lượng giỏ hàng trên header ở MỌI trang (cùng key 'gvn_cart' với trang Cart/Product)
(function () {
    function updateCartBadge() {
        var el = document.getElementById("cart-count");
        if (!el) return;
        var cart = [];
        try { cart = JSON.parse(localStorage.getItem("gvn_cart")) || []; } catch (e) { }
        el.textContent = cart.reduce(function (sum, x) { return sum + (x.qty || 0); }, 0);
    }
    updateCartBadge();
    window.addEventListener("storage", updateCartBadge); // đồng bộ khi mở nhiều tab
})();
