using System.Text.Json.Serialization;

namespace LinQHomework.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [JsonIgnore]
        public virtual List<OrderItem> OrderItems { get; set; }
    }
}
