using lestoma.App.Views.Actividades;
using lestoma.CommonUtils.Requests;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class ActividadViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private ObservableCollection<ActividadRequest> _actividades;
        
        public ActividadViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            loadActividades();
        }
        public Command MoreInformationCommand { get; set; }

        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await Prism.PrismApplicationBase.Current.MainPage.Navigation.PushModalAsync(new CrearOrEditActividadPage()); 
                });
            }
        }

        private async void loadActividades()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                await _navigationService.GoBackAsync();
            }
        }
    }
}
