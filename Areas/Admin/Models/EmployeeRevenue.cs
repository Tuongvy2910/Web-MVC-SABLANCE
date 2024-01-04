using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SABLANCE.Areas.Admin.Models
{
    public class EmployeeRevenue
    {
        public decimal ID { get; set; }
        public string FistName { get; set; }
        public string LastName { get; set; }
        public int QuantityOrder { get; set; }
        public int QuantitySO { get; set; }
        public double RevenueOrder { get; set; }
        public double RevenueSO { get; set; }
    }
}