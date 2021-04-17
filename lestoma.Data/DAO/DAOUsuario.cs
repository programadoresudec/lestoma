using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.Data.DAO
{
    public class DAOUsuario
    {
        public async Task<EUsuario> Logeo(RequestLogin login, Mapeo db)
        {

            var lista = (await (from persona in db.TablaUsuarios
                                join roles in db.TablaRoles on persona.RolId equals roles.Id
                                join estados in db.TablaEstadosUsuarios on persona.EstadoId equals estados.Id
                                where login.Clave.Equals(persona.Clave) && login.Email.Equals(persona.Email)

                                select new
                                {
                                    persona,
                                    roles,
                                    estados
                                }).ToListAsync());

            return lista.Select(m => new EUsuario

            {
                Id = m.persona.Id,
                EstadoUsuario = new EEstadoUsuario { Id = m.estados.Id, DescripcionEstado = m.estados.DescripcionEstado },
                Rol = new ERol { Id = m.roles.Id, NombreRol = m.roles.NombreRol }
            }).FirstOrDefault();
        }
    }
}
