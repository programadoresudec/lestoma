using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class LaboratorioRepository : GenericRepository<LaboratorioOffline>, ILaboratorioRepository
    {
        public LaboratorioRepository(DatabaseOffline db)
            : base(db)
        {
        }

        public async Task ChangeIsMigrated(IEnumerable<Guid> ids)
        {
            try
            {
                var data = await _dbSet.Where(x => ids.Contains(x.Id)).ToListAsync();
                data.ForEach(x => x.IsMigrated = true);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                throw;
            }
        }

        public async override Task<bool> ExistData()
        {
            return await _dbSet.AnyAsync(x => x.IsMigrated == false);
        }

        public async Task<IEnumerable<LaboratorioRequest>> GetDataOffline()
        {
            try
            {
                var dataOffline = await _dbSet.Where(x => x.IsMigrated == false).ToListAsync();
                return dataOffline.Adapt<IEnumerable<LaboratorioRequest>>();
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                throw;
            }
        }

        public async Task<ResponseDTO> SaveDataOffline(LaboratorioRequest laboratorioRequest)
        {
            try
            {
                var data = laboratorioRequest.Adapt<LaboratorioOffline>();
                await Create(data);
                return Responses.SetCreatedResponse();
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                return Responses.SetInternalErrorResponse(ex, "Error no se pudo guardar en bd del dispositivo.");
            }
        }
    }
}