using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Interfaces
{
    public interface IActividadService : IGenericCRUD<ActividadRequest, Guid>
    {
        public void MergeEntity(List<ActividadDTO> listado);
        public Task<List<ActividadDTO>> GetAll();

    }
}
