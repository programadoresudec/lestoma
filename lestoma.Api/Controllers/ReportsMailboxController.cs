using AutoMapper;
using lestoma.Api.Helpers;
using lestoma.CommonUtils.Requests;
using lestoma.Logica.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsMailboxController : BaseController
    {
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly IBuzonService _buzonService;
        private readonly string contenedor = "ReportesDelBuzon";
        public ReportsMailboxController(IMapper mapper, IAlmacenadorArchivos almacenadorArchivos, IBuzonService buzonService)
            : base(mapper)
        {

            _buzonService = buzonService;
            _almacenadorArchivos = almacenadorArchivos;
        }

        [Authorize]
        [HttpGet("numeros")]
        public async Task<IEnumerable> Lista()
        {
          List<int> enteros = new()
          {
             5,3,6,7
          };
            return enteros;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CrearReporteDelBuzon(BuzonCreacionRequest buzon)
        {
            if (!string.IsNullOrEmpty(buzon.Extension))
            {
                buzon.Detalle.PathImagen = await _almacenadorArchivos.GuardarArchivo(buzon.Imagen, buzon.Extension, contenedor);
            }
            Respuesta = await _buzonService.AgregarReporte(buzon);
            return Created(string.Empty, Respuesta);
        }
    }
}
