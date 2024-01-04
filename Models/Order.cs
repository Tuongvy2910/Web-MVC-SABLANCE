//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SABLANCE.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
            this.OrderStatusDetails = new HashSet<OrderStatusDetail>();
            this.PointDetaileds = new HashSet<PointDetailed>();
        }
    
        public decimal ID { get; set; }
        public Nullable<decimal> EmployeeID { get; set; }
        public Nullable<decimal> CustomerID { get; set; }
        public Nullable<decimal> PointID { get; set; }
        public string OrderNo { get; set; }
        public string RecipientPhone { get; set; }
        public string DeliveryAddress { get; set; }
        public System.DateTime OrderDate { get; set; }
        public string Note { get; set; }
        public Nullable<double> TotalPayment { get; set; }
        public string PaymentMethods { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Employee Employee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual PointDetailed PointDetailed { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderStatusDetail> OrderStatusDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PointDetailed> PointDetaileds { get; set; }
    }
}
