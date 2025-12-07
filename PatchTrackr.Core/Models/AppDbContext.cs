using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PatchTrackr.Core.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MCompany> MCompanies { get; set; }

    public virtual DbSet<MForm> MForms { get; set; }

    public virtual DbSet<MProject> MProjects { get; set; }

    public virtual DbSet<MUser> MUsers { get; set; }

    public virtual DbSet<MUserRight> MUserRights { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MCompany>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__M_Compan__2D971CACD5FAC52B");

            entity.ToTable("M_Companies");

            entity.HasIndex(e => e.CompanyName, "UQ__M_Compan__9BCE05DC4EBD5B4B").IsUnique();

            entity.Property(e => e.CompanyId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.AddressLine1).HasMaxLength(100);
            entity.Property(e => e.AddressLine2).HasMaxLength(100);
            entity.Property(e => e.AddressLine3).HasMaxLength(100);
            entity.Property(e => e.CompanyEmail)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.CompanyWebsite)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MForm>(entity =>
        {
            entity.HasKey(e => e.FormId).HasName("PK__M_Forms__FB05B7DD4B953FEE");

            entity.ToTable("M_Forms");

            entity.HasIndex(e => e.FormName, "UQ__M_Forms__81B78A2F54752818").IsUnique();

            entity.Property(e => e.ActionName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ControllerName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FormName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FormTitle).HasMaxLength(50);
        });

        modelBuilder.Entity<MProject>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__M_Projec__761ABEF057389688");

            entity.ToTable("M_Projects");

            entity.HasIndex(e => e.ProjectCode, "UQ__M_Projec__2F3A494828E5CEE6").IsUnique();

            entity.Property(e => e.ProjectId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedIp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.ProjectCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProjectDesc)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedIp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<MUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__M_Users__1788CC4C1CF7342A");

            entity.ToTable("M_Users");

            entity.HasIndex(e => e.UserName, "UQ__M_Users__C9F284562F8B2154").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedIp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.PhoneNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedIp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MUserRight>(entity =>
        {
            entity.HasKey(e => e.UserRightId).HasName("PK__M_UserRi__956097A247DD6692");

            entity.ToTable("M_UserRights");

            entity.Property(e => e.CreatedIp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.UpdatedIp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
