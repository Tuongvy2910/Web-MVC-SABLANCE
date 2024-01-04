function notify(messenge, type) {
    switch (type) {
        case 'success':
            toastr.success(messenge);
            break;
        case 'info':
            toastr.info(messenge);
            break;
        case 'error':
            toastr.error(messenge);
            break;
        case 'warning':
            toastr.warning(messenge);
            break;
        default:
            toastr.error(messenge);
    }
}