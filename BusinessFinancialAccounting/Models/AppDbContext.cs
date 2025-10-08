using Microsoft.EntityFrameworkCore;


namespace BusinessFinancialAccounting.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<FinancialOperation> FinancialOperations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ReceiptProduct> ReceiptProducts { get; set; }
        public DbSet<Receipt> Reciepts { get; set; }

    }
}
