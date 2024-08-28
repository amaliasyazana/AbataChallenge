using AbataChallenge.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace AbataChallenge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public FoodsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllFoods")]
        public string GetFoods()
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Food", con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<Food> Foodlist = new List<Food>();
            Response response = new Response();
            if(dt.Rows.Count > 0)
            {
                for(int i=0; i<dt.Rows.Count; i++)
                {
                    Food food = new Food();
                    food.FoodID = Convert.ToInt32(dt.Rows[i]["FoodID"]);
                    food.Name = Convert.ToString(dt.Rows[i]["FoodName"]);
                    food.Price = Convert.ToDecimal(dt.Rows[i]["Price"]);
                    food.Description = Convert.ToString(dt.Rows[i]["Description"]);
                    Foodlist.Add(food);
                }
                
            }
            if(Foodlist.Count > 0)
            {
                return JsonConvert.SerializeObject(Foodlist);
            }
            else
            {
                response.StatusCode = 100;
                response.Message = "No data found";
                return JsonConvert.SerializeObject(response);
            }
        }

        [HttpGet]
        [Route("GetFoodById/{id}")]
        public string GetFoodById(int id)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Food WHERE FoodID = @id", con);
            da.SelectCommand.Parameters.AddWithValue("@id", id);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                Food food = new Food
                {
                    FoodID = Convert.ToInt32(dt.Rows[0]["FoodID"]),
                    Name = Convert.ToString(dt.Rows[0]["FoodName"]),
                    Price = Convert.ToDecimal(dt.Rows[0]["Price"]),
                    Description = Convert.ToString(dt.Rows[0]["Description"])
                };
                return JsonConvert.SerializeObject(food);
            }
            else
            {
                Response response = new Response
                {
                    StatusCode = 100,
                    Message = "No data found"
                };
                return JsonConvert.SerializeObject(response);
            }
        }

    }
}
