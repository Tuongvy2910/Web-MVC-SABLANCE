using System;
using System.Linq;
using System.Web.Mvc;
using SABLANCE.Models;
using SABLANCE.ViewModel;

using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;
using System.Data.Entity.Infrastructure;

namespace SABLANCE.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        DataSablancaShopEntities db = new DataSablancaShopEntities();

        public ActionResult Login()
        {
            return View();
        }

        //Lấy dữ liệu
        [HttpPost]
        public ActionResult Login(string accountType, string password)
        {
            // Kiểm tra đăng nhập tại đây
            
            
            var account = db.Accounts.SingleOrDefault(a => a.AccountType == accountType && a.Password == password);
            if (account != null)
            {
                // Đăng nhập thành công
                if (account.EmployeeID != null && account.CustomerID == null)
                {
                    // Nếu là nhân viên, chuyển hướng đến trang chính của nhân viên
                    
                    if (password == account.Password && accountType == account.AccountType)
                    {
                        Session["Account"] = account;
                        string script = @"
        Toastify({
            text: 'Đăng nhập tài khoản thành công',
            duration: 3000,
            close: true,
            gravity: 'bottom',
            position: 'right',
            backgroundColor: '#4CAF50',
            stopOnFocus: true,
        }).showToast();";

                        TempData["ToastScript"] = script;

                        return RedirectToAction("HomeAdmin", "HomeAdmin", new { area = "Admin" });
                    }
                    else if (password != account.Password)
                    {
                        ModelState.AddModelError("Password", "Mật khẩu đăng nhập không đúng.");
                        return View();
                    }
                }
                else if (account.CustomerID != null && account.EmployeeID == null)
                {
                    if(password == account.Password && accountType == account.AccountType )
                    {
                        Session["Account"] = account;// Nếu là khách hàng, chuyển hướng đến trang chính của khách hàng
                        Session["CountCart"] = db.Carts.Where(a => a.IdCustommer == account.CustomerID).ToList().Count();
                        string script = @"
        Toastify({
            text: 'Đăng nhập tài khoản thành công',
            duration: 3000,
            close: true,
            gravity: 'bottom',
            position: 'right',
            backgroundColor: '#4CAF50',
            stopOnFocus: true,
        }).showToast();";

                        TempData["ToastScript"] = script;
                        return RedirectToAction("Home", "Home", new { Id = account.CustomerID });
                    }
                    else if(password != account.Password)
                    {
                                ModelState.AddModelError("Password", "Mật khẩu đăng nhập không đúng.");
                                return View();
                    }   
                    
                }
            }
            else
            {
                if (accountType == "" && password == "")
                {
                    ModelState.AddModelError("AccountType", "Vui lòng nhập tên tài khoản đăng nhập.");
                    ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu đăng nhập.");
                    return View();
                }
                if (accountType == "" && password !=  "")
                {
                    ModelState.AddModelError("AccountType", "Vui lòng nhập tên tài khoản đăng nhập.");
                    return View();
                }
                if (accountType !=  "" && password == "")
                {
                    ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu đăng nhập.");
                    return View();
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string phone_no)
        {
            var user = db.Customers.FirstOrDefault(u => u.Phone == phone_no);

            if (user != null)
            {
                ViewBag.Message = "Số điện thoại này đã được đăng ký. Hãy đăng nhập.";
                return RedirectToAction("Login", "Login");
            }
            else
            {
                // Nếu chưa đăng ký, có thể chuyển hướng hoặc thực hiện các bước đăng ký khác
                return Json(new { status = 1, title = "", text = "success", obj = "" }); ;
            }

        }


        private string GenerateRandomCode()
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            int length = random.Next(5, 11); // Độ dài ngẫu nhiên từ 5 đến 10 ký tự
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = characters[random.Next(characters.Length)];
            }

            return new string(result);
        }
        public ActionResult RegisterSuccess()
        {
            return View();

        }
        [HttpPost]

        public ActionResult RegisterSuccess(CustomerModel cus, string phone_no)
        {
            string  phone;
            
            try
            {
                //code = GenerateRandomCode();
                phone = phone_no;
                Customer customer = new Customer();

                customer.Code = GenerateRandomCode();
                customer.FirstName = cus.FirstName;
                customer.LastName = cus.LastName;
                customer.DateOfBirth = cus.DateOfBirth;
                customer.Gender = cus.Gender;
                customer.Address = cus.Address;
                customer.Phone = phone;
                customer.Email = cus.Email;
                customer.MemberID = 5;


                Account acc = new Account();
                acc.AccountType = phone_no;
                acc.CustomerID = customer.ID;
                acc.Password = cus.Password;
                acc.RegistrationDate = DateTime.UtcNow.AddHours(7);

                if (!ModelState.IsValid)
                {
                    return Json(new { status = -1, message = "Errors" });
                }


                db.Customers.Add(customer);
                db.Accounts.Add(acc);
                db.SaveChanges();
                try
                {
                    customer.AccountID = acc.ID;
                    db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    db.Accounts.Remove(acc);
                    db.SaveChanges();
                    return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = 1, message = "Success" });
            }

            catch (DbEntityValidationException ex)
            {
                string err = "";
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        err += $"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}" + "\n";
                    }
                }
                return Json(new { status = -1, title = "", text = err, obj = "" }, JsonRequestBehavior.AllowGet);
            }


        }
        public class OrderHistoryViewModel
        {
            public decimal OrderID { get; set; }
            public string OrderNo { get; set; }
            public DateTime OrderDate { get; set; }
            public int TotalQuantity { get; set; }
            public double TotalAmount { get; set; }
            public string LatestOrderStatus { get; set; }
        }
        [HttpGet]
        public ActionResult ProfileCustomer(int Id)
        {
            var cus = db.Customers.Where(n => n.ID == Id).FirstOrDefault();
            var query = from order in db.Orders
                        join orderStatus in db.OrderStatusDetails
                            on order.ID equals orderStatus.OrderID into gj
                        from subOrderStatus in gj.DefaultIfEmpty()
                        where (order.CustomerID == Id) && (subOrderStatus == null ||
                            (subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate)))
                        orderby order.OrderDate descending
                        select new OrderHistoryViewModel
                        {
                            OrderID = order.ID,
                            OrderNo = order.OrderNo,
                            OrderDate = order.OrderDate,
                            TotalAmount = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                            LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null,
                            // Đếm số lượng sản phẩm trong đơn đặt hàng
                            TotalQuantity = order.OrderDetails.Count()
                        };

            var history = query.ToList();
            ViewBag.HistoryOrder = history;

            var totalPoints = db.Orders.Where(o => o.CustomerID == Id).Sum(o => o.PointDetailed.Point);
            if(totalPoints <= 100 || totalPoints == null)
            {
                cus.MemberID = 5;
                db.SaveChanges();
            } 
            else if(totalPoints > 100 && totalPoints <=250)
            {
                cus.MemberID = 1;
                db.SaveChanges();
            }
            else if (totalPoints > 250 && totalPoints <= 400)
            {
                cus.MemberID = 2;
                db.SaveChanges();
            }
            else if (totalPoints > 400 && totalPoints <= 600)
            {
                cus.MemberID = 3;
                db.SaveChanges();
            }
            else
            {
                cus.MemberID = 4;
                db.SaveChanges();
            }
            ViewBag.Point = totalPoints;
            return View(cus);
        }

        [HttpPost]
        public ActionResult OrderDetailView(int Id)
        {
            var order = db.Orders.Find(Id);
            ViewBag.OrdersCustomer = order;

            var ordetail = db.OrderDetails.Where(a => a.OrderID == Id).ToList();
            ViewBag.OrdersDetailCustomer = ordetail;


            var dataPoint = db.PointDetaileds.FirstOrDefault(c => c.OrderID == Id);
            if (dataPoint != null)
            {
                ViewBag.Point = dataPoint.Point;
            }
            else
            {
                ViewBag.Point = "Không cập nhật được điểm";
            }


            var dataStatus = db.OrderStatusDetails.Where(c => c.OrderID == Id).ToList();
            if (dataStatus != null)
            {
                ViewBag.OrderStatus = dataStatus;
            }
            else
            {
                ViewBag.OrderStatus = "Chưa xác định";
            }



            return PartialView("_OrderDetailCustomerView", order);
        }
        private bool IsPhoneNumberValid(string phoneNumber)
        {
            // Kiểm tra xem chuỗi chỉ chứa các chữ số không
            return Regex.IsMatch(phoneNumber, @"^\d+$");
        }
        private bool IsPasswordValid(string password)
        {
            // Kiểm tra xem mật khẩu có chứa ít nhất một chữ cái, một số và một ký tự đặc biệt không
            return Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$");
        }
        private bool IsAlphaOnly(string input)
        {
            return Regex.IsMatch(input, @"^[a-zA-Z]+$");
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ChangePasswords()
        {
            return View();
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
        [HttpGet]
        public ActionResult ForgetPass()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPass(string phone_no)
        {
            var user = db.Customers.FirstOrDefault(u => u.Phone == phone_no);

            if (user != null)
            {
                return Json(new { status = 1, title = "", text = "success", obj = "" });
            }
            else
            {
                ViewBag.Message = "Số điện thoại này chưa đăng ký. Bạn vui lòng đăng ký tài khoản.";
                return RedirectToAction("Login", "Login");
            }
        }

        public ActionResult ForgetPassSuccess(string phone_no)
        {
            var cusID = db.Customers.FirstOrDefault(a => a.Phone == phone_no);
            if (cusID != null)
            {
                return View(cusID);
            }
            else
            {
                return Json(new { status = -1, title = "", text = "Không tìm thấy tài khoản của bạn.", obj = "" }); ;
            }

        }
        [HttpPost]

        public ActionResult ForgetPassSuccess(int Id, string newPassword)
        {
            var cus = db.Accounts.FirstOrDefault(a => a.CustomerID == Id);

            if (cus != null)
            {
                //cập nhật mật khẩu mới
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
        // Your other controller methods...
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
        public ActionResult Logout()
        {
            Session.Clear();//remove session
            return RedirectToAction("Home", "Home");

        }
    }

}