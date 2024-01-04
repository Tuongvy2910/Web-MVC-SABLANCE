function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkOrder() {

    var codeValue = $("#Code").val();
    var cityValue = $("#city").val();
    var districtValue = $("#district").val();
    var wardValue = $("#ward").val();
    var Total = $("#TotalOrders").val();
    var orderdateValue = $("#OrderDate").val();
    var phoneValue = $("#Phone").val();
    var addressValue = $("#Address").val();

    

    if (Total === "") {
        ModelStateAddError("TotalOrders", "Vui lòng chọn sản phẩm.");
    }
    else if (parseInt(Total) <= 0) {
        ModelStateAddError("TotalOrders", "Giá trị tổng hóa đơn không hợp lệ vui lòng xem lại.");
    }
    else {
        ModelStateRemoveError("TotalOrders");
    }
    //kiểm tra ngày dặt hàng
    if (orderdateValue === "" || orderdateValue === null) {
        ModelStateAddError("OrderDate", "Vui lòng chọn ngày đặt hàng.");
    }
    else if (orderdateValue) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();

        var selectedDate = new Date(orderdateValue);
        if (selectedDate >= currentDate ) {
            ModelStateAddError("OrderDate", "Ngày đặt hàng phải nhỏ hơn ngày hiện tại.");
        }
    }
    else {
        ModelStateRemoveError("OrderDate");
    }

    //kiểm tra số điện thoại
    if (phoneValue === "") {
        ModelStateAddError("phone", "Số điện thoại không được để trống.");
    }
    else if (!/^\d{8,12}$/.test(phoneValue)) {
        ModelStateAddError("phone", "Số điện thoại phải chứa từ 8 đến 12 ký tự số.");
    }
    else {
        ModelStateRemoveError("phone");
    }
   
    //kiểm tra địa chỉ
    if (cityValue === "") {
        ModelStateAddError("city", "Vui lòng chọn tỉnh thành.");
    } else {
        ModelStateRemoveError("city");
    }

    if (districtValue === "") {
        ModelStateAddError("district", "Vui lòng chọn quận/huyện.");
    } else {
        ModelStateRemoveError("district");
    }

    if (wardValue === "") {
        ModelStateAddError("ward", "Vui lòng chọn phường/xã.");
    } else {
        ModelStateRemoveError("ward");
    }

    if (addressValue === "") {
        ModelStateAddError("address", "Địa chỉ không được để trống.");
    }
    else {
        ModelStateRemoveError("address");
    }
    var isValidProductQuantity = true;
    $('#cusMem tr:has([id^="QuantityPro"])').each(function () {
        var row = $(this);
        var quantityInput = row.find('input[id="QuantityPro"]');
        var errorContainer = row.find('.error-message');
        var quantity = quantityInput.val();
        console.log(quantity);
        if (quantity === "" || quantity === undefined ) {
            errorContainer.text("Vui lòng nhập số lượng sản phẩm.");
            isValidProductQuantity = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        }
        else if (parseInt(quantity) <= 0){
            errorContainer.text("Số lượng sản phẩm là số nguyên dương.");
            isValidProductQuantity = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || cityValue === "" || districtValue === "" || wardValue === "" || $("#OrderDateError").text() !== ""
        || $("#phoneError").text() !== "" || $("#addressError").text() !== ""
        || $("#TotalOrdersError").text() !== "" || !isValidProductQuantity) {
        return false;
    }
    
    

    return true;

}

function checkEditOrder() {
    var Status = $("#OrderStatus").val();
    var Total = $("#TotalOrders").val();
    var orderdateValue = $("#OrderDate").val();
    var phoneValue = $("#Phone").val().replace(/^\s+|\s+$/g, '');
    var addressValue = $("#Address").val();
    console.log(Status);
    if (Status === "0" || Status === "") {
        ModelStateAddError("OrderStatus", "Vui lòng chọn xác nhận tình trạng đơn hàng.");
    }
    else {
        ModelStateRemoveError("OrderStatus");
    }

    if (Total === "") {
        ModelStateAddError("TotalOrders", "Vui lòng chọn sản phẩm.");
    }
    else if (parseInt(Total) <= 0) {
        ModelStateAddError("TotalOrders", "Giá trị tổng hóa đơn không hợp lệ vui lòng xem lại.");
    }
    else {
        ModelStateRemoveError("TotalOrders");
    }
    //kiểm tra ngày dặt hàng
    if (orderdateValue === "" || orderdateValue === null) {
        ModelStateAddError("OrderDate", "Vui lòng chọn ngày đặt hàng.");
    }
    else if (orderdateValue) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();

        var selectedDate = new Date(orderdateValue);
        if (selectedDate >= currentDate) {
            ModelStateAddError("OrderDate", "Ngày đặt hàng phải nhỏ hơn ngày hiện tại.");
        }
    }
    else {
        ModelStateRemoveError("OrderDate");
    }

    //kiểm tra số điện thoại
    if (phoneValue === "") {
        ModelStateAddError("phone", "Số điện thoại không được để trống.");
    }
    else if (!/^\d{8,12}$/.test(phoneValue)) {
        ModelStateAddError("phone", "Số điện thoại phải chứa từ 8 đến 12 ký tự số.");
    }
    else {
        ModelStateRemoveError("phone");
    }

    if (addressValue === "") {
        ModelStateAddError("Address", "Địa chỉ không được để trống.");
    }
    else {
        ModelStateRemoveError("Address");
    }
    var isValidProductQuantity = true;
    $('#cusMem tr:has([id^="QuantityPro"])').each(function () {
        var row = $(this);
        var quantityInput = row.find('input[id="QuantityPro"]');
        var errorContainer = row.find('.error-message');
        var quantity = quantityInput.val();
        console.log(quantity);
        if (quantity === "" || quantity === undefined) {
            errorContainer.text("Vui lòng nhập số lượng sản phẩm.");
            isValidProductQuantity = false;
            return false; // Ngừng vòng lặp khi gặp lỗi
        }
        else if (parseInt(quantity) <= 0) {
            errorContainer.text("Số lượng sản phẩm là số nguyên dương.");
            isValidProductQuantity = false;
            return false;
        }
        else {
            errorContainer.text("");
        }
    });


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#CodeError").text() !== "" || $("#OrderDateError").text() !== "" || $("#OrderStatusError").text() !== ""
        || $("#phoneError").text() !== "" || $("#AddressError").text() !== ""
        || $("#TotalOrdersError").text() !== "" || !isValidProductQuantity) {
        return false;
    }



    return true;

}

function generateRandomCode() {
    const prefix = "ORDNO";
    const characters = "0123456789";
    const length = 5;

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

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

});
$('#Quantity').on('input', function () {
    ModelStateRemoveError("Quantity");
});
$('#OrderDate').on('input', function () {
    ModelStateRemoveError("OrderDate");
});
$('#Phone').on('input', function () {
    ModelStateRemoveError("phone");
});
$('#ProductID').on('change', function () {
    ModelStateRemoveError("TotalOrders");
});
$('#cusMem tr:has([id^="QuantityPro"])').each(function () {
    var row = $(this);
    var quantityInput = row.find('input[id="QuantityPro"]');

    quantityInput.on('input', function () {
        errorContainer.text("");
    });
});


$('#Address').on('input', function () {
    ModelStateRemoveError("Address");
});
$('#OrderStatus').on('change', function () {
    ModelStateRemoveError("OrderStatus");
});
$('#city').on('change', function () {
    if ($(this).val() === "") {
        ModelStateAddError("city", "Vui lòng chọn quận/huyện.");
    } else {
        ModelStateRemoveError("city");
    }
});
$('#district').on('change', function () {
    if ($(this).val() === "") {
        ModelStateAddError("district", "Vui lòng chọn quận/huyện.");
    } else {
        ModelStateRemoveError("district");
    }
});
$('#ward').on('change', function () {
    if ($(this).val() === "") {
        ModelStateAddError("ward", "Vui lòng chọn quận/huyện.");
    } else {
        ModelStateRemoveError("ward");
    }
});


function displayErrors(errors) {
    // Xóa tất cả các thông báo lỗi trước đó
    /*$(".error-message").text("");*/

    // Hiển thị các thông báo lỗi mới
    errors.forEach(function (error) {
        ModelStateAddError(error.field, error.message);
    });
}