using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Models;

namespace SafetyCopilot.API.Data;

public class SafetyDbContext : DbContext
{
    public SafetyDbContext(
        DbContextOptions<SafetyDbContext> options)
        : base(options)
    {
    }


    public DbSet<User> Users
        => Set<User>();

    public DbSet<Project> Projects
        => Set<Project>();

    public DbSet<RequirementDocument>
        RequirementDocuments
        => Set<RequirementDocument>();

    public DbSet<Requirement> Requirements
        => Set<Requirement>();

    public DbSet<HumanClassification>
        HumanClassifications
        => Set<HumanClassification>();

    public DbSet<AiClassification>
        AiClassifications
        => Set<AiClassification>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        ConfigureUser(modelBuilder);

        ConfigureProject(modelBuilder);

        ConfigureRequirementDocument(
            modelBuilder);

        ConfigureRequirement(
            modelBuilder);

        ConfigureHumanClassification(
            modelBuilder);

        ConfigureAiClassification(
            modelBuilder);
    }


    private static void ConfigureUser(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(
            entity =>
            {
                entity.ToTable(
                    "tblUser");

                entity.HasKey(
                    x => x.Id);


                entity.Property(
                        x => x.Email)
                    .HasMaxLength(255)
                    .IsRequired();


                entity.Property(
                        x => x.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();


                entity.Property(
                        x => x.DisplayName)
                    .HasMaxLength(200);


                entity.Property(
                        x =>
                            x.OpenAiApiKeyEncrypted)
                    .HasColumnType(
                        "nvarchar(max)");


                entity.HasIndex(
                        x => x.Email)
                    .IsUnique();
            });
    }


    private static void ConfigureProject(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(
            entity =>
            {
                entity.ToTable(
                    "tblProject");

                entity.HasKey(
                    x => x.Id);


                entity.Property(
                        x => x.Name)
                    .HasMaxLength(200)
                    .IsRequired();


                entity.Property(
                        x => x.Description)
                    .HasMaxLength(2000);


                entity.HasOne(
                        x => x.User)
                    .WithMany(
                        x => x.Projects)
                    .HasForeignKey(
                        x => x.UserId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });
    }


    private static void
        ConfigureRequirementDocument(
            ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<RequirementDocument>(
                entity =>
                {
                    entity.ToTable(
                        "tblRequirementDocument");

                    entity.HasKey(
                        x => x.Id);


                    entity.Property(
                            x => x.FileName)
                        .HasMaxLength(500)
                        .IsRequired();


                    entity.Property(
                            x => x.ContentType)
                        .HasMaxLength(100)
                        .IsRequired();


                    entity.Property(
                            x => x.PdfContent)
                        .HasColumnType(
                            "varbinary(max)")
                        .IsRequired();


                    entity.Property(
                            x => x.ExtractedText)
                        .HasColumnType(
                            "nvarchar(max)");


                    entity.HasOne(
                            x => x.Project)
                        .WithMany(
                            x =>
                                x.RequirementDocuments)
                        .HasForeignKey(
                            x => x.ProjectId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
    }


    private static void
        ConfigureRequirement(
            ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Requirement>(
                entity =>
                {
                    entity.ToTable(
                        "tblRequirement");

                    entity.HasKey(
                        x => x.Id);


                    entity.Property(
                            x =>
                                x.RequirementNumber)
                        .HasMaxLength(100);


                    entity.Property(
                            x =>
                                x.RequirementText)
                        .HasColumnType(
                            "nvarchar(max)")
                        .IsRequired();


                    entity.HasOne(
                            x => x.Document)
                        .WithMany(
                            x => x.Requirements)
                        .HasForeignKey(
                            x => x.DocumentId)
                        .OnDelete(
                            DeleteBehavior.Cascade);


                    entity.HasIndex(
                        x => new
                        {
                            x.DocumentId,
                            x.SequenceNumber
                        });
                });
    }


    private static void
        ConfigureHumanClassification(
            ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<HumanClassification>(
                entity =>
                {
                    entity.ToTable(
                        "tblHumanClassification");

                    entity.HasKey(
                        x => x.Id);


                    entity.HasOne(
                            x => x.Requirement)
                        .WithOne(
                            x =>
                                x.HumanClassification)
                        .HasForeignKey
                            <HumanClassification>(
                                x =>
                                    x.RequirementId)
                        .OnDelete(
                            DeleteBehavior.Cascade);


                    entity.HasOne(
                            x => x.User)
                        .WithMany(
                            x =>
                                x.HumanClassifications)
                        .HasForeignKey(
                            x => x.UserId)
                        .OnDelete(
                            DeleteBehavior.Restrict);


                    entity.HasIndex(
                            x =>
                                x.RequirementId)
                        .IsUnique();
                });
    }


    private static void
        ConfigureAiClassification(
            ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<AiClassification>(
                entity =>
                {
                    entity.ToTable(
                        "tblAiClassification");

                    entity.HasKey(
                        x => x.Id);


                    entity.Property(
                            x => x.ModelName)
                        .HasMaxLength(200);


                    entity.Property(
                            x => x.Explanation)
                        .HasColumnType(
                            "nvarchar(max)");


                    entity.HasOne(
                            x => x.Requirement)
                        .WithOne(
                            x =>
                                x.AiClassification)
                        .HasForeignKey
                            <AiClassification>(
                                x =>
                                    x.RequirementId)
                        .OnDelete(
                            DeleteBehavior.Cascade);


                    entity.HasIndex(
                            x =>
                                x.RequirementId)
                        .IsUnique();
                });
    }
}