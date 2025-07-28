using System.ComponentModel.DataAnnotations;

namespace MemoryCaching.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
