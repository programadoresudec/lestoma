using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Buzon
{
    public class CrearReporteDelBuzonViewModel : BaseViewModel
    {
        #region Fields
        private readonly IApiService _apiService;
        private readonly IFilesHelper _filesHelper;
        private ImageSource _image;
        private ObservableCollection<string> _tiposDeGravedad;
        private DetalleBuzonDTO _detalleBuzon;
        private MediaFile _file;
        #endregion

        #region Constructor
        public CrearReporteDelBuzonViewModel(INavigationService navigationService, IApiService apiService, IFilesHelper filesHelper)
              : base(navigationService)
        {
            _apiService = apiService;
            _filesHelper = filesHelper;
            Title = "Crear Reporte";
            Image = "DefaultImagen.png";
            SendReportCommand = new Command(SendClicked);
            ChangeImageCommand = new Command(ChangeImageAsync);
            DetalleBuzon = new DetalleBuzonDTO();
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

        public DetalleBuzonDTO DetalleBuzon
        {
            get => _detalleBuzon;
            set => SetProperty(ref _detalleBuzon, value);
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
        private bool AreFieldsValid()
        {
            bool isTituloValid = !string.IsNullOrWhiteSpace(DetalleBuzon.Titulo);
            bool isDescripcionValid = !string.IsNullOrWhiteSpace(DetalleBuzon.Descripcion);
            bool isTipoGravedadValid = !string.IsNullOrWhiteSpace(DetalleBuzon.TipoDeGravedad);
            return isTituloValid && isDescripcionValid && isTipoGravedadValid;
        }
        private async void SendClicked()
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    if (AreFieldsValid())
                    {
                        UserDialogs.Instance.ShowLoading("Guardando...");
                        byte[] imageArray = null;
                        if (_file != null)
                        {
                            imageArray = _filesHelper.ReadFully(_file.GetStream());
                        }

                        BuzonCreacionRequest buzon = new BuzonCreacionRequest
                        {
                            Extension = _file != null ? Path.GetExtension(_file.Path) : string.Empty,
                            UsuarioId = TokenUser.User.Id,
                            Imagen = imageArray,
                            Detalle = DetalleBuzon
                        };

                        ResponseDTO respuesta = await _apiService.PostAsyncWithToken(URL_API, "buzon-de-reportes/create", buzon, TokenUser.Token);
                        AlertSuccess(respuesta.MensajeHttp);
                        this.DetalleBuzon = new DetalleBuzonDTO();
                        await NavigationService.GoBackAsync();
                    }
                    else
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage($"Error: Todos los campos son obligatorios.", Constants.ICON_WARNING));
                    }
                }
                else
                {
                    AlertNoInternetConnection();
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void ChangeImageAsync()
        {
            try
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
                        AlertError("No soporta la Cámara.");
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
                        AlertError("No hay galeria.");
                        return;
                    }

                    _file = await CrossMedia.Current.PickPhotoAsync();
                }

                if (_file != null)
                {
                    Image = ImageSource.FromStream(() =>
                    {
                        Stream stream = _file.GetStream();
                        return stream;
                    });
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
        }
        #endregion
    }
}
