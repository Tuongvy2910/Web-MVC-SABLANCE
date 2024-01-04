using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SABLANCE.Areas.Admin.Models
{
    public class OrderDetailModel
    {
        public decimal OrderId { get; set; }
        public string OrderNo { get; set; }
        public string EmployeeName { get; set; }
        public decimal? EmployeeId    { get; set; }
        public string CustomerName { get; set; }
        public decimal? CustomerId { get; set; }
        public string ReceiptPhone { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalPayment { get; set; }
        public string LatestOrderStatus { get; set; }
    }
}