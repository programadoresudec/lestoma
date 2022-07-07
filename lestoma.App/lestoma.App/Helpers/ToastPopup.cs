using Plugin.Toast;

namespace lestoma.App.Helpers
{
    public class ToastPopup
    {

        #region Alerts de CrossToastPopUp
        public  void AlertNoInternetConnection()
        {
     

        }
        public  void AlertError(string Error = "")
        {
            CrossToastPopUp.Current.ShowToastError($"ERROR {Error}", Plugin.Toast.Abstractions.ToastLength.Long);

        }
        public  void AlertWarning(string mensaje = "")
        {
            CrossToastPopUp.Current.ShowToastWarning($"{mensaje}", Plugin.Toast.Abstractions.ToastLength.Long);

        }

        public void AlertSuccess(string mensaje = "EXITO")
        {
            CrossToastPopUp.Current.ShowToastSuccess($"{mensaje}", Plugin.Toast.Abstractions.ToastLength.Long);
        }
        #endregion
    }
}
