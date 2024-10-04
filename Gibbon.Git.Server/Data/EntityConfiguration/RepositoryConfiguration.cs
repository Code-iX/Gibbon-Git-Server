using Gibbon.Git.Server.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gibbon.Git.Server.Data.EntityConfiguration;

public class RepositoryConfiguration : IEntityTypeConfiguration<Repository>
{
    public void Configure(EntityTypeBuilder<Repository> builder)
    {
        ((EntityTypeBuilder)builder).ToTable("Repository");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("Id");

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Group)
            .HasColumnName("Group")
            .HasMaxLength(255);

        builder.Property(t => t.Description)
            .HasColumnName("Description")
            .HasMaxLength(255);

        builder.Property(t => t.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(255)
            .UseCollation("NOCASE");
        
        builder.Property(t => t.Anonymous)
            .HasColumnName("Anonymous");
        
        builder.Property(t => t.AuditPushUser)
            .HasColumnName("AuditPushUser");
        
        builder.Property(t => t.AllowAnonymousPush)
            .HasColumnName("AllowAnonymousPush");
        
        builder.Property(t => t.LinksRegex)
            .HasColumnName("LinksRegex");
        
        builder.Property(t => t.LinksUrl)
            .HasColumnName("LinksUrl");
        
        builder.Property(t => t.LinksUseGlobal)
            .HasColumnName("LinksUseGlobal");

        builder.HasMany(t => t.Teams)
            .WithMany(t => t.Repositories)
            .UsingEntity<Dictionary<string, object>>(
                "TeamRepository_Permission",
                j => j.HasOne<Team>().WithMany().HasForeignKey("Team_Id").OnDelete(DeleteBehavior.Cascade), 
                j => j.HasOne<Repository>().WithMany().HasForeignKey("Repository_Id").OnDelete(DeleteBehavior.Cascade)); 

        builder.HasMany(t => t.Administrators)
            .WithMany(t => t.AdministratedRepositories)
            .UsingEntity<Dictionary<string, object>>(
                "UserRepository_Administrator",
                j => j.HasOne<User>().WithMany().HasForeignKey("User_Id").OnDelete(DeleteBehavior.Cascade), 
                j => j.HasOne<Repository>().WithMany().HasForeignKey("Repository_Id").OnDelete(DeleteBehavior.Cascade)); 

        builder.HasMany(t => t.Users)
            .WithMany(t => t.Repositories)
            .UsingEntity<Dictionary<string, object>>(
                "UserRepository_Permission",
                j => j.HasOne<User>().WithMany().HasForeignKey("User_Id").OnDelete(DeleteBehavior.Cascade), 
                j => j.HasOne<Repository>().WithMany().HasForeignKey("Repository_Id").OnDelete(DeleteBehavior.Cascade)); 
    }
}
