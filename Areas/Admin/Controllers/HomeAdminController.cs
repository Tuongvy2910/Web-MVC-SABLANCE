using SABLANCE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SABLANCE.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Admin/Home
        public ActionResult HomeAdmin()
        {
            int orderCount = GetOrderCount(); // Hãy thay thế hàm này với cách lấy dữ liệu của bạn
            decimal revenue = GetRevenueTotal();

            decimal customer = GetCustomerCount();
            decimal employee = GetEmployeeCount();
            // Gán số lượng đơn hàng vào ViewBag
            ViewBag.OrderCount = orderCount;

            ViewBag.ReveneTotal = revenue.ToString("0,0 VNĐ");

            ViewBag.CustomerCount = customer;

            ViewBag.EmployeeCount = employee;

            var data = db.Orders
            .GroupBy(order => new { Month = order.OrderDate.Month, Year = order.OrderDate.Year })
            .Select(group => new
            {
                Month = group.Key.Month,
                Year = group.Key.Year,
                CustomerCount = group.Select(order => order.CustomerID).Distinct().Count(),
                OrderCount = group.Count(),
                Revenue = group.Sum(order => order.TotalPayment)

            })
            .OrderBy(group => group.Year)
            .ThenBy(group => group.Month)
            .ToList();

            ViewBag.ChartData = data;

            var currentDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var topProducts = db.OrderDetails
        .Where(detail => detail.Order.OrderDate >= firstDayOfMonth && detail.Order.OrderDate <= lastDayOfMonth)
        .GroupBy(detail => detail.ProductDetail)
        .Select(productGroup => new TopProductData
        {
            ProductName = productGroup.Key.Code,
            QuantitySold = productGroup.Sum(detail => detail.Quantity)
        })
        .OrderByDescending(productGroup => productGroup.QuantitySold)
        .Take(5)
        .ToList();

            ViewBag.TopProducts = topProducts;

            // Chuyển danh sách tên sản phẩm và số lượng thành mảng để sử dụng trong JavaScript
            ViewBag.TopProductLabels = topProducts.Select(product => product.ProductName).ToArray();
            ViewBag.TopProductData = topProducts.Select(product => product.QuantitySold).ToArray();


            var firstDayOfCurrentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var firstDayOf3MonthsAgo = firstDayOfCurrentMonth.AddMonths(-3);

            // Lấy danh sách sản phẩm bán chạy trong 3 tháng gần nhất
            var topProducts3month = db.OrderDetails
                .Where(detail => detail.Order.OrderDate >= firstDayOf3MonthsAgo && detail.Order.OrderDate < firstDayOfCurrentMonth)
                .GroupBy(detail => detail.ProductDetail)
                .Select(productGroup => new TopProductData
                {
                    ProductName =productGroup.Key.Code,
                    QuantitySold = productGroup.Sum(detail => detail.Quantity)
                })
                .OrderByDescending(productGroup => productGroup.QuantitySold)
                .Take(5)
                .ToList();

            // Chuyển danh sách tên sản phẩm và số lượng thành mảng để sử dụng trong JavaScript
            ViewBag.TopProduct3MonthsLabels = topProducts3month.Select(product => product.ProductName).ToArray();
            ViewBag.TopProduct3MonthsData = topProducts3month.Select(product => product.QuantitySold).ToArray();

            return View();


        }
        public class TopProductData
        {
            public string ProductName { get; set; }
            public int QuantitySold { get; set; }
        }

        private decimal GetRevenueTotal()
        {
            DateTime currentDate = DateTime.Now;

            // Bắt đầu từ đầu ngày hiện tại
            DateTime startDate = currentDate.Date;

            // Kết thúc tại cuối ngày hiện tại
            DateTime endDate = startDate.AddDays(1).AddTicks(-1);

            using (DataSablancaShopEntities data = new DataSablancaShopEntities())
            {
                // Lấy tổng doanh thu của các đơn hàng có tình trạng mới nhất là 4
                decimal totalRevenue = (decimal)data.Orders
                    .Where(order => order.OrderStatusDetails.OrderByDescending(osd => osd.ConfirmationDate)
                        .FirstOrDefault().OrderStatusID == 4)
                    .Sum(order => order.TotalPayment);

                return totalRevenue;
            }
        }
        private decimal GetCustomerCount()
        {

            using (DataSablancaShopEntities data = new DataSablancaShopEntities()) // Thay "YourDbContext" bằng tên DbContext thực tế của bạn
            {
                // Tính tổng doanh thu từ trước tới giờ
                decimal customercount = (decimal)data.Customers.Count();

                // Tính tổng doanh thu trong ngày hiện tại


                // Tổng doanh thu từ trước tới giờ và trong ngày hiện tại
                decimal customer = customercount;

                return customer;
            }
        }
        private decimal GetEmployeeCount()
        {

            using (DataSablancaShopEntities data = new DataSablancaShopEntities()) // Thay "YourDbContext" bằng tên DbContext thực tế của bạn
            {
                // Tính tổng doanh thu từ trước tới giờ
                decimal employeecount = (decimal)data.Employees.Count();

                // Tính tổng doanh thu trong ngày hiện tại


                // Tổng doanh thu từ trước tới giờ và trong ngày hiện tại
                decimal employee = employeecount;

                return employee;
            }
        }

        private int GetOrderCount()
        {
            // Lấy ngày hiện tại
            DateTime currentDate = DateTime.Now;

            // Bắt đầu từ đầu ngày hiện tại
            DateTime startDate = currentDate.Date;

            // Kết thúc tại cuối ngày hiện tại
            DateTime endDate = startDate.AddDays(1).AddTicks(-1);

            using (DataSablancaShopEntities data = new DataSablancaShopEntities()) // Thay "YourDbContext" bằng tên DbContext thực tế của bạn
            {
                // Đếm số đơn hàng từ trước tới giờ
                int totalOrders = data.Orders.Count();

                // Đếm số đơn hàng trong ngày hiện tại
                //int ordersToday = data.Orders.Count(order => order.OrderDate >= startDate && order.OrderDate <= endDate);

                // Tổng số đơn hàng từ trước tới giờ và trong ngày hiện tại
                int totalAndTodayOrders = totalOrders /*+ ordersToday*/;

                return totalAndTodayOrders;
            }
        }
        public ActionResult LogoutAdmin()
        {
            // Xóa các session, cookie hoặc thông tin đăng nhập của tài khoản admin
            // Ví dụ:
            Session.Clear(); // Xóa session
            FormsAuthentication.SignOut(); // Đăng xuất khỏi Forms Authentication (nếu được sử dụng)

            // Chuyển hướng đến trang đăng nhập hoặc trang chủ
            return RedirectToAction("Home", "Home", new { area = "" }); // Điều hướng đến trang đăng nhập
                                                                        //return RedirectToAction("Index", "Home"); // Điều hướng đến trang chủ
        }

        public ActionResult SettingEmployee(int Id)
        {
           var em = db.Employees.Where(n => n.ID == Id).FirstOrDefault();

            var ordersCount = db.Orders.Where(o => o.EmployeeID == Id).Count();
            var importCount = db.InventoryIssues.Where(i => i.EmployeeID == Id).Count();
            var exportCount = db.InventoryReceipts.Where(e => e.EmployeeID == Id).Count();

            ViewBag.OrdersCount = ordersCount;
            ViewBag.ImportCount = importCount;
            ViewBag.ExportCount = exportCount;
            return View(em);
                                                                        
        }
        [HttpPost]
        public ActionResult ChangePasswords(int Id, string oldPassword, string newPassword)
        {
            var cus = db.Accounts.Find(Id);

            if (cus != null && VerifyPassword(oldPassword, cus.Password))
            {
                // Mật khẩu cũ đúng, cập nhật mật khẩu mới
                cus.Password = HashPassword(newPassword); // Hash mật khẩu mới trước khi lưu vào cơ sở dữ liệu

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                return Json(new { status = 1 });
            }
            else
            {
                // Mật khẩu cũ không đúng hoặc không tìm thấy khách hàng
                return Json(new { status = -1 });
            }

        }
        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            // Thực hiện kiểm tra mật khẩu ở đây
            // (Phụ thuộc vào cách bạn đã lưu mật khẩu, ví dụ: sử dụng BCrypt, SHA-256, ...)
            // Trả về true nếu mật khẩu đúng, ngược lại trả về false
            return enteredPassword == storedPassword;
        }

        // Hàm hash mật khẩu
        private string HashPassword(string password)
        {
            // Thực hiện hash mật khẩu ở đây
            // (Phụ thuộc vào cách bạn muốn hash, ví dụ: sử dụng BCrypt, SHA-256, ...)
            // Trả về mật khẩu đã được hash
            return password; // Đây là ví dụ đơn giản, bạn cần sử dụng phương pháp hash an toàn hơn trong ứng dụng thực tế
        }
    }
}