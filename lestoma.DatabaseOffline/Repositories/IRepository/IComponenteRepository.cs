using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.DTOs.Sync;
using lestoma.DatabaseOffline.ModelsOffline;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.IRepository
{
    public interface IComponenteRepository : IGenericRepository<ComponenteOffline>
    {
        Task DeleteBulk();
        Task<IEnumerable<NameDTO>> GetModulos();
        public Task<ResponseDTO> MigrateDataToDevice(List<DataOnlineSyncDTO> dataOnline);
    }
}
