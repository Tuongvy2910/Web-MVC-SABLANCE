using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SABLANCE.Areas.Admin.Models
{
    public class ProductDetailModel
    {
        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public decimal? CategoriesProductId { get; set; }
        public List<decimal?> ProductId { get; set; }
        public List<decimal?> SizeIds { get; set; }
        public List<decimal?> ColorIds { get; set; }
        public List<string> CodeProducts { get; set; }
        public List<string> Images { get; set; }
        public List<double> Prices { get; set; }
        public List<string> Status { get; set; }


    }
}