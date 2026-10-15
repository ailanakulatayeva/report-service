using API.Domain.Abstractions;
using API.Domain.Entities;
using API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static API.Domain.Entities.ReportParameter;

namespace API.Infrastructure.ModelBuilders
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportParameter> ReportParameters { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ReportModelBuilder());
            modelBuilder.ApplyConfiguration(new ReportParameterModelBuilder());
        }
    }

    //public partial interface IDbContext
    //{
    //    DbSet<Report> Report { get; set; }
    //    DbSet<ReportParameter> ReportParameter { get; set; }
    //    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    //}


}
