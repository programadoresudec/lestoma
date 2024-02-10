using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.DTOs.Sync;
using lestoma.CommonUtils.Helpers;
using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public async Task DeleteBulk()
        {
            var data = await _dbSet.ToListAsync();
            _context.RemoveRange(data);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NameDTO>> GetModulos()
        {
            return await _dbSet.GroupBy(x => new { x.ModuloId, x.NombreModulo }).Select(x => new NameDTO
            {
                Id = x.Key.ModuloId,
                Nombre = x.Key.NombreModulo
            }).OrderBy(y => y.Nombre).ToListAsync();
        }

        public async Task<IEnumerable<NameDTO>> GetUpas()
        {
            return await _dbSet.GroupBy(x => new { x.UpaId, x.NombreUpa }).Select(x => new NameDTO
            {
                Id = x.Key.UpaId,
                Nombre = x.Key.NombreUpa
            }).OrderBy(y => y.Nombre).ToListAsync();
        }

        public async Task<IEnumerable<NameProtocoloDTO>> GetProtocolos(Guid idUpa)
        {
            var data = new List<NameProtocoloDTO>();
            var datos = await _dbSet.ToListAsync();
            var protocolos = await _dbSet.Where(y => y.UpaId == idUpa).Select(x => x.Protocolos).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(protocolos))
            {
                data = JsonConvert.DeserializeObject<List<NameProtocoloDTO>>(protocolos);
            }
            return data;
        }

        public async Task<ResponseDTO> MigrateDataToDevice(List<DataOnlineSyncDTO> dataOnline)
        {
            try
            {   // crear una instancia de DataOnlineSyncDTO mapear a ComponenteOffline
                var componentesOffline = dataOnline.Adapt<IEnumerable<ComponenteOffline>>();
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

        public async Task<IEnumerable<ComponentePorModuloDTO>> GetComponentesPorModuloUpa(Guid upaId, Guid moduloId)
        {
            var data = await _dbSet.Where(x => x.UpaId == upaId && x.ModuloId == moduloId)
                .Select(y => new ComponentePorModuloDTO
                {
                    Actividad = y.NombreActividad,
                    DireccionRegistro = y.DireccionRegistro,
                    Id = y.Id,
                    Nombre = y.NombreComponente,
                    EstadoComponente = JsonConvert.DeserializeObject<EstadoComponenteDTO>(y.DecripcionEstadoJson)
                }).OrderBy(y => y.Nombre).ToListAsync();
            return data;
        }


    }
}