using Android.Runtime;
using System.Collections.Generic;

namespace lestoma.CommonUtils.Helpers
{
    public class BluetoothProfileData
    {
        public Dictionary<ProfileType, (string Name, string UUID)> profileData;

        public BluetoothProfileData()
        {
            InitializeProfileData();
        }

        private void InitializeProfileData()
        {
            profileData = new Dictionary<ProfileType, (string, string)>()
        {
            { ProfileType.SerialPort, ("Puerto serie (SPP)", "00001101-0000-1000-8000-00805F9B34FB") },
            { ProfileType.HID, ("Audio estéreo (A2DP)", "00001124-0000-1000-8000-00805F9B34F") },
            { ProfileType.A2dp, ("Audio estéreo (A2DP)", "0000110A-0000-1000-8000-00805F9B34FB") },
            { ProfileType.A2dpSink, ("Audio estéreo (A2DP Sink)", "0000110B-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Avrcp, ("Control remoto de audio/vídeo (AVRCP)", "0000110C-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Gatt, ("Servidor GATT (Bluetooth LE)", "00001801-0000-1000-8000-00805F9B34FB") },
            { ProfileType.GattServer, ("Cliente GATT (Bluetooth LE)", "00001802-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Handsfree, ("Manos libres (HFP)","0000111E-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Headset, ("Auricular (HSP)", "00001108-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Health, ("Dispositivo de salud (HDP)", "0000180D-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Nap, ("Punto de acceso de red (NAP)", "00001116-0000-1000-8000-00805F9B34FB") },
            { ProfileType.ObexFtp, ("Transferencia de archivos (FTP)", "00001106-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Pan, ("Red de área personal (PAN)", "00001115-0000-1000-8000-00805F9B34FB") },
            { ProfileType.Sap, ("Acceso a servicios de SIM (SAP)", "0000112D-0000-1000-8000-00805F9B34FB") },
        };
        }

        public bool TryGetProfileData(ProfileType profile, out (string Name, string UUID) profileInfo)
        {
            return profileData.TryGetValue(profile, out profileInfo);
        }
    }
    public enum ProfileType
    {
        [Preserve(AllMembers = true)]
        SerialPort,
        [Preserve(AllMembers = true)]
        HID,
        [Preserve(AllMembers = true)]
        A2dp,
        [Preserve(AllMembers = true)]
        A2dpSink,
        [Preserve(AllMembers = true)]
        Avrcp,
        [Preserve(AllMembers = true)]
        Gatt,
        [Preserve(AllMembers = true)]
        GattServer,
        [Preserve(AllMembers = true)]
        Handsfree,
        [Preserve(AllMembers = true)]
        Headset,
        [Preserve(AllMembers = true)]
        Health,
        [Preserve(AllMembers = true)]
        Nap,
        [Preserve(AllMembers = true)]
        ObexFtp,
        [Preserve(AllMembers = true)]
        Pan,
        [Preserve(AllMembers = true)]
        Sap

    }
}
