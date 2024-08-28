namespace AbataChallenge.Models
{
    public class Order
    {
        public int UserID { get; set; }
        public List<OrderProduct> Products { get; set; }
    }

    public class OrderProduct
    {
        public int FoodID { get; set; }
        public int Quantity { get; set; }
    }
}
