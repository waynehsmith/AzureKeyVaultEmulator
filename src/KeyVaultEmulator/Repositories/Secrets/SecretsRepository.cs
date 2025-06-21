using AzureKeyVaultEmulator.Data;
using AzureKeyVaultEmulator.Repositories.Secrets;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureKeyVaultEmulator.Repositories
{
    public class SecretsRepository : ISecretsRepository
    {
        private readonly KeyVaultEmulatorContext _dbContext;

        public SecretsRepository(KeyVaultEmulatorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Secret>> GetSecretsAsync(string secretName, int? maxResults)
        {
            var lowerSecretName = secretName.ToLower();

            var secretsQueryable = _dbContext.Secrets
                .Where(s =>
                    (s.Name.ToLower() == lowerSecretName)
                    && !s.Removed);

            if (maxResults.HasValue)
            {
                secretsQueryable = secretsQueryable.Take(maxResults.Value);
            }

            var secrets = await secretsQueryable.ToListAsync<Secret>();

            return secrets;
        }

        public async Task<Secret> SetSecretAsync(string secretName, SecretSetParameters secretSetParameters)
        {
            if (secretSetParameters is null)
            {
                throw new ArgumentNullException(nameof(secretSetParameters));
            }

            var id = Utilities.GenerateId();

            var secret = new Secret
            {
                Name = secretName,
                ContentType = secretSetParameters.ContentType,
                Created = secretSetParameters.SecretAttributes?.Created,
                Enabled = secretSetParameters.SecretAttributes?.Enabled,
                Expires = secretSetParameters.SecretAttributes?.Expires,
                NotBefore = secretSetParameters?.SecretAttributes?.NotBefore,
                RecoveryLevel = DeletionRecoveryLevel.Purgeable,
                Tags = secretSetParameters.Tags?.Select(t => new Tag
                {
                    Key = t.Key,
                    Value = t.Value
                }).ToList(),
                Updated = secretSetParameters.SecretAttributes?.Updated,
                Value = secretSetParameters.Value,
                VersionId = id
            };

            var entity = _dbContext.Add(secret);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return secret;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task GetSecret(string secretName, string secretVersion)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }
    }
}