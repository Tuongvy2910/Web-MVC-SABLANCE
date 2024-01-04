$(document).ready(function () {
    // Ẩn modal khi nhấn nút "OK"
    var employeeId = $('#IdEm').val();
    $('#successModal').on('hidden.bs.modal', function () {
        window.location.href = "/Admin/HomeAdmin/SettingEmployee/" + employeeId;
    });
});

function validateForm() {
    var oldPassword = $('#oldPassword').val();
    var password = $('#passAcc').val();
    var newPassword = $('#newPassword').val();
    var confirmPassword = $('#confirmPassword').val();
    //kiểm tra pass mới và pass cũ
    if (confirmPassword.trim() === "") {
        ModelStateAddError("confirmPassword", "Vui lòng nhập lại mật khẩu mới.");
    }
    else if (newPassword !== confirmPassword) {
        ModelStateAddError("confirmPassword", "Xác nhận mật khẩu mới không khớp.");
    } else {
        ModelStateRemoveError("confirmPassword");
    }
    //kiểm tra pass cũ
    if (oldPassword.trim() === "") {
        ModelStateAddError("oldPassword", "Vui lòng nhập lại mật khẩu cũ.");
    }
    else if (password !== oldPassword) {
        ModelStateAddError("oldPassword", "Mật khẩu cũ không đúng");
    } else {
        ModelStateRemoveError("oldPassword");
    }

    //kiểm tra nhập pass mới
    if (newPassword.trim() === "") {
        ModelStateAddError("newPassword", "Mật khẩu mới không được để trống.");
    }
    else if (newPassword.length < 8 || !(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@@$!%*?&])[A-Za-z\d@@$!%*?&]+$/.test(newPassword))) {
        ModelStateAddError("newPassword", "Mật khẩu mới phải chứa ít nhất 8 ký tự, một chữ cái, một số và một ký tự đặc biệt.");
    }
    else {
        ModelStateRemoveError("newPassword");
    }

    if ($("#oldPasswordError").text() !== "" || $("#newPasswordError").text() !== ""
        || $("#confirmPasswordError").text() !== "") {
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
$('#oldPassword').on('input', function () {
    ModelStateRemoveError("oldPassword");
});
$('#newPassword').on('input', function () {
    ModelStateRemoveError("newPassword");
});
$('#confirmPassword').on('input', function () {
    ModelStateRemoveError("confirmPassword");
});