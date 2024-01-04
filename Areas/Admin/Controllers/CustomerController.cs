using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using SABLANCE.Models;
using System.Data.Entity.Infrastructure;
using SABLANCE.Areas.Admin.Models;
using System.Threading.Tasks;
using System.Data.Entity.Validation;
using System.IO;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SABLANCE.Areas.Admin.Controllers
{
    public class CustomerController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Admin/Customer
        public ActionResult ListCustomer(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<Customer> customers = db.Customers.ToList();
            return View(customers.ToPagedList((int)page, (int)pageSize));
        }

        public class MemberRatingWithCustomerCountViewModel
        {
            public List<MemberRating> MemberRatings { get; set; }
            public int CustomerCount { get; set; }
        }
        public ActionResult ListMemberRatings(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<MemberRating> memberRatings = db.MemberRatings.ToList();

            // Tạo một Dictionary để lưu trữ số lượng khách hàng cho từng MemberID
            Dictionary<decimal, decimal> customerCountsByMemberID = new Dictionary<decimal, decimal>();

            // Đếm số lượng khách hàng cho từng MemberID
            foreach (var memberRating in memberRatings)
            {
                int customerCount = db.Customers.Count(c => c.MemberID == memberRating.ID);
                customerCountsByMemberID.Add(memberRating.ID, customerCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.CustomerCountsByMemberID = customerCountsByMemberID;

            return View(memberRatings.ToPagedList((int)page, (int)pageSize));

        }
        [HttpPost]
        public JsonResult CheckDuplicateCode(string code)
        {
            // Thực hiện kiểm tra trùng mã trên server
            bool isDuplicate = CheckIfCodeIsDuplicate(code);

            return Json(new { isDuplicate = isDuplicate }, JsonRequestBehavior.AllowGet);
        }
        private bool CheckIfCodeIsDuplicate(string code)
        {
            // Kiểm tra xem có nhân viên nào có mã Code trùng với mã được kiểm tra hay không
            return db.Customers.Any(e => e.Code == code);
        }

        [HttpPost]
        public ActionResult RegisterCustomer(string phone_no)
        {
            var user = db.Accounts.FirstOrDefault(u => u.AccountType == phone_no);

            if (user != null)
            {
                
                return Json(new { status = -1, title = "", text = "success", obj = "" }); ;
            }
            else
            {
                // Nếu chưa đăng ký, có thể chuyển hướng hoặc thực hiện các bước đăng ký khác
                return Json(new { status = 1, title = "", text = "success", obj = "" }); ;
            }

        }

        [HttpGet]
        public ActionResult CreateCustomerView()
        {
            var dataMember = db.MemberRatings.ToList();
            ViewBag.dataMember = dataMember;
            return PartialView("_createCustomerView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateCustomer(CustomerModel model)
        {

                var user = db.Accounts.FirstOrDefault(u => u.AccountType == model.AccountType);
                if (user != null)
                {
                    return Json(new { status = -1, title = "Lỗi đăng ký", text = "Số điện thoại này đã được đăng ký. Vui lòng đăng ký số khác.", obj = "" }); ;
                }
                else
                {
                    try
                    {

                        var data = new Customer()
                        {
                            DateOfBirth = model.DateOfBirth,
                            AccountID = null,
                            Address = model.Address,
                            Code = model.Code.Trim(),
                            FirstName = model.FirstName ?? "",
                            LastName = model.LastName,
                            MemberID = model.MemberRatingsID,
                            Email = model.Email,
                            Phone = model.Phone,
                            Gender = model.Gender,
                        };
                        db.Customers.Add(data);
                        await db.SaveChangesAsync();
                        try
                        {
                            var acc = new Account()
                            {
                                AccountType = model.AccountType,
                                CustomerID = data.ID,
                                Password = model.Password.Trim(),
                                RegistrationDate = DateTime.UtcNow.AddHours(7),
                            };
                            db.Accounts.Add(acc);
                            await db.SaveChangesAsync();
                            try
                            {
                                data.AccountID = acc.ID;
                                db.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                await db.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                db.Accounts.Remove(acc);
                                await db.SaveChangesAsync();
                                return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        catch (Exception e)
                        {
                            db.Customers.Remove(data);
                            await db.SaveChangesAsync();
                            return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { status = 1, title = "", text = "Bạn đã thêm khách hàng thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        }
        public ActionResult GetCustomerDetail(int Id)
        {
            var customer = db.Customers.Find(Id);
            ViewBag.Customer = customer;
            return PartialView("_CustomerViewDetails");
        }

        [HttpGet]
        public ActionResult EditCustomerView(int Id)
        {
            var dataMember = db.MemberRatings.ToList();
            ViewBag.dataMember = dataMember;

            var customer = db.Customers.Find(Id);
            ViewBag.EditCustomer = customer;
            

            var model = new CustomerModel()
            {
                ID = customer.ID,
                DateOfBirth = customer.DateOfBirth,
                Address = customer.Address,
                Code = customer.Code,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                MemberRatingsID = (decimal)customer.MemberID,
                Email = customer.Email,
                Phone = customer.Phone,
                Gender = customer.Gender,
            };
            return PartialView("_EditCustomerView", model);
        }
        [HttpPost]
        public ActionResult EditCustomer(CustomerModel model)
        {
            DateTime dateOfBirth = model.DateOfBirth.Value;
            try
            {
                var cus = db.Customers.FirstOrDefault(e => e.ID == model.ID);
                cus.Code = model.Code;
                cus.Address = model.Address;
                cus.FirstName = model.FirstName;
                cus.LastName = model.LastName;
                cus.MemberID = model.MemberRatingsID;
                cus.Email = model.Email;
                cus.Phone = model.Phone.Trim();
                cus.Gender = model.Gender;
                cus.DateOfBirth = model.DateOfBirth;
                db.SaveChanges();
                return Json(new { status = 1, title = "", text = "Bạn đã cập nhật thông tin nhân viên thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult DeleteCustomer(int Id)
        {
            Customer cus = db.Customers.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (cus != null)
                    {
                        var orders = db.Orders.Where(od => od.CustomerID == Id);
                        foreach (var order in orders)
                        {
                            var ID = order.ID;
                            var orderToUpdate = db.Orders.Find(ID);
                            if (orderToUpdate != null)
                            {
                                orderToUpdate.CustomerID = null;
                            }
                        }
                        
                        db.SaveChanges();
                        var accounts = db.Accounts.Where(acc => acc.CustomerID == Id);
                        db.Accounts.RemoveRange(accounts);
                        db.SaveChanges();
                        db.Customers.Remove(cus);
                        db.SaveChanges();
                        dbContextTransaction.Commit();
                        return Json(new { status = 1, message = "Success" });
                    }
                }
                catch (Exception)
                {

                    dbContextTransaction.Rollback();
                    return Json(new { status = -1, message = "Error" });
                }
            }
            return Json(new { status = -1, message = "Error" });

        }
        [HttpPost]
        public ActionResult CustomerMemberView(int MemberId)
        {
            var dataCustomer = db.Customers.Where(c => c.MemberID == MemberId).ToList();
            return PartialView("_CustomerMemberView", dataCustomer);
        }
        public ActionResult CreateMemberView()
        {
            var dataCus = db.Customers.Where(c => c.MemberID == null).ToList();
            ViewBag.dataCus = dataCus;
            return PartialView("_createMemberView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateMember(MemberRating model, List<decimal> customerId)
        {

            try
            { 
                db.MemberRatings.Add(model);
                await db.SaveChangesAsync();
                decimal newMemberRatingId = model.ID;

                // Lặp qua danh sách các customerId và cập nhật MemberID cho mỗi customer
                if(customerId != null)
                {
                    foreach (var Id in customerId)
                    {
                        var customerToUpdate = db.Customers.Find(Id);
                        if (customerToUpdate != null)
                        {
                            customerToUpdate.MemberID = newMemberRatingId;
                        }
                    }
                }    
                
                await db.SaveChangesAsync();

                return Json(new { status = 1, title = "", text = "Bạn đã thêm loại thành viên thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        [HttpGet]
        public ActionResult EditMemberView(int Id)
        {
            var dataCus = db.Customers.Where(c => c.MemberID == Id).ToList(); 
            ViewBag.dataMember = dataCus;

            var dataCustomer = db.Customers.Where(a => a.MemberID == null).ToList(); 
            ViewBag.dataCutomer = dataCustomer;

            var member = db.MemberRatings.Find(Id);
            ViewBag.EditMember = member;

            return PartialView("_EditMemberView");
        }
        [HttpPost]
        public ActionResult EditMember(MemberRating model, List<decimal> customerId)
        {
            try
            {
                var member = db.MemberRatings.FirstOrDefault(e => e.ID == model.ID);
                member.Code = model.Code;
                member.Name = model.Name;
                db.SaveChanges();

                decimal newMemberRatingId = model.ID;

                // Lặp qua danh sách các customerId và cập nhật MemberID cho mỗi customer
                if (customerId != null)
                {
                    foreach (var Id in customerId)
                    {
                        var customerToUpdate = db.Customers.Find(Id);
                        if (customerToUpdate != null)
                        {
                            customerToUpdate.MemberID = newMemberRatingId;
                        }
                    }

                    db.SaveChanges();
                }    
               

                return Json(new { status = 1, title = "", text = "Bạn đã cập nhật thông tin loại thành viên thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult DeleteCustomerMember(int Id)
        {
            Customer cus = db.Customers.FirstOrDefault(x => x.ID == Id);
            if (cus != null)
            {
                cus.MemberID = null; // Sử dụng toán tử gán (=) để thiết lập giá trị mới
                db.SaveChanges();
                return Json(new { status = 1, message = "Success", text ="Xóa khách hàng thành công" });
            }
            else
            {
                return Json(new { status = 0, message = "Customer not found" , text = "Không thể xóa khách hàng này, vui lòng thử lại." });
            }
        }

        public ActionResult DeleteMember(int Id)
        {
            MemberRating mem = db.MemberRatings.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (mem != null)
                    {
                        var customer = db.Customers.Where(m => m.MemberID == Id);
                        foreach (var cus in customer)
                        {
                            var ID = cus.ID;
                            var memberToUpdate = db.Customers.Find(ID);
                            if (memberToUpdate != null)
                            {
                                memberToUpdate.MemberID = null;
                            }
                        }
                        db.SaveChanges();

                        db.MemberRatings.Remove(mem);
                        db.SaveChanges();
                        dbContextTransaction.Commit();
                        return Json(new { status = 1, message = "Success" });
                    }
                }
                catch (Exception)
                {

                    dbContextTransaction.Rollback();
                    return Json(new { status = -1, message = "Error" });
                }
            }
            return Json(new { status = -1, message = "Error" });

        }
    }
}