using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            SetPrimaryKey();
            SetProperties();
            SetTableAndColumnMappings();
        }


        private void SetTableAndColumnMappings()
        {
            ToTable("User");
            Property(t => t.Id).HasColumnName("Id");
            Property(t => t.GivenName).HasColumnName("Name");
            Property(t => t.Surname).HasColumnName("Surname");
            Property(t => t.Username).HasColumnName("Username");
            Property(t => t.Password).HasColumnName("Password");
            Property(t => t.PasswordSalt).HasColumnName("PasswordSalt");
            Property(t => t.Email).HasColumnName("Email");
        }

        private void SetProperties()
        {
            Property(t => t.GivenName)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Surname)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Username)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Password)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(255);
        }

        private void SetPrimaryKey()
        {
            HasKey(t => t.Id);
        }
    }
}
