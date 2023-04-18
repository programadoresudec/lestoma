using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.DTOs.Sync;
using lestoma.CommonUtils.Helpers;
using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class ComponenteRepository : GenericRepository<ComponenteOffline>, IComponenteRepository
    {

        public ComponenteRepository(DatabaseOffline db)
            : base(db)
        {
        }

        public async Task<IEnumerable<NameDTO>> GetModulos()
        {
            return await _context.TablaComponentes.Select(x => new NameDTO
            {
                Id = x.ModuloId,
                Nombre = x.NombreModulo
            }).ToListAsync();
        }

        public async Task<ResponseDTO> MigrateDataToDevice(List<DataOnlineSyncDTO> dataOnline)
        {
            try
            {   // crear una instancia de DataOnlineSyncDTO
                // mapear a ComponenteOffline
                var componentesOffline = _mapper.Map<IEnumerable<ComponenteOffline>>(dataOnline);
                await _context.AddRangeAsync(componentesOffline);
                await _context.SaveChangesAsync();
                return Responses.SetOkResponse(null, "Migración satisfactoria.");
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                return Responses.SetInternalErrorResponse(ex, "Error al migrar los datos.");
            }

        }
    }
}