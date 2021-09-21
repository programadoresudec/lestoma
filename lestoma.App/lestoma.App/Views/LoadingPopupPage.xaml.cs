using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lestoma.App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPopupPage : PopupPage
    {
        public LoadingPopupPage(string texto = "Cargando...")
        {
            InitializeComponent();
            BusyIndicator.Title = texto;
        }
    }
}