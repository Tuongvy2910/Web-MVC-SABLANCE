using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;

namespace SABLANCE.Areas.Admin.Models
{
    public class Inventory
    {
        public decimal ID { get; set; }
        public decimal ProductID { get; set; }
        public string ProductCode { get; set; }
        public double Quantity { get; set; }
        public double QuantityOrder { get; set; }
        public double QuantitySO { get; set; }
        public DateTime PODate { get; set; }
    }
    class Program
    {
        static void Main()
        {
            // Chuỗi kết nối đến cơ sở dữ liệu SQL Server
            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

            // Truy vấn SQL
            string sqlQuery = @"
            WITH LatestInventory AS ( SELECT
    ProductDetails.ID AS ProductID,
    COALESCE(InventoryReceiptDetails.Quantity, 0) AS ReceiptQuantity,
    InventoryReceipt.PODate,
    COALESCE(SUM(OrderDetails.Quantity), 0) AS SoldQuantity,
    COALESCE(SUM(InventoryIssueDetails.Quantity), 0) AS QuantitySO,
    ROW_NUMBER() OVER (PARTITION BY ProductDetails.ID ORDER BY InventoryReceipt.PODate DESC) AS RowNum
  FROM
    ProductDetails
    LEFT JOIN InventoryReceiptDetails ON InventoryReceiptDetails.ProductID = ProductDetails.ID
    LEFT JOIN InventoryReceipt ON InventoryReceipt.ID = InventoryReceiptDetails.ReceiptID
    LEFT JOIN OrderDetails ON ProductDetails.ID = OrderDetails.ProductID
    LEFT JOIN InventoryIssueDetails ON ProductDetails.ID = InventoryIssueDetails.ProductID
  GROUP BY
    ProductDetails.ID, InventoryReceiptDetails.Quantity, InventoryReceipt.PODate
)
SELECT ProductID, ReceiptQuantity - (SoldQuantity + QuantitySO) as Tonkho, PODate
FROM LatestInventory
WHERE RowNum = 1;";

            // Kết nối đến cơ sở dữ liệu và thực hiện truy vấn
            using (var connection = new SqlConnection(connectionString))
            {
                // Sử dụng thư viện Dapper để thực hiện truy vấn và ánh xạ kết quả vào danh sách Inventory
                List<Inventory> inventories = connection.Query<Inventory>(sqlQuery).AsList();

                // Hiển thị kết quả
                foreach (var inventory in inventories)
                {
                    Console.WriteLine($"ProductID: {inventory.ProductID}, Invenotry: {inventory.Quantity}, PODate: {inventory.PODate}");
                }
            }
        }
    }
}