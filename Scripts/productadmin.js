function ModelStateAddError(fieldName, errorMessage) {
    // Hiển thị thông báo lỗi của trường
    $("#" + fieldName + "Error").text(errorMessage).show();
}

function ModelStateRemoveError(fieldName) {
    // Xóa thông báo lỗi của trường
    $("#" + fieldName + "Error").text("");
}
function checkCateProduct() {
    var Code = $('#CodeCate').val();
    var Name = $('#Name').val();


    if (Code === "") {
        ModelStateAddError("Code", "Bạn vui lòng nhập mã định danh loại sản phẩm.");
        $("#Code").prop("disabled", true);
    }
    else {
        // Xóa lỗi khỏi ModelState (nếu có)
        ModelStateRemoveError("Code");

        // Bật trường nhập liệu
    }

    if (Name === "") {
        ModelStateAddError("Name", "Bạn vui lòng nhập tên loại sản phẩm.");
    } else if ($("#Name").val().length > 100) {
        ModelStateAddError("Name", "Tên loại sản phẩm không được quá 100 ký tự");
    }
    else if (/[!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(Name)) {
        ModelStateAddError("Name", "Tên loại sản phẩm không được chứa ký tự đặc biệt");
    }
    else {
        ModelStateRemoveError("Name");
    }

    if ($("#CodeError").text() !== "" || $("#NameError").text() !== "") {
        return false;
    }

    return true;
}

function checkCateProductDetail() {
    var Code = $('#CodeCate').val();
    var Name = $('#Name').val();
    console.log(Code)
    var NameProduct = $('#product-table input[id^="ProductName"').map(function () {
        return $(this).val();
    }).get();



    if (Code === "") {
        ModelStateAddError("Code", "Bạn vui lòng nhập mã định danh loại sản phẩm.");
        $("#Code").prop("disabled", true);
    }
    else {
        // Xóa lỗi khỏi ModelState (nếu có)
        ModelStateRemoveError("Code");

        // Bật trường nhập liệu
    }

    if (Name === "") {
        ModelStateAddError("Name", "Bạn vui lòng nhập tên loại sản phẩm.");
    } else if ($("#Name").val().length > 100) {
        ModelStateAddError("Name", "Tên loại sản phẩm không được quá 100 ký tự");
    }
    else if (/[!@#$%^&*()_+\-=<,.>:;"'{[}\]?/]/.test(Name)) {
        ModelStateAddError("Name", "Tên loại sản phẩm không được chứa ký tự đặc biệt");
    }
    else {
        ModelStateRemoveError("Name");
    }

    if ($("#CodeError").text() !== "" || $("#NameError").text() !== "" ) {
        return false;
    }

    return true;
}

function checkProduct() {

    var NameValue = $("#Name").val();
    var ColorValue = $("#Color").val();
    var PricesValue = $("#Prices").val();


    

    //kiểm tra tên
    if (NameValue === "") {
        ModelStateAddError("Name", "Vui lòng nhập Tên sản phẩm.");
    } else if (/[!@#$%^&*()_+\-=<,.>:;"'{}\]?/]/.test(NameValue)) {
        ModelStateAddError("Name", "Tên sản phẩm không được chứa ký tự đặc biệt.");
    }
    else {
        ModelStateRemoveError("Name");
    }

    
    //kiểm tra màu
    if (ColorValue === "0") {
            ModelStateAddError("Color", "Vui lòng chọn màu sắc của sản phẩm.");
    }
    else {
        ModelStateRemoveError("Color");
    }
    //kiểm tra giá
    if (PricesValue === "") {
        ModelStateAddError("Prices", "Nhập đơn giá của sản phẩm.");
    }
    else if (!/^[0-9]+$/.test(PricesValue) || parseInt(PricesValue) <= 0) {
        ModelStateAddError("Prices", "Giá phải là một số và lớn hơn 0");
    }
    else {
        ModelStateRemoveError("Prices");
    }


   


    //// Nếu có ít nhất một trường không hợp lệ, không submit form
    if ($("#ImageError").text() !== "" || $("#CodeError").text() !== "" || $("#NameError").text() !== ""
        || $("#ColorError").text() !== "" || $("#PricesError").text() !== "") {
        return false;
    }

    return true;

}
function checkColor() {
    var CodeValue = $("#HexCode").val();
    var NameValue = $("#Name").val();

    if (CodeValue === "") {
        ModelStateAddError("Code", "Vui lòng chọn màu sắc.");
    }
    else {
        ModelStateRemoveError("Code");
    }

    if (NameValue === "") {
        ModelStateAddError("Name", "Vui lòng chọn màu sắc.");
    }
    else if (NameValue.length > 100) {
        ModelStateAddError("Name", "Tên màu sắc quá dài(dưới 100 ký tự)");
    }
    else {
        ModelStateRemoveError("Name");
    }
    if ( $("#CodeError").text() !== "" || $("#NameError").text() !== "") {
        return false;
    }

    return true;

}

function checkSize() {
    var CodeValue = $("#CodeSzie").val();
    var NameValue = $("#Name").val();

    if (CodeValue === "") {
        ModelStateAddError("Code", "Vui lòng nhập mã màu.");
    }
    else {
        ModelStateRemoveError("Code");
    }

    if (NameValue === "") {
        ModelStateAddError("Name", "Vui lòng nhập tên kích thước.");
    }
    else if (NameValue.length > 100) {
        ModelStateAddError("Name", "Tên kích thước quá dài(dưới 100 ký tự)");
    }
    else {
        ModelStateRemoveError("Name");
    }
    if ($("#CodeError").text() !== "" || $("#NameError").text() !== "") {
        return false;
    }

    return true;

}


function generateRandomCode() {
    const prefix = "PRODUCT";
    const characters = "0123456789";
    const length = 3;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}

function RandomCodeSize() {
    const prefix = "SIZE";
    const characters = "0123456789";
    const length = 6;

    let result = prefix;
    for (let i = 0; i < length; i++) {
        result += characters[Math.floor(Math.random() * characters.length)];
    }

    return result;
}

function RandomCodeProduct() {
    const prefix = "DET";
    const characters = "0123456789";
    const length = 7;

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
    var CodeProductInput = $("#CodeProduct");
    var CodeSize = $("#CodeSize");

    // Gọi hàm sinh chuỗi ngẫu nhiên cho Code và đặt giá trị vào trường input "Code"
    var generatedCode = generateRandomCode();
    codeInput.val(generatedCode);

    var generatedCodePro = RandomCodeProduct();
    CodeProductInput.val(generatedCodePro);

    var generatedCodeSize = RandomCodeSize();
    CodeSize.val(generatedCodeSize);

  
});
$('#Code').on('input', function () {
    ModelStateRemoveError("Code");
});
$('#ProductName').on('input', function () {
    ModelStateRemoveError("ProductName");
});
$('#Name').on('input', function () {
    ModelStateRemoveError("Name");
});

$('#Prices').on('input', function () {
    ModelStateRemoveError("Prices");
});
$('#Color').on('input', function () {
    ModelStateRemoveError("Color");
});
$('#image-upload').on('change', function () {
    ModelStateRemoveError("Image");
});



function displayErrors(errors) {
    // Xóa tất cả các thông báo lỗi trước đó
    /*$(".error-message").text("");*/

    // Hiển thị các thông báo lỗi mới
    errors.forEach(function (error) {
        ModelStateAddError(error.field, error.message);
    });
}

