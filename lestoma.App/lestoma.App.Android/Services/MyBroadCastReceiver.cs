using Android.App;
using Android.Content;
using Android.Content.PM;

namespace lestoma.App.Droid.Services
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class MyBroadCastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted)
            {
                var foreGroundServiceIntent = new Intent(Android.App.Application.Context, typeof(ForegroundService));
                Android.App.Application.Context.StartForegroundService(intent);
                context.StartForegroundService(foreGroundServiceIntent);
            }
        }
    }
}