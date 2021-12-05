using AutoMapper;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.Interfaces;
using lestoma.DatabaseOffline.Models;
using lestoma.DatabaseOffline.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Logica
{
    public class LSActividad : IActividadService
    {
        private readonly Response _respuesta = new Response();
        public readonly IMapper _mapper = Mapper.CreateMapper();
        private readonly ActividadRepository _actividadRepository;
        public LSActividad(string dbPathSqlLite)
        {
            DatabaseOffline _db = new DatabaseOffline(dbPathSqlLite);
            _actividadRepository = new ActividadRepository(_db);
        }

        public string DbPath
        {
            get; set;

        }

        public Task<Response> ActualizarAsync(ActividadRequest entidad)
        {
            throw new NotImplementedException();
        }
        public void MergeEntity(List<ActividadDTO> listado)
        {
            var actividades = _mapper.Map<List<ActividadModel>>(listado);
            _actividadRepository.Merge(actividades);
        }
        public async Task<Response> CrearAsync(ActividadRequest entidad)
        {
            bool existe = await _actividadRepository.ExisteActividad(entidad.Nombre);
            if (!existe)
            {
                var actividad = _mapper.Map<ActividadModel>(entidad);
                actividad.Id = Guid.NewGuid();
                await _actividadRepository.Create(actividad);
                _respuesta.IsExito = true;
                _respuesta.StatusCode = (int)HttpStatusCode.Created;
                _respuesta.Mensaje = "se ha creado satisfactoriamente.";
            }
            else
            {
                _respuesta.Mensaje = "El nombre ya está en uso utilice otro.";
            }
            return _respuesta;

        }

        public Task EliminarAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public Task<Response> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ActividadDTO>> GetAll()
        {
            var query = await _actividadRepository.GetAll();
            var listado = _mapper.Map<List<ActividadDTO>>(query.ToList());
            return listado;
        }
    }
}
