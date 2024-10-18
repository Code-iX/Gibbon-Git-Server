using System.ComponentModel.DataAnnotations;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Benchmarks;

public class SqliteBenchmark
{
    [Params(10, 100, 1000, 10000)]
    public int LoopCount;

    public class TestDbContext : DbContext
    {
        public DbSet<TestIntEntity> TestIntEntities { get; set; }
        public DbSet<TestGuidEntity> TestGuidEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("DataSource=benchmark.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestIntEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<TestGuidEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }

    public class TestIntEntity
    {
        public int Id { get; set; }
        public string? Data { get; set; }
    }

    public class TestGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string? Data { get; set; }
    }

    [Benchmark]
    public void InsertWithIntKey()
    {
        using var context = new TestDbContext();
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        for (int i = 0; i < LoopCount; i++)
        {
            context.TestIntEntities.Add(new TestIntEntity { Data = $"data_{i}" });
        }

        context.SaveChanges();
    }

    [Benchmark]
    public void InsertWithGuidKey()
    {
        using var context = new TestDbContext();
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        for (int i = 0; i < LoopCount; i++)
        {
            context.TestGuidEntities.Add(new TestGuidEntity { Data = $"data_{i}" });
        }

        context.SaveChanges();
    }

    [Benchmark]
    public void ReadWithIntKey()
    {
        using var context = new TestDbContext();
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        for (int i = 0; i < LoopCount; i++)
        {
            context.TestIntEntities.Add(new TestIntEntity { Data = $"data_{i}" });
        }
        context.SaveChanges();

        int totalLength = 0;
        for (int i = 0; i < LoopCount; i++)
        {
            var testEntity = context.TestIntEntities.Find(i + 1);
            if (testEntity != null)
            {
                totalLength += testEntity.Data.Length;
            }
        }
        _ = totalLength + 1;
    }

    [Benchmark]
    public void ReadWithGuidKey()
    {
        using var context = new TestDbContext();
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        var guids = new Guid[LoopCount];
        for (int i = 0; i < LoopCount; i++)
        {
            var guid = Guid.NewGuid();
            guids[i] = guid;
            context.TestGuidEntities.Add(new TestGuidEntity { Id = guid, Data = $"data_{i}" });
        }
        context.SaveChanges();

        int totalLength = 0;
        for (int i = 0; i < LoopCount; i++)
        {
            var testEntity = context.TestGuidEntities.Find(guids[i]);
            if (testEntity != null)
            {
                totalLength += testEntity.Data!.Length;
            }
        }
        _ = totalLength + 1;
    }

    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<SqliteBenchmark>();
    }
}
