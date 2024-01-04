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
    public class PromotionController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Admin/Promotion
        public ActionResult ListPromotion(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<Promotion> promotion = db.Promotions.ToList();

            var promotionProductCounts = db.PromotionDetails.GroupBy(pd => pd.PromotionID).ToDictionary(
         group => group.Key,
         group => group.Count()
     );

            ViewBag.PromotionProductCounts = promotionProductCounts;

            return View(promotion.ToPagedList((int)page, (int)pageSize));
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
        public ActionResult CreatePromotionView()
        {


            var dataPro = db.ProductDetails.ToList();
            ViewBag.ProductDatas = dataPro;

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

            ViewBag.ProductPromotionList = dataproduct;

            var dataCate = db.CategoriesPromotions.ToList();
            ViewBag.CatePromotion = dataCate;
            return PartialView("_CreatePromotionView");
        }
        [HttpPost]
        public ActionResult CreatePromotion(Promotion model, HttpPostedFileBase ImageData, List<decimal> productId, List<int> quantity, DateTime start, DateTime end)
        {
            if (ImageData != null && ImageData.ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(ImageData.FileName).ToLower();
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };

                if (allowedExtensions.Contains(fileExtension))
                {
                    string fileName = Path.GetFileName(ImageData.FileName);
                    string rootImage = "~/Image/Promotion/" + fileName;
                    ImageData.SaveAs(Server.MapPath(rootImage));
                    model.Image = Path.GetFileName(ImageData.FileName); ;
                }
                else
                {
                    ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).");
                    return Json(new { status = -1, title = "", text = "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                Promotion promotion = new Promotion();
                promotion.Code = model.Code;
                promotion.Title= model.Title;
                promotion.Detail = model.Detail;
                promotion.StartTime = start;
                promotion.EndTime = end;
                promotion.Image = model.Image;
                promotion.CategoriesPromotionID = model.CategoriesPromotionID;
                db.Promotions.Add(promotion);
                db.SaveChanges();


                if (productId != null)
                {

                    for (int i = 0; i < productId.Count; i++)
                    {
                        decimal proID = productId[i];
                        int Quantity = quantity[i];
                        var checkpro = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                        if (checkpro.StatusProduct == "Hết hàng")
                        {
                            var removeor = db.Promotions.FirstOrDefault(a => a.ID == promotion.ID);
                            db.Promotions.Remove(removeor);
                            db.SaveChanges();
                            return Json(new { status = -1, title = "", text = "Sản phẩm " + checkpro.Code + " hết hàng.", obj = "" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            PromotionDetail detail = new PromotionDetail();
                            detail.ProductDetailsID = proID;
                            detail.PromotionID = promotion.ID;
                            double originalValue = Quantity / 100.0; // Chia cho 100.0 để đảm bảo kết quả là số thực
                            double roundedValue = Math.Round(originalValue, 2);
                            detail.Discount = roundedValue;
                            db.PromotionDetails.Add(detail);
                            db.SaveChanges();
                        }

                    }
                }
               
                db.SaveChanges();



                return Json(new { status = 1, title = "", text = "Bạn đã tạo chương trình khuyến mãi mới thành công", obj = "" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult PromotionDetailView(int Id)
        {
            var pro = db.Promotions.Find(Id);
            ViewBag.PromotionDetail = pro;

            var ordetail = db.PromotionDetails.Where(a => a.PromotionID == Id).ToList();
            ViewBag.PromotionList = ordetail;

            return PartialView("_PromotionDetailView", pro);
        }

        [HttpGet]
        public ActionResult EditPromotionView(int Id)
        {
            var order = db.Promotions.Find(Id);
            ViewBag.PromotionEdit = order;
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

            ViewBag.ProductEdit = dataproduct;

            var ordetail = db.PromotionDetails.Where(a => a.PromotionID == Id).ToList();
            ViewBag.ProEdit = ordetail;

            var dataPro = db.ProductDetails.ToList();
            ViewBag.LstProduct = dataPro;

            var dataCate = db.CategoriesPromotions.ToList();
            ViewBag.CatePromotion = dataCate;

            // Gán dữ liệu vào ViewBag

            return PartialView("_EditPromotionView");
        }
        [HttpPost]
        public ActionResult EditPromotion(Promotion model, HttpPostedFileBase ImageData, List<decimal> productId, List<int> quantity, DateTime end)
        {
            if (ImageData != null && ImageData.ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(ImageData.FileName).ToLower();
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };

                if (allowedExtensions.Contains(fileExtension))
                {
                    string fileName = Path.GetFileName(ImageData.FileName);
                    string rootImage = "~/Image/Promotion/" + fileName;
                    ImageData.SaveAs(Server.MapPath(rootImage));
                    model.Image = Path.GetFileName(ImageData.FileName); ;
                }
                else
                {
                    ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).");
                    return Json(new { status = -1, title = "", text = "Định dạng tệp không được hỗ trợ. Vui lòng chọn ảnh (png, jpg, jpeg).", obj = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                var promotion = db.Promotions.FirstOrDefault(e => e.ID == model.ID);
                if (model.Code == promotion.Code.Trim())
                {
                    promotion.Title = model.Title;
                    promotion.Detail = model.Detail;
                    promotion.EndTime = end;
                    promotion.Image = model.Image;
                    promotion.CategoriesPromotionID = model.CategoriesPromotionID;
                    db.SaveChanges();


                    decimal newCateEmployeeId = model.ID;
                    var prodetail = db.PromotionDetails.Where(a => a.PromotionID == model.ID).ToList();
                    if(prodetail.Count > 0)
                    {
                        if (productId != null)
                        {
                            db.PromotionDetails.RemoveRange(prodetail);
                            db.SaveChanges();

                            for (int i = 0; i < productId.Count; i++)
                            {
                                decimal proID = productId[i];
                                int Quantity = quantity[i];
                                var checkpro = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                                if (checkpro.StatusProduct == "Hết hàng")
                                {
                                    return Json(new { status = -1, title = "", text = "Sản phẩm " + checkpro.Code + " hết hàng.", obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    PromotionDetail detail = new PromotionDetail();
                                    detail.ProductDetailsID = proID;
                                    detail.PromotionID = promotion.ID;
                                    double originalValue = Quantity / 100.0; // Chia cho 100.0 để đảm bảo kết quả là số thực
                                    double roundedValue = Math.Round(originalValue, 2);
                                    detail.Discount = roundedValue;
                                    db.PromotionDetails.Add(detail);
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
                                var checkpro = db.ProductDetails.FirstOrDefault(a => a.ID == proID);
                                if (checkpro.StatusProduct == "Hết hàng")
                                {
                                    return Json(new { status = -1, title = "", text = "Sản phẩm " + checkpro.Code + " hết hàng.", obj = "" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    PromotionDetail detail = new PromotionDetail();
                                    detail.ProductDetailsID = proID;
                                    detail.PromotionID = promotion.ID;
                                    double originalValue = Quantity / 100.0; // Chia cho 100.0 để đảm bảo kết quả là số thực
                                    double roundedValue = Math.Round(originalValue, 2);
                                    detail.Discount = roundedValue;
                                    db.PromotionDetails.Add(detail);
                                    db.SaveChanges();
                                }

                            }
                        }
                    }
                    

                    db.SaveChanges();


                    return Json(new { status = 1, title = "", text = "Cập nhật chương trình khuyến mãi thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã số khuyến mãi không đúng, cập nhật khuyến mãi thất bại", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DeletePromotion(int Id)
        {
            Promotion pro = db.Promotions.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (pro != null)
                    {
                        var detail = db.PromotionDetails.Where(m => m.PromotionID == Id);
                        db.PromotionDetails.RemoveRange(detail);
                        db.SaveChanges();

                        db.Promotions.Remove(pro);
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

        //danh mục chương trình khuyến mãi
        public ActionResult ListCatePromotion(int? page, int? pageSize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 15;
            }
            List<CategoriesPromotion> catepro = db.CategoriesPromotions.ToList();

            // Tạo một Dictionary để lưu trữ số lượng khách hàng cho từng MemberID
            Dictionary<decimal, decimal> PromtionCountsByCatePromotion= new Dictionary<decimal, decimal>();

            // Đếm số lượng khách hàng cho từng MemberID
            foreach (var pro in catepro)
            {
                int proCount = db.Promotions.Count(c => c.CategoriesPromotionID == pro.ID);
                PromtionCountsByCatePromotion.Add(pro.ID, proCount);
            }

            // Lưu Dictionary vào ViewBag
            ViewBag.PromtionCountsByCatePromotion = PromtionCountsByCatePromotion;

            return View(catepro.ToPagedList((int)page, (int)pageSize));

        }
        [HttpGet]
        public ActionResult CreateCatePromotionView()
        {

            return PartialView("_CreateCatePromotionView");
        }
        [HttpPost]
        public async Task<ActionResult> CreateCatePromotion(CategoriesPromotion model)
        {

            try
            {
                var code = db.CategoriesPromotions.FirstOrDefault(a => a.Code == model.Code);
                if (code == null)
                {
                    db.CategoriesPromotions.Add(model);
                    await db.SaveChangesAsync();
                    decimal newMemberRatingId = model.ID;
                    return Json(new { status = 1, title = "", text = "Bạn đã thêm danh mục chương trình khuyến mãi thành công", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Mã danh mục chương trình khuyến mãi này đã tốn tại!", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DetailCatePromotionView(int Id)
        {
            var dataSize = db.CategoriesPromotions.FirstOrDefault(c => c.ID == Id);
            ViewBag.CatePromotion = dataSize;
            var dataPro = db.Promotions.Where(a => a.CategoriesPromotionID == Id).ToList();
            ViewBag.DataPromotionDetail = dataPro;


            var promotionProductCounts = db.PromotionDetails.GroupBy(pd => pd.PromotionID).ToDictionary(
         group => group.Key,
         group => group.Count()
     );

            ViewBag.PromotionProCounts = promotionProductCounts;

            return PartialView("_DetailCatePromotionView", dataPro);
        }

        public ActionResult EditCatePromotionView(int Id)
        {
            var dataSize = db.CategoriesPromotions.FirstOrDefault(c => c.ID == Id);
            ViewBag.CatePromotionData = dataSize;
            var dataPro = db.Promotions.Where(a => a.CategoriesPromotionID == Id).ToList();
            ViewBag.DataProductDetail = dataPro;

            var promotionProductCounts = db.PromotionDetails.GroupBy(pd => pd.PromotionID).ToDictionary(
         group => group.Key,
         group => group.Count()
     );

            ViewBag.PromotionDetailProCounts = promotionProductCounts;
            return PartialView("_EditCatePromotionView", dataPro);
        }
        public ActionResult DeletePromotionDetails(int Id)
        {
            Promotion pro = db.Promotions.FirstOrDefault(x => x.ID == Id);
            if (pro != null)
            {
                var promotion = db.PromotionDetails.Where(od => od.PromotionID == Id);
                db.PromotionDetails.RemoveRange(promotion);
                db.SaveChanges();
                
                db.Promotions.Remove(pro);
                db.SaveChanges();
                return Json(new { status = 1, message = "Success", text = "Xóa chương trình khuyến mãi thành công" });
            }
            else
            {
                return Json(new { status = 0, message = "Customer not found", text = "Không thể xóa chương trình khuyến mãi này, vui lòng thử lại." });
            }
        }
        public ActionResult EditCatePromotion(CategoriesPromotion model)
        {
            try
            {
                var cate = db.CategoriesPromotions.FirstOrDefault(e => e.ID == model.ID);
                if(cate != null)
                {
                    cate.Code = model.Code;
                    cate.Name = model.Name;
                    db.SaveChanges();

                    return Json(new { status = 1, title = "", text = "Bạn đã cập nhật danh mục chương trình khuyến mãi thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, title = "", text = "Cập nhật danh mục chương trình khuyến mãi không thành công.", obj = "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DeleteCatePromotion(int Id)
        {
            CategoriesPromotion cate = db.CategoriesPromotions.FirstOrDefault(x => x.ID == Id);
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (cate != null)
                    {
                        var promotion = db.Promotions.Where(x => x.CategoriesPromotionID == Id);
                        foreach (var pro in promotion)
                        {
                            var ID = pro.ID;
                            var cateProToUpdate = db.Promotions.Find(ID);
                            if (cateProToUpdate != null)
                            {
                                cateProToUpdate.CategoriesPromotionID = null;
                            }
                            else
                            {
                                db.SaveChanges();
                            }
                        }
                        

                        db.CategoriesPromotions.Remove(cate);
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

        [HttpGet]
        public ActionResult CreatePromotionDetailView(int Id)
        {

            var dataPro = db.ProductDetails.ToList();
            ViewBag.ProductDataDetail = dataPro;

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

            ViewBag.ProductPromotionDetailList = dataproduct;

            var dataCate = db.CategoriesPromotions.FirstOrDefault(a=> a.ID == Id);
            ViewBag.CatePromotionId = dataCate;
            return PartialView("_CreatePromotionDetailView");
        }
    }
}