namespace Order.API.ViewModals
{
    public class CreateOrderVM
    {
        public int BuyerId { get; set; }
        public List<OrderItemsVM> OrderItems { get; set; }
    }
}
