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

namespace SABLANCE.Areas.Admin.Controllers
{
    public class ProductAdController : Controller
    {
        // GET: Admin/Product
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Admin/Employee
        public ActionResult ListProduct(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 20;
            }
            List<Product> products = db.Products.ToList();

            Dictionary<decimal, decimal> CountsByProductrID = new Dictionary<decimal, decimal>();

            // Đếm số lượng sản phẩm cho từng Product
            foreach (var pro in products)
            {
                int proCount = db.ProductDetails.Count(c => c.ProductID == pro.ID);
                CountsByProductrID.Add(pro.ID, proCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.CountsByProductID = CountsByProductrID;
            return View(products.ToPagedList((int)page, (int)pageSize));
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
        public ActionResult CreateProductView()
        {
            var dataPro = db.CategoriesProducts.ToList();
            ViewBag.dataPro = dataPro;

            var dataColor = db.Colors.ToList();
            ViewBag.dataColor = dataColor;

            var dataSize = db.Sizes.ToList();
            ViewBag.dataSize = dataSize;

            return PartialView("_createProductView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateProducts(ProductDetailModel model, List<HttpPostedFileBase> ImageData)
        {
            if (ImageData != null && ImageData.Count > 0)
            {
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                var imageList = new List<string>();

                foreach (var imageFile in ImageData)
                {
                    string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                    if (allowedExtensions.Contains(fileExtension))
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string rootImage = "~/Image/Product/" + fileName;
                        imageFile.SaveAs(Server.MapPath(rootImage));
                        imageList.Add(Path.GetFileName(imageFile.FileName));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).");
                        return Json(new { status = -1, title = "", text = "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

                model.Images = imageList;
            }

            try
            {

                var data = new Product()
                {
                    Code = model.Code,
                    Name = model.Name,
                    Unit = model.Unit,
                    Description = model.Description,
                    CategoriesProductID = model.CategoriesProductId,

                };
                db.Products.Add(data);
                await db.SaveChangesAsync();

                try
                {
                    decimal? SizeID;
                    for (int i = 0; i < model.CodeProducts.Count; i++)
                    {
                        if (model.SizeIds[i]<=0)
                        {
                            SizeID = null;
                        }
                        else
                        {
                            SizeID = model.SizeIds[i];
                        }
                        var prodetail = new ProductDetail()
                        {
                            ProductID = data.ID,
                            SizeID = SizeID,
                            ColorID = model.ColorIds[i],
                            Code = model.CodeProducts[i].Trim(),
                            Image = model.Images[i],
                            Prices = model.Prices[i],
                            StatusProduct = model.Status[i],
                        };
                        db.ProductDetails.Add(prodetail);
                        await db.SaveChangesAsync();
                        // Tiếp tục xử lý đối tượng prodetail ở đây (ví dụ: lưu vào cơ sở dữ liệu)
                    }
                    
                }
                catch (Exception e)
                {
                    db.Products.Remove(data);
                    await db.SaveChangesAsync();
                    return Json(new { status = -1, title = "", text = e.Message, obj = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { status = 1, title = "", text = "Bạn đã thêm sản phẩm thành công", obj = "" }, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetProductDetail(int Id)
        {

            var proDetail = db.ProductDetails.FirstOrDefault(a => a.ProductID == Id);
            ViewBag.ProductDetails = proDetail;
            var proList = db.ProductDetails.Where(a => a.ProductID == Id).ToList();
            ViewBag.ProductList = proList;

            return PartialView("_ProductDetails");
        }

        [HttpGet]
        public ActionResult EditProductView(int Id)
        {
            var dataPro = db.CategoriesProducts.ToList();
            ViewBag.dataPro = dataPro;

            var dataColor = db.Colors.ToList();
            ViewBag.dataColor = dataColor;

            var dataSize = db.Sizes.ToList();
            ViewBag.dataSize = dataSize;

            var product = db.ProductDetails.Where(a => a.ProductID == Id).ToList(); ;
            ViewBag.ProductLst = product;

            var proDetail = db.ProductDetails.FirstOrDefault(a => a.ProductID == Id);
            ViewBag.ProductDetails = proDetail;
            var pro = db.Products.Find(Id);
            decimal? ID;
            if(pro.ID > 0)
            {
                ID = pro.ID;
            }
            else
            {
                ID = null;
            }
            ProductDetailModel model = new ProductDetailModel();
            model.Id = (decimal)ID;
            model.Code = pro.Code;
            model.Description= pro.Description;
            model.Unit = pro.Unit;
            if (pro.CategoriesProductID.HasValue)
            {
                model.CategoriesProductId = (decimal)pro.CategoriesProductID;
            }
            else
            {
                model.CategoriesProductId = 0; 
            }
            model.ProductId = new List<decimal?>();
            model.SizeIds = new List<decimal?>();
            model.ColorIds = new List<decimal?>();
            model.Images = new List<string>();
            model.Prices = new List<double>();
            model.CodeProducts = new List<string>();
            model.Status = new List<string>();
            decimal? SizeID;
            decimal? ColorID;
            decimal? ProID;
            string Img, stt;
            double Price;
            string Code;
            for (int i = 0; i < product.Count; i++)
            {
                ProID = product[i].ID;
                if(product[i].SizeID >0)
                {
                    SizeID = product[i].SizeID;
                }    
                else
                {
                    SizeID = null;
                }
                ColorID = product[i].ColorID;
                Img = product[i].Image;
                stt = product[i].StatusProduct;
                Price = product[i].Prices;
                Code = product[i].Code;
                model.ProductId.Add(ProID);
                model.SizeIds.Add(SizeID);
                model.ColorIds.Add(ColorID);
                model.Images.Add(Img);
                model.Prices.Add(Price);
                model.CodeProducts.Add(Code.Trim());
                model.Status.Add(stt);
            }
            return PartialView("_EditProductView" , model);
        }
        private string SaveImageAndGetFileName(HttpPostedFileBase imageFile)
        {
            // Thực hiện lưu ảnh và trả về tên tệp ảnh đã lưu
            // (Điều này tùy thuộc vào cách bạn triển khai hàm lưu ảnh của mình)
            string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
            string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

            if (allowedExtensions.Contains(fileExtension))
            {
                string fileName = Path.GetFileName(imageFile.FileName);
                string rootImage = "~/Image/Product/" + fileName;
                imageFile.SaveAs(Server.MapPath(rootImage));
                return Path.GetFileName(imageFile.FileName);
            }

            // Nếu định dạng không đúng, bạn có thể xử lý lỗi ở đây
            return null;
        }
        [HttpPost]
        public ActionResult EditProduct(ProductDetailModel model, List<HttpPostedFileBase> ImageData)
        {
            if (ImageData != null)
            {
                for (int i = 0; i < ImageData.Count; i++)
                {
                    if (ImageData.Count > 0 && ImageData[i] != null)
                    {

                        var newImage = SaveImageAndGetFileName(ImageData[1]); // Thay bằng hàm lưu ảnh của bạn

                        // Tìm dòng có ảnh cũ và cập nhật với ảnh mới
                        if (ImageData[i] != null)
                        {
                            // Gán giá trị mới cho Image[i]
                            model.Images.Add(newImage);
                        }
                    }
                }
            }     
            try
            {
                var pro = db.Products.FirstOrDefault(e => e.ID == model.Id);
                pro.Code = model.Code;
                pro.Name = model.Name;
                pro.Description = model.Description;
                pro.Unit = model.Unit;
                pro.CategoriesProductID = model.CategoriesProductId;
                db.SaveChanges();

                decimal? SizeID;
                decimal? ColorID;
                string Img, stt;
                double Price;
                string Code;
                if(model.ProductId != null)
                {
                    for (int i = 0; i < model.ProductId.Count; i++)
                    {
                        var productId = model.ProductId[i];
                        ColorID = model.ColorIds[i];
                        Img = model.Images[i];
                        stt = model.Status[i];
                        Price = model.Prices[i];
                        Code = model.CodeProducts[i];
                        if (model.SizeIds[i] <= 0)
                        {
                            SizeID = null;
                        }
                        else
                        {
                            SizeID = model.SizeIds[i];
                        }
                        var product = db.ProductDetails.FirstOrDefault(a => a.ID == productId);
                        if (product != null)
                        {
                            product.Code = Code;
                            product.ProductID = pro.ID;
                            product.SizeID = SizeID;
                            product.ColorID = ColorID;
                            product.Prices = Price;
                            product.Image = Img;
                            product.StatusProduct = stt;
                            db.SaveChanges();
                        }
                        else
                        {
                            return Json(new { status = -1, title = "", text = "Không tìm thấy mặt hàng này trong kho dữ liệu", obj = "" }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }

                return Json(new { status = 1, title = "", text = "Bạn đã cập nhật thông tin sản phẩm thành công", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult AddProductDetail(int Id)
        {
            var pro = db.Products.FirstOrDefault(a => a.ID == Id);
            ViewBag.Products = pro;
            var dataPro = db.CategoriesProducts.ToList();
            ViewBag.dataPro = dataPro;
            var dataColor = db.Colors.ToList();
            ViewBag.dataColor = dataColor;

            var dataSize = db.Sizes.ToList();
            ViewBag.dataSize = dataSize;
            return PartialView("_AddProductDetails");
        }

        public ActionResult CreateProductDetail(ProductDetailModel model, List<HttpPostedFileBase> ImageData)
        {
            if (ImageData != null && ImageData.Count > 0)
            {
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                var imageList = new List<string>();

                foreach (var imageFile in ImageData)
                {
                    string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                    if (allowedExtensions.Contains(fileExtension))
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string rootImage = "~/Image/Product/" + fileName;
                        imageFile.SaveAs(Server.MapPath(rootImage));
                        imageList.Add(Path.GetFileName(imageFile.FileName));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).");
                        return Json(new { status = -1, title = "", text = "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).", obj = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

                model.Images = imageList;
            }

            try
            {
                decimal? Id = model.Id;
                if(Id.HasValue)
                {
                    var data = db.Products.FirstOrDefault(i => i.ID == Id);
                    decimal? SizeID;
                    for (int i = 0; i < model.CodeProducts.Count; i++)
                    {
                        if (model.SizeIds[i] <= 0)
                        {
                            SizeID = null;
                        }
                        else
                        {
                            SizeID = model.SizeIds[i];
                        }
                        var prodetail = new ProductDetail()
                        {
                            ProductID = data.ID,
                            SizeID = SizeID,
                            ColorID = model.ColorIds[i],
                            Code = model.CodeProducts[i].Trim(),
                            Image = model.Images[i],
                            Prices = model.Prices[i],
                            StatusProduct = model.Status[i],
                        };
                        db.ProductDetails.Add(prodetail);
                        db.SaveChanges();
                        // Tiếp tục xử lý đối tượng prodetail ở đây (ví dụ: lưu vào cơ sở dữ liệu)
                    }
                    return Json(new { status = 1, title = "", text = "Bạn đã thêm mặt hàng mới thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Thêm mặt hàng mới không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DeleteProductDetails(int Id)
        {
            ProductDetail pro = db.ProductDetails.FirstOrDefault(x => x.ID == Id);
            if (pro != null)
            {
                var orders = db.OrderDetails.Where(od => od.ProductID == Id);
                db.OrderDetails.RemoveRange(orders);
                db.SaveChanges();
                var receipt = db.InventoryReceiptDetails.Where(od => od.ProductID == Id);
                db.InventoryReceiptDetails.RemoveRange(receipt);
                db.SaveChanges();
                var issuse = db.InventoryIssueDetails.Where(od => od.ProductID == Id);
                db.InventoryIssueDetails.RemoveRange(issuse);
                db.SaveChanges();
                var promotion = db.PromotionDetails.Where(od => od.ProductDetailsID == Id);
                db.PromotionDetails.RemoveRange(promotion);
                db.SaveChanges();
                db.ProductDetails.Remove(pro);
                db.SaveChanges();
                return Json(new { status = 1, message = "Success", text = "Xóa mặt hàng thành công" });
            }
            else
            {
                return Json(new { status = 0, message = "Customer not found", text = "Không thể xóa mặt hàng này, vui lòng thử lại." });
            }
        }
        public ActionResult DeleteProduct(int Id)
        {
            Product pro = db.Products.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (pro != null)
                    {
                        var products = db.ProductDetails.FirstOrDefault(od => od.ProductID == Id);
                        var orders = db.OrderDetails.Where(od => od.ProductID == products.ID);
                        db.OrderDetails.RemoveRange(orders);
                        db.SaveChanges();

                        var receipt = db.InventoryReceiptDetails.Where(od => od.ProductID == products.ID);
                        db.InventoryReceiptDetails.RemoveRange(receipt);
                        db.SaveChanges();

                        var issuse = db.InventoryIssueDetails.Where(od => od.ProductID == products.ID);
                        db.InventoryIssueDetails.RemoveRange(issuse);
                        db.SaveChanges();

                        
                        var promotion = db.PromotionDetails.Where(od => od.ProductDetailsID == products.ID);
                        db.PromotionDetails.RemoveRange(promotion);
                        db.SaveChanges();

                        var product = db.ProductDetails.Where(od => od.ProductID == Id);
                        db.ProductDetails.RemoveRange(product);
                        db.SaveChanges();
                        
                        db.Products.Remove(pro);
                        db.SaveChanges();
                        dbContextTransaction.Commit();
                        return Json(new { status = 1, message = "Success", text =" Xóa sản phẩm thành công." });
                    }
                }
                catch (Exception)
                {

                    dbContextTransaction.Rollback();
                    return Json(new { status = -1, message = "Error", text = " Xóa sản phẩm không thành công." });
                }
            }
            return Json(new { status = -1, message = "Error", text = " Xóa sản phẩm thành công." });
        }

        public ActionResult ListColor(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 10;
            }
            List<Color> products = db.Colors.ToList();

            Dictionary<decimal, decimal> ProductCountsByColorID = new Dictionary<decimal, decimal>();

            // Đếm số lượng sản phẩm cho từng Product
            foreach (var pro in products)
            {
                int proCount = db.ProductDetails.Count(c => c.ColorID == pro.ID);
                ProductCountsByColorID.Add(pro.ID, proCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.ProductCountsByColorID = ProductCountsByColorID;
            return View(products.ToPagedList((int)page, (int)pageSize));
        }

        public ActionResult CreateColorView()
        {

            return PartialView("_CreateColorView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateColor(Color model)
        {

            try
            {
                var code = db.Colors.FirstOrDefault(a => a.Code == model.Code);
                if(code == null)
                {
                    db.Colors.Add(model);
                    await db.SaveChangesAsync();
                    decimal newMemberRatingId = model.ID;
                    return Json(new { status = 1, title = "", text = "Bạn đã thêm màu sắc thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã màu này đã tốn tại. Vui lòng chọn màu khác!", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetColorDetailView(int Id)
        {
            var dataColor = db.Colors.FirstOrDefault(c => c.ID == Id);
            ViewBag.DataColor = dataColor;
            var dataPro = db.ProductDetails.Where(a => a.ColorID == Id).ToList();
            ViewBag.DataProduct = dataColor;
            var dataSize = db.Sizes.ToList();
            ViewBag.DataSize = dataSize;
            return PartialView("_DetailColorView", dataPro);
        }

        public ActionResult EditColorView(int Id)
        {
            var dataColor = db.Colors.FirstOrDefault(c => c.ID == Id);
            ViewBag.Color = dataColor;
            var dataPro = db.ProductDetails.Where(a => a.ColorID == Id).ToList();
            ViewBag.Product = dataColor;
            var dataSize = db.Sizes.ToList();
            ViewBag.Size = dataSize;
            return PartialView("_EditColorView", dataPro);
        }
        public ActionResult EditColor(Color model)
        {
            try
            {

                    var color = db.Colors.FirstOrDefault(e => e.ID == model.ID);
                    color.Code = model.Code;
                    color.Name = model.Name;
                    db.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Bạn đã cập nhật màu sắc thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult DeleteColor(int Id)
        {
            Color color = db.Colors.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (color != null)
                    {
                        var product = db.ProductDetails.Where(m => m.ColorID == Id);
                        foreach (var cus in product)
                        {
                            var ID = cus.ID;
                            var colorToUpdate = db.ProductDetails.Find(ID);
                            if (colorToUpdate != null)
                            {
                                colorToUpdate.ColorID = null;
                            }
                        }
                        db.SaveChanges();

                        db.Colors.Remove(color);
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

        //Danh mục sản phẩm

        public ActionResult ListCateProduct(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<CategoriesProduct> catepro = db.CategoriesProducts.ToList();

            // Tạo một Dictionary để lưu trữ số lượng khách hàng cho từng MemberID
            Dictionary<decimal, decimal> ProductCountsByCateProduct = new Dictionary<decimal, decimal>();

            // Đếm số lượng khách hàng cho từng MemberID
            foreach (var pro in catepro)
            {
                int proCount = db.Products.Count(c => c.CategoriesProductID == pro.ID);
                ProductCountsByCateProduct.Add(pro.ID, proCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.ProductCountsByCatePro= ProductCountsByCateProduct;

            return View(catepro.ToPagedList((int)page, (int)pageSize));

        }
        public class ProductDTO
        {
            public decimal Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            // Thêm các thuộc tính khác bạn quan tâm
        }
        [HttpGet]
        public ActionResult CreateCateProView()
        {
            var dataPro = db.Products.Where(c => c.CategoriesProductID == null).ToList(); 
            ViewBag.dataProduct = dataPro;

            var dataproduct = db.Products.Where(c => c.CategoriesProductID == null).Select(e => new ProductDTO
            {
                Id = e.ID,
                Name = e.Name,
                Code = e.Code,
            })
                     .ToList();
            ViewBag.ProNull = dataproduct;
            return PartialView("_createCateProView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateCatePro(CategoriesProduct model, List<decimal> productId)
        {

            try
            {
                db.CategoriesProducts.Add(model);
                await db.SaveChangesAsync();
                decimal newCateProId = model.ID;

                // Lặp qua danh sách các customerId và cập nhật MemberID cho mỗi customer
                if(productId != null)
                {
                    foreach (var Id in productId)
                    {
                        var productToUpdate = db.Products.Find(Id);
                        if (productToUpdate != null)
                        {
                            productToUpdate.CategoriesProductID = newCateProId;
                        }
                    }
                }    
                
                await db.SaveChangesAsync();

                return Json(new { status = 1, title = "", text = "Bạn đã thêm loại sản phẩm thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult CateProductDetailView(int cateId)
        {
            var cate = db.CategoriesProducts.Find(cateId);
            ViewBag.CateProduct = cate;
            var dataCate = db.Products.Where(c => c.CategoriesProductID == cateId).ToList();
            ViewBag.ProductList = dataCate;
            Dictionary<decimal, decimal> ProductCounts = new Dictionary<decimal, decimal>();

            // Đếm số lượng khách hàng cho từng MemberID
            foreach (var pro in dataCate)
            {
                int proCount = db.ProductDetails.Count(c => c.ProductID == pro.ID);
                ProductCounts.Add(pro.ID, proCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.ProductCounts = ProductCounts;
            return PartialView("_CateProductDetailView", dataCate);
        }

        [HttpGet]
        public ActionResult EditCateProductView(int Id)
        {
            var dataPro = db.Products.Where(c => c.CategoriesProductID == Id).ToList(); ;
            ViewBag.EditProduct = dataPro;

            var member = db.CategoriesProducts.Find(Id);
            ViewBag.EditCate = member;

            Dictionary<decimal, decimal> ProductCounts = new Dictionary<decimal, decimal>();

            // Đếm số lượng khách hàng cho từng MemberID
            foreach (var pro in dataPro)
            {
                int proCount = db.ProductDetails.Count(c => c.ProductID == pro.ID);
                ProductCounts.Add(pro.ID, proCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.ProductCountCate = ProductCounts;

            return PartialView("_EditCateProductView");
        }
        [HttpPost]
        public ActionResult EditCateProduct(CategoriesProduct model, List<decimal> IdPro, List<string> CodePro, List<string> NamePro, List<string> Unit, List<string> Description)
        {
            try
            {
                var cate = db.CategoriesProducts.FirstOrDefault(e => e.ID == model.ID);
                cate.Code = model.Code;
                cate.Name = model.Name;
                db.SaveChanges();
                if(IdPro != null)
                {
                    for (int i = 0; i < IdPro.Count; i++)
                    {
                        decimal ProId = IdPro[i];
                        string ProCode = CodePro[i];
                        string ProName = NamePro[i];
                        string ProUnit = Unit[i];
                        string ProDes = Description[i];
                        var datapro = db.Products.FirstOrDefault(e => e.ID == ProId);
                        datapro.Code = ProCode;
                        datapro.Name = ProName;
                        datapro.Unit = ProUnit;
                        datapro.Description = ProDes;
                        datapro.CategoriesProductID = cate.ID;
                        db.SaveChanges();
                    }
                    
                    return Json(new { status = 1, title = "", text = "Bạn đã cập nhật thông tin loại sản phẩm thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Bạn đã cập nhật thông tin loại sản phẩm không thành công", obj = "" }, JsonRequestBehavior.AllowGet);
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

        public ActionResult DeleteCateProduct(int Id)
        {
            CategoriesProduct cate = db.CategoriesProducts.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (cate != null)
                    {
                        var product = db.Products.Where(m => m.CategoriesProductID == Id);
                        foreach (var cus in product)
                        {
                            var ID = cus.ID;
                            var colorToUpdate = db.Products.Find(ID);
                            if (colorToUpdate != null)
                            {
                                colorToUpdate.CategoriesProductID = null;
                            }
                        }
                        db.SaveChanges();

                        db.CategoriesProducts.Remove(cate);
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

        // kích thước

        public ActionResult ListSize(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 10;
            }
            List<Size> size = db.Sizes.ToList();

            Dictionary<decimal, decimal> ProductCountsBySizeID = new Dictionary<decimal, decimal>();

            // Đếm số lượng sản phẩm cho từng Product
            foreach (var pro in size)
            {
                int proCount = db.ProductDetails.Count(c => c.SizeID == pro.ID);
                ProductCountsBySizeID.Add(pro.ID, proCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.ProductCountsBySizeID = ProductCountsBySizeID;
            return View(size.ToPagedList((int)page, (int)pageSize));
        }

        public ActionResult CreateSizeView()
        {

            return PartialView("_CreateSizeView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateSize(Size model)
        {

            try
            {
                var code = db.Sizes.FirstOrDefault(a => a.Code == model.Code);
                if (code == null)
                {
                    db.Sizes.Add(model);
                    await db.SaveChangesAsync();
                    decimal newMemberRatingId = model.ID;
                    return Json(new { status = 1, title = "", text = "Bạn đã thêm kích thước thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã kích thước này đã tốn tại!", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetSizeDetailView(int Id)
        {
            var dataSize = db.Sizes.FirstOrDefault(c => c.ID == Id);
            ViewBag.Size = dataSize;
            var dataPro = db.ProductDetails.Where(a => a.SizeID == Id).ToList();
            ViewBag.DataProductDetail = dataPro;
            var dataColor = db.Colors.ToList();
            ViewBag.Color = dataColor;
            return PartialView("_DetailSizeView", dataPro);
        }

        public ActionResult EditSizeView(int Id)
        {
            var dataSize = db.Sizes.FirstOrDefault(c => c.ID == Id);
            ViewBag.Size = dataSize;
            var dataPro = db.ProductDetails.Where(a => a.SizeID == Id).ToList();
            ViewBag.DataProductDetail = dataPro;
            var dataColor = db.Colors.ToList();
            ViewBag.Color = dataColor;
            return PartialView("_EditSizeView", dataPro);
        }
        public ActionResult EditSize(Size model)
        {
            try
            {
                    var size = db.Sizes.FirstOrDefault(e => e.ID == model.ID);
                    size.Code = model.Code;
                    size.Name = model.Name;
                    db.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Bạn đã cập nhật kích thước thành công", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DeleteSize(int Id)
        {
            Size size = db.Sizes.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (size != null)
                    {
                        var product = db.ProductDetails.Where(m => m.SizeID == Id);
                        foreach (var cus in product)
                        {
                            var ID = cus.ID;
                            var sizeToUpdate = db.ProductDetails.Find(ID);
                            if (sizeToUpdate != null)
                            {
                                sizeToUpdate.SizeID = null;
                            }
                        }
                        db.SaveChanges();

                        db.Sizes.Remove(size);
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