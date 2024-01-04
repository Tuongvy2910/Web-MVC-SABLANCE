using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SABLANCE.ViewModel
{
    public class ProductWithPromotion
    {
        public decimal ProductID { get; set; }
        public decimal ProductDetailID { get; set; }
        public decimal PromotionID { get; set; }
        public string Image { get; set; }
        public double Prices { get; set; }
        public double Discount { get; set; }
        public string Color { get; set; }

        public decimal SizeID { get; set; }
        public decimal ColorID { get; set; }
        public string Size { get; set; }
        public string ProductName { get; set; }
    }
}