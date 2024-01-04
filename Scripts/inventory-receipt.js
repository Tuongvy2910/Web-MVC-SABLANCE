function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkPO() {

    var codeValue = $("#PONo").val();
    var POdateValue = $("#PODate").val();
    var Product = $("#ProductID").val();
    if (Product === "0" || Product === "") {
        ModelStateAddError("Product", "Vui lòng chọn sản phẩm.");
    }
    else {
        ModelStateRemoveError("Product");
    }



    //kiểm tra ngày dặt hàng
    if (POdateValue === "" || POdateValue === null) {
        ModelStateAddError("PODate", "Vui lòng chọn thời gian nhập kho.");
    }
    else if (POdateValue) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();

        var selectedDate = new Date(POdateValue);
        if (selectedDate >= currentDate) {
            ModelStateAddError("PODate", "Ngày nhập kho phải nhỏ hơn ngày hiện tại.");
        }
    }
    else {
        ModelStateRemoveError("PODate");
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
            errorContainer.text("Số lượng sản phẩm nhập là số nguyên dương.");
            isValidProductQuantity = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });

    var isValidProductPrices = true;
    $('#cusMem tr:has([id^="PricesPro"])').each(function () {
        var row = $(this);
        var pricesInput = row.find('input[id="PricesPro"]');
        var errorContainer = row.find('#ProPricesError');
        var prices = pricesInput.val();

        if (prices === "" || prices === undefined) {
            errorContainer.text("Vui lòng nhập đơn giá sản phẩm nhập kho.");
            isValidProductPrices = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        }
        else if (parseInt(prices) <= 0) {
            errorContainer.text("Đơn giá nhập của sản phẩm là số nguyên dương.");
            isValidProductPrices = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || $("#PODateError").text() !== "" || $("#ProductError").text() !== ""
        || !isValidProductQuantity || !isValidProductPrices) {
        return false;
    }



    return true;

}

function checkEditPO() {

    var POdateValue = $("#PODate").val();
    var Product = $("#ProductID").val();
    if (Product === "0" || Product === "") {
        ModelStateAddError("Product", "Vui lòng chọn sản phẩm.");
    }
    else {
        ModelStateRemoveError("Product");
    }



    //kiểm tra ngày dặt hàng
    if (POdateValue === "" || POdateValue === null) {
        ModelStateAddError("PODate", "Vui lòng chọn thời gian nhập kho.");
    }
    else if (POdateValue) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();

        var selectedDate = new Date(POdateValue);
        if (selectedDate >= currentDate) {
            ModelStateAddError("PODate", "Ngày nhập kho phải nhỏ hơn ngày hiện tại.");
        }
    }
    else {
        ModelStateRemoveError("PODate");
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
            errorContainer.text("Số lượng sản phẩm nhập là số nguyên dương.");
            isValidProductQuantity = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });

    var isValidProductPrices = true;
    $('#cusMem tr:has([id^="PricesPro"])').each(function () {
        var row = $(this);
        var pricesInput = row.find('input[id="PricesPro"]');
        var errorContainer = row.find('#ProPricesError');
        var prices = pricesInput.val();

        if (prices === "" || prices === undefined) {
            errorContainer.text("Vui lòng nhập đơn giá sản phẩm nhập kho.");
            isValidProductPrices = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        }
        else if (parseInt(prices) <= 0) {
            errorContainer.text("Đơn giá nhập của sản phẩm là số nguyên dương.");
            isValidProductPrices = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || $("#PODateError").text() !== "" || $("#ProductError").text() !== ""
        || !isValidProductQuantity || !isValidProductPrices) {
        return false;
    }



    return true;

}

function generateRandomCode() {
    const prefix = "PO";
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
    var codeInput = $("#PONo");

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

});
$('#PONo').on('input', function () {
    ModelStateRemoveError("Code");
});
$('#PODate').on('input', function () {
    ModelStateRemoveError("PODate");
});
//$('#Phone').on('input', function () {
//    ModelStateRemoveError("phone");
//});
//$('#ProductID').on('change', function () {
//    ModelStateRemoveError("TotalOrders");
//});
//$('#cusMem tr:has([id^="QuantityPro"])').each(function () {
//    var row = $(this);
//    var quantityInput = row.find('input[id="QuantityPro"]');

//    quantityInput.on('input', function () {
//        errorContainer.text("");
//    });
//});




function displayErrors(errors) {
    // Xóa tất cả các thông báo lỗi trước đó
    /*$(".error-message").text("");*/

    // Hiển thị các thông báo lỗi mới
    errors.forEach(function (error) {
        ModelStateAddError(error.field, error.message);
    });
}