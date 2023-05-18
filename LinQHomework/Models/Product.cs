using System.Text.Json.Serialization;

namespace LinQHomework.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore]
        public virtual List<OrderItem> OrderItems { get; set; }
    }
}
