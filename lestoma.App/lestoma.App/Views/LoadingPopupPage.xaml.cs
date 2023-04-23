﻿using Rg.Plugins.Popup.Pages;
using Syncfusion.SfBusyIndicator.XForms;
using Xamarin.Forms.Xaml;

namespace lestoma.App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPopupPage : PopupPage
    {
        public LoadingPopupPage(AnimationTypes animation = AnimationTypes.Cupertino, string texto = "Cargando...")
        {
            InitializeComponent();
            BusyIndicator.Title = texto;
            BusyIndicator.AnimationType = animation;

        }
        public LoadingPopupPage(string texto = "Cargando...")
        {
            InitializeComponent();
            BusyIndicator.Title = texto;
            BusyIndicator.AnimationType = AnimationTypes.HorizontalPulsingBox;

        }

        public LoadingPopupPage()
        {
            InitializeComponent();
            BusyIndicator.Title = "Cargando...";
            BusyIndicator.AnimationType = AnimationTypes.HorizontalPulsingBox;

        }
    }
}