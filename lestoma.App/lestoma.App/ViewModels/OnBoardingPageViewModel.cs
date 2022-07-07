using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class OnBoardingPageViewModel : BaseViewModel
    {
        private ObservableCollection<OnboardingModel> items;
        private int position;
        private string skipButtonText;
        public OnBoardingPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            SetSkipButtonText("SALTAR");
            InitializeOnBoarding();
            InitializeSkipCommand();
        }
        public Command SkipCommand { get; private set; }
        private void SetSkipButtonText(string skipButtonText)
                => SkipButtonText = skipButtonText;
        private void InitializeOnBoarding()
        {
            Items = new ObservableCollection<OnboardingModel>
            {
                new OnboardingModel
                {
                    Title = "Bienvenido a \n LESTOMA",
                    Content = "Aplicativo para Monitoreo y Control de Sistemas Acuaponicos.",
                    ImageUrl = "Logolestoma.svg"
                },
                new OnboardingModel
                {
                    Title = "Registrate",
                    Content = "Para que comencemos!!!",
                    ImageUrl = "ilust2.svg"
                },
                new OnboardingModel
                {
                    Title = "Controla y Monitorea tu Sistema",
                    Content = "Ahora es mas Facil",
                    ImageUrl = "monitoreo.svg"
                },
                new OnboardingModel
                {
                    Title = "Una solución Eficiente",
                    Content = "Universidad De Cundinamarca",
                    ImageUrl = "EscudoCundinamarca.svg"
                }

            };

        }

        private void InitializeSkipCommand()
        {
            SkipCommand = new Command(() =>
            {
                if (LastPositionReached())
                {
                    ExitOnBoarding();
                }
                else
                {
                    MoveToNextPosition();
                }
            });
        }

        private async void ExitOnBoarding()
        {
            try
            {

                await _navigationService.NavigateAsync($"{nameof(LoadingPopupPage)}");
                await _navigationService.NavigateAsync($"/NavigationPage/{nameof(LoginPage)}");
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
            }
            finally
            {
                await _navigationService.ClearPopupStackAsync();
            }

        }


        private void MoveToNextPosition()
        {
            var nextPosition = ++Position;
            Position = nextPosition;
        }

        private bool LastPositionReached()
            => Position == Items.Count - 1;

        public ObservableCollection<OnboardingModel> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        public string SkipButtonText
        {
            get => skipButtonText;
            set => SetProperty(ref skipButtonText, value);
        }

        public int Position
        {
            get => position;
            set
            {
                if (SetProperty(ref position, value))
                {
                    UpdateSkipButtonText();
                }
            }
        }

        private void UpdateSkipButtonText()
        {
            if (LastPositionReached())
            {
                SetSkipButtonText("ENTENDIDO");
            }
            else
            {
                SetSkipButtonText("SALTAR");
            }
        }
    }
}
