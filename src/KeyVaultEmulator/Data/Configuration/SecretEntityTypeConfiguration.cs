using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace AzureKeyVaultEmulator.Data.Configuration
{
    public class SecretEntityTypeConfiguration : IEntityTypeConfiguration<Secret>
    {
        public void Configure(EntityTypeBuilder<Secret> builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Configure the owned collection of Tags
            builder.OwnsMany(e => e.Tags, tag =>
            {
                tag.WithOwner().HasForeignKey("SecretId");

                tag.Property(t => t.Key).IsRequired();
                tag.Property(t => t.Value);

                // Avoid auto-gen composite key problems
                tag.HasKey("SecretId", "Key");
            });

        }
    }
}
