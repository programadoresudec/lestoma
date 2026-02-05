using lestoma.AppMaui.Models;
using lestoma.AppMaui.PageModels;

namespace lestoma.AppMaui.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}