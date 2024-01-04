function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkCustomer() {

    var codeValue = $("#Code").val();
    var cityValue = $("#city").val();
    var districtValue = $("#district").val();
    var wardValue = $("#ward").val();
    var lastNameValue = $("#LastName").val();
    var firstNameValue = $("#FirstName").val();
    var cccdValue = $("#cccd").val();
    var dateOfBirthValue = $("#DateOfBirth").val();
    var emailRegex = /^[a-zA-Z0-9._-]+@gmail\.com$/;
    var emailValue = $("#Email").val();
    var phoneValue = $("#Phone").val();
    var addressValue = $("#Address").val();
    var acctype = $("#AccountType").val();
    var passwordValue = $("#Password").val();


    if (codeValue !== "") {
        // Gọi đến phương thức kiểm tra trùng lặp ở server
        $.ajax({
            url: "/Customer/CheckDuplicateCode",
            method: "POST",
            data: { code: codeValue },
            success: function (data) {
                if (data.isDuplicate) {
                    // Thêm lỗi vào trường mã định danh
                    ModelStateAddError("Code", "Mã định danh bị trùng.");
                    var newCode = generateRandomCode();
                    $("#Code").val(newCode);
                } else {
                    // Xoá lỗi nếu không còn mã định danh bị trùng
                    ModelStateRemoveError("Code");
                }
            },
            error: function (error) {
                console.error(error);
            }
        });
    }
    else {
        ModelStateRemoveError("Code");
    }
    //kiểm tra họ
    if (lastNameValue !== "" && /[0-9!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(lastNameValue)) {
        ModelStateAddError("lastName", "Họ chỉ được chứa ký tự chữ.");
    } else {
        ModelStateRemoveError("lastName");
    }
    //kiểm tra tên
    if (firstNameValue === "") {
        ModelStateAddError("firstName", "Tên không được để  trống.");
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


    // Kiểm tra email theo regex
    if (emailValue !== "" && !emailRegex.test(emailValue)) {
        ModelStateAddError("email", "Email bạn nhập sai định dạng");
    }
    else {
        ModelStateRemoveError("email");
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

    //kiểm tra tên tài khoản
    if (acctype === "") {
        ModelStateAddError("accType", "Tên tài khoản không được để trống. Vui lòng nhập giá trị.");
    } else if (!/^\d{8,12}$/.test(phoneValue)) {
        ModelStateAddError("accType", "Tên tài khoản khách hàng phải chứa từ 8 đến 12 ký tự số.");
    } else {
        ModelStateRemoveError("accType")
    }

   
    //kiểm tra mật khẩu
    if (passwordValue === "") {
        ModelStateAddError("password", "Mật khẩu không được để trống.");
    }
    else if (passwordValue.length < 8 || !(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@@$!%*?&])[A-Za-z\d@@$!%*?&]+$/.test(passwordValue))) {
        ModelStateAddError("password", "Mật khẩu phải chứa ít nhất 8 ký tự, một chữ cái, một số và một ký tự đặc biệt.");
    }
    else {
        ModelStateRemoveError("password");
    }


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ( $("#CodeError").text() !== "" || $("#lastNameError").text() !== "" || $("#firstNameError").text() !== ""
        || cityValue === "" || districtValue === "" || wardValue === "" || $("#dateOfBirthError").text() !== ""
        || $("#emailError").text() !== "" || $("#phoneError").text() !== "" || $("#addressError").text() !== ""
        || $("#passwordError").text() !== "" || $("#accTypeError").text() !== "") {
        return false;
    }

    return true;

}

function generateRandomCode() {
    const prefix = "KH";
    const characters = "0123456789";
    const length = 8;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}


function checkEditCustomer() {

    var lastNameValue = $("#LastName").val();
    var firstNameValue = $("#FirstName").val();
    var dateOfBirthValue = $("#DateOfBirth").val();
    var emailRegex = /^[a-zA-Z0-9._-]+@gmail\.com$/;
    var emailValue = $("#Email").val();
    var phoneValue = $("#Phone").val();
    var addressValue = $("#Address").val();

    phoneValue = phoneValue.replace(/\s+/g, '');


    //kiểm tra họ
    if (lastNameValue !== "" && /[0-9!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(lastNameValue)) {
        ModelStateAddError("lastName", "Họ chỉ được chứa ký tự chữ.");
    } else {
        ModelStateRemoveError("lastName");
    }
    //kiểm tra tên
    if (firstNameValue === "") {
        ModelStateAddError("firstName", "Vui lòng nhập Tên.");
    } else if (/[0-9!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(firstNameValue)) {
        ModelStateAddError("firstName", "Tên chỉ được chứa ký tự chữ.");
    }
    else {
        ModelStateRemoveError("firstName");
    }
    //kiểm tra ngày sinh
    if (dateOfBirthValue) {
        var currentDate = new Date();
        var minimumBirthDate = new Date(currentDate.getFullYear() - 18, currentDate.getMonth(), currentDate.getDate());

        var selectedDate = new Date(dateOfBirthValue);

        if (selectedDate >= currentDate || selectedDate > minimumBirthDate) {
            ModelStateAddError("dateOfBirth", "Ngày sinh phải nhỏ hơn ngày hiện tại và đủ 18 tuổi.");
        }
    }
    else {
        ModelStateRemoveError("dateOfBirth");
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


    // Kiểm tra email theo regex
    if (emailValue !== "" && !emailRegex.test(emailValue)) {
        ModelStateAddError("email", "Email bạn nhập sai định dạng");
    }
    else {
        ModelStateRemoveError("email");
    }

    if (addressValue === "") {
        ModelStateAddError("address", "Địa chỉ không được để trống.");
    }
    else {
        ModelStateRemoveError("address");
    }





    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#lastNameError").text() !== "" || $("#firstNameError").text() !== ""
        || $("#dateOfBirthError").text() !== ""   || $("#emailError").text() !== "" || $("#phoneError").text() !== "" || $("#addressError").text() !== ""
        || $("#passwordError").text() !== "" || $("#accTypeError").text() !== "") {
        return false;
    }

    return true;

}

document.getElementById("Phone").addEventListener("input", function () {
    // Lấy giá trị của thẻ "Phone"
    var phoneValue = this.value;

    // Gán giá trị của "Phone" cho thẻ "AccountType"
    document.getElementById("AccountType").value = phoneValue;
});
// Đặt giá trị ngẫu nhiên vào các trường input khi tài liệu được tải
$(document).ready(function () {
    // Lấy thẻ input theo ID
    var codeInput = $("#Code");

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

});
$('#lastName').on('input', function () {
    ModelStateRemoveError("lastName");
});
$('#firstName').on('input', function () {
    ModelStateRemoveError("firstName");
});
$('#cccd').on('input', function () {
    ModelStateRemoveError("cccd");
});
$('#image-upload').on('input', function () {
    ModelStateRemoveError("Image");
});
$('#Phone').on('input', function () {
    ModelStateRemoveError("phone");
    ModelStateRemoveError("accType");
});
$('#Email').on('input', function () {
    ModelStateRemoveError("email");
});
$('#Password').on('input', function () {
    ModelStateRemoveError("password");
});
$('#AccountType').on('input', function () {
    ModelStateRemoveError("accType");
});

$('#DateOfBirth').on('input', function () {
    ModelStateRemoveError("dateOfBirth");
});
$('#Address').on('input', function () {
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