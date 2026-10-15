using API.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Domain.Entities
{
    public class Report : BaseEntity<int>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string SsrsReportPath { get; set; }
        public ICollection<ReportParameter> Parameters { get; set; } = new List<ReportParameter>();

    }
    internal class ReportModelBuilder : BaseModelBuilder<Report, int>
    {
        public override void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Report");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired(true);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsRequired(true);

            builder.Property(x => x.SsrsReportPath)
                .HasColumnName("ssrs_report_path")
                .HasMaxLength(255)
                .IsRequired(true);

            base.Configure(builder);

            builder.HasIndex(x => new { x.Id, x.DeletedOn })
                .IsUnique(true);
        }
    }
}
