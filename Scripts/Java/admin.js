$(document).ready(function () {
    // Khởi tạo DataTables hỗ trợ tìm kiếm và phân trang
    if ($('#productTable').length) {
        $('#productTable').DataTable({
            "language": {
                "search": "Tìm kiếm:",
                "lengthMenu": "Hiển thị _MENU_ mục",
                "info": "Hiển thị _START_ đến _END_ trong _TOTAL_ mục",
                "paginate": { "previous": "Trước", "next": "Sau" }
            }
        });
    }

    if ($('#orderTable').length) {
        $('#orderTable').DataTable({
            "language": {
                "search": "Tìm kiếm:",
                "lengthMenu": "Hiển thị _MENU_ mục",
                "info": "Hiển thị _START_ đến _END_ trong _TOTAL_ mục",
                "paginate": { "previous": "Trước", "next": "Sau" }
            }
        });
    }
});

// Hàm chuẩn bị dữ liệu cho Modal Thêm sản phẩm
function openAddModal() {
    $('#modalTitle').text('Thêm sản phẩm mẫu');
    $('#productForm')[0].reset();
    $('#prodId').val('');
}

// Hàm chuẩn bị dữ liệu cho Modal Sửa sản phẩm
function openEditModal(id, name, category, price) {
    $('#modalTitle').text('Chỉnh sửa sản phẩm');
    $('#prodId').val(id);
    $('#prodName').val(name);
    $('#prodCategory').val(category);
    $('#prodPrice').val(price);

    var modal = new bootstrap.Modal(document.getElementById('productModal'));
    modal.show();
}

// Giả lập lưu dữ liệu
function saveProduct() {
    alert('Thao tác lưu dữ liệu thành công (Mock Data)!');
    var modalElement = document.getElementById('productModal');
    var modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) modal.hide();
}

// Giả lập xóa sản phẩm
function deleteProduct(id) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm mã ' + id + ' không?')) {
        alert('Đã xóa sản phẩm!');
    }
}