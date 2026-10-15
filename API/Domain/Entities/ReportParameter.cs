using API.Domain.Abstractions;
using API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Domain.Entities
{
    public class ReportParameter : BaseEntity<int>
    {
        //public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public ParameterType Type { get; set; }

        public bool IsRequired { get; set; }

        public string? DefaultValue { get; set; }

        public int ReportId { get; set; }
        public Report Report { get; set; } = null!;

        internal class ReportParameterModelBuilder : BaseModelBuilder<ReportParameter, int>
        {
            public override void Configure(EntityTypeBuilder<ReportParameter> builder)
            {
                builder.ToTable("ReportParameter");

                builder.Property(x => x.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired(true);

                builder.Property(x => x.DisplayName)
                    .HasColumnName("display_name")
                    .HasMaxLength(50)
                    .IsRequired(true);

                builder.Property(x => x.Type)
                    .HasColumnName("type")
                    .HasConversion<string>()
                    .IsRequired(true);

                builder.Property(x => x.IsRequired)
                    .HasColumnName("is_required")
                    .IsRequired(true);

                builder.Property(x => x.DefaultValue)
                   .HasColumnName("default_value")
                   .IsRequired(false);

                builder.HasOne(p => p.Report)
                    .WithMany(r => r.Parameters)
                    .HasForeignKey(p => p.ReportId)
                    .OnDelete(DeleteBehavior.Cascade);

                base.Configure(builder);

                builder.HasIndex(x => new { x.Id, x.DeletedOn })
                       .IsUnique(true);

            }
        }
    }
}
