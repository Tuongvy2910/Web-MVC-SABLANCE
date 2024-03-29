﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DataSablancaShopEntities : DbContext
    {
        public DataSablancaShopEntities()
            : base("name=DataSablancaShopEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CategoriesEmployee> CategoriesEmployees { get; set; }
        public virtual DbSet<CategoriesProduct> CategoriesProducts { get; set; }
        public virtual DbSet<CategoriesPromotion> CategoriesPromotions { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<InventoryIssue> InventoryIssues { get; set; }
        public virtual DbSet<InventoryIssueDetail> InventoryIssueDetails { get; set; }
        public virtual DbSet<InventoryReceipt> InventoryReceipts { get; set; }
        public virtual DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }
        public virtual DbSet<MemberRating> MemberRatings { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderStatu> OrderStatus { get; set; }
        public virtual DbSet<OrderStatusDetail> OrderStatusDetails { get; set; }
        public virtual DbSet<PointDetailed> PointDetaileds { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductDetail> ProductDetails { get; set; }
        public virtual DbSet<Promotion> Promotions { get; set; }
        public virtual DbSet<PromotionDetail> PromotionDetails { get; set; }
        public virtual DbSet<Size> Sizes { get; set; }
        public virtual DbSet<Slide> Slides { get; set; }
        public virtual DbSet<TopicPage> TopicPages { get; set; }
    }
}
