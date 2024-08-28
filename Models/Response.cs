using AbataChallenge.Controllers;

namespace AbataChallenge.Models
{
    public class Response
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        public OrderResponse Order { get; set; }

    }
}
