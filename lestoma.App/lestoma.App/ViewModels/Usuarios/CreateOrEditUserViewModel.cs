using Android.Runtime;
using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.ViewModels.Account;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Usuarios
{
    [Preserve(AllMembers = true)]
    public class CreateOrEditUserViewModel : LoginViewModel
    {
        private ValidatableObject<string> name;
        private ValidatableObject<string> lastName;
        private ValidatablePair<string> password;
        private ValidatableObject<string> estado;
        private ValidatableObject<string> rol;
        private InfoUserDTO _usuario;
        private RolDTO _rolActual;
        private EstadoDTO _estadoActual;
        private ObservableCollection<EstadoDTO> _estados;
        private ObservableCollection<RolDTO> _roles;
        private bool _isEdit = true;
        private bool _isvisible = false;
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        public CreateOrEditUserViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            this.InitializeProperties();
            this.AddValidationRules();
            CreateOrEditCommand = new Command(CreateOrEditClicked);

        }
        #region Property
        public InfoUserDTO Usuario
        {
            get => _usuario;
            set
            {
                SetProperty(ref _usuario, value);
            }
        }

        public RolDTO RolActual
        {
            get => _rolActual;
            set
            {
                SetProperty(ref _rolActual, value);
            }
        }

        public EstadoDTO EstadoActual
        {
            get => _estadoActual;
            set
            {
                SetProperty(ref _estadoActual, value);
            }
        }

        public ObservableCollection<RolDTO> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }


        public ObservableCollection<EstadoDTO> Estados
        {
            get => _estados;
            set => SetProperty(ref _estados, value);
        }

        public ValidatableObject<string> Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.name == value)
                {
                    return;
                }

                this.SetProperty(ref this.name, value);
            }
        }
        public ValidatableObject<string> Estado
        {
            get
            {
                return this.estado;
            }

            set
            {
                if (this.estado == value)
                {
                    return;
                }

                this.SetProperty(ref this.estado, value);
            }
        }
        public ValidatableObject<string> Rol
        {
            get
            {
                return this.rol;
            }

            set
            {
                if (this.rol == value)
                {
                    return;
                }

                this.SetProperty(ref this.rol, value);
            }
        }
        public ValidatableObject<string> LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (this.lastName == value)
                {
                    return;
                }

                this.SetProperty(ref this.lastName, value);
            }
        }
        public ValidatablePair<string> Password
        {
            get
            {

                return this.password;
            }

            set
            {
                if (this.password == value)
                {
                    return;
                }

                this.SetProperty(ref this.password, value);
            }
        }

        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        public bool IsVisible
        {
            get => _isvisible;
            set => SetProperty(ref _isvisible, value);
        }
        #endregion

        #region validaciones

        /// <summary>
        /// Initialize whether fieldsvalue are true or false.
        /// </summary>
        /// <returns>true or false </returns>
        public bool AreFieldsValid()
        {
            bool isPasswordValid = true;
            bool isEmailValid = this.Email.Validate();
            bool isNameValid = this.Name.Validate();
            bool isLastNameValid = this.LastName.Validate();
            if (IsEdit)
            {
                isPasswordValid = this.Password.Validate();
            }
            bool isEstadoValid = this.Estado.Validate();
            bool isRolValid = this.Rol.Validate();
            return isPasswordValid && isNameValid && isLastNameValid && isEmailValid && isRolValid && isEstadoValid;
        }
        /// <summary>
        /// Initializing the properties.
        /// </summary>
        private void InitializeProperties()
        {
            this.Name = new ValidatableObject<string>();
            this.LastName = new ValidatableObject<string>();
            this.Password = new ValidatablePair<string>();
            this.Estado = new ValidatableObject<string>();
            this.Rol = new ValidatableObject<string>();
            this.EstadoActual = new EstadoDTO();
            this.RolActual = new RolDTO();
        }

        /// <summary>
        /// this method contains the validation rules
        /// </summary>
        private void AddValidationRules()
        {
            this.Name.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Nombre requerido." });
            this.Rol.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Rol requerido." });
            this.Estado.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Estado requerido." });
            this.LastName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Apellido requerido." });
            this.Password.Item1.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Contraseña requerida." });
            this.Password.Item1.Validations.Add(new IsLenghtValidRule<string> { ValidationMessage = "Debe tener entre 8 y 30 caracteres.", MaximumLenght = 30, MinimumLenght = 8 });
            this.Password.Item2.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "confirmar contraseña requerida." });
            this.Password.Validations.Add(new MatchPairValidationRule<string> { ValidationMessage = "Las contraseñas no coinciden." });
        }
        #endregion

        public Command CreateOrEditCommand { get; }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("usuario"))
            {
                IsVisible = false;
                var json = parameters.GetValue<string>("usuario");
                _usuario = JsonConvert.DeserializeObject<InfoUserDTO>(json);
                this.Usuario = _usuario;
                Title = "Editar";
                LoadListados(_usuario);
            }
            else
            {
                IsVisible = true;
                Title = "Crear";
                LoadListados();
            }
        }

        private void CargarDatos()
        {
            Name.Value = Usuario != null ? Usuario.Nombre : string.Empty;
            LastName.Value = Usuario != null ? Usuario.Apellido : string.Empty;
            Email.Value = Usuario != null ? Usuario.Email : string.Empty;
            Rol.Value = Usuario != null ? Usuario.Rol.NombreRol : string.Empty;
            Estado.Value = Usuario != null ? Usuario.Estado.NombreEstado : string.Empty;
        }

        private async void CreateOrEditClicked(object obj)
        {
            try
            {
                CargarDatos();
                if (this.AreFieldsValid())
                {

                    if (_apiService.CheckConnection())
                    {
                        await _navigationService.NavigateAsync($"{nameof(LoadingPopupPage)}");

                        if (Usuario.Id == 0)
                        {
                            var request = new RegistroRequest
                            {
                                Nombre = Name.Value,
                                Apellido = LastName.Value,
                                Email = Email.Value,
                                EstadoId = EstadoActual.Id,
                                RolId = RolActual.Id
                            };
                            Response respuesta = await _apiService.PostAsyncWithToken(URL, "usuarios/crear", request, TokenUser.Token);
                            if (respuesta.IsExito)
                            {
                                AlertSuccess(respuesta.Mensaje);
                                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                                await _navigationService.GoBackAsync(parameters);
                            }
                            else
                            {
                                AlertWarning(respuesta.Mensaje);
                            }
                        }
                        else
                        {
                            var request = new RegistroUpdateRequest
                            {
                                Nombre = Name.Value,
                                Apellido = LastName.Value,
                                UsuarioId = Usuario.Id,
                                EstadoId = EstadoActual.Id,
                                RolId = RolActual.Id
                            };
                            Response respuesta = await _apiService.PutAsyncWithToken(URL, "usuarios/editar", request, TokenUser.Token);
                            if (respuesta.IsExito)
                            {
                                AlertSuccess(respuesta.Mensaje);
                                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                                await _navigationService.GoBackAsync(parameters);
                            }
                            else
                            {
                                AlertWarning(respuesta.Mensaje);
                            }
                        }
                    }
                    else
                    {
                        AlertNoInternetConnection();
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                if (PopupNavigation.Instance.PopupStack.Any())
                    await PopupNavigation.Instance.PopAsync();
            }
        }

        private async void LoadListados(InfoUserDTO infoUser = null)
        {
            if (!IsVisible)
                IsBusy = true;
            try
            {
                if (_apiService.CheckConnection())
                {

                    Response roles = await _apiService.GetListAsyncWithToken<List<RolDTO>>(URL,
                           "usuarios/listado-roles", TokenUser.Token);
                    Roles = new ObservableCollection<RolDTO>((List<RolDTO>)roles.Data);
                    Response estados = await _apiService.GetListAsyncWithToken<List<EstadoDTO>>(URL,
                        "usuarios/listado-estados", TokenUser.Token);
                    Estados = new ObservableCollection<EstadoDTO>((List<EstadoDTO>)estados.Data);
                    if (infoUser != null)
                    {
                        IsEdit = false;
                        RolActual = Roles.Where(x => x.Id == infoUser.Rol.Id).FirstOrDefault();
                        EstadoActual = Estados.Where(x => x.Id == infoUser.Estado.Id).FirstOrDefault();
                        Usuario = infoUser;
                    }
                }
                else
                {
                    AlertNoInternetConnection();
                    await _navigationService.GoBackAsync();
                }

            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                IsBusy = false;
                IsVisible = true;
            }
        }

    }
}
