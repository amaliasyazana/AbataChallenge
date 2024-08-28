using AbataChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AbataChallenge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CartsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("AddToCart")]
        public IActionResult AddToCart([FromBody] CartItem cartItem)
        {
            if (cartItem == null || cartItem.FoodID <= 0 || cartItem.Quantity <= 0)
            {
                return BadRequest(new Response { StatusCode = 400, Message = "Invalid input data" });
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO Cart (UserID, FoodID, Quantity) VALUES (@UserID, @FoodID, @Quantity)", con);
                cmd.Parameters.AddWithValue("@UserID", cartItem.UserID);
                cmd.Parameters.AddWithValue("@FoodID", cartItem.FoodID);
                cmd.Parameters.AddWithValue("@Quantity", cartItem.Quantity);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new Response { StatusCode = 200, Message = "Product added to cart successfully" });
        }

        [HttpGet]
        [Route("GetCartItems/{userId}")]
        public IActionResult GetCartItems(int userId)
        {
            List<CartItem> cartItems = new List<CartItem>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("SELECT c.UserID, c.FoodID, c.Quantity FROM Cart c WHERE c.UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cartItems.Add(new CartItem
                    {

                        UserID = Convert.ToInt32(reader["UserID"]),
                        FoodID = Convert.ToInt32(reader["FoodID"]),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                    });
                }

                con.Close();
            }

            if (cartItems.Count == 0)
            {
                return NotFound(new Response { StatusCode = 404, Message = "No items found in the cart for this user." });
            }

            return Ok(cartItems);
        }
    }
}
