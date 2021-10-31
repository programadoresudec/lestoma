﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DatabaseOffline _context;
        private DbSet<T> _entities;

        public GenericRepository(DatabaseOffline context)
        {
            this._context = context;
            _entities = context.Set<T>();
        }


        public async Task<IEnumerable<T>> GetAll()
        {
            return await _entities.ToListAsync();
        }
        public async Task<T> GetById(object id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task Create(T entidad)
        {
            if (entidad == null) Debug.WriteLine($"{nameof(entidad)} no debe ser nula");
            try
            {
                _context.Add(entidad);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var SQLiteException = GetInnerException<SqliteException>(ex);
                if (SQLiteException != null)
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido mezclar: {SQLiteException.Message}");
                }
                else
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido mezclar: {ex.Message}");
                }
            }
        }

        public async Task Update(T entidad)
        {
            if (entidad == null) throw new ArgumentNullException($"{nameof(entidad)} no debe ser nula");
            try
            {
                _context.Update(entidad);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var SQLiteException = GetInnerException<SqliteException>(ex);
                if (SQLiteException != null)
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido mezclar: {SQLiteException.Message}");
                }
                else
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido mezclar: {ex.Message}");
                }
            }
        }

        public async Task Delete(T entidad)
        {
            if (entidad == null) throw new ArgumentNullException($"{nameof(entidad)} no debe ser nula");
            try
            {
                _context.Remove(entidad);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var SQLiteException = GetInnerException<SqliteException>(ex);
                if (SQLiteException != null)
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido mezclar: {SQLiteException.Message}");
                }
                else
                {
                    Debug.WriteLine($"{nameof(entidad)} no se ha podido mezclar: {ex.Message}");
                }
            }
        }
        public void Merge(List<T> ListadoEntidad)
        {
            try
            {
                _context.BulkSynchronize(ListadoEntidad);
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
