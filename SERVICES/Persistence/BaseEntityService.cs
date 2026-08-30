using AutomationAPI.DATA;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES.Persistence
{
    public class BaseEntityService<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _dbContext;
        public BaseEntityService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async Task<string> GenerateUniqueIdAsync(Func<string> generator)
        {
            string KeyName = _dbContext.Model
                .FindEntityType(typeof(TEntity))
                .FindPrimaryKey()
                .Properties
                .Select(p => p.Name)
                .FirstOrDefault();

            if (KeyName == null)
                throw new Exception($"No primary key found for {typeof(TEntity).Name}");

            string newId;
            do
            {
                newId = generator();
            }
            while (await _dbContext.Set<TEntity>().AnyAsync(e => EF.Property<string>(e, KeyName) == newId));

            return newId;
        }
    }
}
