using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SABLANCE.Areas.Admin.Models
{
    public class RevenueIssuse
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int QuantitySO{ get; set; }
        public int QuantityProduct { get; set; }
        public double TotalIssue { get; set; }
    }
}