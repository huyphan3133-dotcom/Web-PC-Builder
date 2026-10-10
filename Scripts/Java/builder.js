(function () {
    'use strict';

    // ---------- 1. MOCK DATA ----------
    var CATEGORIES = [
        {
            key: 'cpu', label: 'CPU - Bộ vi xử lý', icon: 'fa-microchip', items: [
                { id: 'cpu1', name: 'Intel Core i5-13400F', spec: '10 nhân 16 luồng, 4.6GHz, LGA1700', price: 4590000 },
                { id: 'cpu2', name: 'Intel Core i7-13700K', spec: '16 nhân 24 luồng, 5.4GHz, LGA1700', price: 10290000 },
                { id: 'cpu3', name: 'AMD Ryzen 5 7600', spec: '6 nhân 12 luồng, 5.1GHz, AM5', price: 5190000 },
                { id: 'cpu4', name: 'AMD Ryzen 7 7800X3D', spec: '8 nhân 16 luồng, 5.0GHz, AM5', price: 10990000 }
            ]
        },
        {
            key: 'main', label: 'Mainboard', icon: 'fa-th-large', items: [
                { id: 'mb1', name: 'ASUS PRIME B760M-K D4', spec: 'LGA1700, DDR4, mATX', price: 2390000 },
                { id: 'mb2', name: 'MSI MAG B650 TOMAHAWK WIFI', spec: 'AM5, DDR5, ATX, Wi-Fi 6E', price: 5490000 },
                { id: 'mb3', name: 'Gigabyte Z790 AORUS ELITE', spec: 'LGA1700, DDR5, ATX', price: 6290000 }
            ]
        },
        {
            key: 'ram', label: 'RAM', icon: 'fa-memory', items: [
                { id: 'ram1', name: 'Kingston Fury Beast 16GB (1x16) DDR4 3200', spec: 'DDR4, 3200MHz', price: 790000 },
                { id: 'ram2', name: 'Corsair Vengeance 32GB (2x16) DDR5 5600', spec: 'DDR5, 5600MHz', price: 2690000 },
                { id: 'ram3', name: 'G.Skill Trident Z5 RGB 32GB DDR5 6000', spec: 'DDR5, 6000MHz, RGB', price: 3490000 }
            ]
        },
        {
            key: 'vga', label: 'VGA - Card màn hình', icon: 'fa-tv', items: [
                { id: 'vga1', name: 'ASUS DUAL RTX 3050 6GB', spec: 'GDDR6 6GB, 128-bit', price: 4690000 },
                { id: 'vga2', name: 'MSI RTX 4060 Ti VENTUS 2X 8GB', spec: 'GDDR6 8GB, 128-bit', price: 10490000 },
                { id: 'vga3', name: 'Gigabyte RTX 4070 SUPER WINDFORCE 12GB', spec: 'GDDR6X 12GB, 192-bit', price: 17990000 },
                { id: 'vga4', name: 'ASUS TUF RTX 5080 16GB', spec: 'GDDR7 16GB, 256-bit', price: 34990000 }
            ]
        },
        {
            key: 'ssd', label: 'SSD - Ổ cứng', icon: 'fa-hdd', items: [
                { id: 'ssd1', name: 'Kingston NV2 500GB NVMe', spec: 'M.2 PCIe Gen4, 500GB', price: 890000 },
                { id: 'ssd2', name: 'Samsung 980 Pro 1TB NVMe', spec: 'M.2 PCIe Gen4, 1TB', price: 2590000 },
                { id: 'ssd3', name: 'WD Black SN850X 2TB NVMe', spec: 'M.2 PCIe Gen4, 2TB', price: 4290000 }
            ]
        },
        {
            key: 'psu', label: 'PSU - Nguồn', icon: 'fa-plug', items: [
                { id: 'psu1', name: 'Cooler Master MWE 550W 80+ Bronze', spec: '550W, 80 Plus Bronze', price: 1090000 },
                { id: 'psu2', name: 'Corsair RM750e 750W 80+ Gold', spec: '750W, Full Modular', price: 2690000 },
                { id: 'psu3', name: 'MSI MPG A1000G 1000W 80+ Gold', spec: '1000W, ATX 3.0', price: 4190000 }
            ]
        },
        {
            key: 'case', label: 'Case - Vỏ máy tính', icon: 'fa-server', items: [
                { id: 'case1', name: 'Xigmatek Gemini II', spec: 'Mid Tower, kính cường lực', price: 890000 },
                { id: 'case2', name: 'NZXT H5 Flow', spec: 'Mid Tower, airflow tốt', price: 2190000 },
                { id: 'case3', name: 'Lian Li LANCOOL 216', spec: 'Mid Tower, 2 quạt 160mm', price: 2790000 }
            ]
        },
        {
            key: 'cooler', label: 'Tản nhiệt', icon: 'fa-fan', items: [
                { id: 'cl1', name: 'Deepcool AG400', spec: 'Tản khí, 1 quạt 120mm', price: 390000 },
                { id: 'cl2', name: 'Thermalright Peerless Assassin 120', spec: 'Tản khí 2 tháp, 2 quạt', price: 890000 },
                { id: 'cl3', name: 'Corsair iCUE H100i Elite 240mm', spec: 'Tản nước AIO 240mm', price: 3290000 }
            ]
        },
        {
            key: 'monitor', label: 'Màn hình', icon: 'fa-desktop', items: [
                { id: 'mh1', name: 'ViewSonic VA24G1-H 24" IPS 144Hz', spec: '24 inch, IPS, 144Hz', price: 2190000 },
                { id: 'mh2', name: 'ASUS TUF VG27AQ5F 27" 2K 220Hz', spec: '27 inch, Fast IPS, 2K, 220Hz', price: 5090000 },
                { id: 'mh3', name: 'MSI MAG 275QF E21 27" 2K 210Hz', spec: '27 inch, Rapid IPS, 2K, 210Hz', price: 4590000 }
            ]
        },
        {
            key: 'kbm', label: 'Bàn phím + Chuột', icon: 'fa-keyboard', items: [
                { id: 'kb1', name: 'Combo Dareu bàn phím + chuột', spec: 'Có dây, LED', price: 490000 },
                { id: 'kb2', name: 'Logitech G304 + AKKO 3068B', spec: 'Không dây, 75%', price: 2390000 },
                { id: 'kb3', name: 'ASUS ROG Falchion Ace + ROG Keris', spec: 'Gaming, RGB', price: 7490000 }
            ]
        }
    ];

    // ---------- 2. TRẠNG THÁI ----------
    var STORAGE_KEY = 'pcbuilder_selected';
    var selected = {};       // { cpu: item, main: item, ... }
    var currentKey = null;   // linh kiện đang mở trong modal

    // ---------- 3. HÀM TIỆN ÍCH ----------
    function $(id) { return document.getElementById(id); }

    function money(n) {
        return String(n).replace(/\B(?=(\d{3})+(?!\d))/g, '.') + 'đ';
    }

    function esc(s) {
        return String(s).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }

    function findCat(key) {
        for (var i = 0; i < CATEGORIES.length; i++) {
            if (CATEGORIES[i].key === key) return CATEGORIES[i];
        }
        return null;
    }

    function save() {
        try {
            var ids = {};
            Object.keys(selected).forEach(function (k) { ids[k] = selected[k].id; });
            localStorage.setItem(STORAGE_KEY, JSON.stringify(ids));
        } catch (e) { /* trình duyệt chặn localStorage thì bỏ qua */ }
    }

    function load() {
        try {
            var ids = JSON.parse(localStorage.getItem(STORAGE_KEY) || '{}');
            Object.keys(ids).forEach(function (k) {
                var cat = findCat(k);
                if (!cat) return;
                cat.items.forEach(function (it) { if (it.id === ids[k]) selected[k] = it; });
            });
        } catch (e) { selected = {}; }
    }

    // ---------- 4. VẼ BẢNG ----------
    function renderRows() {
        var html = '';
        CATEGORIES.forEach(function (cat) {
            var it = selected[cat.key];
            html += '<tr class="' + (it ? 'has-item' : '') + '">';
            html += '<td><div class="pcb-type"><i class="fas ' + cat.icon + '"></i>' + esc(cat.label) + '</div></td>';
            if (it) {
                html += '<td><div class="pcb-name">' + esc(it.name) + '</div><div class="pcb-spec">' + esc(it.spec) + '</div></td>';
                html += '<td class="pcb-price">' + money(it.price) + '</td>';
                html += '<td><div class="pcb-actions">' +
                    '<button type="button" class="pcb-btn pcb-btn-ghost pcb-btn-sm" data-pick="' + cat.key + '">Đổi</button>' +
                    '<button type="button" class="pcb-remove" data-remove="' + cat.key + '" aria-label="Xóa ' + esc(cat.label) + '">&times;</button>' +
                    '</div></td>';
            } else {
                html += '<td class="pcb-empty">Chưa chọn linh kiện</td><td></td>';
                html += '<td><div class="pcb-actions"><button type="button" class="pcb-btn pcb-btn-sm" data-pick="' + cat.key + '">Chọn</button></div></td>';
            }
            html += '</tr>';
        });
        $('pcb-rows').innerHTML = html;
    }

    function updateTotal() {
        var total = 0, count = 0;
        Object.keys(selected).forEach(function (k) { total += selected[k].price; count++; });
        $('pcb-total').textContent = money(total);
        $('pcb-count').textContent = count;
        return total;
    }

    function refresh() {
        renderRows();
        updateTotal();
        save();
    }

    // ---------- 5. MODAL CHỌN LINH KIỆN ----------
    function openModal(id) {
        var m = $(id);
        m.classList.add('open');
        m.setAttribute('aria-hidden', 'false');
    }

    function closeModals() {
        var list = document.querySelectorAll('.pcb-modal');
        for (var i = 0; i < list.length; i++) {
            list[i].classList.remove('open');
            list[i].setAttribute('aria-hidden', 'true');
        }
    }

    function renderModalList() {
        var cat = findCat(currentKey);
        if (!cat) return;
        var q = $('pcb-search').value.trim().toLowerCase();
        var picked = selected[currentKey] ? selected[currentKey].id : null;
        var html = '', n = 0;

        cat.items.forEach(function (it) {
            if (q && (it.name + ' ' + it.spec).toLowerCase().indexOf(q) === -1) return;
            n++;
            html += '<div class="pcb-item' + (it.id === picked ? ' picked' : '') + '">' +
                '<div class="pcb-item-info"><div class="pcb-name">' + esc(it.name) + '</div>' +
                '<div class="pcb-spec">' + esc(it.spec) + '</div></div>' +
                '<div class="pcb-item-end"><span class="pcb-price">' + money(it.price) + '</span>' +
                '<button type="button" class="pcb-btn pcb-btn-sm pcb-pick" data-item="' + it.id + '">' +
                (it.id === picked ? 'Đang chọn' : 'Chọn') + '</button></div></div>';
        });

        $('pcb-modal-list').innerHTML = n ? html : '<div class="pcb-none">Không tìm thấy linh kiện phù hợp.</div>';
    }

    function openPicker(key) {
        currentKey = key;
        $('pcb-modal-title').textContent = 'Chọn ' + findCat(key).label;
        $('pcb-search').value = '';
        renderModalList();
        openModal('pcb-modal');
        $('pcb-search').focus();
    }

    // ---------- 6. XUẤT CẤU HÌNH ----------
    function openSummary() {
        var keys = Object.keys(selected);
        if (!keys.length) {
            alert('Bạn chưa chọn linh kiện nào. Hãy chọn ít nhất một linh kiện để xuất cấu hình.');
            return;
        }
        var html = '<table class="pcb-sum"><thead><tr><th>Linh kiện</th><th>Sản phẩm</th><th>Giá</th></tr></thead><tbody>';
        CATEGORIES.forEach(function (cat) {
            var it = selected[cat.key];
            if (!it) return;
            html += '<tr><td>' + esc(cat.label) + '</td><td>' + esc(it.name) + '</td><td>' + money(it.price) + '</td></tr>';
        });
        html += '</tbody></table>';
        html += '<div class="pcb-sum-total">Tổng cộng: ' + money(updateTotal()) + '</div>';
        $('pcb-summary-body').innerHTML = html;
        openModal('pcb-summary');
    }

    // ---------- 7. SỰ KIỆN ----------
    function bind() {
        // Bấm Chọn / Đổi / Xóa trong bảng
        $('pcb-rows').addEventListener('click', function (e) {
            var pick = e.target.closest('[data-pick]');
            var rem = e.target.closest('[data-remove]');
            if (pick) openPicker(pick.getAttribute('data-pick'));
            if (rem) { delete selected[rem.getAttribute('data-remove')]; refresh(); }
        });

        // Chọn sản phẩm trong modal
        $('pcb-modal-list').addEventListener('click', function (e) {
            var btn = e.target.closest('[data-item]');
            if (!btn) return;
            var cat = findCat(currentKey);
            cat.items.forEach(function (it) {
                if (it.id === btn.getAttribute('data-item')) selected[currentKey] = it;
            });
            closeModals();
            refresh();
        });

        $('pcb-search').addEventListener('input', renderModalList);

        // Đóng modal: nút X, nút Đóng, bấm nền tối, phím Esc
        document.addEventListener('click', function (e) {
            if (e.target.closest('[data-close]') || e.target.classList.contains('pcb-modal')) closeModals();
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeModals();
        });

        $('pcb-reset').addEventListener('click', function () {
            if (Object.keys(selected).length && !confirm('Xóa toàn bộ linh kiện đã chọn?')) return;
            selected = {};
            refresh();
        });

        $('pcb-export').addEventListener('click', openSummary);
        $('pcb-print').addEventListener('click', function () { window.print(); });
    }

    // ---------- 8. KHỞI TẠO ----------
    document.addEventListener('DOMContentLoaded', function () {
        if (!$('pcb-rows')) return; // không phải trang Build PC
        $('pcb-max').textContent = CATEGORIES.length;
        load();
        refresh();
        bind();
    });
})();
