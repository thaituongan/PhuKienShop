$(document).ready(function () {
    $('#searchInput').on('input', function () {
        let searchTerm = $(this).val();
        if (searchTerm.length > 0) {
            $.ajax({
                url: '/Products/SearchPartial',
                type: 'GET',
                data: { searchTerm: searchTerm },
                success: function (data) {
                    $('#searchResults').html(data);
                }
            });
        } else {
            $('#searchResults').empty();
        }
    });
});
