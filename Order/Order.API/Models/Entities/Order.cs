using Order.API.Models.Enums;

namespace Order.API.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int  BuyerId { get; set; }
        public OrderStatu OrderStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalPrice { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
