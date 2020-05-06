using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Synthesis.InProductTrainingService.InternalApi.Models;
using Synthesis.InProductTrainingService.InternalApi.Responses;
using Synthesis.InProductTrainingService.Models;

namespace Synthesis.InProductTrainingService.Data
{
    public class SynthesisDataContext : DbContext
    {
        private IConfiguration _configuration;

        public SynthesisDataContext(DbContextOptions<SynthesisDataContext> options, 
                                    IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _configuration.GetConnectionString("SynthesisDatabase");
            optionsBuilder.UseSqlServer(connectionString);
        }

        public virtual DbSet<ClientApplication> ClientApplications { get; set; }
        public virtual DbSet<InProductTrainingSubject> InProductTrainingSubjects { get; set; }
        public virtual DbSet<InProductTrainingView> InProductTrainingViews { get; set; }
        public virtual DbSet<ViewedWizard> ViewedWizards { get; set; }
        public virtual DbSet<InProductTrainingViewResponse> InProductTrainingViewResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientApplication>()
                .HasMany(e => e.InProductTrainingSubjects)
                .WithOne(e => e.ClientApplication)
                .IsRequired()
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserType>()
                .HasMany(e => e.InProductTrainingViews)
                .WithOne(e => e.UserType)
                .IsRequired()
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InProductTrainingSubject>()
                .HasMany(e => e.InProductTrainingViews)
                .WithOne(e => e.InProductTrainingSubject)
                .IsRequired()
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ViewedWizard>();
        }
    }
}