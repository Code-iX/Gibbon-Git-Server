using Gibbon.Git.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gibbon.Git.Server.Data.EntityConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {         
        builder.ToTable("User");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(t => t.GivenName)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(255); 

        builder.Property(t => t.Surname)
            .HasColumnName("Surname")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Username)
            .HasColumnName("Username")
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(t => t.Username)
            .IsUnique();

        builder.Property(t => t.Password)
            .HasColumnName("Password");

        builder.Property(t => t.PasswordSalt)
            .HasColumnName("PasswordSalt");

        builder.Property(t => t.Email)
            .HasColumnName("Email");

        builder.Property(t => t.GivenName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Surname)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Username)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Password)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasData(new User
        {
            Id = 1,
            GivenName = "admin",
            Surname = "",
            Username = "admin",
            Password = "2dpBKPc2rPqPa03udauh6LUo4uNHFSNQZaH4P1BIkNizmUmuir/61Vgkr5MaXlr+bVWnefxQD1H1ciMEtEr/hQ==",
            PasswordSalt = "/4fKgvYmp7iCSD7JJMPhrw==",
            Email = ""
        });
    }
}
