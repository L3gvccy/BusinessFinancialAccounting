using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MAUI.Models
{
    public class CartItemDTO : INotifyPropertyChanged
    {
        private decimal _quantity;

        public int Code { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public decimal Price { get; set; }
        public decimal MaxQuantity { get; set; }

        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class AddByCodeDTO
    {
        public int Code { get; set; }
        public decimal Quantity { get; set; }
    }

    public class AddByCodeResponseDTO
    {
        public ProductToAddDTO Product { get; set; }
        public string Message { get; set; }
        public decimal MaxQty { get; set; }
    }

    public class ProductToAddDTO
    {
        public int Code { get; set; }
        public string Name { get; set; } = "";
        public string Units { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class ChangeQuantityDTO
    {
        public int Code { get; set; }
        public decimal Quantity { get; set; }
    }

    public class PayRequestDTO
    {
        public string Method { get; set; } = ""; // "cash" або "card"
        public List<PayProductDTO> Products { get; set; } = new();
    }

    public class PayProductDTO
    {
        public int Code { get; set; }        // код товару
        public decimal Quantity { get; set; } // кількість
    }


}
