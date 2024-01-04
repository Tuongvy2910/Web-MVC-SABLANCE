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
    public class InventoryReceiptController : Controller
    {
        // GET: Admin/InventoryReceipt
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        public ActionResult ListInventoryReceipt(int? page, int? pageSize)
        {

            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<InventoryReceipt> po = db.InventoryReceipts
                .OrderByDescending(ir => ir.PODate) // Sắp xếp theo ngày giảm dần
                .ToList();

            var POProductCounts = db.InventoryReceiptDetails
                .GroupBy(pd => pd.ReceiptID)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            ViewBag.POProductCounts = POProductCounts;

            return View(po.ToPagedList((int)page, (int)pageSize));

        }
        public class ProductDTO
        {
            public decimal Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

            public string Color { get; set; }
            public string Size { get; set; }
            // Thêm các thuộc tính khác bạn quan tâm
        }

        [HttpGet]
        public ActionResult CreateInventoryReceiptView()
        {
            var dataEm = db.Employees.Where(a => a.CategoriesEmployeeID == 2 || a.CategoriesEmployeeID == 4).ToList();
            ViewBag.DataEmployeePO = dataEm;

            var dataPro = db.ProductDetails.ToList();
            ViewBag.DataProductPO = dataPro;

            var dataproduct = db.ProductDetails
                       .Select(e => new ProductDTO
                       {
                           Id = e.ID,
                           Name = e.Product.Name,
                           Code = e.Code,
                           Color = e.Color.Code,
                           Size = e.Size.Name,
                           // Chỉ chọn các thuộc tính cần thiết
                       }).ToList();

            ViewBag.ProductPOList = dataproduct;
            return PartialView("_CreateInventoryReceiptView");
        }
        [HttpPost]
        public ActionResult CreateInventoryReceiptPO(InventoryReceipt model, List<decimal> productId, List<int> quantity, List<double> Prices , DateTime podate)
        {
            try
            {
                var checkinventory = db.InventoryReceipts.FirstOrDefault(a => a.PO_No == model.PO_No);
                if(checkinventory == null)
                {
                    InventoryReceipt po = new InventoryReceipt();
                    po.PO_No = model.PO_No;
                    po.EmployeeID = model.EmployeeID;
                    po.PODate = podate;
                    po.Note = model.Note;
                    db.InventoryReceipts.Add(po);
                    db.SaveChanges();

                    decimal newPOId = po.ID;

                    if (productId != null)
                    {

                        for (int i = 0; i < productId.Count; i++)
                        {
                            decimal proID = productId[i];
                            int Quantity = quantity[i];
                            double Price = Prices[i];
                            var checkpro = db.InventoryReceiptDetails.FirstOrDefault(a => a.ReceiptID == po.ID && a.ProductID == proID);
                            if (checkpro != null)
                            {
                                var removeor = db.InventoryReceipts.FirstOrDefault(a => a.ID == po.ID);
                                db.InventoryReceipts.Remove(removeor);
                                db.SaveChanges();
                                return Json(new { status = -1, title = "", text = "Tạo phiếu nhập kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                InventoryReceiptDetail receipt  = new InventoryReceiptDetail();
                                receipt.ProductID = proID;
                                receipt.ReceiptID = po.ID;
                                receipt.Quantity = Quantity;
                                receipt.Prices = Price;
                                db.InventoryReceiptDetails.Add(receipt);
                                db.SaveChanges();
                                var pro = db.ProductDetails.Where(a => a.ID == proID).FirstOrDefault();
                                if(Quantity >5 && Quantity <=10)
                                {
                                    pro.StatusProduct = "Sắp hết hàng";
                                    db.SaveChanges();
                                }
                                else
                                {
                                    pro.StatusProduct = "Còn hàng";
                                    db.SaveChanges();
                                }
                                
                            }

                        }
                    }
 
                    db.SaveChanges();



                    return Json(new { status = 1, title = "", text = "Bạn đã tạo phiếu nhập kho mới thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã số phiếu nhập kho đã tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
                }

                

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
        [HttpPost]
        public ActionResult DetailInventoryReceiptView(int Id)
        {
            var po = db.InventoryReceipts.Find(Id);
            ViewBag.InventoryReceiptsDetail = po;

            var ordetail = db.InventoryReceiptDetails.Where(a => a.ReceiptID == Id).ToList();
            ViewBag.PODetailList = ordetail;

            return PartialView("_DetailInventoryReceiptView", po);
        }
        [HttpGet]
        public ActionResult EditInventoryReceiptView(int Id)
        {
            var dataEm = db.Employees.Where(a => a.CategoriesEmployeeID == 2 || a.CategoriesEmployeeID == 4).ToList();
            ViewBag.DataEmployeePOEdit = dataEm;
            var po = db.InventoryReceipts.Find(Id);
            ViewBag.POEdit = po;
            var dataproduct = db.ProductDetails
                       .Select(e => new ProductDTO
                       {
                           Id = e.ID,
                           Name = e.Product.Name,
                           Code = e.Code,
                           Color = e.Color.Code,
                           Size = e.Size.Name,
                       }).ToList();

            ViewBag.ProductEditPO = dataproduct;

            var ordetail = db.InventoryReceiptDetails.Where(a => a.ReceiptID == Id).ToList();
            ViewBag.PODetailEdit = ordetail;

            var dataPro = db.ProductDetails.ToList();
            ViewBag.POLstProduct = dataPro;


            return PartialView("_EditInventoryReceiptView");
        }
        [HttpPost]
        public ActionResult EditInventoryReceipt(InventoryReceipt model, List<decimal> productId, List<int> quantity, List<double> Prices, DateTime podate)
        {
            try
            {
                var po = db.InventoryReceipts.FirstOrDefault(e => e.ID == model.ID);
                if (po != null)
                {
                    if (model.PO_No == po.PO_No.Trim())
                    {
                        po.PODate = podate;
                        po.EmployeeID = model.EmployeeID;
                        po.Note = model.Note;
                        db.SaveChanges();


                        decimal newCateEmployeeId = model.ID;
                        var prodetail = db.InventoryReceiptDetails.Where(a => a.ReceiptID == model.ID).ToList();
                        if (prodetail != null)
                        {
                            if (productId != null)
                            {
                                var removeor = db.InventoryReceiptDetails.Where(a => a.ReceiptID == po.ID);
                                db.InventoryReceiptDetails.RemoveRange(removeor);
                                db.SaveChanges();
                                for (int i = 0; i < productId.Count; i++)
                                {
                                    decimal proID = productId[i];
                                    int Quantity = quantity[i];
                                    double Price = Prices[i];
                                    var checkpro = db.InventoryReceiptDetails.FirstOrDefault(a => a.ReceiptID == po.ID && a.ProductID == proID);
                                    if (checkpro != null)
                                    {
                                        return Json(new { status = -1, title = "", text = "Tạo phiếu nhập kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        InventoryReceiptDetail receipt = new InventoryReceiptDetail();
                                        receipt.ProductID = proID;
                                        receipt.ReceiptID = po.ID;
                                        receipt.Quantity = Quantity;
                                        receipt.Prices = Price;
                                        db.InventoryReceiptDetails.Add(receipt);
                                        db.SaveChanges();
                                        var pro = db.ProductDetails.Where(a => a.ID == proID).FirstOrDefault();
                                        if (Quantity > 5 && Quantity <= 10)
                                        {
                                            pro.StatusProduct = "Sắp hết hàng";
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            pro.StatusProduct = "Còn hàng";
                                            db.SaveChanges();
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            if (productId != null)
                            {

                                for (int i = 0; i < productId.Count; i++)
                                {
                                    decimal proID = productId[i];
                                    int Quantity = quantity[i];
                                    double Price = Prices[i];
                                    var checkpro = db.InventoryReceiptDetails.FirstOrDefault(a => a.ReceiptID == po.ID && a.ProductID == proID);
                                    if (checkpro != null)
                                    {
                                        return Json(new { status = -1, title = "", text = "Tạo phiếu nhập kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        InventoryReceiptDetail receipt = new InventoryReceiptDetail();
                                        receipt.ProductID = proID;
                                        receipt.ReceiptID = po.ID;
                                        receipt.Quantity = Quantity;
                                        receipt.Prices = Price;
                                        db.InventoryReceiptDetails.Add(receipt);
                                        db.SaveChanges();
                                        var pro = db.ProductDetails.Where(a => a.ID == proID).FirstOrDefault();
                                        if (Quantity > 5 && Quantity <= 10)
                                        {
                                            pro.StatusProduct = "Sắp hết hàng";
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            pro.StatusProduct = "Còn hàng";
                                            db.SaveChanges();
                                        }
                                    }

                                }
                            }
                        }


                        db.SaveChanges();


                        return Json(new { status = 1, title = "", text = "Cập nhật phiếu nhập kho thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = -1, title = "", text = "Mã phiếu nhập kho không đúng, cập nhật phiếu nhập kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy phiếu nhập kho này", obj = "" }, JsonRequestBehavior.AllowGet);
                }

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
        public ActionResult DeleteInventoryReceipt(int Id)
        {
            InventoryReceipt po = db.InventoryReceipts.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (po != null)
                    {
                        var detail = db.InventoryReceiptDetails.Where(m => m.ReceiptID == Id);
                        db.InventoryReceiptDetails.RemoveRange(detail);
                        db.SaveChanges();

                        db.InventoryReceipts.Remove(po);
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