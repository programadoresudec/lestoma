using app.lestoma.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace app.lestoma.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}