namespace BusinessFinancialAccounting.Models.DTO
{
    public class AddByCodeDTO
    {
        public int Code { get; set; }
        public decimal Quantity { get; set; }
    }
    public class ChangeQuantityDTO
    {
        public int Code { get; set; }
        public decimal Quantity { get; set; }
    }
    public class ProductToAddDTO
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Units { get; set; }
    }
    public class CartItemDTO
    {
        public int Code { get; set; }
        public decimal Quantity { get; set; }
    }

    public class PayRequestDTO
    {
        public string Method { get; set; }
        public List<CartItemDTO> Products { get; set; }
    }
}
