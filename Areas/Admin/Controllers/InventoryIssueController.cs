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
using Dapper;

namespace SABLANCE.Areas.Admin.Controllers
{
    public class InventoryIssueController : Controller
    {
        // GET: Admin/InventoryIssue
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        public ActionResult ListInventoryIssue(int? page, int? pageSize)
        {

            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<InventoryIssue> po = db.InventoryIssues
                .OrderByDescending(ir => ir.SODate) // Sắp xếp theo ngày giảm dần
                .ToList();

            var POProductCounts = db.InventoryIssueDetails
                .GroupBy(pd => pd.IssueID)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            ViewBag.SOProductCounts = POProductCounts;

            return View(po.ToPagedList((int)page, (int)pageSize));

        }
        public class ProductDTO
        {
            public decimal Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

            public string Color { get; set; }
            public string Size { get; set; }
            public double Prices { get; set; }
            // Thêm các thuộc tính khác bạn quan tâm
        }

        [HttpGet]
        public ActionResult CreateInventoryIssueView()
        {
            var dataEm = db.Employees.Where(a => a.CategoriesEmployeeID == 2 || a.CategoriesEmployeeID == 4).ToList();
            ViewBag.DataEmployeeSO = dataEm;

            var dataPro = db.ProductDetails.ToList();
            ViewBag.DataProductSO = dataPro;

            var dataproduct = db.ProductDetails
                       .Select(e => new ProductDTO
                       {
                           Id = e.ID,
                           Name = e.Product.Name,
                           Code = e.Code,
                           Color = e.Color.Code,
                           Size = e.Size.Name,
                           Prices = e.Prices,
                           // Chỉ chọn các thuộc tính cần thiết
                       }).ToList();

            ViewBag.ProductSOList = dataproduct;
            return PartialView("_CreateInventoryIssueView");
        }
        [HttpPost]
        public ActionResult CreateInventoryIssuseSO(InventoryIssue model, List<decimal> productId, List<int> quantity, DateTime sodate)
        {
            try
            {
                var checkinventory = db.InventoryReceipts.FirstOrDefault(a => a.PO_No == model.SO_No);
                if (checkinventory == null)
                {
                    InventoryIssue so = new InventoryIssue();
                    so.SO_No = model.SO_No;
                    so.EmployeeID = model.EmployeeID;
                    so.SODate = sodate;
                    so.Note = model.Note;
                    db.InventoryIssues.Add(so);
                    db.SaveChanges();

                    string connectionString = "Server =DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

                    string sqlQuery = @"
WITH LatestInventory AS (
  SELECT
    ProductDetails.ID AS ProductID,
	ProductDetails.Code AS ProductCode,
    COALESCE(InventoryReceiptDetails.Quantity, 0) AS ReceiptQuantity,
    InventoryReceipt.PODate,
    COALESCE(SUM(OrderDetails.Quantity), 0) AS SoldQuantity,
    COALESCE(SUM(InventoryIssueDetails.Quantity), 0) AS QuantitySO,
    ROW_NUMBER() OVER (PARTITION BY ProductDetails.ID ORDER BY InventoryReceipt.PODate DESC) AS RowNum
  FROM
    ProductDetails
    LEFT JOIN InventoryReceiptDetails ON InventoryReceiptDetails.ProductID = ProductDetails.ID
    LEFT JOIN InventoryReceipt ON InventoryReceipt.ID = InventoryReceiptDetails.ReceiptID
    LEFT JOIN OrderDetails ON ProductDetails.ID = OrderDetails.ProductID
    LEFT JOIN InventoryIssueDetails ON ProductDetails.ID = InventoryIssueDetails.ProductID
  GROUP BY
    ProductDetails.ID, InventoryReceiptDetails.Quantity, InventoryReceipt.PODate, ProductDetails.Code
)
SELECT ProductID, ProductCode, ReceiptQuantity -(SoldQuantity + QuantitySO) as Quantity, SoldQuantity as QuantityOrder, QuantitySO , PODate
FROM LatestInventory
WHERE RowNum = 1;";

                    var connection = new SqlConnection(connectionString);
                    List<Inventory> inventories = connection.Query<Inventory>(sqlQuery).AsList();

                    decimal newPOId = so.ID;

                    if (productId != null)
                    {

                        for (int i = 0; i < productId.Count; i++)
                        {
                            decimal proID = productId[i];
                            int Quantity = quantity[i];
                            var invantory = inventories.FirstOrDefault(a => a.ProductID == proID);
                            var checkpro = db.InventoryIssueDetails.FirstOrDefault(a => a.IssueID == so.ID && a.ProductID == proID);
                            if (checkpro != null)
                            {
                                var removeor = db.InventoryIssues.FirstOrDefault(a => a.ID == so.ID);
                                db.InventoryIssues.Remove(removeor);
                                db.SaveChanges();
                                return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                if(invantory != null)
                                {
                                    var check = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                                    if (Quantity <= invantory.Quantity)
                                    {
                                        if (check.StatusProduct == "Hết hàng")
                                        {
                                            var removeor = db.InventoryIssues.FirstOrDefault(a => a.ID == so.ID);
                                            db.InventoryIssues.Remove(removeor);
                                            db.SaveChanges();
                                            return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại, sản phẩm " + check.Code + " hết hàng", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            InventoryIssueDetail issuse = new InventoryIssueDetail();
                                            issuse.ProductID = proID;
                                            issuse.IssueID = so.ID;
                                            issuse.Quantity = Quantity;
                                            db.InventoryIssueDetails.Add(issuse);
                                            db.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        var removeor = db.InventoryIssues.FirstOrDefault(a => a.ID == so.ID);
                                        db.InventoryIssues.Remove(removeor);
                                        db.SaveChanges();
                                        return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại, sản phẩm " + check.Code + " hết hàng", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }    
                                        
                                }    
                               
                                
                            }

                        }
                        db.SaveChanges();



                        return Json(new { status = 1, title = "", text = "Bạn đã tạo phiếu xuất kho mới thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var removeor = db.InventoryIssues.FirstOrDefault(a => a.ID == so.ID);
                        db.InventoryIssues.Remove(removeor);
                        db.SaveChanges();
                        return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                    }                   
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã số phiếu xuất kho đã tồn tại", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DetailInventoryIssueView(int Id)
        {
            var so = db.InventoryIssues.Find(Id);
            ViewBag.InventoryIssusesDetail = so;

            var ordetail = db.InventoryIssueDetails.Where(a => a.IssueID == Id).ToList();
            ViewBag.SODetailList = ordetail;

            return PartialView("_DetailInventoryIssueView", so);
        }
        [HttpGet]
        public ActionResult EditInventoryIssuseView(int Id)
        {
            var dataEm = db.Employees.Where(a => a.CategoriesEmployeeID == 2 || a.CategoriesEmployeeID == 4).ToList();
            ViewBag.DataEmployeeSOEdit = dataEm;
            var po = db.InventoryIssues.Find(Id);
            ViewBag.SOEdit = po;
            var dataproduct = db.ProductDetails
                       .Select(e => new ProductDTO
                       {
                           Id = e.ID,
                           Name = e.Product.Name,
                           Code = e.Code,
                           Color = e.Color.Code,
                           Size = e.Size.Name,
                           Prices = e.Prices,
                       }).ToList();

            ViewBag.ProductEditSO = dataproduct;

            var ordetail = db.InventoryIssueDetails.Where(a => a.IssueID == Id).ToList();
            ViewBag.SODetailEdit = ordetail;

            var dataPro = db.ProductDetails.ToList();
            ViewBag.SOLstProduct = dataPro;


            return PartialView("_EditInventoryIssuseView");
        }
        [HttpPost]
        public ActionResult EditInventoryIssuse(InventoryIssue model, List<decimal> productId, List<int> quantity, DateTime sodate)
        {
            try
            {
                var so = db.InventoryIssues.FirstOrDefault(e => e.ID == model.ID);
                if (so != null)
                {
                    if (model.SO_No == so.SO_No.Trim())
                    {
                        so.SODate = sodate;
                        so.EmployeeID = model.EmployeeID;
                        so.Note = model.Note;
                        db.SaveChanges();


                        decimal newCateEmployeeId = model.ID;
                        var prodetail = db.InventoryIssueDetails.Where(a => a.IssueID == model.ID).ToList();
                        if (prodetail != null)
                        {
                            if (productId != null)
                            {
                                var removeor = db.InventoryIssueDetails.Where(a => a.IssueID == so.ID);
                                db.InventoryIssueDetails.RemoveRange(removeor);
                                db.SaveChanges();
                                for (int i = 0; i < productId.Count; i++)
                                {
                                    decimal proID = productId[i];
                                    int Quantity = quantity[i];
                                    var checkpro = db.InventoryIssueDetails.FirstOrDefault(a => a.IssueID == so.ID && a.ProductID == proID);
                                    if (checkpro != null)
                                    {
                                        return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var check = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                                        if (check.StatusProduct == "Hết hàng")
                                        {
                                            return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại, sản phẩm " + check.Code + " hết hàng", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            InventoryIssueDetail issuse = new InventoryIssueDetail();
                                            issuse.ProductID = proID;
                                            issuse.IssueID = so.ID;
                                            issuse.Quantity = Quantity;
                                            db.InventoryIssueDetails.Add(issuse);
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
                                    var checkpro = db.InventoryIssueDetails.FirstOrDefault(a => a.IssueID == so.ID && a.ProductID == proID);
                                    if (checkpro != null)
                                    {
                                        return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var check = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                                        if (check.StatusProduct == "Hết hàng")
                                        {
                                            return Json(new { status = -1, title = "", text = "Tạo phiếu xuất kho thất bại, sản phẩm " + check.Code + " hết hàng", obj = "" }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            InventoryIssueDetail issuse = new InventoryIssueDetail();
                                            issuse.ProductID = proID;
                                            issuse.IssueID = so.ID;
                                            issuse.Quantity = Quantity;
                                            db.InventoryIssueDetails.Add(issuse);
                                            db.SaveChanges();
                                        }
                                    }

                                }
                            }
                        }


                        db.SaveChanges();


                        return Json(new { status = 1, title = "", text = "Cập nhật phiếu xuất kho thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = -1, title = "", text = "Mã phiếu xuất kho không đúng, cập nhật phiếu nhập kho thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy phiếu xuất kho này", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DeleteInventoryIssuse(int Id)
        {
            InventoryIssue so = db.InventoryIssues.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (so != null)
                    {
                        var detail = db.InventoryIssueDetails.Where(m => m.IssueID == Id);
                        db.InventoryIssueDetails.RemoveRange(detail);
                        db.SaveChanges();

                        db.InventoryIssues.Remove(so);
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