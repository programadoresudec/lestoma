using lestoma.Data;
using lestoma.Logica.Interfaces;
using lestoma.Logica.LogicaService;
using Microsoft.Extensions.DependencyInjection;

namespace lestoma.Api.Helpers
{
    public static class IoC
    {
        public static IServiceCollection AddDependency(this IServiceCollection services)
        {
            services.AddScoped<IUsuarioService, LSUsuario>();
            services.AddScoped<IMailHelper, MailHelper>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            return services;
        }

    }
}
