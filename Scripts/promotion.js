function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkPromotion() {

    var NameValue = $("#Name").val();
    var Description = $("#Description").val();
    var StartDate = $("#StartDate").val();
    var EndDate = $("#EndDate").val();
    var Product = $("#ProductID").val();
    if (Product === "0" || Product === "") {
        ModelStateAddError("Product", "Vui lòng chọn sản phẩm.");
    }
    else {
        ModelStateRemoveError("Product");
    }



    
    //kiểm tra ngày dặt hàng
    if (StartDate === "" || StartDate === null) {
        ModelStateAddError("StartDate", "Vui lòng chọn ngày bắt đầu.");
    }
    else if (StartDate) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();

        var selectedDate = new Date(StartDate);
        if (selectedDate <= currentDate) {
            ModelStateAddError("StartDate", "Ngày bắt đầu phải lớn hơn ngày hiện tại.");
        }
    }
    else {
        ModelStateRemoveError("StartDate");
    }

    if (EndDate === "" || EndDate === null) {
        ModelStateAddError("EndDate", "Vui lòng chọn ngày kết thúc.");
    }
    else if (EndDate) {
        var currentDate = new Date();
        var selectedStartDate = new Date(StartDate);
        var selectedEndDate = new Date(EndDate);

        if (selectedEndDate <= currentDate) {
            ModelStateAddError("EndDate", "Ngày kết thúc phải lớn hơn ngày hiện tại.");
        } else if (selectedEndDate <= selectedStartDate) {
            ModelStateAddError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
        }
    }
    else {
        ModelStateRemoveError("EndDate");
    }

    //kiểm tra Tên CTKM
    if (NameValue === "") {
        ModelStateAddError("Name", "Vui lòng nhập tên chương trình khuyến mãi.");
    }
    else {
        ModelStateRemoveError("Name");
    }
    //kiểm tra Mô tả CTKM
    if (Description === "") {
        ModelStateAddError("Description", "Vui lòng nhập mô tả chi tiết chương trình khuyến mãi.");
    }
    else {
        ModelStateRemoveError("Description");
    }
    
    var isValidProductQuantity = true;
    $('#cusMem tr:has([id^="QuantityPro"])').each(function () {
        var row = $(this);
        var quantityInput = row.find('input[id="QuantityPro"]');
        var errorContainer = row.find('.error-message');
        var quantity = quantityInput.val();
        console.log(quantity);
        if (quantity === "" || quantity === undefined) {
            errorContainer.text("Vui lòng nhập phần trăm giảm giá của sản phẩm.");
            isValidProductQuantity = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        } else {
            var quantityValue = parseInt(quantity);
            if (isNaN(quantityValue) || quantityValue <= 0) {
                errorContainer.text("Phần trăm giảm giá phải là số nguyên dương lớn hơn 0.");
                isValidProductQuantity = false;
                return false;
            } else if (quantityValue > 100) {
                errorContainer.text("Phần trăm giảm giá không được lớn hơn 100.");
                isValidProductQuantity = false;
                return false;
            } else {
                errorContainer.text("");
            }
        }
    });


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || $("#DescriptionError").text() !== ""
        || $("#NameError").text() !== "" || $("#StartDateError").text() !== "" || $("#ProductError").text() !== ""
        || $("#EndDateError").text() !== "" || !isValidProductQuantity) {
        return false;
    }



    return true;

}

function checkEditPromotion() {

    var codeValue = $("#Code").val();
    var NameValue = $("#Name").val();
    var Description = $("#Description").val();
    var StartDate = $("#StartDate").val();
    var EndDate = $("#EndDate").val();
    var Product = $("#ProductID").val();




    //kiểm tra ngày dặt hàng


    if (EndDate === "" || EndDate === null) {
        ModelStateAddError("EndDate", "Vui lòng chọn ngày kết thúc.");
    }
    else if (EndDate) {
        var selectedStartDate = new Date(StartDate);
        var selectedEndDate = new Date(EndDate);

        if (selectedEndDate <= selectedStartDate) {
            ModelStateAddError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
        }
    }
    else {
        ModelStateRemoveError("EndDate");
    }

    //kiểm tra Tên CTKM
    if (NameValue === "") {
        ModelStateAddError("Name", "Vui lòng nhập tên chương trình khuyến mãi.");
    }
    else {
        ModelStateRemoveError("Name");
    }
    //kiểm tra Mô tả CTKM
    if (Description === "") {
        ModelStateAddError("Description", "Vui lòng nhập mô tả chi tiết chương trình khuyến mãi.");
    }
    else {
        ModelStateRemoveError("Description");
    }

    var isValidProductQuantity = true;
    $('#cusMem tr:has([id^="QuantityPro"])').each(function () {
        var row = $(this);
        var quantityInput = row.find('input[id="QuantityPro"]');
        var errorContainer = row.find('.error-message');
        var quantity = quantityInput.val();
        console.log(quantity);
        if (quantity === "" || quantity === undefined) {
            errorContainer.text("Vui lòng nhập phần trăm giảm giá của sản phẩm.");
            isValidProductQuantity = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        } else {
            var quantityValue = parseInt(quantity);
            if (isNaN(quantityValue) || quantityValue <= 0) {
                errorContainer.text("Phần trăm giảm giá phải là số nguyên dương lớn hơn 0.");
                isValidProductQuantity = false;
                return false;
            } else if (quantityValue > 100) {
                errorContainer.text("Phần trăm giảm giá không được lớn hơn 100.");
                isValidProductQuantity = false;
                return false;
            } else {
                errorContainer.text("");
            }
        }
    });


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || $("#DescriptionError").text() !== ""
        || $("#NameError").text() !== "" || $("#ProductError").text() !== ""
        || $("#EndDateError").text() !== "" || !isValidProductQuantity) {
        return false;
    }



    return true;
}

function checkCatePromotion() {
    var CodeValue = $("#CodeCate").val();
    var NameValue = $("#NameCate").val();

    if (CodeValue === "") {
        ModelStateAddError("CodeCate", "Vui lòng nhập mã danh mục chương trình khuyến mãi.");
    }
    else {
        ModelStateRemoveError("CodeCate");
    }

    if (NameValue === "") {
        ModelStateAddError("NameCate", "Vui lòng nhập tên danh mục chương trình khuyến mãi.");
    }
    else if (NameValue.length > 70) {
        ModelStateAddError("NameCate", "Tên danh mục chương trình khuyến mãi quá dài(dưới 70 ký tự)");
    }
    else {
        ModelStateRemoveError("NameCate");
    }
    if ($("#CodeCateError").text() !== "" || $("#NameCateError").text() !== "") {
        return false;
    }

    return true;

}

function generateRandomCode() {
    const prefix = "PROMO";
    const characters = "0123456789";
    const length = 5;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}

function generateRandomCateCode() {
    const prefix = "TYPROMO";
    const characters = "0123456789";
    const length = 3;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}




// Đặt giá trị ngẫu nhiên vào các trường input khi tài liệu được tải
$(document).ready(function () {
    // Lấy thẻ input theo ID
    var codeInput = $("#Code");
    var codeCateInput = $("#CodeCate");

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

    var generatedCateCode = generateRandomCateCode();
    codeCateInput.val(generatedCateCode);

});
$('#Quantity').on('input', function () {
    ModelStateRemoveError("Quantity");
});
$('#Code').on('input', function () {
    ModelStateRemoveError("Code");
});
$('#Name').on('input', function () {
    ModelStateRemoveError("Name");
});
$('#Description').on('input', function () {
    ModelStateRemoveError("Description");
});
$('#ProductID').on('change', function () {
    ModelStateRemoveError("Product");
});


$('#EndDate').on('input', function () {
    ModelStateRemoveError("EndDate");
});
$('#StartDate').on('input', function () {
    ModelStateRemoveError("StartDate");
});



function displayErrors(errors) {
    // Xóa tất cả các thông báo lỗi trước đó
    /*$(".error-message").text("");*/

    // Hiển thị các thông báo lỗi mới
    errors.forEach(function (error) {
        ModelStateAddError(error.field, error.message);
    });
}