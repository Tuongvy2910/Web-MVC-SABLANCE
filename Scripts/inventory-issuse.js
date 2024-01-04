function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkSO() {

    var codeValue = $("#SONo").val();
    var SOdateValue = $("#SODate").val();
    var Product = $("#ProductID").val();
    if (Product === "0" || Product === "") {
        ModelStateAddError("Product", "Vui lòng chọn sản phẩm.");
    }
    else {
        ModelStateRemoveError("Product");
    }



    //kiểm tra ngày dặt hàng
    if (SOdateValue === "" || SOdateValue === null) {
        ModelStateAddError("SODate", "Vui lòng chọn thời gian xuất kho.");
    }
    else if (SOdateValue) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();

        var selectedDate = new Date(SOdateValue);
        if (selectedDate >= currentDate) {
            ModelStateAddError("SODate", "Ngày xuất kho phải nhỏ hơn ngày hiện tại.");
        }
    }
    else {
        ModelStateRemoveError("SODate");
    }

    var isValidProductQuantity = true;
    $('#cusMem tr:has([id^="QuantityPro"])').each(function () {
        var row = $(this);
        var quantityInput = row.find('input[id="QuantityPro"]');
        var errorContainer = row.find('.error-message');
        var quantity = quantityInput.val();

        if (quantity === "" || quantity === undefined) {
            errorContainer.text("Vui lòng nhập số lượng sản phẩm.");
            isValidProductQuantity = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        }
        else if (parseInt(quantity) <= 0) {
            errorContainer.text("Số lượng sản phẩm xuất kho là số nguyên dương.");
            isValidProductQuantity = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });

  


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || $("#PODateError").text() !== "" || $("#ProductError").text() !== ""
        || !isValidProductQuantity) {
        return false;
    }



    return true;

}

function checkEditSO() {

    var SOdateValue = $("#SODate").val();
    var Product = $("#ProductID").val();
    if (Product === "0" || Product === "") {
        ModelStateAddError("Product", "Vui lòng chọn sản phẩm.");
    }
    else {
        ModelStateRemoveError("Product");
    }



    //kiểm tra ngày dặt hàng
    if (SOdateValue === "" || SOdateValue === null) {
        ModelStateAddError("SODate", "Vui lòng chọn thời gian xuất kho.");
    }
    else if (SOdateValue) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();

        var selectedDate = new Date(SOdateValue);
        if (selectedDate >= currentDate) {
            ModelStateAddError("SODate", "Ngày xuất kho phải nhỏ hơn ngày hiện tại.");
        }
    }
    else {
        ModelStateRemoveError("SODate");
    }

    var isValidProductQuantity = true;
    $('#cusMem tr:has([id^="QuantityPro"])').each(function () {
        var row = $(this);
        var quantityInput = row.find('input[id="QuantityPro"]');
        var errorContainer = row.find('.error-message');
        var quantity = quantityInput.val();

        if (quantity === "" || quantity === undefined) {
            errorContainer.text("Vui lòng nhập số lượng sản phẩm.");
            isValidProductQuantity = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        }
        else if (parseInt(quantity) <= 0) {
            errorContainer.text("Số lượng sản phẩm xuất kho là số nguyên dương.");
            isValidProductQuantity = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });

   


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || $("#PODateError").text() !== "" || $("#ProductError").text() !== ""
        || !isValidProductQuantity) {
        return false;
    }



    return true;

}

function generateRandomCode() {
    const prefix = "SO";
    const characters = "0123456789";
    const length = 8;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}




// Đặt giá trị ngẫu nhiên vào các trường input khi tài liệu được tải
$(document).ready(function () {
    // Lấy thẻ input theo ID
    var codeInput = $("#SONo");

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

});
$('#SONo').on('input', function () {
    ModelStateRemoveError("Code");
});
$('#SODate').on('input', function () {
    ModelStateRemoveError("SODate");
});





function displayErrors(errors) {
    // Xóa tất cả các thông báo lỗi trước đó
    /*$(".error-message").text("");*/

    // Hiển thị các thông báo lỗi mới
    errors.forEach(function (error) {
        ModelStateAddError(error.field, error.message);
    });
}