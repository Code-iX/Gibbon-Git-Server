﻿// <auto-generated />
using System;
using Gibbon.Git.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Gibbon.Git.Server.Migrations
{
    [DbContext(typeof(GibbonGitServerContext))]
    partial class GibbonGitServerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("Gibbon.Git.Server.Data.Entities.Repository", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("Id");

                    b.Property<int>("AllowAnonymousPush")
                        .HasColumnType("INTEGER")
                        .HasColumnName("AllowAnonymousPush");

                    b.Property<bool>("Anonymous")
                        .HasColumnType("INTEGER")
                        .HasColumnName("Anonymous");

                    b.Property<bool>("AuditPushUser")
                        .HasColumnType("INTEGER")
                        .HasColumnName("AuditPushUser");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Description");

                    b.Property<string>("Group")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Group");

                    b.Property<string>("LinksRegex")
                        .HasColumnType("TEXT")
                        .HasColumnName("LinksRegex");

                    b.Property<string>("LinksUrl")
                        .HasColumnType("TEXT")
                        .HasColumnName("LinksUrl");

                    b.Property<bool>("LinksUseGlobal")
                        .HasColumnType("INTEGER")
                        .HasColumnName("LinksUseGlobal");

                    b.Property<byte[]>("Logo")
                        .HasColumnType("BLOB");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Name")
                        .UseCollation("NOCASE");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Repository", (string)null);
                });

            modelBuilder.Entity("Gibbon.Git.Server.Data.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("Id");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Name")
                        .UseCollation("NOCASE");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Role", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("a3139d2b-5a59-427f-bb2d-af251dce00e4"),
                            Description = "System administrator",
                            Name = "Administrator"
                        });
                });

            modelBuilder.Entity("Gibbon.Git.Server.Data.Entities.ServerSettingsEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AllowAnonymousPush")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AllowAnonymousRegistration")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AllowPushToCreate")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AllowUserRepositoryCreation")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DefaultLanguage")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsCommitAuthorAvatarVisible")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LinksRegex")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("LinksUrl")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("SiteCssUrl")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("SiteLogoUrl")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("SiteTitle")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ServerSettings", (string)null);
                });

            modelBuilder.Entity("Gibbon.Git.Server.Data.Entities.Team", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("Id");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Name")
                        .UseCollation("NOCASE");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Team", (string)null);
                });

            modelBuilder.Entity("Gibbon.Git.Server.Data.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("Id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Email");

                    b.Property<string>("GivenName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Password");

                    b.Property<string>("PasswordSalt")
                        .HasColumnType("TEXT")
                        .HasColumnName("PasswordSalt");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Surname");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnName("Username")
                        .UseCollation("NOCASE");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("User", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("3eb9995e-99e3-425a-b978-1409bdd61fb6"),
                            Email = "",
                            GivenName = "admin",
                            Password = "2dpBKPc2rPqPa03udauh6LUo4uNHFSNQZaH4P1BIkNizmUmuir/61Vgkr5MaXlr+bVWnefxQD1H1ciMEtEr/hQ==",
                            PasswordSalt = "/4fKgvYmp7iCSD7JJMPhrw==",
                            Surname = "",
                            Username = "admin"
                        });
                });

            modelBuilder.Entity("Gibbon.Git.Server.Data.Entities.UserSettingsEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DateFormat")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("DefaultHomePage")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("PreferredLanguage")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("PreferredThemeMode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<bool>("ReceiveEmailNotifications")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TimeZone")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("UserSettings", (string)null);
                });

            modelBuilder.Entity("TeamRepository_Permission", b =>
                {
                    b.Property<Guid>("Repository_Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Team_Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Repository_Id", "Team_Id");

                    b.HasIndex("Team_Id");

                    b.ToTable("TeamRepository_Permission");
                });

            modelBuilder.Entity("UserRepository_Administrator", b =>
                {
                    b.Property<Guid>("Repository_Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("User_Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Repository_Id", "User_Id");

                    b.HasIndex("User_Id");

                    b.ToTable("UserRepository_Administrator");
                });

            modelBuilder.Entity("UserRepository_Permission", b =>
                {
                    b.Property<Guid>("Repository_Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("User_Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Repository_Id", "User_Id");

                    b.HasIndex("User_Id");

                    b.ToTable("UserRepository_Permission");
                });

            modelBuilder.Entity("UserRole_InRole", b =>
                {
                    b.Property<Guid>("Role_Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("User_Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Role_Id", "User_Id");

                    b.HasIndex("User_Id");

                    b.ToTable("UserRole_InRole");

                    b.HasData(
                        new
                        {
                            Role_Id = new Guid("a3139d2b-5a59-427f-bb2d-af251dce00e4"),
                            User_Id = new Guid("3eb9995e-99e3-425a-b978-1409bdd61fb6")
                        });
                });

            modelBuilder.Entity("UserTeam_Member", b =>
                {
                    b.Property<Guid>("Team_Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("User_Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Team_Id", "User_Id");

                    b.HasIndex("User_Id");

                    b.ToTable("UserTeam_Member");
                });

            modelBuilder.Entity("TeamRepository_Permission", b =>
                {
                    b.HasOne("Gibbon.Git.Server.Data.Entities.Repository", null)
                        .WithMany()
                        .HasForeignKey("Repository_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Gibbon.Git.Server.Data.Entities.Team", null)
                        .WithMany()
                        .HasForeignKey("Team_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UserRepository_Administrator", b =>
                {
                    b.HasOne("Gibbon.Git.Server.Data.Entities.Repository", null)
                        .WithMany()
                        .HasForeignKey("Repository_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Gibbon.Git.Server.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("User_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UserRepository_Permission", b =>
                {
                    b.HasOne("Gibbon.Git.Server.Data.Entities.Repository", null)
                        .WithMany()
                        .HasForeignKey("Repository_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Gibbon.Git.Server.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("User_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UserRole_InRole", b =>
                {
                    b.HasOne("Gibbon.Git.Server.Data.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("Role_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Gibbon.Git.Server.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("User_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UserTeam_Member", b =>
                {
                    b.HasOne("Gibbon.Git.Server.Data.Entities.Team", null)
                        .WithMany()
                        .HasForeignKey("Team_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Gibbon.Git.Server.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("User_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
