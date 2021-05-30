using AutoMapper;
using lestoma.CommonUtils.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsMailbox : BaseController
    {
        public ReportsMailbox(IMapper mapper)
            :base(mapper)
        {

        }

        [HttpPost("crear")]
        public async Task<IActionResult> CrearReporteDelBuzon([FromForm] BuzonCreacionRequest buzon)
        {
         
            return Created(string.Empty, Respuesta);
        }
    }
}
