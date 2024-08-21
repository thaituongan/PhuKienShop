//hiển thị phần kéo xuống dưới bình luận nếu mình nhấn vào Thêm Bình luận
$(document).ready(function () {
	// Khi nhấn vào "10 Bình luận"
	$('a[href="#tab2"]').on('click', function (event) {
		event.preventDefault(); // Ngăn chặn hành vi mặc định của liên kết
		$('.tab-nav li').removeClass('active'); // Loại bỏ active khỏi tất cả các tab
		$('a[href="#tab2"]').closest('li').addClass('active'); // Thêm active cho tab đúng
		$('.tab-content .tab-pane').removeClass('in active'); // Ẩn tất cả nội dung tab
		$('#tab2').addClass('in active'); // Hiển thị nội dung của tab 2

		// Cuộn trang xuống phần Bình luận (3)
		$('html, body').animate({
			scrollTop: $('#tab2').offset().top - $('.tab-nav').height()
		}, 500); //thời gian cuộn
	});
});