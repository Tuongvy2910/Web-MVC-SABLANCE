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
    
    public partial class OrderStatusDetail
    {
        public decimal OrderStatusID { get; set; }
        public decimal OrderID { get; set; }
        public System.DateTime ConfirmationDate { get; set; }
    
        public virtual Order Order { get; set; }
        public virtual OrderStatu OrderStatu { get; set; }
    }
}
