using AutoMapper;
using lestoma.DatabaseOffline.Repositories.IRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly IMapper _mapper = Mapper.CreateMapper();
        protected readonly DatabaseOffline _context;
        protected DbSet<T> _dbSet;

        public GenericRepository(DatabaseOffline context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }
        public virtual async Task<T> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task Create(T entidad)
        {
            if (entidad == null) Debug.WriteLine($"{nameof(entidad)} no debe ser nula");
            try
            {
                await _dbSet.AddAsync(entidad);
            }
            catch (Exception ex)
            {
                var SQLiteException = GetInnerException<SqliteException>(ex);
                if (SQLiteException != null)
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido agregar: {SQLiteException.Message}");
                }
                else
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido agregar: {ex.Message}");
                }
            }
        }

        public async Task Merge(List<T> ListadoEntidad)
        {
            try
            {
                foreach (var item in ListadoEntidad)
                {
                    await Create(item);
                }
            }
            catch (Exception ex)
            {

                var SQLiteException = GetInnerException<SqliteException>(ex);
                if (SQLiteException != null)
                {
                    Debug.WriteLine($"{nameof(ListadoEntidad)} no se ha podido mezclar: {SQLiteException.Message}");
                }
                else
                {
                    Debug.WriteLine($"{nameof(ListadoEntidad)} no se ha podido mezclar: {ex.Message}");
                }
            }
        }
        public static TException GetInnerException<TException>(Exception exception)
           where TException : Exception
        {
            Exception innerException = exception;
            while (innerException != null)
            {
                if (innerException is TException result)
                {
                    return result;
                }
                innerException = innerException.InnerException;

            }
            return null;
        }

    }
}
