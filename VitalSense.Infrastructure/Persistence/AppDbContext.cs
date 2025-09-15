using Microsoft.EntityFrameworkCore;
using VitalSense.Domain.Entities;

namespace VitalSense.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<MealDay> MealDays { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }
        public DbSet<QuestionnaireQuestion> QuestionnaireQuestions { get; set; }
        public DbSet<QuestionnaireSubmission> QuestionnaireSubmissions { get; set; }
        public DbSet<QuestionnaireAnswer> QuestionnaireAnswers { get; set; }
    }
}