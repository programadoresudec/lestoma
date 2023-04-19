using Android.App;
using Android.OS;
using Android.Runtime;
using lestoma.App.Droid.Services;
using System.Threading.Tasks;
using System;
using Xamarin.Forms;
using lestoma.CommonUtils.Interfaces;
using Android.Content;
using System.Threading;
using AndroidX.Core.App;

[assembly: Dependency(typeof(ForegroundService))]
namespace lestoma.App.Droid.Services
{
    [Service]
    public class ForegroundService : Service, IForegroundService
    {
        public static bool IsForegroundServiceRunning;
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Task.Run(() =>
            {
                while (IsForegroundServiceRunning)
                {
                    System.Diagnostics.Debug.WriteLine("Sincronizando los datos...");
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            });
            string channelID = "SyncData";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var notfificationChannel = new NotificationChannel(channelID, channelID, NotificationImportance.High);
                notificationManager.CreateNotificationChannel(notfificationChannel);
            }

            var notificationBuilder = new NotificationCompat.Builder(this, channelID)
                                         .SetContentTitle("Sincronizando datos al dispositivo móvil...")
                                         .SetSmallIcon(Resource.Mipmap.icon)
                                         .SetContentText("Servicio ejecutandose en segundo plano.")
                                         .SetPriority(1)
                                         .SetOngoing(true)
                                         .SetChannelId(channelID)
                                         .SetAutoCancel(true);


            StartForeground(1001, notificationBuilder.Build());
            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            IsForegroundServiceRunning = true;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            IsForegroundServiceRunning = false;
        }

        public void StartMyForegroundService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(ForegroundService));
            Android.App.Application.Context.StartForegroundService(intent);
        }

        public void StopMyForegroundService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(ForegroundService));
            Android.App.Application.Context.StopService(intent);
        }

        public bool IsForeGroundServiceRunning()
        {
            return IsForegroundServiceRunning;
        }
    }
}