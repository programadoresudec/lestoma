using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.DTOs.Sync;
using lestoma.DatabaseOffline.ModelsOffline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.IRepository
{
    public interface IComponenteRepository : IGenericRepository<ComponenteOffline>
    {
        Task DeleteBulk();
        Task<IEnumerable<ComponentePorModuloDTO>> GetComponentesPorModuloUpa(Guid upaId, Guid moduloId);
        Task<IEnumerable<NameDTO>> GetModulos();
        Task<IEnumerable<NameProtocoloDTO>> GetProtocolos(Guid idUpa);
        Task<IEnumerable<NameDTO>> GetUpas();
        public Task<ResponseDTO> MigrateDataToDevice(List<DataOnlineSyncDTO> dataOnline);
    }
}
