using MyApp.Common;

namespace MyApp.Entities
{
    public class ProductImage : BaseEntity
    {
        public int Id { get; set; }

        // Foreign key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // File details
        public string FileName { get; set; }
        public string FileExtension { get; set; }

        // Image binary data
        public byte[] FileData { get; set; }
    }
}
