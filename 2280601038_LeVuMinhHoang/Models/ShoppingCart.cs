namespace _2280601038_LeVuMinhHoang.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            if (item == null) return;

            var existingItem = Items?.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items?.Add(item);
            }
        }

        public void RemoveItem(int productId)
        {
            Items?.RemoveAll(i => i.ProductId == productId);
        }

        public decimal GetSubTotal()
        {
            return Items?.Sum(i => i.Price * i.Quantity) ?? 0;
        }

        public decimal CalculateTax(decimal subTotal, decimal taxRate = 0.1m)
        {
            return subTotal * taxRate;
        }

        public decimal CalculateTotal(decimal subTotal, decimal shippingFee, decimal tax)
        {
            return subTotal + shippingFee + tax;
        }

        public void Clear()
        {
            Items?.Clear();
        }
    }
}
