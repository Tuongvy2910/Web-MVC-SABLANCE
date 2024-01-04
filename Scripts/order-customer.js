function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkOrder() {

    var cityValue = $("#city").val();
    var districtValue = $("#district").val();
    var wardValue = $("#ward").val();
    var emailRegex = /^[a-zA-Z0-9._-]+@gmail\.com$/;
    var emailValue = $("#EmailCus").val();
    var phoneValue = $("#PhoneCustomer").val().replace(/\s/g, '');
    var addressValue = $("#address").val();

    var selectedPaymentMethod = $("input[name='paymentmethod']:checked").val();

    // Validate payment method selection
    if (!selectedPaymentMethod) {
        ModelStateAddError("PaymentMethod", "Vui lòng chọn phương thức thanh toán.");
    } else {
        ModelStateRemoveError("PaymentMethod");
    }
    if (emailValue !== "" && !emailRegex.test(emailValue)) {
        ModelStateAddError("EmailCus", "Email bạn nhập sai định dạng");
    }
    else {
        ModelStateRemoveError("EmailCus");
    }

    //kiểm tra số điện thoại
    if (phoneValue === "") {
        ModelStateAddError("PhoneCustomer", "Số điện thoại không được để trống.");
    }
    else if (!/^\d{8,12}$/.test(phoneValue)) {
        ModelStateAddError("PhoneCustomer", "Số điện thoại phải chứa từ 8 đến 12 ký tự số.");
    }
    else {
        ModelStateRemoveError("PhoneCustomer");
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

    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if (cityValue === "" || districtValue === "" || wardValue === ""
        || $("#PhoneCustomerError").text() !== "" || $("#addressError").text() !== ""
        || $("#EmailCusError").text() !== "" || $("#PaymentMethodError").text() !== "")
    {
        return false;
    }



    return true;

}


$('#EmailCus').on('input', function () {
    ModelStateRemoveError("EmailCus");
});
$('#PhoneCustomer').on('input', function () {
    ModelStateRemoveError("PhoneCustomer");
});
$('#COD_radio').on('click', function () {
    ModelStateRemoveError("PaymentMethod");
});
$('#VISA_radio').on('click', function () {
    ModelStateRemoveError("PaymentMethod");
});
$('#VNPAY_radio').on('click', function () {
    ModelStateRemoveError("PaymentMethod");
});
$('#MOMO_radio').on('click', function () {
    ModelStateRemoveError("PaymentMethod");
});


$('#address').on('input', function () {
    ModelStateRemoveError("address");
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