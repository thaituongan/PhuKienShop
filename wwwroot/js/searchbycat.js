// Khi danh mục thay đổi, gửi form và lưu giá trị đã chọn vào sessionStorage
document.getElementById('categorySelect').addEventListener('change', function () {
    var selectedCategory = this.value; // Lấy giá trị danh mục đã chọn
    sessionStorage.setItem('selectedCategory', selectedCategory); // Lưu vào sessionStorage
    // Đợi một chút trước khi gửi form để đảm bảo giá trị đã được lưu
    setTimeout(function () {
        document.getElementById('searchForm').submit(); // Gửi form ngay lập tức
    }, 100); // Thời gian chờ có thể điều chỉnh tùy theo nhu cầu
});

// Khi trang load lại, thiết lập lại giá trị đã chọn dựa trên giá trị lưu trong sessionStorage
window.onload = function () {
    var savedCategory = sessionStorage.getItem('selectedCategory');
    if (savedCategory) {
        document.getElementById('categorySelect').value = savedCategory;
    }
};
