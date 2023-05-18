using System.Text.Json.Serialization;

namespace LinQHomework.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        [JsonIgnore]
        public virtual List<Order> Orders { get; set; }
    }
}
