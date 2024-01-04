function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function generateRandomCode() {
    const prefix = "MEMBER";
    const characters = "0123456789";
    const length = 4;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;

}
$(document).ready(function () {
    // Lấy thẻ input theo ID
    var codeInput = $("#Code");

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

});

function checkMember() {
    var Code = $('#Code').val();
    var Name = $('#Name').val();


    if (Code === "") {
        ModelStateAddError("Code", "Bạn vui lòng nhập mã Code.");
        $("#Code").prop("disabled", true);
    }
    else {
        // Xóa lỗi khỏi ModelState (nếu có)
        ModelStateRemoveError("Code");

        // Bật trường nhập liệu
    }

    if (Name === "") {
        ModelStateAddError("NameMember", "Bạn vui lòng nhập tên loại thành viên.");
    } else if ($("#Name").val().length > 100) {
        ModelStateAddError("NameMember", "Tên loại thành viên không được quá 100 ký tự");
    }
    else if (/[!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(Name)) {
        ModelStateAddError("NameMember", "Tên loại thành viên không được chứa ký tự đặc biệt");
    }
    else {
        ModelStateRemoveError("NameMember");
    }

    if ($("#CodeError").text() !== "" || $("#NameMemberError").text() !== "") {
        return false;
    }

    return true;
}


$('#Code').on('input', function () {
    ModelStateRemoveError("Code");
});
$('#Name').on('input', function () {
    ModelStateRemoveError("NameMember");
});