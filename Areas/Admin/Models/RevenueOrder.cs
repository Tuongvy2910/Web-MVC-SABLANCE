using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SABLANCE.Areas.Admin.Models
{
    public class RevenueOrder
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int QuantityOrder { get; set; }
        public double TotalRevenue { get; set; }
    }
}