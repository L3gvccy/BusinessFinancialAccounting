using System;
using System.Collections.Generic;
using System.Text;

namespace MAUI.Models
{
    public class ReceiptProductDTO
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Units { get; set; } = "";
    }

    public class ReceiptDTO
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string PaymentMethod { get; set; } = "";
        public List<ReceiptProductDTO> Products { get; set; } = new();
        public decimal Total => Products.Sum(p => p.TotalPrice);
    }

    public class ReportDTO
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal CashDeposits { get; set; }
        public decimal CardDeposits { get; set; }
        public decimal CashProfit { get; set; }
        public decimal CardProfit { get; set; }
        public decimal Tax { get; set; }
    }

    public class ReportDetailsDTO
    {
        public ReportDTO Report { get; set; } = new();
        public List<ReceiptDTO> Receipts { get; set; } = new();
    }

}
