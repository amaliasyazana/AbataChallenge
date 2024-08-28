using System;

namespace AbataChallenge.Models
{
    /*public class CartItem
    {
        public int FoodID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }*/

    public class CartItem
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int FoodID { get; set; }
        public int Quantity { get; set; }
 
    }
}
