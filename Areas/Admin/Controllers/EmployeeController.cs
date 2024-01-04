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
    public class EmployeeController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Admin/Employee
        public ActionResult ListEmployee(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<Employee> employees = db.Employees.ToList();
            return View(employees.ToPagedList((int)page, (int)pageSize));
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
            return db.Employees.Any(e => e.Code == code);
        }

        [HttpGet]
        public ActionResult CreateView()
        {
            var dataCate = db.CategoriesEmployees.ToList();
            ViewBag.dataCate = dataCate;
            return PartialView("_createView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateEmployee(EmployeeCreateModel model, HttpPostedFileBase ImageData)
        {
            if (ImageData != null && ImageData.ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(ImageData.FileName).ToLower();
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };

                if (allowedExtensions.Contains(fileExtension))
                {
                    string fileName = Path.GetFileName(ImageData.FileName);
                    string rootImage = "~/Image/User/" + fileName;
                    ImageData.SaveAs(Server.MapPath(rootImage));
                    model.Image = Path.GetFileName(ImageData.FileName); ;
                }
                else
                {
                    ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).");

                }
            }

            try
            {
                var data = new Employee()
                {
                    DateOfBirth = model.DateOfBirth,
                    AccountID = null,
                    Address = model.Address,
                    Code = model.Code.Trim(),
                    FirstName = model.FirstName ?? "",
                    LastName = model.LastName,
                    CategoriesEmployeeID = model.CategoriesEmployeeID,
                    Email = model.Email,
                    Phone = model.Phone,
                    Gender = model.Gender,
                    CCCD = model.CCCD,
                    Image = model.Image,
                };
                db.Employees.Add(data);
                await db.SaveChangesAsync();
                try
                {
                    var acc = new Account()
                    {
                        AccountType = model.AccountType,
                        EmployeeID = data.ID,
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
                    db.Employees.Remove(data);
                    await db.SaveChangesAsync();
                    return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = 1, title = "", text = "Bạn đã thêm nhân viên thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult GetEmployeeDetail(int Id)
        {
            var employee = db.Employees.Find(Id);
            ViewBag.Employee = employee;
            return PartialView("_EmployeeDetails");
        }

        [HttpGet]
        public ActionResult EditEmployeeView(int Id)
        {
            var dataCate = db.CategoriesEmployees.ToList();
            ViewBag.dataCate = dataCate;

            var employee = db.Employees.Find(Id);
            ViewBag.EditEmployee = employee;
            DateTime dateOfBirth = employee.DateOfBirth.Value;

        var model = new EmployeeCreateModel()
        {
            ID = employee.ID,
            DateOfBirth = employee.DateOfBirth,
            Address = employee.Address,
            Code = employee.Code,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            CategoriesEmployeeID = (decimal)employee.CategoriesEmployeeID,
            Email = employee.Email,
            Phone = employee.Phone,
            Gender = employee.Gender,
            CCCD = employee.CCCD,
            Image = employee.Image
        };
            return PartialView("_EditEmployeeView", model);
        }
        [HttpPost]
        public ActionResult EditEmployee(EmployeeCreateModel model, HttpPostedFileBase ImageData)
        {
            if (ImageData != null && ImageData.ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(ImageData.FileName).ToLower();
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };

                if (allowedExtensions.Contains(fileExtension))
                {
                    string fileName = Path.GetFileName(ImageData.FileName);
                    string rootImage = "~/Image/User/" + fileName;
                    ImageData.SaveAs(Server.MapPath(rootImage));
                    model.Image = Path.GetFileName(ImageData.FileName); ;
                }
                else
                {
                    ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).");

                }
            }
            
            try
            {
                var em = db.Employees.FirstOrDefault(e => e.ID == model.ID);
                em.DateOfBirth = model.DateOfBirth;  
                em.Code = model.Code;
                em.Address = model.Address;
                em.FirstName = model.FirstName;
                em.LastName = model.LastName;
                em.CategoriesEmployeeID = model.CategoriesEmployeeID;
                em.Email = model.Email;
                em.Phone = model.Phone.Trim();
                em.Gender = model.Gender;
                if(!string.IsNullOrEmpty(model.CCCD))
                {
                    em.CCCD = model.CCCD.Trim();
                }
                else
                {
                    em.CCCD = model.CCCD;
                }
                em.Image = model.Image;
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
        public ActionResult DeleteEmployee(int Id)
        {
            Employee em = db.Employees.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (em != null)
                    {
                        var orders = db.Orders.Where(od => od.EmployeeID == Id);
                        foreach (var order in orders)
                        {
                            var ID = order.ID;
                            var orderToUpdate = db.Orders.Find(ID);
                            if (orderToUpdate != null)
                            {
                                orderToUpdate.EmployeeID = null;
                            }
                        }
                        var receipts = db.InventoryReceipts.Where(od => od.EmployeeID == Id);

                        // Cập nhật EmployeeID thành null cho các đơn hàng liên quan
                        foreach (var receipt in receipts)
                        {
                            var ID = receipt.ID;
                            var receiptToUpdate = db.InventoryReceipts.Find(ID);
                            if (receiptToUpdate != null)
                            {
                                receiptToUpdate.EmployeeID = null;
                            }
                        }

                        var issuses = db.InventoryIssues.Where(od => od.EmployeeID == Id);

                        // Cập nhật EmployeeID thành null cho các đơn hàng liên quan
                        foreach (var issuse in issuses)
                        {
                            var ID = issuse.ID;
                            var issuseToUpdate = db.InventoryIssues.Find(ID);
                            if (issuseToUpdate != null)
                            {
                                issuseToUpdate.EmployeeID = null;
                            }
                        }
                        db.SaveChanges();
                        var accounts = db.Accounts.Where(acc => acc.EmployeeID == Id);
                        db.Accounts.RemoveRange(accounts);
                        db.SaveChanges();
                        db.Employees.Remove(em);
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
        //chức vụ nhân viên

        public ActionResult ListCateEmployee(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<CategoriesEmployee> cateEmployee = db.CategoriesEmployees.ToList();

            // Tạo một Dictionary để lưu trữ số lượng nhân viên cho từng chức vụ
            Dictionary<decimal, decimal> employeeCountsByCateID = new Dictionary<decimal, decimal>();

            // Đếm số lượng nhân viên cho từng MemberID
            foreach (var cate in cateEmployee)
            {
                int employeeCount = db.Employees.Count(c => c.CategoriesEmployeeID == cate.ID);
                employeeCountsByCateID.Add(cate.ID, employeeCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.EmployeeCountsByCaterID = employeeCountsByCateID;

            return View(cateEmployee.ToPagedList((int)page, (int)pageSize));

        }
        [HttpGet]
        public ActionResult CreateCateEmployeeView()
        {
            var dataEmployee = db.Employees.Where(em => em.CategoriesEmployeeID == null).ToList();
            ViewBag.NullEmployee = dataEmployee;

            var dataem = db.Employees.Where(em => em.CategoriesEmployeeID == null).Select(e => new EmployeeDTO
            {
                Id = e.ID,
                FirstName = e.FirstName,
                LastName = e.LastName,
                DateOfBirth = e.DateOfBirth,
                Phone = e.Phone,
            })
                     .ToList();
            ViewBag.EmNull = dataem;
            return PartialView("_createCateEmployeeView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateCateEmployee(CategoriesEmployee model, List<decimal> employeeId)
        {

            try
            {
                db.CategoriesEmployees.Add(model);
                await db.SaveChangesAsync();
                decimal newCateEmplyeeId = model.ID;

                // Lặp qua danh sách các customerId và cập nhật MemberID cho mỗi customer
                if (employeeId != null)
                {
                    foreach (var Id in employeeId)
                    {
                        var employeeToUpdate = db.Employees.Find(Id);
                        if (employeeToUpdate != null)
                        {
                            employeeToUpdate.CategoriesEmployeeID = newCateEmplyeeId;
                        }
                    }
                }

                await db.SaveChangesAsync();

                return Json(new { status = 1, title = "", text = "Bạn đã thêm chức vụ nhân viên thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult DetailEmployeeCateView(int Id)
        {
            var dataEmployee = db.Employees.Where(c => c.CategoriesEmployeeID == Id).ToList();

            var cate = db.CategoriesEmployees.Find(Id);
            ViewBag.CateEmployee = cate;
            return PartialView("_DetailEmployeeCateView", dataEmployee);

        }
        public class EmployeeDTO
        {
            public decimal Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string Phone { get; set; }
            // Thêm các thuộc tính khác bạn quan tâm
        }
        [HttpGet]
        public ActionResult EditCateEmployeeView(int Id)
        {
            var dataEmployee = db.Employees.Where(em => em.CategoriesEmployeeID == null).Select(e => new EmployeeDTO
            {
                Id = e.ID,
                FirstName = e.FirstName,
                LastName = e.LastName,
                DateOfBirth = e.DateOfBirth,
                Phone = e.Phone,
            })
                     .ToList();
            ViewBag.employeeNull = dataEmployee;
            var employee = db.Employees.Where(em => em.CategoriesEmployeeID == null).ToList();
            ViewBag.EmNull = employee;

            var dataEm = db.Employees.Where(c => c.CategoriesEmployeeID == Id).ToList(); 
            ViewBag.EmployeeList = dataEm;


            var cate = db.CategoriesEmployees.Find(Id);
            ViewBag.EditcateEmployee = cate;

            return PartialView("_EditCateEmployeeView");
        }
        [HttpPost]
        public ActionResult EditCateEmployee(CategoriesEmployee model, List<decimal> employeeId)
        {
            try
            {
                var cate = db.CategoriesEmployees.FirstOrDefault(e => e.ID == model.ID);
                cate.Code = model.Code;
                cate.Name = model.Name;
                db.SaveChanges();

                decimal newCateEmployeeId = model.ID;

                if (employeeId != null)
                {
                    foreach (var Id in employeeId)
                    {
                        var employeeToUpdate = db.Employees.Find(Id);
                        if (employeeToUpdate != null)
                        {
                            employeeToUpdate.CategoriesEmployeeID = newCateEmployeeId;
                        }
                    }
                    db.SaveChanges();
                }




                return Json(new { status = 1, title = "", text = "Bạn đã cập nhật thông tin chức vụ nhân viên thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult DeleteEmployeeInCate(int Id)
        {
            Employee cus = db.Employees.FirstOrDefault(x => x.ID == Id);
            if (cus != null)
            {
                cus.CategoriesEmployeeID = null; // Sử dụng toán tử gán (=) để thiết lập giá trị mới
                db.SaveChanges();
                return Json(new { status = 1, message = "Success", text = "Xóa nhân viên thành công" });
            }
            else
            {
                return Json(new { status = 0, message = "Customer not found", text = "Không thể xóa nhân viên, vui lòng thử lại." });
            }
        }
        public ActionResult DeleteCateEmployee(int Id)
        {
            CategoriesEmployee cate = db.CategoriesEmployees.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (cate != null)
                    {
                        var employee = db.Employees.Where(m => m.CategoriesEmployeeID == Id);
                        foreach (var em in employee)
                        {
                            var ID = em.ID;
                            var employeeToUpdate = db.Employees.Find(ID);
                            if (employeeToUpdate != null)
                            {
                                employeeToUpdate.CategoriesEmployeeID = null;
                            }
                        }
                        db.SaveChanges();

                        db.CategoriesEmployees.Remove(cate);
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