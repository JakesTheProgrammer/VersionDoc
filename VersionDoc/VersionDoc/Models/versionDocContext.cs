using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VersionDoc.Models
{
    public partial class versionDocContext : DbContext
    {
        public versionDocContext()
        {
        }

        public versionDocContext(DbContextOptions<versionDocContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<File> File { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<SharedOwnership> SharedOwnership { get; set; }
        public virtual DbSet<UserDetail> UserDetail { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=(LocalDb)\\MSSQLLocalDB;Database=versionDoc;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);

                entity.HasIndex(e => e.UserId);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            modelBuilder.Entity<File>(entity =>
            {
                entity.Property(e => e.FileId)
                    .HasColumnName("fileId")
                    .ValueGeneratedNever();

                entity.Property(e => e.FileDirectory)
                    .IsRequired()
                    .HasColumnName("fileDirectory")
                    .HasMaxLength(512);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasColumnName("fileName")
                    .HasMaxLength(100);

                entity.Property(e => e.FilePermission).HasColumnName("filePermission");

                entity.Property(e => e.FileSize)
                    .IsRequired()
                    .HasColumnName("fileSize")
                    .HasMaxLength(50);

                entity.Property(e => e.FileType)
                    .IsRequired()
                    .HasColumnName("fileType")
                    .HasMaxLength(50);

                entity.Property(e => e.FileUploadDate)
                    .HasColumnName("fileUploadDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.File)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_File_userDetail");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.LogId).HasColumnName("logId");

                entity.Property(e => e.FileId).HasColumnName("fileId");

                entity.Property(e => e.LogDateTime)
                    .HasColumnName("logDateTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.LogSize)
                    .IsRequired()
                    .HasColumnName("logSize")
                    .HasMaxLength(50);

                entity.Property(e => e.LogUploader)
                    .IsRequired()
                    .HasColumnName("logUploader")
                    .HasMaxLength(256);

                entity.HasOne(d => d.File)
                    .WithMany(p => p.Log)
                    .HasForeignKey(d => d.FileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Log_File");
            });

            modelBuilder.Entity<SharedOwnership>(entity =>
            {
                entity.HasKey(e => new { e.UsersId, e.FilesId });

                entity.ToTable("sharedOwnership");

                entity.Property(e => e.UsersId).HasColumnName("usersId");

                entity.Property(e => e.FilesId).HasColumnName("filesId");

                entity.Property(e => e.SharedBy)
                    .HasColumnName("sharedBy")
                    .HasMaxLength(256);

                entity.HasOne(d => d.Files)
                    .WithMany(p => p.SharedOwnership)
                    .HasForeignKey(d => d.FilesId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sharedOwnership_File");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.SharedOwnership)
                    .HasForeignKey(d => d.UsersId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sharedOwnership_userDetail");
            });

            modelBuilder.Entity<UserDetail>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("userDetail");

                entity.HasIndex(e => e.LoginId)
                    .HasName("UQ__userDeta__1F5EF4CEE5069E0E")
                    .IsUnique();

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .ValueGeneratedNever();

                entity.Property(e => e.LoginId)
                    .IsRequired()
                    .HasColumnName("loginId");

                entity.Property(e => e.UserEmail)
                    .HasColumnName("userEmail")
                    .HasMaxLength(256);

                entity.Property(e => e.UserFirstName)
                    .HasColumnName("userFirstName")
                    .HasMaxLength(100);

                entity.Property(e => e.UserLastName)
                    .HasColumnName("userLastName")
                    .HasMaxLength(100);

                entity.HasOne(d => d.Login)
                    .WithOne(p => p.UserDetail)
                    .HasForeignKey<UserDetail>(d => d.LoginId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_userDetail_AspNetUsers");
            });
        }
    }
}
