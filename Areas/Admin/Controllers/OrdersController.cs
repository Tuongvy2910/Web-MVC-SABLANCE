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
using OrderDetailModel = SABLANCE.Areas.Admin.Models.OrderDetailModel;
using Dapper;

namespace SABLANCE.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Admin/Orders
        public ActionResult ListOrder(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 25;
            }
            var query = from order in db.Orders
                        join orderStatus in db.OrderStatusDetails
                        on order.ID equals orderStatus.OrderID into gj
                        from subOrderStatus in gj.DefaultIfEmpty()
                        where subOrderStatus == null || subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate)
                        orderby order.OrderDate descending  // Thêm sắp xếp theo ngày giảm dần
                        select new OrderDetailModel
                        {
                            OrderId = order.ID,
                            OrderNo = order.OrderNo,
                            EmployeeName = order.Employee.LastName + " " + order.Employee.FirstName,
                            EmployeeId = order.EmployeeID,
                            CustomerName = order.Customer.LastName + " " + order.Customer.FirstName,
                            CustomerId = order.CustomerID,
                            ReceiptPhone = order.RecipientPhone,
                            OrderDate = order.OrderDate,
                            TotalPayment = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                            LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null
                        };

            var orders = query.ToList();

            return View(orders.ToPagedList((int)page, (int)pageSize));
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
        public ActionResult CreateOrderView()
        {
            var dataEm = db.Employees.Where(a=> a.CategoriesEmployeeID ==1 || a.CategoriesEmployeeID == 4).ToList();
            ViewBag.DataEmployee = dataEm;

            var dataCus = db.Customers.ToList();
            ViewBag.DataCustomer = dataCus;

            var dataPro = db.ProductDetails.ToList();
            ViewBag.DataProduct = dataPro;

            var dataproduct = db.ProductDetails
                       .Select(e => new ProductDTO
                        {
                            Id = e.ID,
                            Name = e.Product.Name,
                            Code = e.Code,
                            Color = e.Color.Code,
                            Size = e.Size.Name,
                            Prices = e.Prices
        // Chỉ chọn các thuộc tính cần thiết
                        }).ToList();

            ViewBag.ProductList = dataproduct;
            return PartialView("_CreateOrderView");
        }
        [HttpPost]
        public ActionResult CreateOrder(Order model, List<decimal> productId, List<int> quantity, List<double> Prices)
        {
            try
            {

               

                if (productId != null)
                {
                    db.Orders.Add(model);
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

                    decimal newCateEmployeeId = model.ID;
                    if (productId != null)
                    {
                        for (int i = 0; i < productId.Count; i++)
                        {
                            decimal proID = productId[i];
                            int Quantity = quantity[i];
                            double Price = Prices[i];
                            var invantory = inventories.FirstOrDefault(a => a.ProductID == proID);
                            var check = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                            if (invantory != null)
                            {
                                if (Quantity <= invantory.Quantity)
                                {

                                    if (check.StatusProduct == "Hết hàng")
                                    {
                                        var remove = db.Orders.FirstOrDefault(a => a.ID == model.ID);
                                        db.Orders.Remove(remove);
                                        db.SaveChanges();
                                        return Json(new { status = -1, title = "", text = "Sản phẩm " + check.Code + " hết hàng!", obj = "" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        OrderDetail order = new OrderDetail();
                                        order.ProductID = proID;
                                        order.OrderID = model.ID;
                                        order.Quantity = Quantity;
                                        order.Price = Price;
                                        db.OrderDetails.Add(order);
                                        db.SaveChanges();
                                    }
                                }
                                else
                                {
                                    var remove = db.Orders.FirstOrDefault(a => a.ID == model.ID);
                                    db.Orders.Remove(remove);
                                    db.SaveChanges();
                                    return Json(new { status = -1, title = "", text = "Sản phẩm " + check.Code + " hết hàng!", obj = "" }, JsonRequestBehavior.AllowGet);
                                }

                            }
                            else
                            {
                                return Json(new { status = -1, title = "", text = "Không tìm thấy sản phẩm " + check.Code + " trong kho!", obj = "" }, JsonRequestBehavior.AllowGet);
                            }


                        }
                    }
                    OrderStatusDetail orstatus = new OrderStatusDetail();
                    orstatus.OrderID = model.ID;
                    orstatus.OrderStatusID = 1;
                    orstatus.ConfirmationDate = DateTime.Now;
                    db.OrderStatusDetails.Add(orstatus);
                    db.SaveChanges();

                    double totalPayment = (double)model.TotalPayment;
                    int roundedValue = (int)Math.Round(totalPayment / 100000, MidpointRounding.AwayFromZero);
                    PointDetailed diem = new PointDetailed();
                    diem.OrderID = model.ID;
                    diem.Point = roundedValue;

                    db.PointDetaileds.Add(diem);


                    var or = db.Orders.FirstOrDefault(a => a.ID == newCateEmployeeId);
                    or.PointID = diem.ID;

                    db.SaveChanges();



                    return Json(new { status = 1, title = "", text = "Bạn đã tạo đơn hàng mới thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Vui lòng chọn sản phẩm", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult OrderDetailView(int Id)
        {
            var order = db.Orders.Find(Id);
            ViewBag.Orders = order;

            var ordetail = db.OrderDetails.Where(a =>  a.OrderID == Id).ToList();
            ViewBag.OrdersDetail = ordetail;

           
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
                


            return PartialView("_OrderDetailView", order);
        }

        [HttpGet]
        public ActionResult EditOrderView(int Id)
        {
            var order = db.Orders.Find(Id);
            ViewBag.OrdersEdit = order;
            var dataproduct = db.ProductDetails
                       .Select(e => new ProductDTO
                       {
                           Id = e.ID,
                           Name = e.Product.Name,
                           Code = e.Code,
                           Color = e.Color.Code,
                           Size = e.Size.Name,
                           Prices = e.Prices
                       }).ToList();

            ViewBag.ProductOrder = dataproduct;

            var ordetail = db.OrderDetails.Where(a => a.OrderID == Id).ToList();
            ViewBag.OrDetail = ordetail;

            var dataEm = db.Employees.Where(a => a.CategoriesEmployeeID == 1 || a.CategoriesEmployeeID == 4).ToList();
            ViewBag.EmployeeOrder = dataEm;

            var dataCus = db.Customers.ToList();
            ViewBag.CustomerData = dataCus;

            var dataPro = db.ProductDetails.ToList();
            ViewBag.ListProduct = dataPro;

            var dataPoint = db.PointDetaileds.FirstOrDefault(c => c.OrderID == Id);
            if (dataPoint != null)
            {
                ViewBag.PointOrder = dataPoint.Point;
            }
            else
            {
                ViewBag.PointOrder = "Không cập nhật được điểm";
            }

            var dataStatus = db.OrderStatus.ToList();
            ViewBag.OrderStt = dataStatus;

            var latestOrderStatusId = (from or in db.Orders
                                       join orderStatus in db.OrderStatusDetails
                                       on or.ID equals orderStatus.OrderID into orderStatusGroup
                                       from latestStatus in orderStatusGroup
                                           .OrderByDescending(os => os.ConfirmationDate)
                                           .Take(1)
                                           .DefaultIfEmpty()
                                       where or.ID == Id
                                       select (decimal?)latestStatus.OrderStatusID) // Lấy giá trị ID của latestStatus
                          .FirstOrDefault();

            ViewBag.LatestOrderStatusId = latestOrderStatusId ?? 0;

            // Gán dữ liệu vào ViewBag

            return PartialView("_EditOrderView");
        }
        [HttpPost]
        public ActionResult EditOrder(Order model,int status,  List<decimal> productId, List<int> quantity, List<double> Prices)
        {
            try
            {
                var order = db.Orders.FirstOrDefault(e => e.ID == model.ID);
                if (model.OrderNo == order.OrderNo.Trim())
                {
                    order.CustomerID = model.CustomerID;
                    order.EmployeeID = model.EmployeeID;
                    order.OrderDate = model.OrderDate;
                    order.PaymentMethods = model.PaymentMethods;
                    order.RecipientPhone = model.RecipientPhone;
                    order.DeliveryAddress = model.DeliveryAddress;
                    order.Note = model.Note;
                    order.TotalPayment = model.TotalPayment;
                    db.SaveChanges();


                    decimal newCateEmployeeId = model.ID;
                    var orderdetail = db.OrderDetails.Where(a => a.OrderID == model.ID).ToList();
                    if (orderdetail.Count > 0)
                    {
                        db.OrderDetails.RemoveRange(orderdetail);
                        db.SaveChanges();
                        if (productId != null)
                        {
                            for (int i = 0; i < productId.Count; i++)
                            {
                                decimal proID = productId[i];
                                int Quantity = quantity[i];
                                double Price = Prices[i];
                                var checkpro = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                                if (checkpro.StatusProduct == "Hết hàng")
                                {
                                    return Json(new { status = -1, title = "", text = "Sản phẩm " + checkpro.Code + " hết hàng.", obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    OrderDetail detail = new OrderDetail();
                                    detail.ProductID = proID;
                                    detail.OrderID = model.ID;
                                    detail.Quantity = Quantity;
                                    detail.Price = Price;
                                    db.OrderDetails.Add(detail);
                                    db.SaveChanges();
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
                                OrderDetail detail = new OrderDetail();
                                detail.ProductID = proID;
                                detail.OrderID = model.ID;
                                detail.Quantity = Quantity;
                                detail.Price = Price;
                                db.OrderDetails.Add(detail);
                                db.SaveChanges();
                            }
                        }
                    }
                    var statusorder = db.OrderStatusDetails.FirstOrDefault(a => a.OrderID == model.ID && a.OrderStatusID == status);
                    if(statusorder != null)
                    {
                        db.SaveChanges();
                    }
                    else
                    {
                        OrderStatusDetail orstatus = new OrderStatusDetail();
                        orstatus.OrderID = model.ID;
                        orstatus.OrderStatusID = status;
                        orstatus.ConfirmationDate = DateTime.Now;
                        db.OrderStatusDetails.Add(orstatus);
                        db.SaveChanges();

                    }

                   

                    double totalPayment = (double)model.TotalPayment;
                    int roundedValue = (int)Math.Round(totalPayment / 100000, MidpointRounding.AwayFromZero);
                    var diem = db.PointDetaileds.FirstOrDefault(b => b.OrderID == model.ID);
                    if(diem != null)
                    {
                        diem.OrderID = model.ID;
                        diem.Point = roundedValue;
                        db.SaveChanges();
                    }
                    else
                    {
                       
                        PointDetailed diemnew = new PointDetailed();
                        diemnew.OrderID = model.ID;
                        diemnew.Point = roundedValue;

                        db.PointDetaileds.Add(diemnew);
                        var or = db.Orders.FirstOrDefault(a => a.ID == newCateEmployeeId);
                        or.PointID = diemnew.ID;

                        db.SaveChanges();
                    }
                    

                    return Json(new { status = 1, title = "", text = "Cập nhật đơn hàng thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã số đơn hàng không đúng, cập nhật đơn hàng thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DeleteOrder(int Id)
        {
            Order order = db.Orders.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (order != null)
                    {
                        var detail = db.OrderDetails.Where(m => m.OrderID == Id);
                        db.OrderDetails.RemoveRange(detail);

                        var point = db.PointDetaileds.FirstOrDefault(m => m.OrderID == Id);
                        db.PointDetaileds.Remove(point);

                        var status = db.OrderStatusDetails.Where(m => m.OrderID == Id);
                        db.OrderStatusDetails.RemoveRange(status);
                        db.SaveChanges();

                        db.Orders.Remove(order);
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
        public ActionResult ListOrderConfirmation(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 25;
            }
            var query = from order in db.Orders
                        join orderStatus in db.OrderStatusDetails
                        on order.ID equals orderStatus.OrderID into gj
                        from subOrderStatus in gj.DefaultIfEmpty()
                        where (subOrderStatus == null || subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate))
                            && (subOrderStatus == null || subOrderStatus.OrderStatusID == 1)
                        orderby order.OrderDate descending
                        select new OrderDetailModel
                        {
                            OrderId = order.ID,
                            OrderNo = order.OrderNo,
                            EmployeeName = order.Employee.LastName + " " + order.Employee.FirstName,
                            EmployeeId = order.EmployeeID,
                            CustomerName = order.Customer.LastName + " " + order.Customer.FirstName,
                            CustomerId = order.CustomerID,
                            ReceiptPhone = order.RecipientPhone,
                            OrderDate = order.OrderDate,
                            TotalPayment = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                            LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null
                        };
            var orders = query.ToList();
            return View(orders.ToPagedList((int)page, (int)pageSize));
        }
        public ActionResult ListOrderPrepared(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 25;
            }
            var query = from order in db.Orders
                        join orderStatus in db.OrderStatusDetails
                        on order.ID equals orderStatus.OrderID into gj
                        from subOrderStatus in gj.DefaultIfEmpty()
                        where (subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate))
                            && (subOrderStatus.OrderStatusID == 2)
                        orderby order.OrderDate descending
                        select new OrderDetailModel
                        {
                            OrderId = order.ID,
                            OrderNo = order.OrderNo,
                            EmployeeName = order.Employee.LastName + " " + order.Employee.FirstName,
                            EmployeeId = order.EmployeeID,
                            CustomerName = order.Customer.LastName + " " + order.Customer.FirstName,
                            CustomerId = order.CustomerID,
                            ReceiptPhone = order.RecipientPhone,
                            OrderDate = order.OrderDate,
                            TotalPayment = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                            LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null
                        };
            var orders = query.ToList();
            return View(orders.ToPagedList((int)page, (int)pageSize));
        }
        public ActionResult ListOrdeShipping(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 25;
            }
            var query = from order in db.Orders
                        join orderStatus in db.OrderStatusDetails
                        on order.ID equals orderStatus.OrderID into gj
                        from subOrderStatus in gj.DefaultIfEmpty()
                        where ( subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate))
                            && ( subOrderStatus.OrderStatusID == 3)
                        orderby order.OrderDate descending
                        select new OrderDetailModel
                        {
                            OrderId = order.ID,
                            OrderNo = order.OrderNo,
                            EmployeeName = order.Employee.LastName + " " + order.Employee.FirstName,
                            EmployeeId = order.EmployeeID,
                            CustomerName = order.Customer.LastName + " " + order.Customer.FirstName,
                            CustomerId = order.CustomerID,
                            ReceiptPhone = order.RecipientPhone,
                            OrderDate = order.OrderDate,
                            TotalPayment = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                            LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null
                        };
            var orders = query.ToList();
            return View(orders.ToPagedList((int)page, (int)pageSize));
        }

        public ActionResult ListOrdeComplete(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 25;
            }
            var query = from order in db.Orders
                        join orderStatus in db.OrderStatusDetails
                        on order.ID equals orderStatus.OrderID into gj
                        from subOrderStatus in gj.DefaultIfEmpty()
                        where ( subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate))
                            && (subOrderStatus.OrderStatusID == 4)
                        orderby order.OrderDate descending
                        select new OrderDetailModel
                        {
                            OrderId = order.ID,
                            OrderNo = order.OrderNo,
                            EmployeeName = order.Employee.LastName + " " + order.Employee.FirstName,
                            EmployeeId = order.EmployeeID,
                            CustomerName = order.Customer.LastName + " " + order.Customer.FirstName,
                            CustomerId = order.CustomerID,
                            ReceiptPhone = order.RecipientPhone,
                            OrderDate = order.OrderDate,
                            TotalPayment = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                            LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null
                        };
            var orders = query.ToList();
            return View(orders.ToPagedList((int)page, (int)pageSize));
        }
        public ActionResult ListOrdeFail(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 25;
            }
            var query = from order in db.Orders
                        join orderStatus in db.OrderStatusDetails
                        on order.ID equals orderStatus.OrderID into gj
                        from subOrderStatus in gj.DefaultIfEmpty()
                        where (subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate))
                            && (subOrderStatus.OrderStatusID == 5)
                        orderby order.OrderDate descending
                        select new OrderDetailModel
                        {
                            OrderId = order.ID,
                            OrderNo = order.OrderNo,
                            EmployeeName = order.Employee.LastName + " " + order.Employee.FirstName,
                            EmployeeId = order.EmployeeID,
                            CustomerName = order.Customer.LastName + " " + order.Customer.FirstName,
                            CustomerId = order.CustomerID,
                            ReceiptPhone = order.RecipientPhone,
                            OrderDate = order.OrderDate,
                            TotalPayment = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                            LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null
                        };
            var orders = query.ToList();
            return View(orders.ToPagedList((int)page, (int)pageSize));
        }
    }

}