using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace AzureKeyVaultEmulator
{
    public static class Utilities
    {
        /// <summary>
        /// Generates a 32 bit hex string to use as an ID
        /// </summary>
        /// <returns></returns>
        public static string GenerateId()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[16];
                rng.GetBytes(randomBytes);
                return string.Concat(randomBytes.Select(x => x.ToString("X2"))).ToLower();
            }
        }

        public static void SetCaseInsensitiveSearchesForSQLite(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.UseCollation("NOCASE");

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                                                    .SelectMany(t => t.GetProperties())
                                                    .Where(p => p.ClrType == typeof(string)))
            {
                property.SetCollation("NOCASE");
            }
        }
    }
}
