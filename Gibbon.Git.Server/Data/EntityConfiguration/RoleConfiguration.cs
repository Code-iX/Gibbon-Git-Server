using Gibbon.Git.Server.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gibbon.Git.Server.Data.EntityConfiguration;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role");

        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Name)  
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(255)
            .UseCollation("NOCASE");

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Description)
            .HasMaxLength(255);

        builder.Property(t => t.Description)
            .HasColumnName("Description");

        builder.HasMany(t => t.Users)
            .WithMany(t => t.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "UserRole_InRole",
                j => j.HasOne<User>().WithMany().HasForeignKey("User_Id"),
                j => j.HasOne<Role>().WithMany().HasForeignKey("Role_Id")
            )
            .HasData(new
            {
                User_Id = 1,
                Role_Id = 1
            });

        builder.HasData(new Role
        {
            Id = 1,
            Name = "Administrator",
            Description = "System administrator"
        });
    }
}
