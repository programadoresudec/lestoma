using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.ModelsOffline;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.IRepository
{
    public interface ILaboratorioRepository : IGenericRepository<LaboratorioOffline>
    {
        Task ChangeIsMigrated(IEnumerable<Guid> ids);
        Task<IEnumerable<LaboratorioRequest>> GetDataOffline();
        Task<ResponseDTO> SaveDataOffline(LaboratorioRequest laboratorioRequest);
    }
}
