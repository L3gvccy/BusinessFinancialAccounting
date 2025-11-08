using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = default!;
        public string Units { get; set; } = default!;
        public decimal Quantity { get; set; }   
        public decimal Price { get; set; }
    }

    public class ProductCreateDTO
    {
        public int Code { get; set; }

        public string? Name { get; set; }

        public string? Units { get; set; }

        public decimal Quantity { get; set; }      

        public decimal Price { get; set; }
    }

    public class ProductUpdateDTO
    {
        public int Id { get; set; }
        public int Code { get; set; }

        public string? Name { get; set; }

        public string? Units { get; set; }

        public decimal Quantity { get; set; }       

        public decimal Price { get; set; }
    }

    public class ProductBriefDTO
    {
        public int Code { get; set; }
        public string Name { get; set; } = default!;
        public string Units { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
