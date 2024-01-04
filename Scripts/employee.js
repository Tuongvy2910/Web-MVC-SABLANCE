function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkEmployee() {

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
    var image = $("#image-upload").val();


    if (codeValue !== "") {
        // Gọi đến phương thức kiểm tra trùng lặp ở server
        $.ajax({
            url: "/Employee/CheckDuplicateCode",
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
    if (firstNameValue === "" ) {
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
        var minimumBirthDate = new Date(currentDate.getFullYear() - 18, currentDate.getMonth(), currentDate.getDate());

        var selectedDate = new Date(dateOfBirthValue);

        if (selectedDate >= currentDate || selectedDate > minimumBirthDate) {
            ModelStateAddError("dateOfBirth", "Ngày sinh phải nhỏ hơn ngày hiện tại và đủ 18 tuổi.");
        }
    }
    else {
        ModelStateRemoveError("dateOfBirth");
    }
    //kiểm tra cccd
    if (cccdValue !== "") {
        if (cccdValue.length > 20 || !/^\d+$/.test(cccdValue)) {
            // Nếu không đạt điều kiện, thực hiện xử lý lỗi ở đây
            ModelStateAddError("cccd", "CCCD không hợp lệ. Vui lòng kiểm tra lại.");
        }
    }
    else {
        ModelStateRemoveError("cccd");
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
    } else if (acctype.length > 100) {
        ModelStateAddError("accType", "Tên tài khoản không được vượt quá 100 ký tự.");
    } else if (/[^a-zA-Z0-9]/.test(acctype)) {
        // Kiểm tra không chứa ký tự đặc biệt
        ModelStateAddError("accType", "Tên tài khoản không được chứa ký tự đặc biệt. Vui lòng chỉ sử dụng chữ cái và số.");
    }
    else {
        ModelStateRemoveError("accType");
    }
    //kiểm tra mật khẩu
    if (passwordValue ==="") {
        ModelStateAddError("password", "Mật khẩu không được để trống.");
    }
    else if (passwordValue.length < 8 || !(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@@$!%*?&])[A-Za-z\d@@$!%*?&]+$/.test(passwordValue))) {
        ModelStateAddError("password", "Mật khẩu phải chứa ít nhất 8 ký tự, một chữ cái, một số và một ký tự đặc biệt.");
    }
    else {
        ModelStateRemoveError("password");
    }


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#ImageError").text() !== "" || $("#CodeError").text() !== "" || $("#cccdError").text() !== ""
        || $("#lastNameError").text() !== "" || $("#firstNameError").text() !== ""
        || cityValue === "" || districtValue === "" || wardValue === "" || $("#dateOfBirthError").text() !== ""
        || $("#emailError").text() !== "" || $("#phoneError").text() !== "" || $("#addressError").text() !== ""
        || $("#passwordError").text() !== "" || $("#accTypeError").text() !== "") {
        return false;
    }

    return true;

}

function generateRandomCode() {
    const prefix = "NV";
    const characters = "0123456789";
    const length = 8;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}

function generateRandomCateEmCode() {
    const prefix = "POSN";
    const characters = "0123456789";
    const length = 6;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}

function checkEditEmployee() {

    var lastNameValue = $("#LastName").val();
    var firstNameValue = $("#FirstName").val();
    var cccdValue = $("#cccd").val();
    var dateOfBirthValue = $("#DateOfBirth").val();
    var emailRegex = /^[a-zA-Z0-9._-]+@gmail\.com$/;
    var emailValue = $("#Email").val();
    var phoneValue = $("#Phone").val();
    var addressValue = $("#Address").val();
    phoneValue = phoneValue.replace(/\s+/g, '');
    cccdValue = cccdValue.replace(/\s+/g, '');

    
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
    //kiểm tra cccd
    if (cccdValue !== "") {
        if (cccdValue?.length > 20 ) {
            ModelStateAddError("cccd", "CCCD vượt quá số ký tự.");
        }
        if (!/^\d+$/.test(cccdValue)) {
            ModelStateAddError("cccd", "CCCD chỉ được nhập số");
        }
    }
    else {
        ModelStateRemoveError("cccd");
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

    //kiểm tra tên tài khoản
    
    


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#ImageError").text() !== "" ||  $("#cccdError").text() !== ""
        || $("#lastNameError").text() !== "" || $("#firstNameError").text() !== ""
        || $("#dateOfBirthError").text() !== ""
        || $("#emailError").text() !== "" || $("#phoneError").text() !== "" || $("#addressError").text() !== ""
        || $("#passwordError").text() !== "" || $("#accTypeError").text() !== "") {
        return false;
    }

    return true;

}
function checkCateEm() {
    var Code = $('#CodeCate').val();
    var Name = $('#Name').val();


    if (Code === "") {
        ModelStateAddError("Code", "Bạn vui lòng nhập mã chức vụ.");
        $("#Code").prop("disabled", true);
    }
    else {
        // Xóa lỗi khỏi ModelState (nếu có)
        ModelStateRemoveError("Code");

        // Bật trường nhập liệu
    }

    if (Name === "") {
        ModelStateAddError("NameCate", "Bạn vui lòng nhập tên chức vụ nhân viên.");
    } else if ($("#Name").val().length > 100) {
        ModelStateAddError("NameCate", "Tên chức vụ nhân viên không được quá 100 ký tự");
    }
    else if (/[!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(Name)) {
        ModelStateAddError("NameCate", "Tên chức vụ nhân viên không được chứa ký tự đặc biệt");
    }
    else {
        ModelStateRemoveError("NameCate");
    }

    if ($("#CodeError").text() !== "" || $("#NameCateError").text() !== "") {
        return false;
    }

    return true;
}
// Hàm sinh chuỗi ngẫu nhiên cho AccountType
function generateRandomAccountType(code) {
    const prefix = "sablanca";
    return prefix + code.substr(2); // Kết hợp "sablanca" với phần số random từ Code
}

// Đặt giá trị ngẫu nhiên vào các trường input khi tài liệu được tải
$(document).ready(function () {
    // Lấy thẻ input theo ID
    var codeInput = $("#Code");
    var accountTypeInput = $("#AccountType");
    var CodeCateInput = $("#CodeCate");

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

    // Gọi hàm sinh chuỗi ngẫu nhiên cho AccountType và đặt giá trị vào trường input "AccountType"
    var generatedAccountType = generateRandomAccountType(generatedCode);
    accountTypeInput.val(generatedAccountType);

    var generatedCateEm = generateRandomCateEmCode();
    CodeCateInput.val(generatedCateEm);
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
$('#Name').on('input', function () {
    ModelStateRemoveError("NameCate");
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