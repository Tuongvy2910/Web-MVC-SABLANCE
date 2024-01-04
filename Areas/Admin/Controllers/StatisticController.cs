using SABLANCE.Areas.Admin.Models;
using SABLANCE.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using PagedList;
using OfficeOpenXml;
using System.IO;
using System.Web.Hosting;
using ClosedXML.Excel;
using System.Data.Entity;
using System.Globalization;

namespace SABLANCE.Areas.Admin.Controllers
{
    public class StatisticController : Controller
    {
        DataSablancaShopEntities db = new DataSablancaShopEntities();
        // GET: Admin/Statistic
        public class Revenue
        {
            public string RevenueOrder { get; set; }
            public int RevenueSO { get; set; }
        }
        public ActionResult RevenueStatistics()
        {
            var dataOrders = db.Orders
     .Where(order => order.OrderStatusDetails
         .OrderByDescending(osd => osd.ConfirmationDate)
         .FirstOrDefault()
         .OrderStatusID == 4)
     .GroupBy(order => new { Month = order.OrderDate.Month, Year = order.OrderDate.Year })
     .Select(group => new
     {
         Month = group.Key.Month,
         Year = group.Key.Year,
         Revenue = group.Sum(order => order.TotalPayment)
     })
     .OrderBy(group => group.Year)
     .ThenBy(group => group.Month)
     .ToList();

            ViewBag.ChartDataCombined = dataOrders;

            var dataOrdersStatus = db.Orders
     .SelectMany(order => order.OrderStatusDetails
         .OrderByDescending(osd => osd.ConfirmationDate)
         .Take(1),
         (order, latestStatus) => new { Order = order, LatestStatus = latestStatus })
     .GroupBy(item => new { LatestStatusID = item.LatestStatus.OrderStatusID, LatestStatusName = item.LatestStatus.OrderStatu.Name })
     .Select(group => new
     {
         LatestStatusID = group.Key.LatestStatusID,
         LatestStatusName = group.Key.LatestStatusName,
         OrderCount = group.Count()
     })
     .ToList();

            ViewBag.OrderStatus = dataOrdersStatus;

            var dataInventoryIssues = db.InventoryIssues
    .GroupJoin(
        db.InventoryIssueDetails,
        iis => iis.ID,
        isl => isl.IssueID,
        (iis, issueDetails) => new { InventoryIssue = iis, InventoryIssueDetails = issueDetails }
    )
    .SelectMany(
        result => result.InventoryIssueDetails.DefaultIfEmpty(),
        (result, issueDetail) => new { result.InventoryIssue, IssueDetail = issueDetail }
    )
    .GroupBy(
        result => new { Month = result.InventoryIssue.SODate.Month, Year = result.InventoryIssue.SODate.Year, IssueID = result.IssueDetail.IssueID },
        result => result.IssueDetail
    )
    .Select(group => new
    {
        Month = group.Key.Month,
        Year = group.Key.Year,
        TotalIssue = group.Sum(detail => (detail.Quantity != null && detail.ProductDetail.Prices != null) ? detail.Quantity * detail.ProductDetail.Prices : 0)

    })
    .OrderBy(group => group.Year)
    .ThenBy(group => group.Month)
    .ToList();

            ViewBag.IssuseChart = dataInventoryIssues;

            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";
            string sqlQuery = @" SELECT
  MONTH(Orders.OrderDate) AS Month,
  YEAR(Orders.OrderDate) AS Year,
  Sum(Orders.TotalPayment) as TotalRevenue,
  COUNT(osd.OrderID) AS QuantityOrder-- Lấy tất cả thông tin từ bảng OrderDetails
FROM
  OrderStatusDetails osd
INNER JOIN OrderStatus os ON osd.OrderStatusID = os.ID
LEFT JOIN Orders ON Orders.ID = osd.OrderID
WHERE
  osd.ConfirmationDate = (
    SELECT
      MAX(ConfirmationDate)
    FROM
      OrderStatusDetails
    WHERE
      OrderID = osd.OrderID
  ) and osd.OrderStatusID = 4
Group by  os.Name, osd.OrderStatusID, os.ID ,MONTH(Orders.OrderDate),
  YEAR(Orders.OrderDate)
ORDER BY
  YEAR(Orders.OrderDate),
  MONTH(Orders.OrderDate);";

            string sqlstt = @" SELECT
  os.Name as Name,
  osd.OrderStatusID as IDStatus ,
  Sum(Orders.TotalPayment) as TotalOrder,
  COUNT(osd.OrderID) AS QuantityOrder-- Lấy tất cả thông tin từ bảng OrderDetails
FROM
  OrderStatusDetails osd
INNER JOIN OrderStatus os ON osd.OrderStatusID = os.ID
LEFT JOIN Orders ON Orders.ID = osd.OrderID
WHERE
  osd.ConfirmationDate = (
    SELECT
      MAX(ConfirmationDate)
    FROM
      OrderStatusDetails
    WHERE
      OrderID = osd.OrderID
  ) 
Group by  os.Name, osd.OrderStatusID, os.ID;";

            string sqlissuse = @"SELECT
    MONTH(InventoryIssue.SODate) AS Month,
    YEAR(InventoryIssue.SODate) AS Year,
	COALESCE(COUNT(InventoryIssue.ID), 0) as QuantitySO,
    COALESCE(SUM(InventoryIssueDetails.Quantity), 0) as QuantityProduct,
    COALESCE(SUM(InventoryIssueDetails.Quantity*ProductDetails.Prices), 0) as TotalIssue
FROM
    InventoryIssue
LEFT JOIN
    InventoryIssueDetails ON InventoryIssue.ID = InventoryIssueDetails.IssueID
LEFT JOIN
    ProductDetails  ON ProductDetails.ID = InventoryIssueDetails.ProductID
GROUP BY
    MONTH(InventoryIssue.SODate),
    YEAR(InventoryIssue.SODate)
ORDER BY
    YEAR(InventoryIssue.SODate) DESC, MONTH(InventoryIssue.SODate) DESC";

            string employee = @"SELECT
    Employee.ID as ID,
    Employee.FirstName as FistName,
    Employee.LastName as LastName,
	(
       SELECT
    COUNT(Orders.ID) 
FROM
    Orders
WHERE
    Orders.EmployeeID = Employee.ID
    ) AS QuantityOrder,
    (
       SELECT
    COUNT(InventoryIssue.ID) 
FROM
    InventoryIssue
WHERE
    InventoryIssue.EmployeeID = Employee.ID
    ) AS QuantitySO,
	(select COALESCE(Sum(Orders.TotalPayment),0) from Orders 
where Orders.EmployeeID = Employee.ID ) As RevenueOrder,
	(Select COALESCE(Sum(InventoryIssueDetails.Quantity*ProductDetails.Prices),0) 
From 
	InventoryIssue left join
	InventoryIssueDetails on InventoryIssue.ID = InventoryIssueDetails.IssueID inner join ProductDetails
	on InventoryIssueDetails.ProductID = ProductDetails.ID
Where
	InventoryIssue.EmployeeID = Employee.ID) As RevenueSO
FROM
    Employee
LEFT JOIN
    InventoryIssue ON Employee.ID = InventoryIssue.EmployeeID
GROUP BY
    Employee.ID, Employee.FirstName, Employee.LastName;
";

            using (var connection = new SqlConnection(connectionString))
            {
                List<RevenueOrder> revenues = connection.Query<RevenueOrder>(sqlQuery).AsList();
                List<RevenueInStatus> revenueInStatuses = connection.Query<RevenueInStatus>(sqlstt).AsList();
                ViewBag.Status = revenueInStatuses;

                List<RevenueIssuse> revenueIssuse = connection.Query<RevenueIssuse>(sqlissuse).AsList();
                ViewBag.Issuse = revenueIssuse;

                List<EmployeeRevenue> revenuempoloyee = connection.Query<EmployeeRevenue>(employee).AsList();
                ViewBag.Employee = revenuempoloyee;

                return View(revenues);
            } 

            
        }
        public ActionResult InventoryStatistic(int? page, int? pageSize)
        {
            int pageNumber = page ?? 1;
            int pageSizeValue = pageSize ?? 30;
            string connectionString = "Server =DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

            string sqlQuery = @"
WITH LatestInventory AS (
  SELECT
    ProductDetails.ID AS ProductID,
	ProductDetails.Code AS ProductCode,
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
    ProductDetails.ID, InventoryReceiptDetails.Quantity, InventoryReceipt.PODate, ProductDetails.Code
)
SELECT ProductID, ProductCode, ReceiptQuantity -(SoldQuantity + QuantitySO) as Quantity, SoldQuantity as QuantityOrder, QuantitySO , PODate
FROM LatestInventory
WHERE RowNum = 1;";

            using (var connection = new SqlConnection(connectionString))
            {
                List<Inventory> inventories = connection.Query<Inventory>(sqlQuery).AsList();
                IPagedList<Inventory> pagedInventories = inventories.ToPagedList(pageNumber, pageSizeValue);
                return View(pagedInventories);
            }
        }
        public ActionResult Search(string keyword)
        {
            List<Inventory> searchResults = GetSearchResults(keyword);

            return PartialView("_SearchResultPartialView", searchResults);
        }
        public List<Inventory> GetSearchResults(string keyword)
        {
            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";
           
            using (var connection = new SqlConnection(connectionString))
            {
                string sqlQuery = @"
WITH LatestInventory AS(
  SELECT
    ProductDetails.ID AS ProductID,
    ProductDetails.Code AS ProductCode,
    COALESCE(InventoryReceiptDetails.Quantity, 0) AS ReceiptQuantity,
    InventoryReceipt.PODate,
    COALESCE(SUM(OrderDetails.Quantity), 0) AS SoldQuantity,
    COALESCE(SUM(InventoryIssueDetails.Quantity), 0) AS QuantitySO,
    ROW_NUMBER() OVER(PARTITION BY ProductDetails.ID ORDER BY InventoryReceipt.PODate DESC) AS RowNum
  FROM
    ProductDetails
    LEFT JOIN InventoryReceiptDetails ON InventoryReceiptDetails.ProductID = ProductDetails.ID
    LEFT JOIN InventoryReceipt ON InventoryReceipt.ID = InventoryReceiptDetails.ReceiptID
    LEFT JOIN OrderDetails ON ProductDetails.ID = OrderDetails.ProductID
    LEFT JOIN InventoryIssueDetails ON ProductDetails.ID = InventoryIssueDetails.ProductID
  WHERE ProductDetails.Code LIKE '%' + @Keyword + '%'
  GROUP BY
    ProductDetails.ID, InventoryReceiptDetails.Quantity, InventoryReceipt.PODate, ProductDetails.Code
)
SELECT ProductID, ProductCode, ReceiptQuantity -(SoldQuantity + QuantitySO) as Quantity, SoldQuantity as QuantityOrder, QuantitySO , PODate
FROM LatestInventory
WHERE RowNum = 1;";

                // Thực hiện truy vấn và ánh xạ kết quả vào danh sách Inventory
                List<Inventory> searchResults = connection.Query<Inventory>(sqlQuery, new { Keyword = keyword }).AsList();

                return searchResults;
            }
        }
        public ActionResult GetProductDetailInventory(int Id)
        {

            var proDetail = db.ProductDetails.FirstOrDefault(a => a.ID == Id);
            ViewBag.ProductDetailsInventory = proDetail;
            return PartialView("_InventoryProductDetails");
        }
        public void ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

            string sqlQuery = @"
    WITH LatestInventory AS (
        SELECT
            ProductDetails.ID AS ProductID,
            ProductDetails.Code AS ProductCode,
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
            ProductDetails.ID, InventoryReceiptDetails.Quantity, InventoryReceipt.PODate, ProductDetails.Code
    )
    SELECT ProductID, ProductCode, ReceiptQuantity -(SoldQuantity + QuantitySO) as Quantity, SoldQuantity as QuantityOrder, QuantitySO , PODate
    FROM LatestInventory
    WHERE RowNum = 1;";

            using (var connection = new SqlConnection(connectionString))
            {
                List<Inventory> inventories = connection.Query<Inventory>(sqlQuery).AsList();

                var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("InventorySheet");

                var titleStyle = worksheet.Cells["A1:F1"].Style;
                titleStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                titleStyle.Font.Color.SetColor(System.Drawing.Color.Red);
                titleStyle.Font.Bold = true;
                titleStyle.Font.Size = 18; // Đặt kích thước font chữ

                // Căn giữa và cài đặt border cho tiêu đề
                titleStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                titleStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


                worksheet.Cells["A1:F1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO KẾT QUẢ TỒN KHO CỦA CÁC MẶT HÀNG";

                // Đặt font chữ và canh lề cho các ô cột dữ liệu
                for (int i = 2; i <= 6; i++)
                {
                    worksheet.Cells[2, i].Style.Font.Size = 14; // Đặt kích thước font chữ
                    worksheet.Cells[2, i].Style.Font.Bold = true; // In đậm
                    worksheet.Cells[2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                }

                // Đặt cỡ chữ và canh lề cho dữ liệu
                for (int i = 3; i <= inventories.Count + 3; i++)
                {
                    for (int j = 1; j <= 6; j++)
                    {
                        worksheet.Cells[i, j].Style.Font.Size = 12; // Đặt kích thước font chữ
                        worksheet.Cells[i, j].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                    }
                }
                worksheet.Cells["A3"].Value = "ID";
                worksheet.Cells["B3"].Value = "Mã mặt hàng";
                worksheet.Cells["C3"].Value = "Số lượng tồn kho";
                worksheet.Cells["D3"].Value = "Số lượng sản phẩm đã bán";
                worksheet.Cells["E3"].Value = "Số lượng sản phẩm xuất kho";
                worksheet.Cells["F3"].Value = "Ngày nhập kho";
                worksheet.Cells["A3:F3"].Style.Font.Size = 14; // Đặt kích thước font chữ
                worksheet.Cells["A3:F3"].Style.Font.Bold = true; // In đậm
                worksheet.Cells["A3:F3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                worksheet.Cells["A3:F3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // Boder

                // Ghi dữ liệu
                int row = 4;
                foreach (var inventory in inventories)
                {
                    string formattedDate = inventory.PODate.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cells[$"A{row}"].Value = inventory.ProductID;
                    worksheet.Cells[$"B{row}"].Value = inventory.ProductCode;
                    worksheet.Cells[$"C{row}"].Value = inventory.Quantity;
                    worksheet.Cells[$"D{row}"].Value = inventory.QuantityOrder;
                    worksheet.Cells[$"E{row}"].Value = inventory.QuantitySO;
                    worksheet.Cells[$"F{row}"].Value = formattedDate;

                    // Cài đặt border cho dữ liệu
                    worksheet.Cells[$"A{row}:F{row}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:F{row}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:F{row}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:F{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    row++;
                }

                // Thiết lập font chữ là Times New Roman
                worksheet.Cells.Style.Font.Name = "Times New Roman";

                // Lưu tệp Excel
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=InventoryReport.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }
        public ActionResult InventoryWithDateTime(DateTime datesearch)
        {
            try
            {
                string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";
                string search = @"WITH LatestInventory AS (
  SELECT
    ProductDetails.ID AS ProductID,
    ProductDetails.Code AS ProductCode,
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
    ProductDetails.ID, InventoryReceiptDetails.Quantity, InventoryReceipt.PODate, ProductDetails.Code
)
SELECT 
            ProductDetails.ID AS ProductID, 
            ProductDetails.Code AS ProductCode, 
            COALESCE(ReceiptQuantity - (SoldQuantity + QuantitySO), 0) as Quantity, 
            SoldQuantity AS QuantityOrder,
            QuantitySO AS QuantitySO,
            COALESCE(PODate, '') AS PODate
        FROM 
            ProductDetails
        LEFT JOIN LatestInventory ON ProductDetails.ID = LatestInventory.ProductID AND LatestInventory.RowNum = 1
        WHERE 
            PODate <= @datesearch OR PODate IS NULL; ";

                using (var connection = new SqlConnection(connectionString))
                {
                    List<Inventory> inventories = connection.Query<Inventory>(search, new { datesearch }).AsList();
                    return Json(new { status = 1, message = "Lấy dữ liệu thành công", data = inventories }, JsonRequestBehavior.AllowGet) ;
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = -1,
                    message = "Lỗi: " + ex.Message,
                    data = new List<Inventory>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public void ExportExcelRevenueOrder()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

            string sqlQuery = @" SELECT
  MONTH(Orders.OrderDate) AS Month,
  YEAR(Orders.OrderDate) AS Year,
  Sum(Orders.TotalPayment) as TotalRevenue,
  COUNT(osd.OrderID) AS QuantityOrder-- Lấy tất cả thông tin từ bảng OrderDetails
FROM
  OrderStatusDetails osd
INNER JOIN OrderStatus os ON osd.OrderStatusID = os.ID
LEFT JOIN Orders ON Orders.ID = osd.OrderID
WHERE
  osd.ConfirmationDate = (
    SELECT
      MAX(ConfirmationDate)
    FROM
      OrderStatusDetails
    WHERE
      OrderID = osd.OrderID
  ) and osd.OrderStatusID = 4
Group by  os.Name, osd.OrderStatusID, os.ID ,MONTH(Orders.OrderDate),
  YEAR(Orders.OrderDate)
ORDER BY
  YEAR(Orders.OrderDate),
  MONTH(Orders.OrderDate);";

            using (var connection = new SqlConnection(connectionString))
            {
                List<RevenueOrder> inventories = connection.Query<RevenueOrder>(sqlQuery).AsList();

                var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("RevenueOrderSheet");

                var titleStyle = worksheet.Cells["A1:C1"].Style;
                titleStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                titleStyle.Font.Color.SetColor(System.Drawing.Color.Red);
                titleStyle.Font.Bold = true;
                titleStyle.Font.Size = 18; // Đặt kích thước font chữ

                // Căn giữa và cài đặt border cho tiêu đề
                titleStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                titleStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


                worksheet.Cells["A1:C1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO THỐNG KÊ DOANH THU THEO ĐƠN ĐẶT HÀNG";

                // Đặt font chữ và canh lề cho các ô cột dữ liệu
                for (int i = 2; i <= 3; i++)
                {
                    worksheet.Cells[2, i].Style.Font.Size = 14; // Đặt kích thước font chữ
                    worksheet.Cells[2, i].Style.Font.Bold = true; // In đậm
                    worksheet.Cells[2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                }

                // Đặt cỡ chữ và canh lề cho dữ liệu
                for (int i = 3; i <= inventories.Count + 3; i++)
                {
                    for (int j = 1; j <= 3; j++)
                    {
                        worksheet.Cells[i, j].Style.Font.Size = 12; // Đặt kích thước font chữ
                        worksheet.Cells[i, j].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                    }
                }
                worksheet.Cells["A3"].Value = "Thời gian";
                worksheet.Cells["B3"].Value = "Số lượng đơn hàng";
                worksheet.Cells["C3"].Value = "Tổng doanh thu";
                worksheet.Cells["A3:C3"].Style.Font.Size = 14; // Đặt kích thước font chữ
                worksheet.Cells["A3:C3"].Style.Font.Bold = true; // In đậm
                worksheet.Cells["A3:C3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                worksheet.Cells["A3:C3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // Boder

                // Ghi dữ liệu
                int row = 4;
                foreach (var inventory in inventories)
                {
                    string formattedDate = inventory.Month+"/"+ inventory.Year;
                    worksheet.Cells[$"A{row}"].Value = formattedDate;
                    worksheet.Cells[$"B{row}"].Value = inventory.QuantityOrder;
                    worksheet.Cells[$"C{row}"].Value = inventory.TotalRevenue;

                    // Cài đặt border cho dữ liệu
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    row++;
                }

                // Thiết lập font chữ là Times New Roman
                worksheet.Cells.Style.Font.Name = "Times New Roman";

                // Lưu tệp Excel
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=RevenueOrderReport.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }

        public void ExportExcelRevenueStatus()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

            string sqlstt = @" SELECT
  os.Name as Name,
  osd.OrderStatusID as IDStatus ,
  Sum(Orders.TotalPayment) as TotalOrder,
  COUNT(osd.OrderID) AS QuantityOrder-- Lấy tất cả thông tin từ bảng OrderDetails
FROM
  OrderStatusDetails osd
INNER JOIN OrderStatus os ON osd.OrderStatusID = os.ID
LEFT JOIN Orders ON Orders.ID = osd.OrderID
WHERE
  osd.ConfirmationDate = (
    SELECT
      MAX(ConfirmationDate)
    FROM
      OrderStatusDetails
    WHERE
      OrderID = osd.OrderID
  ) 
Group by  os.Name, osd.OrderStatusID, os.ID;";

            using (var connection = new SqlConnection(connectionString))
            {
                List<RevenueInStatus> inventories = connection.Query<RevenueInStatus>(sqlstt).AsList();

                var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("RevenueStatusSheet");

                var titleStyle = worksheet.Cells["A1:C1"].Style;
                titleStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                titleStyle.Font.Color.SetColor(System.Drawing.Color.Red);
                titleStyle.Font.Bold = true;
                titleStyle.Font.Size = 18; // Đặt kích thước font chữ

                // Căn giữa và cài đặt border cho tiêu đề
                titleStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                titleStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


                worksheet.Cells["A1:C1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO THỐNG KÊ DOANH THU THEO TÌNH TRẠNG ĐƠN ĐẶT HÀNG";

                // Đặt font chữ và canh lề cho các ô cột dữ liệu
                for (int i = 2; i <= 3; i++)
                {
                    worksheet.Cells[2, i].Style.Font.Size = 14; // Đặt kích thước font chữ
                    worksheet.Cells[2, i].Style.Font.Bold = true; // In đậm
                    worksheet.Cells[2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                }

                // Đặt cỡ chữ và canh lề cho dữ liệu
                for (int i = 3; i <= inventories.Count + 3; i++)
                {
                    for (int j = 1; j <= 3; j++)
                    {
                        worksheet.Cells[i, j].Style.Font.Size = 12; // Đặt kích thước font chữ
                        worksheet.Cells[i, j].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                    }
                }
                worksheet.Cells["A3"].Value = "Tình trạng đơn hàng";
                worksheet.Cells["B3"].Value = "Số lượng đơn hàng";
                worksheet.Cells["C3"].Value = "Tổng doanh thu";
                worksheet.Cells["A3:3"].Style.Font.Size = 14; // Đặt kích thước font chữ
                worksheet.Cells["A3:CF3"].Style.Font.Bold = true; // In đậm
                worksheet.Cells["A3:C3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                worksheet.Cells["A3:C3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // Boder

                // Ghi dữ liệu
                int row = 4;
                foreach (var inventory in inventories)
                {
                    worksheet.Cells[$"A{row}"].Value = inventory.Name;
                    worksheet.Cells[$"B{row}"].Value = inventory.QuantityOrder;
                    worksheet.Cells[$"C{row}"].Value = inventory.TotalOrder.ToString("###,###.0");

                    // Cài đặt border cho dữ liệu
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:C{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    row++;
                }

                // Thiết lập font chữ là Times New Roman
                worksheet.Cells.Style.Font.Name = "Times New Roman";

                // Lưu tệp Excel
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=RevenueStatusReport.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }

        public void ExportExcelRevenueSO()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

            string sqlissuse = @"SELECT
    MONTH(InventoryIssue.SODate) AS Month,
    YEAR(InventoryIssue.SODate) AS Year,
	COALESCE(COUNT(InventoryIssue.ID), 0) as QuantitySO,
    COALESCE(SUM(InventoryIssueDetails.Quantity), 0) as QuantityProduct,
    COALESCE(SUM(InventoryIssueDetails.Quantity*ProductDetails.Prices), 0) as TotalIssue
FROM
    InventoryIssue
LEFT JOIN
    InventoryIssueDetails ON InventoryIssue.ID = InventoryIssueDetails.IssueID
LEFT JOIN
    ProductDetails  ON ProductDetails.ID = InventoryIssueDetails.ProductID
GROUP BY
    MONTH(InventoryIssue.SODate),
    YEAR(InventoryIssue.SODate)
ORDER BY
    YEAR(InventoryIssue.SODate) DESC, MONTH(InventoryIssue.SODate) DESC";

            using (var connection = new SqlConnection(connectionString))
            {
                List<RevenueIssuse> inventories = connection.Query<RevenueIssuse>(sqlissuse).AsList();

                var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("RevenueIssuseSheet");

                var titleStyle = worksheet.Cells["A1:D1"].Style;
                titleStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                titleStyle.Font.Color.SetColor(System.Drawing.Color.Red);
                titleStyle.Font.Bold = true;
                titleStyle.Font.Size = 18; // Đặt kích thước font chữ

                // Căn giữa và cài đặt border cho tiêu đề
                titleStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                titleStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


                worksheet.Cells["A1:D1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO THỐNG KÊ DOANH THU THEO PHIẾU XUẤT KHO";

                // Đặt font chữ và canh lề cho các ô cột dữ liệu
                for (int i = 2; i <= 4; i++)
                {
                    worksheet.Cells[2, i].Style.Font.Size = 14; // Đặt kích thước font chữ
                    worksheet.Cells[2, i].Style.Font.Bold = true; // In đậm
                    worksheet.Cells[2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                }

                // Đặt cỡ chữ và canh lề cho dữ liệu
                for (int i = 3; i <= inventories.Count + 3; i++)
                {
                    for (int j = 1; j <= 4; j++)
                    {
                        worksheet.Cells[i, j].Style.Font.Size = 12; // Đặt kích thước font chữ
                        worksheet.Cells[i, j].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                    }
                }
                worksheet.Cells["A3"].Value = "Thời gian";
                worksheet.Cells["B3"].Value = "Số lượng phiếu xuất kho";
                worksheet.Cells["C3"].Value = "Tổng sản phẩm xuất kho";
                worksheet.Cells["D3"].Value = "Tổng doanh thu xuất kho";
                worksheet.Cells["A3:D3"].Style.Font.Size = 14; // Đặt kích thước font chữ
                worksheet.Cells["A3:D3"].Style.Font.Bold = true; // In đậm
                worksheet.Cells["A3:D3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                worksheet.Cells["A3:D3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // Boder

                // Ghi dữ liệu
                int row = 4;
                foreach (var inventory in inventories)
                {
                    worksheet.Cells[$"A{row}"].Value = inventory.Month+"/"+inventory.Year;
                    worksheet.Cells[$"B{row}"].Value = inventory.QuantityProduct;
                    worksheet.Cells[$"C{row}"].Value = inventory.QuantityProduct;
                    worksheet.Cells[$"D{row}"].Value = inventory.TotalIssue.ToString("###,###.0");

                    // Cài đặt border cho dữ liệu
                    worksheet.Cells[$"A{row}:D{row}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:D{row}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:D{row}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:D{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    row++;
                }

                // Thiết lập font chữ là Times New Roman
                worksheet.Cells.Style.Font.Name = "Times New Roman";

                // Lưu tệp Excel
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=RevenueIssuseReport.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }

        public void ExportExcelRevenueEmployee()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";

            string employee = @"SELECT
    Employee.ID as ID,
    Employee.FirstName as FistName,
    Employee.LastName as LastName,
	(
       SELECT
    COUNT(Orders.ID) 
FROM
    Orders
WHERE
    Orders.EmployeeID = Employee.ID
    ) AS QuantityOrder,
    (
       SELECT
    COUNT(InventoryIssue.ID) 
FROM
    InventoryIssue
WHERE
    InventoryIssue.EmployeeID = Employee.ID
    ) AS QuantitySO,
	(select COALESCE(Sum(Orders.TotalPayment),0) from Orders 
where Orders.EmployeeID = Employee.ID ) As RevenueOrder,
	(Select COALESCE(Sum(InventoryIssueDetails.Quantity*ProductDetails.Prices),0) 
From 
	InventoryIssue left join
	InventoryIssueDetails on InventoryIssue.ID = InventoryIssueDetails.IssueID inner join ProductDetails
	on InventoryIssueDetails.ProductID = ProductDetails.ID
Where
	InventoryIssue.EmployeeID = Employee.ID) As RevenueSO
FROM
    Employee
LEFT JOIN
    InventoryIssue ON Employee.ID = InventoryIssue.EmployeeID
GROUP BY
    Employee.ID, Employee.FirstName, Employee.LastName;
";

            using (var connection = new SqlConnection(connectionString))
            {
                List<EmployeeRevenue> inventories = connection.Query<EmployeeRevenue>(employee).AsList();

                var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("RevenueEmployeeSheet");

                var titleStyle = worksheet.Cells["A1:H1"].Style;
                titleStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                titleStyle.Font.Color.SetColor(System.Drawing.Color.Red);
                titleStyle.Font.Bold = true;
                titleStyle.Font.Size = 18; // Đặt kích thước font chữ

                // Căn giữa và cài đặt border cho tiêu đề
                titleStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                titleStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


                worksheet.Cells["A1:H1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO THỐNG KÊ DOANH THU THEO TỪNG NHÂN VIÊN";

                // Đặt font chữ và canh lề cho các ô cột dữ liệu
                for (int i = 2; i <= 7; i++)
                {
                    worksheet.Cells[2, i].Style.Font.Size = 14; // Đặt kích thước font chữ
                    worksheet.Cells[2, i].Style.Font.Bold = true; // In đậm
                    worksheet.Cells[2, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                }

                // Đặt cỡ chữ và canh lề cho dữ liệu
                for (int i = 3; i <= inventories.Count + 3; i++)
                {
                    for (int j = 1; j <= 7; j++)
                    {
                        worksheet.Cells[i, j].Style.Font.Size = 12; // Đặt kích thước font chữ
                        worksheet.Cells[i, j].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                    }
                }
                worksheet.Cells["A3"].Value = "ID";
                worksheet.Cells["B3"].Value = "Họ nhân viên";
                worksheet.Cells["C3"].Value = "Tên nhân viên";
                worksheet.Cells["D3"].Value = "Số lượng đơn đặt hàng";
                worksheet.Cells["E3"].Value = "Số lượng phiếu xuất kho";
                worksheet.Cells["F3"].Value = "Doanh thu từ đơn đặt hàng";
                worksheet.Cells["G3"].Value = "Doanh thu từ phiếu xuất kho";
                worksheet.Cells["H3"].Value = "Tổng doanh thu";
                worksheet.Cells["A3:H3"].Style.Font.Size = 14; // Đặt kích thước font chữ
                worksheet.Cells["A3:H3"].Style.Font.Bold = true; // In đậm
                worksheet.Cells["A3:H3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Căn giữa
                worksheet.Cells["A3:H3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // Boder

                // Ghi dữ liệu
                int row = 4;
                foreach (var inventory in inventories)
                {
                    worksheet.Cells[$"A{row}"].Value = inventory.ID;
                    worksheet.Cells[$"B{row}"].Value = inventory.LastName;
                    worksheet.Cells[$"C{row}"].Value = inventory.FistName;
                    worksheet.Cells[$"D{row}"].Value = inventory.QuantityOrder;
                    worksheet.Cells[$"E{row}"].Value = inventory.QuantitySO;
                    worksheet.Cells[$"F{row}"].Value = inventory.RevenueOrder.ToString("###,###.0");
                    worksheet.Cells[$"G{row}"].Value = inventory.RevenueSO.ToString("###,###.0");
                    double total = inventory.RevenueOrder + inventory.RevenueSO;
                    worksheet.Cells[$"H{row}"].Value = total.ToString("0.0");

                    // Cài đặt border cho dữ liệu
                    worksheet.Cells[$"A{row}:H{row}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:H{row}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:H{row}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A{row}:H{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    row++;
                }

                // Thiết lập font chữ là Times New Roman
                worksheet.Cells.Style.Font.Name = "Times New Roman";

                // Lưu tệp Excel
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=RevenueEmployeeReport.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }

        public ActionResult RevenueEmployeeWithDateTime(DateTime datestart, DateTime dateend)
        {
            try
            {
                string connectionString = "Server=DII\\MSSQLSERVER01;Database=DataSablanca;User Id=sablanca;Password=sablanca;";
                if (datestart != null && dateend != null)
                {
                   
                    string search = @"SELECT
    Employee.ID as ID,
    Employee.FirstName as FistName,
    Employee.LastName as LastName,
	(
       SELECT
    COUNT(Orders.ID) 
FROM
    Orders
WHERE
    Orders.EmployeeID = Employee.ID AND 
	Orders.OrderDate >= @datestart AND 
    Orders.OrderDate <= @dateend 
    ) AS QuantityOrder,
    (
       SELECT
    COUNT(InventoryIssue.ID) 
FROM
    InventoryIssue
WHERE
    InventoryIssue.EmployeeID = Employee.ID AND
    InventoryIssue.SODate >= @datestart AND 
    InventoryIssue.SODate <=  @dateend) AS QuantitySO,
	(select COALESCE(Sum(Orders.TotalPayment),0) from Orders 
where Orders.EmployeeID = Employee.ID AND
	Orders.OrderDate >= @datestart AND 
    Orders.OrderDate <=  @dateend) As RevenueOrder,
	(
	Select COALESCE(Sum(InventoryIssueDetails.Quantity*ProductDetails.Prices),0) 
From 
	InventoryIssue left join
	InventoryIssueDetails on InventoryIssue.ID = InventoryIssueDetails.IssueID inner join ProductDetails
	on InventoryIssueDetails.ProductID = ProductDetails.ID
Where
	InventoryIssue.EmployeeID = Employee.ID AND
    InventoryIssue.SODate >= @datestart AND 
    InventoryIssue.SODate <=  @dateend) As RevenueSO
FROM
    Employee
LEFT JOIN
    InventoryIssue ON Employee.ID = InventoryIssue.EmployeeID

GROUP BY
    Employee.ID, Employee.FirstName, Employee.LastName";

                    using (var connection = new SqlConnection(connectionString))
                    {
                        List<EmployeeRevenue> inventories = connection.Query<EmployeeRevenue>(search, new { datestart, dateend }).AsList();
                        return Json(new { status = 1, message = "Lấy dữ liệu thành công", data = inventories }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    string employee = @"SELECT
    Employee.ID as ID,
    Employee.FirstName as FistName,
    Employee.LastName as LastName,
	(
       SELECT
    COUNT(Orders.ID) 
FROM
    Orders
WHERE
    Orders.EmployeeID = Employee.ID
    ) AS QuantityOrder,
    (
       SELECT
    COUNT(InventoryIssue.ID) 
FROM
    InventoryIssue
WHERE
    InventoryIssue.EmployeeID = Employee.ID
    ) AS QuantitySO,
	(select COALESCE(Sum(Orders.TotalPayment),0) from Orders 
where Orders.EmployeeID = Employee.ID ) As RevenueOrder,
	(Select COALESCE(Sum(InventoryIssueDetails.Quantity*ProductDetails.Prices),0) 
From 
	InventoryIssue left join
	InventoryIssueDetails on InventoryIssue.ID = InventoryIssueDetails.IssueID inner join ProductDetails
	on InventoryIssueDetails.ProductID = ProductDetails.ID
Where
	InventoryIssue.EmployeeID = Employee.ID) As RevenueSO
FROM
    Employee
LEFT JOIN
    InventoryIssue ON Employee.ID = InventoryIssue.EmployeeID
GROUP BY
    Employee.ID, Employee.FirstName, Employee.LastName;
";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        List<EmployeeRevenue> revenuempoloyee = connection.Query<EmployeeRevenue>(employee).AsList();
                        return Json(new { status = 1, message = "Lấy dữ liệu thành công", data = revenuempoloyee }, JsonRequestBehavior.AllowGet);
                    }
                }
                
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = -1,
                    message = "Lỗi: " + ex.Message,
                    data = new List<EmployeeRevenue>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}