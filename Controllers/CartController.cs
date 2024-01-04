using Dapper;
using MCS.Library.Core;
using SABLANCE.Areas.Admin.Models;
using SABLANCE.Models;
using SABLANCE.Service;
using SABLANCE.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SABLANCE.Controllers
{
    public class CartController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Cart

        public ActionResult Cart()
        {
            try
            {
                DateTime today = DateTime.Today;

                // Lấy tất cả các sản phẩm có khuyến mãi trong ngày hiện tại
                var productsWithPromotion = (
        from promotionDetail in db.PromotionDetails
        join promotion in db.Promotions on promotionDetail.PromotionID equals promotion.ID
        join productDetail in db.ProductDetails on promotionDetail.ProductDetailsID equals productDetail.ID
        where promotion.StartTime <= today && today <= promotion.EndTime
        select new ProductWithPromotion
        {
            ProductID = (decimal)productDetail.ProductID,
            ProductDetailID = productDetail.ID,
            PromotionID = promotion.ID,
            Image = productDetail.Image,
            Prices = productDetail.Prices * (1 - (promotionDetail.Discount)),
            Discount = promotionDetail.Discount,
            Color = productDetail.Color.Code,
            ColorID = productDetail.ColorID ?? 0,
            Size = productDetail.Size.Name,
            SizeID = productDetail.SizeID ?? 0,
            ProductName = productDetail.Product.Name
        }
    ).ToList();
                
                decimal idCustomer = 0;
                var accountObject = (Account)Session["Account"];
                if(accountObject != null)
                {
                    idCustomer = Convert.ToDecimal(accountObject.Customer.ID);
                    var dataCart = db.Carts
                                .Where(a => a.IdCustommer == idCustomer)
                                .ToList()
                                .Select(a => new Cart
                                {
                                    Id = a.Id,
                                    IdCustommer = a.IdCustommer,
                                    Customer = db.Customers.FirstOrDefault(z => z.ID == a.IdCustommer),
                                    IdProductDetail = a.IdProductDetail,
                                    ProductDetail = db.ProductDetails.FirstOrDefault(p => p.ID == a.IdProductDetail),
                                    Quantity = a.Quantity,
                                    SumPrice = a.SumPrice,
                                }).ToList();
                    
                    foreach(var cart in dataCart)
                    {
                        decimal ID =(decimal)cart.IdProductDetail;
                        var checkpro = productsWithPromotion.FirstOrDefault(a => a.ProductDetailID == ID);
                        if(checkpro != null)
                        {
                            cart.SumPrice =(decimal)checkpro.Prices;
                        }
                        else
                        {
                            cart.SumPrice = cart.SumPrice;
                        }    
                    }
                    ViewBag.Carts = dataCart;
                    var dataCus = db.Customers.FirstOrDefault(a=> a.ID == idCustomer);
                    ViewBag.Customer = dataCus;

                    return View();
                }
                else
                {
                    return View();
                }
            }
            catch(Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
            
        }
        public JsonResult OnClickCartCheckUser()
        {
            try
            {
                decimal idCustomer = 0;
                if (Session["Account"] != null)
                {
                    var accountObject = (Account)Session["Account"];
                    idCustomer = Convert.ToDecimal(accountObject.Customer.ID);
                }
                var dataCustommer = db.Customers.Where(a => a.ID == idCustomer).FirstOrDefault();
                if (dataCustommer == null)
                {
                    return Json(new { status = -2, title = "", text = "Vui lòng đăng nhập", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = 1, title = "", text = "Đã đăng nhập", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult AddToCart(int Id)
        {
            try
            {
                DateTime today = DateTime.Today;

                // Lấy tất cả các sản phẩm có khuyến mãi trong ngày hiện tại
                var productsWithPromotion = (
        from promotionDetail in db.PromotionDetails
        join promotion in db.Promotions on promotionDetail.PromotionID equals promotion.ID
        join productDetail in db.ProductDetails on promotionDetail.ProductDetailsID equals productDetail.ID
        where promotion.StartTime <= today && today <= promotion.EndTime
        select new ProductWithPromotion
        {
            ProductID = (decimal)productDetail.ProductID,
            ProductDetailID = productDetail.ID,
            PromotionID = promotion.ID,
            Image = productDetail.Image,
            Prices = productDetail.Prices * (1 - (promotionDetail.Discount)),
            Discount = promotionDetail.Discount,
            Color = productDetail.Color.Code,
            ColorID = productDetail.ColorID ?? 0,
            Size = productDetail.Size.Name,
            SizeID = productDetail.SizeID ?? 0,
            ProductName = productDetail.Product.Name
        }
    ).ToList();

                var pro = db.ProductDetails.SingleOrDefault(a => a.ID == Id);
                if (pro == null)
                {
                    return Json(new { status = -1, title = "", text = "Không tìm thấy sản phẩm", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                if (pro.StatusProduct == "Hết hàng")
                {
                    return Json(new { status = -1, title = "", text = "Sản phẩm đã hết hàng", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                decimal idCustomer=0;
                if (Session["Account"] != null)
                {
                    var accountObject = (Account)Session["Account"];
                    idCustomer = Convert.ToDecimal(accountObject.Customer.ID);
                }
                var dataCustommer = db.Customers.Where(a => a.ID == idCustomer).FirstOrDefault();
                if (dataCustommer == null)
                {
                    return Json(new { status = -2, title = "", text = "Vui lòng đăng nhập", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                var dataCart = db.Carts.Where(a => a.IdCustommer == idCustomer).ToList();
                var checkpro = productsWithPromotion.FirstOrDefault(a => a.ProductDetailID == Id);
                if (dataCart.Any())
                {
                    var cartToUpdate = dataCart.FirstOrDefault(cart => cart.IdProductDetail == Id);

                    if (cartToUpdate != null)
                    {
                        if(checkpro!= null)
                        {
                            cartToUpdate.Quantity++;
                            cartToUpdate.SumPrice = (decimal)checkpro.Prices;
                            db.Entry(cartToUpdate).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            cartToUpdate.Quantity++;
                            cartToUpdate.SumPrice = (decimal)pro.Prices;
                            db.Entry(cartToUpdate).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        return Json(new { status = 1, title = "", text = "Cập nhật sản phẩm thành công " + pro.Product.Name, obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                if(checkpro != null)
                {
                    var newCarts = new Cart()
                    {
                        IdCustommer = idCustomer,
                        IdProductDetail = Id,
                        Quantity = 1,
                        SumPrice = (decimal)checkpro.Prices,
                    };
                    db.Carts.Add(newCarts);
                    db.SaveChanges();
                }
                else
                {
                    var newCart = new Cart()
                    {
                        IdCustommer = idCustomer,
                        IdProductDetail = Id,
                        Quantity = 1,
                        SumPrice = (decimal)pro.Prices,
                    };
                    db.Carts.Add(newCart);
                    db.SaveChanges();
                }
               
                var countproduct = db.Carts.Where(a => a.IdCustommer == idCustomer).ToList().Count();
                Session["CountCart"] = countproduct;
                return Json(new { status = 1, title = "", text = "Thêm sản phẩm vào giỏ hàng thành công sản phẩm "+pro.Product.Name, obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public  JsonResult UpdateCart(int Id,  int quantity)
        {
            try
            {
                var cart =  db.Carts.FirstOrDefault(a => a.Id == Id);
                cart.Quantity = quantity;

                var product = db.ProductDetails.FirstOrDefault(a => a.ID == cart.IdProductDetail);
                decimal idpro = product.ID;
                if(product.ID == 150)
                {
                    return Json(new { status = -1, title = "", text = "Đây là sản phẩm tặng kèm, bạn không thể đặt số lượng nhiều.", obj = "" }, JsonRequestBehavior.AllowGet);
                }    
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
                var invantory = inventories.FirstOrDefault(a => a.ProductID == idpro);
                if(quantity<= invantory.Quantity)
                {
                    db.SaveChanges();
                    return Json(new { status = 1, title = "", text = "Cập nhật số lượng thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }    
                else
                {
                    return Json(new { status = -1, title = "", text = "Kho hàng không đủ số lượng", obj = "" }, JsonRequestBehavior.AllowGet);
                }    

                
                
            }
            catch(Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }    
        public JsonResult DeleteProduct(int Id)
        {
            try
            {
                var cart = db.Carts.FirstOrDefault(a => a.Id == Id);
                decimal idcus =(decimal) cart.IdCustommer;
                db.Carts.Remove(cart);
                db.SaveChanges();
                var countproduct = db.Carts.Where(a => a.IdCustommer == idcus).ToList().Count();
                Session["CountCart"] = countproduct;
                return Json(new { status = 1, title = "", text = "Xóa sản phẩm thành công", obj = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, title = "", text = ex.Message, obj = "" }, JsonRequestBehavior.AllowGet);
            }
        }
       

        [HttpPost]
        public  async Task<JsonResult> CreateOrder(Order model, PointDetailed point, List<decimal> productId, List<int> quantity, List<double> Prices, string email)
        {
            try
            {
                DateTime today = DateTime.Today;

                // Lấy tất cả các sản phẩm có khuyến mãi trong ngày hiện tại
                var productsWithPromotion = (
        from promotionDetail in db.PromotionDetails
        join promotion in db.Promotions on promotionDetail.PromotionID equals promotion.ID
        join productDetail in db.ProductDetails on promotionDetail.ProductDetailsID equals productDetail.ID
        where promotion.StartTime <= today && today <= promotion.EndTime
        select new ProductWithPromotion
        {
            ProductID = (decimal)productDetail.ProductID,
            ProductDetailID = productDetail.ID,
            PromotionID = promotion.ID,
            Image = productDetail.Image,
            Prices = productDetail.Prices * (1 - (promotionDetail.Discount)),
            Discount = promotionDetail.Discount,
            Color = productDetail.Color.Code,
            ColorID = productDetail.ColorID ?? 0,
            Size = productDetail.Size.Name,
            SizeID = productDetail.SizeID ?? 0,
            ProductName = productDetail.Product.Name
        }
    ).ToList();
                var code = new { Success = false, Code = -1, Url = "" };
                if (productId != null)
                {
                    model.EmployeeID = null;
                    model.OrderDate = DateTime.Now;
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


                    if (productId != null)
                    {
                        for (int i = 0; i < productId.Count; i++)
                        {
                            decimal proID = productId[i];
                            int Quantity = quantity[i];
                            double Price = Prices[i];
                            var invantory = inventories.FirstOrDefault(a => a.ProductID == proID);
                            var checkpro = productsWithPromotion.FirstOrDefault(a => a.ProductDetailID == proID);
                            var check = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                            if (invantory != null)
                            {
                                if(Quantity <= invantory.Quantity)
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
                                         if(checkpro != null)
                                        {
                                            OrderDetail order = new OrderDetail();
                                            order.ProductID = proID;
                                            order.OrderID = model.ID;
                                            order.Quantity = Quantity;
                                            order.Price = checkpro.Prices;
                                            db.OrderDetails.Add(order);
                                            db.SaveChanges();
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
                    db.SaveChanges();

                    var cart = db.Carts.Where(a => a.IdCustommer == model.CustomerID);
                    db.Carts.RemoveRange(cart);
                    db.SaveChanges();

                    var or = db.Orders.FirstOrDefault(a => a.ID == model.ID);
                    or.PointID = diem.ID;

                    db.SaveChanges();
                    var payment = db.Orders.FirstOrDefault(a => a.ID == model.ID);
                    var pay = payment.PaymentMethods;   
                    if(pay == "Ví điện tử VNPay")
                    {
                        var paymentVN = PaymentVNPay((int)payment.ID);
                        if (email != null)
                        {
                            // SendMail
                            string Body = "";
                            var productdetail = db.OrderDetails.Where(a => a.OrderID == model.ID);
                            foreach (var sp in productdetail)
                            {
                                Body += "<tr>";
                                Body += "<td>" + sp.ProductDetail.Code + "</td>";
                                Body += "<td>" + sp.ProductDetail.Product.Name + "</td>";
                                Body += "<td>" + sp.ProductDetail.Color.Name + "</td>";
                                if (sp.ProductDetail.SizeID != null)
                                {
                                    Body += "<td>" + sp.ProductDetail.Size.Name + "</td>";
                                }
                                else
                                {
                                    Body += "<td>" + "</td>";
                                }
                                Body += "<td>" + string.Format("{0:N0}",sp.Quantity )+ "</td>";
                                Body += "<td>" + sp.Price + "</td>";
                                Body += "<td>" + string.Format("{0:N0}", sp.Price * sp.Quantity) + "</td>";
                                Body += "</tr>";

                            }
                            ImailService mailService = new ImailService();
                            var orcus = db.Customers.FirstOrDefault(a => a.ID == model.CustomerID);
                            string ten = orcus.LastName + " " + orcus.FirstName;
                            var orstt = db.OrderStatus.FirstOrDefault(a => a.ID == orstatus.OrderStatusID);
                            string stt = orstt.Name;
                            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/Templete/send2.html"));
                            contentCustomer = contentCustomer.Replace("{{MaDon}}", model.OrderNo);
                            contentCustomer = contentCustomer.Replace("{{NgayDat}}", model.OrderDate.ToString("dd/MM/yyyy HH:mm"));
                            contentCustomer = contentCustomer.Replace("{{SanPham}}", Body);
                            contentCustomer = contentCustomer.Replace("{{TongTien}}", string.Format("{0:N0}", model.TotalPayment));
                            contentCustomer = contentCustomer.Replace("{{PTTT}}", model.PaymentMethods);
                            contentCustomer = contentCustomer.Replace("{{Diachi}}", model.DeliveryAddress);
                            contentCustomer = contentCustomer.Replace("{{Phone}}", model.RecipientPhone);
                            contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", ten.ToUpper());
                            contentCustomer = contentCustomer.Replace("{{Email}}", email);
                            contentCustomer = contentCustomer.Replace("{{Tinhtrangdon}}", stt.ToUpper());
                            bool IsSendMail = await mailService.SendEmail(email, "Đơn hàng số #" + model.OrderNo, contentCustomer);// gửi mail gọi lại cái này truyền tham số y chang 
                        }
                        return Json(new { status = 2, title = "", text = paymentVN,  obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (email != null)
                        {
                            // SendMail
                            string Body = "";
                            var productdetail = db.OrderDetails.Where(a => a.OrderID == model.ID);
                            foreach (var sp in productdetail)
                            {
                                Body += "<tr>";
                                Body += "<td>" + sp.ProductDetail.Code + "</td>";
                                Body += "<td>" + sp.ProductDetail.Product.Name + "</td>";
                                Body += "<td>" + sp.ProductDetail.Color.Name + "</td>";
                                if (sp.ProductDetail.SizeID != null)
                                {
                                    Body += "<td>" + sp.ProductDetail.Size.Name + "</td>";
                                }
                                else
                                {
                                    Body += "<td>" + "</td>";
                                }
                                Body += "<td>" +  sp.Quantity + "</td>";
                                Body += "<td>" + string.Format("{0:N0}", sp.Price) + "</td>";
                                Body += "<td>" + string.Format("{0:N0}", sp.Price * sp.Quantity) + "</td>";
                                Body += "</tr>";

                            }
                            ImailService mailService = new ImailService();
                            var orcus = db.Customers.FirstOrDefault(a => a.ID == model.CustomerID);
                            string ten = orcus.LastName + " " + orcus.FirstName;
                            var orstt = db.OrderStatus.FirstOrDefault(a => a.ID == orstatus.OrderStatusID);
                            string stt = orstt.Name;
                            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/Templete/send2.html"));
                            contentCustomer = contentCustomer.Replace("{{MaDon}}", model.OrderNo);
                            contentCustomer = contentCustomer.Replace("{{NgayDat}}", model.OrderDate.ToString("dd/MM/yyyy HH:mm"));
                            contentCustomer = contentCustomer.Replace("{{SanPham}}", Body);
                            contentCustomer = contentCustomer.Replace("{{TongTien}}", string.Format("{0:N0}", model.TotalPayment));
                            contentCustomer = contentCustomer.Replace("{{PTTT}}", model.PaymentMethods);
                            contentCustomer = contentCustomer.Replace("{{Diachi}}", model.DeliveryAddress);
                            contentCustomer = contentCustomer.Replace("{{Phone}}", model.RecipientPhone);
                            contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", ten.ToUpper());
                            contentCustomer = contentCustomer.Replace("{{Email}}", email);
                            contentCustomer = contentCustomer.Replace("{{Tinhtrangdon}}", stt.ToUpper());
                            bool IsSendMail = await mailService.SendEmail(email, "Đơn hàng số #" + model.OrderNo, contentCustomer);// gửi mail gọi lại cái này truyền tham số y chang 
                        }
                        return Json(new { status = 1, title = "", text = "Bạn đã đặt hàng thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                    
                    
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Không thể đặt hàng", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public string PaymentVNPay(int Id)
        {
            var order = db.Orders.FirstOrDefault(a => a.ID == Id);

                
            string url = ConfigurationManager.AppSettings["vnp_Url"];
            string returnUrl = ConfigurationManager.AppSettings["vnp_Returnurl"];
            string tmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

            VnPayLibrary pay = new VnPayLibrary();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", (order.TotalPayment* 100).ToString());
            pay.AddRequestData("vnp_BankCode", "");
            pay.AddRequestData("vnp_CreateDate", order.OrderDate.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng: #" + order.OrderNo);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            //pay.AddRequestData("vnp_TxnRef", newOrderNumber);
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return paymentUrl;
        }
    }
}