using SABLANCE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SABLANCE.Controllers
{
    public class ProductController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Product
        public ActionResult ShoeDepartment(int? CategoriesProductID)
        {
            IQueryable<Product> data;

            if (CategoriesProductID.HasValue)
            {
                // Nếu CategoriesID có giá trị, lấy sản phẩm của loại đó
                data = db.Products.Where(p => p.CategoriesProductID == CategoriesProductID.Value);
            }
            else
            {
                // Nếu CategoriesID không có giá trị, lấy tất cả sản phẩm từ danh sách các loại cụ thể
                var allowedCategories = new List<int> { 1, 2, 4, 5, 6, 7, 8, 9 };
                data = db.Products.Where(p => allowedCategories.Contains((int)p.CategoriesProductID));
            }

            ViewBag.data = data.ToList();
            return View();
        }
        public ActionResult HandBag(int? CategoriesProductID)
        {
            IQueryable<Product> data;

            if (CategoriesProductID.HasValue)
            {
                // Nếu CategoriesID có giá trị, lấy sản phẩm của loại đó
                data = db.Products.Where(p => p.CategoriesProductID == CategoriesProductID.Value);
            }
            else
            {
                // Nếu CategoriesID không có giá trị, lấy tất cả sản phẩm từ danh sách các loại cụ thể
                var allowedCategories = new List<int> { 10, 11, 12};
                data = db.Products.Where(p => allowedCategories.Contains((int)p.CategoriesProductID));
            }

            ViewBag.data = data.ToList();
            return View();
        }
        public ActionResult Balo()
        {
            var data = db.Products.Where(p => p.CategoriesProductID == 13).ToList();
            ViewBag.data = data;

            var datapro = db.ProductDetails.Where(p => p.Product.CategoriesProductID == 13).ToList();
            ViewBag.Product = datapro;
            return View();
        }
        public ActionResult Sablanca()
        {
            var data = db.Products.Where(p => p.CategoriesProductID == 18).ToList();
            ViewBag.data = data;

            var datapro = db.ProductDetails.Where(p => p.Product.CategoriesProductID == 18).ToList();
            ViewBag.Product = datapro;
            return View();
        }

        public ActionResult Jeremy()
        {
            var data = db.Products.Where(p => p.CategoriesProductID == 19).ToList();
            ViewBag.data = data;

            var datapro = db.ProductDetails.Where(p => p.Product.CategoriesProductID == 19).ToList();
            ViewBag.Product = datapro;
            return View();
        }
        public ActionResult Wallet()
        {
            var data = db.Products.Where(p => p.CategoriesProductID == 15 || p.CategoriesProductID== 16).ToList();
            ViewBag.data = data;
            return View();
        }

        public ActionResult Glasses()
        {
            var data = db.Products.Where(p => p.CategoriesProductID == 17).ToList();
            ViewBag.Glasses = data;
            return View();
        }
        [HttpGet]
        public ActionResult ProductDetail(int Id)
        {
            var pro = db.Products.Where(n => n.ID == Id).FirstOrDefault();
            var datapro = db.ProductDetails.Where(n => n.ProductID == Id).ToList();
            ViewBag.ProductDetail = datapro;
            return View(pro);

        }
        public ActionResult GetProductDetail(int productId, int sizeId, int colorId, string code)
        {
            var selectedDetail = db.ProductDetails
                .FirstOrDefault(a => a.ProductID == productId && a.SizeID == sizeId && a.ColorID == colorId &&  a.Code == code);

            if (selectedDetail != null)
            {
                // Chuyển dữ liệu chi tiết sản phẩm vào ViewBag hoặc ViewData để sử dụng trong PartialView
                ViewBag.SelectedDetail = selectedDetail;

                // Trả về PartialView để sử dụng trong modal
                return PartialView("_ProductDetails");
            }
            else
            {
                // Xử lý khi không tìm thấy chi tiết sản phẩm
                return HttpNotFound();
            }
        }
        public JsonResult ColorOnImage(int color, int id)
        {
            var data = (from a in db.ProductDetails
                        join b in db.Colors on a.ColorID equals b.ID
                        where a.ProductID == id && b.ID == color
                        select a).FirstOrDefault();

            return Json(new
            {
                status = 1,
                title = "",
                text = "",
                obj = data != null ? new
                {
                    ProductID = data.ProductID,
                    ColorID = data.ColorID,
                    SizeID = data.SizeID,
                    CodeProduct = data.Code,
                    IDDetail = data.ID,
                    Image = data.Image
                } : null
            }, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult ColorSizeOnImage(decimal color, decimal size, decimal id)
        {
            var data = (from a in db.ProductDetails
                        join b in db.Colors on a.ColorID equals b.ID
                        join c in db.Sizes on a.SizeID equals c.ID
                        where a.ProductID == id && a.ColorID == color
                        where b.ID == color && c.ID == size
                        select a).FirstOrDefault();

            return Json(new
            {
                status = 1,
                title = "",
                text = "",
                obj = data != null ? new
                {
                    ProductID = data.ProductID,
                    ColorID = data.ColorID,
                    SizeID = data.SizeID,
                    SizeName = data.Size.Name,
                    CodeProduct = data.Code,
                    IDDetail = data.ID,
                    Image = data.Image
                } : null
            }, JsonRequestBehavior.AllowGet);
        }

    }
}