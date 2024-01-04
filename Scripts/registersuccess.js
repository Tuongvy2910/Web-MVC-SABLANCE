function validateForm() {
    // Lấy giá trị các trường select
    var cityValue = $("#city").val();
    var districtValue = $("#district").val();
    var wardValue = $("#ward").val();
    var lastNameValue = $("#lastName").val();
    var firstNameValue = $("#firstName").val();
    var dateOfBirthValue = $("#DateOfBirth").val();
    var emailValue = $("#email").val();
    var phoneValue = $("#phone").val();
    var addressValue = $("#address").val();
    var passwordValue = $("#Password").val();
    var confirmPasswordValue = $("#Password-confirm").val();
    var emailRegex = /^[a-zA-Z0-9._-]+@gmail\.com$/;

    //kiểm tra Password-confirm
    if (passwordValue !== confirmPasswordValue) {
        ModelStateAddError("Password-confirm", "Xác nhận mật khẩu không khớp.");
    } else {
        ModelStateRemoveError("Password-confirm");
    }

    // Thực hiện kiểm tra dữ liệu cho từng trường

    //kiểm tra họ
    if (lastNameValue !== "" && /[0-9!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(lastNameValue)) {
        ModelStateAddError("lastName", "Họ chỉ được chứa ký tự chữ.");
    } else {
        ModelStateRemoveError("lastName");
    }
    //kiểm tra tên
    if (firstNameValue.trim() === "") {
        ModelStateAddError("firstName", "Vui lòng nhập Tên.");
    } else if (/[0-9!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(firstNameValue)) {
        ModelStateAddError("firstName", "Tên chỉ được chứa ký tự chữ.");
    }
    else {
        ModelStateRemoveError("firstName");
    }

    //kiểm tra ngày sinh
    if (dateOfBirthValue) {
        // dateOfBirthValue tồn tại
        // Thực hiện kiểm tra ngày nhỏ hơn ngày hiện tại và đủ 15 tuổi
        var currentDate = new Date();
        var minimumBirthDate = new Date(currentDate.getFullYear() - 15, currentDate.getMonth(), currentDate.getDate());

        var selectedDate = new Date(dateOfBirthValue);

        if (selectedDate >= currentDate || selectedDate > minimumBirthDate) {
            ModelStateAddError("dateOfBirth", "Ngày sinh phải nhỏ hơn ngày hiện tại và đủ 15 tuổi.");
        }
    }
    else {
        ModelStateRemoveError("dateOfBirth");
    }

    //kiểm tra email

    if (emailValue !== "" && !emailRegex.test(emailValue)) {
        ModelStateAddError("email", "Email bạn nhập sai định dạng");
    }
    else {
        ModelStateRemoveError("email");
    }

    //kiểm tra số điện thoại
    if (phoneValue ==="") {
        ModelStateAddError("phone", "Số điện thoại không được để trống.");
    }
    else if (!/^\d{8,12}$/.test(phoneValue)) {
        ModelStateAddError("phone", "Số điện thoại phải chứa từ 8 đến 12 ký tự số.");
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

    if (addressValue.trim() === "") {
        ModelStateAddError("address", "Địa chỉ không được để trống.");
    }
    else {
        ModelStateRemoveError("address");
    }


    //kiểm tra mật khẩu
    if (passwordValue.trim() === "") {
        ModelStateAddError("password", "Mật khẩu không được để trống.");
    }
    else if (passwordValue.length < 8 || !(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@@$!%*?&])[A-Za-z\d@@$!%*?&]+$/.test(passwordValue))) {
        ModelStateAddError("password", "Mật khẩu phải chứa ít nhất 8 ký tự, một chữ cái, một số và một ký tự đặc biệt.");
    }
    else {
        ModelStateRemoveError("password");
    }

    // Thêm các điều kiện kiểm tra khác tương tự cho các trường còn lại

    // Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#lastNameError").text() !== "" || $("#firstNameError").text() !== ""
        || cityValue === "" || districtValue === "" || wardValue === "" || $("#dateOfBirthError").text() !== ""
        || $("#emailError").text() !== "" || $("#phoneError").text() !== "" || $("#addressError").text() !== ""
        || $("#passwordError").text() !== "") {
        return false;
    }

    return true;
}
function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}
function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}

$('#lastName').on('input', function () {
    ModelStateRemoveError("lastName");
});
$('#firstName').on('input', function () {
    ModelStateRemoveError("firstName");
});
$('#email').on('input', function () {
    ModelStateRemoveError("email");
});
$('#Password').on('input', function () {
    ModelStateRemoveError("password");
});
$('#Password-confirm').on('input', function () {
    validateConfirmPassword();
});
$('#DateOfBirth').on('input', function () {
    ModelStateRemoveError("dateOfBirth");
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
