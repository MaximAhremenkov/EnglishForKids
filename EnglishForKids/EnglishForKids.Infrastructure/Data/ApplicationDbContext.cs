using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EnglishForKids.Data.Entities;
using EnglishForKids.Data.Entities.Identity;

namespace EnglishForKids.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChildProfile> ChildProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<SentencePart> SentenceParts { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<EarnedAchievement> EarnedAchievements { get; set; }
        public DbSet<VirtualPet> VirtualPets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);  

            // Связь Parent -> Children
            modelBuilder.Entity<ChildProfile>()
                .HasOne(c => c.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(c => c.ParentId);

            // Остальные настройки...
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Topics)
                .HasForeignKey(t => t.CategoryId);

            modelBuilder.Entity<Rule>()
                .HasOne(r => r.Topic)
                .WithMany(t => t.Rules)
                .HasForeignKey(r => r.TopicId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Rule)
                .WithMany(r => r.Questions)
                .HasForeignKey(q => q.RuleId);

            modelBuilder.Entity<QuestionOption>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId);

            modelBuilder.Entity<SentencePart>()
                .HasOne(s => s.Question)
                .WithMany(q => q.SentenceParts)
                .HasForeignKey(s => s.QuestionId);

            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Child)
                .WithMany(c => c.Progresses)
                .HasForeignKey(p => p.ChildProfileId);

            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Topic)
                .WithMany(t => t.Progresses)
                .HasForeignKey(p => p.TopicId);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(u => u.Child)
                .WithMany(c => c.UserAnswers)
                .HasForeignKey(u => u.ChildProfileId);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(u => u.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(u => u.QuestionId);

            modelBuilder.Entity<TestResult>()
                .HasOne(t => t.Child)
                .WithMany(c => c.TestResults)
                .HasForeignKey(t => t.ChildProfileId);

            modelBuilder.Entity<TestResult>()
                .HasOne(t => t.Topic)
                .WithMany()
                .HasForeignKey(t => t.TopicId);

            modelBuilder.Entity<EarnedAchievement>()
                .HasOne(e => e.Child)
                .WithMany()
                .HasForeignKey(e => e.ChildProfileId);

            modelBuilder.Entity<EarnedAchievement>()
                .HasOne(e => e.Achievement)
                .WithMany(a => a.EarnedAchievements)
                .HasForeignKey(e => e.AchievementId);
        }
    }
}