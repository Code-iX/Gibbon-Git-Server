using Gibbon.Git.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gibbon.Git.Server.Data.EntityConfiguration;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Team");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).
            HasColumnName("Id");

        builder.Property(t => t.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(255)
            .UseCollation("NOCASE");

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Description)
            .HasColumnName("Description");

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Description)
            .HasMaxLength(255);

        builder.HasMany(t => t.Users)
            .WithMany(t => t.Teams)
            .UsingEntity<Dictionary<string, object>>(
                "UserTeam_Member",
                j => j.HasOne<User>().WithMany().HasForeignKey("User_Id"),
                j => j.HasOne<Team>().WithMany().HasForeignKey("Team_Id")
            );
    }
}
