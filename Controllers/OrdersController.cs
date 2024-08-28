using AbataChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AbataChallenge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrdersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("PlaceOrder")]
        public IActionResult PlaceOrder(int userId)
        {
            List<OrderItem> orderItems = new List<OrderItem>();
            int orderId;

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Retrieve cart items with food details from the database
                SqlCommand cmd = new SqlCommand("SELECT c.FoodID, c.Quantity, f.FoodName, f.Price FROM Cart c JOIN Food f ON c.FoodID = f.FoodID WHERE c.UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orderItems.Add(new OrderItem
                    {
                        FoodID = Convert.ToInt32(reader["FoodID"]),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                        Name = Convert.ToString(reader["FoodName"]), // Map FoodName to Name
                        Price = Convert.ToDecimal(reader["Price"])
                    });
                }

                con.Close();

                if (!orderItems.Any())
                {
                    return BadRequest(new Response { StatusCode = 400, ErrorMessage = "Cart is empty or invalid user ID" });
                }

                // Insert order into Orders table
                cmd = new SqlCommand("INSERT INTO Orders (UserID, OrderDate, Status, TotalPrice) OUTPUT INSERTED.OrderID VALUES (@UserID, @OrderDate, @Status, @TotalPrice)", con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Status", "Success");
                cmd.Parameters.AddWithValue("@TotalPrice", orderItems.Sum(item => item.Quantity * item.Price));

                con.Open();
                orderId = (int)cmd.ExecuteScalar();
                con.Close();

                // Insert order items into OrderItems table
                foreach (var item in orderItems)
                {
                    cmd = new SqlCommand("INSERT INTO OrderItems (OrderID, FoodID, Quantity, Price) VALUES (@OrderID, @FoodID, @Quantity, @Price)", con);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@FoodID", item.FoodID);
                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    cmd.Parameters.AddWithValue("@Price", item.Price);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                // Clear the cart after placing the order
                cmd = new SqlCommand("DELETE FROM Cart WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            // Fetch the placed order and its items to return in the response
            OrderResponse orderResponse = GetOrderDetails(orderId);

            return Ok(new Response
            {
                StatusCode = 200,
                Message = "Order placed successfully",
                Order = orderResponse
            });
        }

        private OrderResponse GetOrderDetails(int orderId)
        {
            OrderResponse orderResponse = new OrderResponse();
            List<OrderItem> orderItems = new List<OrderItem>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Get order details
                SqlCommand cmd = new SqlCommand("SELECT * FROM Orders WHERE OrderID = @OrderID", con);
                cmd.Parameters.AddWithValue("@OrderID", orderId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    orderResponse.OrderID = Convert.ToInt32(reader["OrderID"]);
                    orderResponse.UserID = Convert.ToInt32(reader["UserID"]);
                    orderResponse.OrderDate = Convert.ToDateTime(reader["OrderDate"]);
                    orderResponse.Status = Convert.ToString(reader["Status"]);
                    orderResponse.TotalPrice = Convert.ToDecimal(reader["TotalPrice"]);
                }

                reader.Close();

                // Get order items
                cmd = new SqlCommand("SELECT oi.FoodID, f.FoodName, oi.Quantity, oi.Price FROM OrderItems oi JOIN Food f ON oi.FoodID = f.FoodID WHERE oi.OrderID = @OrderID", con);
                cmd.Parameters.AddWithValue("@OrderID", orderId);

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orderItems.Add(new OrderItem
                    {
                        FoodID = Convert.ToInt32(reader["FoodID"]),
                        Name = Convert.ToString(reader["FoodName"]),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                        Price = Convert.ToDecimal(reader["Price"])
                    });
                }

                con.Close();
            }

            orderResponse.Items = orderItems;
            return orderResponse;
        }
    }

    public class OrderResponse
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
