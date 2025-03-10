using dotSocialNetwork.Server.Models;
using Microsoft.EntityFrameworkCore;
namespace dotSocialNetwork.Server.Data;
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Dialog> Dialogs { get; set; }
    public DbSet<Message> Messages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dialog>()
                    .HasOne(d => d.User1)
                    .WithMany()
                    .HasForeignKey(d => d.User1Id)
                    .OnDelete(DeleteBehavior.Restrict); // Ограничить удаление

        // Отношение для User2
        modelBuilder.Entity<Dialog>()
            .HasOne(d => d.User2)
            .WithMany()
            .HasForeignKey(d => d.User2Id)
            .OnDelete(DeleteBehavior.Restrict); // Ограничить удаление

        // Опционально: настройка Messages
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Dialog)
            .WithMany()
            .HasForeignKey(m => m.DialogId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId);
    }
}