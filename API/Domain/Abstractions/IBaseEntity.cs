using API.Domain.Enums;

namespace API.Domain.Abstractions
{
    public interface IBaseEntity
    {
        DateTime CreatedOn { get; set; }
        DateTime UpdatedOn { get; set; }
        DateTime? DeletedOn { get; set; }

        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public Guid? DeletedBy { get; set; }

        RecordStatus StatusCode { get; set; }
    }

    public interface IBaseEntity<TKey> : IBaseEntity
    { 
        TKey Id { get; set; }
    }
}
