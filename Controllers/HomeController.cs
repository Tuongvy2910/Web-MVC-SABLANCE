using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SABLANCE.Models;
using System.Web.Mvc;
using SABLANCE.Areas.Admin.Models;
using SABLANCE.ViewModel;
using System.Configuration;
using SABLANCE.Service;
using System.Threading.Tasks;

namespace SABLANCE.Controllers
{
    public class HomeController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        public async Task<ActionResult> Home()
        {
            var data = db.Products.ToList();
            ViewBag.data = data;

            DateTime today = DateTime.Today;

            // Lấy tất cả các sản phẩm có khuyến mãi trong ngày hiện tại
            var productsWithPromotion = (
    from promotionDetail in db.PromotionDetails
    join promotion in db.Promotions on promotionDetail.PromotionID equals promotion.ID
    join productDetail in db.ProductDetails on promotionDetail.ProductDetailsID equals productDetail.ID
    where promotion.StartTime <= today && today <= promotion.EndTime
    select new ProductWithPromotion
    {
        ProductID =(decimal)productDetail.ProductID,
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


            ViewBag.Promotion = productsWithPromotion;

            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                    }
                    else
                    {
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    //log.InfoFormat("Invalid signature, InputData={0}", Request.RawUrl);
                    ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            var cus = db.Customers
                .Where(a => a.DateOfBirth.Value.Month == today.Month && a.DateOfBirth.Value.Day == today.Day)
                .ToList();
            foreach ( var item in cus)
            {
                if (item.Email != null)
                {
                    // SendMail
                    string Body = "";
                    var productdetail = db.ProductDetails.Where(a => a.ID == 150);
                    foreach (var sp in productdetail)
                    {
                        Body += "<tr>";
                        Body += "<td>" + sp.Code + "</td>";
                        Body += "<td>" + sp.Product.Name + "</td>";
                        Body += "<td>" + sp.Color.Name + "</td>";
                        if (sp.SizeID != null)
                        {
                            Body += "<td>" + sp.Size.Name + "</td>";
                        }
                        else
                        {
                            Body += "<td>" + "</td>";
                        }
                        Body += "<td>" +  1 + "</td>";
                        Body += "<td>" + sp.Prices + "</td>";
                        Body += "<td>" + string.Format("{0:N0}", sp.Prices * 1) + "</td>";
                        Body += "</tr>";

                    }
                    ImailService mailService = new ImailService();
                    string ten = item.LastName + " " + item.FirstName;

                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/Templete/send1.html"));

                    contentCustomer = contentCustomer.Replace("{{SanPham}}", Body);
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", ten.ToUpper());
                    contentCustomer = contentCustomer.Replace("{{Email}}", item.Email);
                    contentCustomer = contentCustomer.Replace("{{Diachi}}", item.Address);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", item.Phone);
                    bool IsSendMail = await mailService.SendEmailBirthDay(item.Email, "Chúc mừng sinh nhật " + ten.ToUpper(), contentCustomer);
                    if(IsSendMail)
                    {
                        var car = db.Carts.Where(a => a.IdCustommer == item.ID);
                        if (car == null)
                        {
                            Cart birth = new Cart();
                            birth.IdCustommer = item.ID;
                            birth.IdProductDetail = 150;
                            birth.Quantity = 1;
                            db.Carts.Add(birth);
                            db.SaveChanges();
                        }
                        else
                        {
                            var check = db.Carts.FirstOrDefault(a => a.IdProductDetail == 150 && a.IdCustommer == (decimal)item.ID);
                            if (check != null)
                            {
                                check.Quantity = 1;
                                db.SaveChanges();
                            }
                            else
                            {
                                Cart birth = new Cart();
                                birth.IdCustommer = item.ID;
                                birth.IdProductDetail = 150;
                                birth.Quantity = 1;
                                db.Carts.Add(birth);
                                db.SaveChanges();
                            }    

                        }
                    }    
                }
            }    

            return View();
        }
        public ActionResult Search(string strSearch)
        {
            strSearch = strSearch.ToLower();
            var lmh = db.Products.Where(b => b.Name.ToString().Contains(strSearch) || strSearch == null).ToList();
            ViewBag.Search = lmh;

            return View();
        }

        public ActionResult SearchOrder()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SearchOrderList(string phone_no)
        {
            var user = db.Orders.FirstOrDefault(u => u.RecipientPhone == phone_no);

            if (user == null)
            {
                ViewBag.Message = "Số điện thoại này chưa mua hàng.";
                return RedirectToAction("SearchOrder", "Home");
            }
            else
            {
                // Nếu chưa đăng ký, có thể chuyển hướng hoặc thực hiện các bước đăng ký khác
                return Json(new { status = 1, title = "", text = "success", obj = "" }); ;
            }

        }
        public class OrderHistoryViewModel
        {
            public decimal OrderID { get; set; }
            public string OrderNo { get; set; }
            public string CustomerName { get; set; }
            public decimal? CustomerId { get; set; }
            public DateTime OrderDate { get; set; }
            public int TotalQuantity { get; set; }
            public double TotalAmount { get; set; }
            public string LatestOrderStatus { get; set; }
            public string ReceiptPhone  { get; set; }
        }
        public ActionResult ListOrderSearch(string phone_no)
        {
             var list = db.Orders.Where(a=> a.RecipientPhone == phone_no).ToList();

            var history = (from order in list
                           join orderStatus in db.OrderStatusDetails
                               on order.ID equals orderStatus.OrderID into gj
                           from subOrderStatus in gj.DefaultIfEmpty()
                           where (subOrderStatus == null || (subOrderStatus.ConfirmationDate == gj.Max(os => os.ConfirmationDate)))
                           orderby order.OrderDate descending
                           select new OrderHistoryViewModel
                           {
                               OrderID = order.ID,
                               OrderNo = order.OrderNo,
                               CustomerId = order.CustomerID,
                               ReceiptPhone = order.RecipientPhone,
                               CustomerName = order.Customer.LastName + " " + order.Customer.FirstName,
                               OrderDate = order.OrderDate,
                               TotalAmount = order.TotalPayment.HasValue ? (double)order.TotalPayment : 0.0,
                               LatestOrderStatus = subOrderStatus != null ? subOrderStatus.OrderStatu.Name : null,
                               TotalQuantity = order.OrderDetails.Count()
                           }).ToList();

            ViewBag.SearchOrder = history;
            return View(list);
        }
        public ActionResult Policy()
        {

            return View();
        }
        public ActionResult InstructChooseSIze()
        {

            return View();
        }
        public ActionResult InstructShopping()
        {

            return View();
        }

        public ActionResult Introduce()
        {

            return View();
        }
        public ActionResult Promotion()
        {
            List<Promotion> data = db.Promotions.OrderByDescending(p => p.EndTime).ToList();
            ViewBag.Promotion = data;
            return View();
        }

        [HttpPost]
        public JsonResult ColorOnImage(int color, int id)
        {
            var data = (from a in db.ProductDetails
                        join b in db.Colors on a.ColorID equals b.ID
                        where a.ProductID == id
                        where b.ID == color
                        select a.Image).FirstOrDefault() ;
            return Json(new { status = 1, title = "", text = "", obj = data.ToString()??"" }, JsonRequestBehavior.AllowGet);
        }
    }
}