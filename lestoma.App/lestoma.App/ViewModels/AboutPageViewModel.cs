using Prism.Navigation;

namespace lestoma.App.ViewModels
{
    public class AboutPageViewModel : BaseViewModel
    {
        public AboutPageViewModel(INavigationService navigationService)
             : base(navigationService)
        {
            Title = "Acerca de LESTOMA";
            MessageHelp = "Si desea usar el apartado del laboratorio en modo offline, debe sincronizar los datos desde el menú de configuración\n\n ¡Sincronizar Datos Nube!";
        }
    }
}
