using API.Domain.Abstractions;
using API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Domain.Abstractions
{
    public class BaseEntity : IBaseEntity
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public Guid? DeletedBy { get; set; }
        public RecordStatus StatusCode { get; set; } = RecordStatus.Active;
    }

    public class BaseEntity<TKey> : BaseEntity, IBaseEntity<TKey>
    {
        public TKey Id { get; set; }
    }
   

    internal class BaseModelBuilder<TType> : IEntityTypeConfiguration<TType> where TType : class, IBaseEntity
    {
        public static ValueConverter DateTimeConverter() => new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
            );

        public virtual void Configure(EntityTypeBuilder<TType> builder)
        {
            // On
            builder.Property(x => x.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired(true)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(DateTimeConverter());

            builder.Property(x => x.UpdatedOn)
                .HasColumnName("updated_on")
                .IsRequired(true)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(DateTimeConverter());

            builder.Property(x => x.DeletedOn)
                .HasColumnName("deleted_on")
                .IsRequired(false)
                .HasConversion(DateTimeConverter());

            // By
            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .IsRequired(true);

            builder.Property(x => x.UpdatedBy)
                .HasColumnName("updated_by")
                .IsRequired(true);

            builder.Property(x => x.DeletedBy)
                .HasColumnName("deleted_by");

            // Record Status
            builder.Property(x => x.StatusCode)
                .HasColumnName("status_code")
                .HasConversion<string>()
                .HasDefaultValue(RecordStatus.Active);
        }
    }

    internal class BaseModelBuilder<TType, TKey> : BaseModelBuilder<TType> where TType : class, IBaseEntity<TKey>
    {
        public override void Configure(EntityTypeBuilder<TType> builder)
        {
            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired(true);

            builder.HasKey(x => x.Id);

            base.Configure(builder);
        }
    }

}

