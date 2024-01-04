using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SABLANCE.Areas.Admin.Models
{
    public class RevenueInStatus
    {
        public decimal IDStatus { get; set; }
        public string Name { get; set; }
        public int QuantityOrder { get; set; }
        public double TotalOrder { get; set; }
    }
}