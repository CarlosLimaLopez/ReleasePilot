using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ReleasePilot.Domain.Aggregates;
using ReleasePilot.Domain.ValueObjects;
using ReleasePilot.Application.Ports;

namespace ReleasePilot.Infrastructure.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Promotion> Promotions => Set<Promotion>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.TargetEnvironment).HasConversion<string>();
                entity.Property(p => p.SourceEnvironment).HasConversion<string>();
                entity.Property(p => p.State).HasConversion<string>();

                entity.Property(p => p.Version)
                      .HasConversion(
                          v => v.Value,            
                          v => ApplicationVersion.Create(v)
                      )
                      .IsRequired();

                var workItemComparer = new ValueComparer<IReadOnlyCollection<WorkItemReference>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()
                );

                entity.Property(p => p.WorkItemReferences)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<WorkItemReference>>(v, (JsonSerializerOptions?)null) ?? new List<WorkItemReference>()
                      )
                      .HasColumnType("jsonb")
                      .Metadata.SetValueComparer(workItemComparer);
            });
        }
    }
}