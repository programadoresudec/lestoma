using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Toast;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Buzon
{
    public class CrearReporteDelBuzonViewModel : BaseViewModel
    {
        #region Fields
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;
        private readonly IFilesHelper _filesHelper;
        private bool _isRunning;
        private ImageSource _image;
        private bool _isEnabled;
        private ObservableCollection<string> _tiposDeGravedad;
        private DetalleBuzon _detalleBuzon;
        private MediaFile _file;

        #endregion

        #region Constructor
        public CrearReporteDelBuzonViewModel(INavigationService navigationService, IApiService apiService, IFilesHelper filesHelper)
              : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _filesHelper = filesHelper;
            Title = "Crear Reporte";
            IsEnabled = true;
            Image = App.Current.Resources["UrlNoImage"].ToString();
            this.SendReportCommand = new Command(this.SendClicked);
            this.ChangeImageCommand = new Command(this.ChangeImageAsync);
            this.DetalleBuzon = new DetalleBuzon();
            LoadTiposDeGravedadAsync();
        }


        #endregion

        #region Command


        public Command SendReportCommand { get; set; }

        public Command ChangeImageCommand { get; set; }

        #endregion

        #region Properties

        public ImageSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public ObservableCollection<string> TiposDeGravedad
        {
            get => _tiposDeGravedad;
            set => SetProperty(ref _tiposDeGravedad, value);
        }

        public DetalleBuzon DetalleBuzon
        {
            get => _detalleBuzon;
            set => SetProperty(ref _detalleBuzon, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        #endregion

        #region Methods
        private void LoadTiposDeGravedadAsync()
        {

            List<string> lista = new List<string>
            {
                "Leve",
                "Grave",
                "Muy grave"
            };
            TiposDeGravedad = new ObservableCollection<string>(lista);
        }

        private async void SendClicked()
        {

            IsRunning = true;
            IsEnabled = false;
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                IsRunning = false;
                IsEnabled = true;
                CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                return;
            }

            byte[] imageArray = null;
            if (_file != null)
            {
                imageArray = _filesHelper.ReadFully(_file.GetStream());
            }
            TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
            BuzonCreacionRequest buzon = new BuzonCreacionRequest
            {
                Extension = _file != null ? Path.GetExtension(_file.Path) : string.Empty,
                UsuarioId = UserApp.User.Id,
                Imagen = imageArray,
                Detalle = DetalleBuzon
            };

            string url = App.Current.Resources["UrlAPI"].ToString();
            Response respuesta = await _apiService.PostAsyncWithToken(url, "ReportsMailbox/create", buzon, UserApp.Token);
            IsRunning = false;
            IsEnabled = true;
            if (!respuesta.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                return;
            }
            CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
            await Task.Delay(2000);
            await _navigationService.NavigateAsync($"{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
        }


        private async void ChangeImageAsync()
        {
            await CrossMedia.Current.Initialize();

            string source = await Application.Current.MainPage.DisplayActionSheet(
                "¿Donde quieres tomar tu foto?",
                "Cancelar",
                null,
                "Galería",
               "Cámara");

            if (source == "Cancelar")
            {
                _file = null;
                return;
            }

            if (source == "Cámara")
            {
                if (!CrossMedia.Current.IsCameraAvailable)
                {
                    CrossToastPopUp.Current.ShowToastError("No soporta la Cámara.");
                    return;
                }

                _file = await CrossMedia.Current.TakePhotoAsync(
                    new StoreCameraMediaOptions
                    {
                        Directory = "Sample",
                        Name = "test.jpg",
                        PhotoSize = PhotoSize.Small,
                    }
                );
            }
            else
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    CrossToastPopUp.Current.ShowToastError("No hay galeria.");
                    return;
                }

                _file = await CrossMedia.Current.PickPhotoAsync();
            }

            if (_file != null)
            {
                Image = ImageSource.FromStream(() =>
                {
                    System.IO.Stream stream = _file.GetStream();
                    return stream;
                });
            }
        }
        #endregion



    }
}
