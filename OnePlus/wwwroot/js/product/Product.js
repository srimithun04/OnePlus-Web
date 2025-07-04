function confirmDelete(form) {
    if (confirm("Are you sure you want to delete this product?")) {
        return true;
    }
    return false;
}

$(document).ready(function () {
    // This part is for the Upsert page, but it's safe to have it here.
    // It initializes the bootstrap-select dropdown.
    if ($('.selectpicker').length) {
        $('.selectpicker').selectpicker();
    }

    // Handle the "Add New Category" link on the Upsert page
    $('#addCategoryBtn').on('click', function (e) {
        e.preventDefault();
        var categoryName = prompt("Enter new category name:");
        if (categoryName) {
            $.ajax({
                type: "POST",
                url: "/Product/AddCategory",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ name: categoryName }),
                success: function (response) {
                    // Add the new option to the select list
                    var newOption = new Option(response.name, response.id, false, false);
                    $('.selectpicker').append(newOption);
                    // Refresh the select picker to show the new option
                    $('.selectpicker').selectpicker('refresh');
                    alert('Category added successfully!');
                },
                error: function (xhr, status, error) {
                    alert("Error: " + xhr.responseText);
                }
            });
        }
    });
});
