namespace MyApp.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public DateTime? DeletedOn { get; set; }
        public int? DeletedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
