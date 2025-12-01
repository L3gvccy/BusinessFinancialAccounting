using System;
using System.Collections.Generic;
using System.Text;

namespace MAUI.Models
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = "";
        public string Units { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }


    public class ProductUpdateDTO
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = "";
        public string Units { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }


    public class ProductCreateDTO
    {
        public int Code { get; set; }
        public string Name { get; set; } = "";
        public string Units { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }


    public class ProductBriefDTO
    {
        public int Code { get; set; }
        public string Name { get; set; } = "";
        public string Units { get; set; } = "";
        public decimal Price { get; set; }
    }

}
