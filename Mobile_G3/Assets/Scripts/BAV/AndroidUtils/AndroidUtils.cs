using System;
using UnityEngine;
using UnityEngine.Android;
//using Unity.Notifications.Android;

////////////////////////////////////////////////////////////////////////////////
// Android Utility
////////////////////////////////////////////////////////////////////////////////
public static class AndroidUtils
{
    #if UNITY_ANDROID
    private static AndroidJavaClass unityPlayer;
    private static AndroidJavaClass vibrationManager;
    //private static AndroidJavaClass audioManager;
    //private static AndroidJavaClass bluetoothManager;
    
    private static AndroidJavaObject currentActivity;
    private static AndroidJavaObject vibratorObject;
    private static AndroidJavaObject audioObject;
    private static AndroidJavaObject bluetoothObject;
    private static AndroidJavaObject wifiObject;
    #endif
    
    private static int audioMode;
    private const int androidVersion26 = 26;
    private const string vibratorLengthShort = "LENGTH_SHORT";
    private const string vibratorLengthLong = "LENGTH_LONG";
    private const string CAMERA_PERMISSION = "android.permission.CAMERA";
    private const string READ_EXTERNAL_STORAGE_PERMISSION = "android.permission.READ_EXTERNAL_STORAGE";
    private const string WRITE_EXTERNAL_STORAGE_PERMISSION = "android.permission.WRITE_EXTERNAL_STORAGE";
    private const string ANDROID_WIDGET_TOAST = "android.widget.Toast";
    
    ////////////////////////////////////////////////////////////////////////////////
    // Android create State Of The Phone
    ////////////////////////////////////////////////////////////////////////////////
    public static void CreateCurrentActivity()
    {
        if (!Application.isMobilePlatform) return;
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Detect if a module on the Mobile 
    ////////////////////////////////////////////////////////////////////////////////
    public static void AskPermissionAccess(string permission)
    {
        if(!Permission.HasUserAuthorizedPermission(permission))
        {
            Permission.RequestUserPermission(permission);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Detect if modules are enabled on the Mobile
    ////////////////////////////////////////////////////////////////////////////////
    public static void AskPermissionsAccess(string[] permission)
    {
        foreach (string elementOfPermission in permission)
        {
            if(!Permission.HasUserAuthorizedPermission(elementOfPermission))
            {
                Permission.RequestUserPermission(elementOfPermission);
            }
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////////
    //// Enabled Microphone on Mobile
    //////////////////////////////////////////////////////////////////////////////////
    
    ////////////////////////////////////////////////////////////////////////////////
    // Detect Android Version
    ////////////////////////////////////////////////////////////////////////////////
    public static int AndroidVersion {
        get {
            if (!Application.isMobilePlatform) Debug.LogError("Failed to search the Android API, is the device an Android"); ;
            int idVersionNumber = 0;
            if ( Application.platform == RuntimePlatform.Android ) {
                string androidVersion = SystemInfo.operatingSystem;
                int sdkPos = androidVersion.IndexOf ( "API-", StringComparison.Ordinal);
                idVersionNumber = int.Parse ( androidVersion.Substring ( sdkPos + 4, 2 ) );
            }
            return idVersionNumber;
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Detect Android IP Adress
    ////////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////
    // Android Create Channel Notification
    ////////////////////////////////////////////////////////////////////////////////
    //public static void CreateChannel(string name, Importance importance, string _description, string _id = "channel_id")
    //{
    //    if (!Application.isMobilePlatform) return;
    //    var androidNotificationChannel = new AndroidNotificationChannel()
    //    {
    //        Id = _id,
    //        Name = name,
    //        Importance = importance,
    //        Description = _description,
    //        CanBypassDnd = false,
    //        CanShowBadge = false,
    //        EnableLights = true,
    //        EnableVibration = true,
    //        VibrationPattern = null,
    //        LockScreenVisibility = LockScreenVisibility.Public
    //    };
    //    AndroidNotificationCenter.RegisterNotificationChannel(androidNotificationChannel);
    //}
    
    ////////////////////////////////////////////////////////////////////////////////
    // Android Send Notification
    ////////////////////////////////////////////////////////////////////////////////
    //public static void SendNotifications(string _title, string _text, float _firetime = 0, string _smallIcon = null, string _largeIcon = null, string _channel = "channel_id", string _group = "Default Group")
    //{
    //    if (!Application.isMobilePlatform) return;
    //    var notificationToSend = new AndroidNotification
    //    {
    //        Title = _title,
    //        Text = _text,
    //        FireTime = System.DateTime.Now.AddSeconds(_firetime),
    //        SmallIcon = _smallIcon,
    //        LargeIcon = _largeIcon,
    //        Group = _group
    //    };
    //    AndroidNotificationCenter.SendNotification(notificationToSend, _channel);
    //}

    ////////////////////////////////////////////////////////////////////////////////
    // Android Delete Notification Channel
    ////////////////////////////////////////////////////////////////////////////////
    //public static void DeleteNotification(string _channel = "channel_id")
    //{
    //    AndroidNotificationCenter.DeleteNotificationChannel(_channel);
    //}
    
    ////////////////////////////////////////////////////////////////////////////////
    // Android create a ToastyToast
    ////////////////////////////////////////////////////////////////////////////////
    public static void SendToast(string message, bool _lengthShort = false)
    {
        if (!Application.isMobilePlatform) return;
        AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaClass toastManager = new AndroidJavaClass(ANDROID_WIDGET_TOAST);
        AndroidJavaObject javaText = new AndroidJavaObject("java.lang.String", message);
        AndroidJavaObject toastObject = toastManager.CallStatic<AndroidJavaObject>("makeText", context, javaText,
            _lengthShort ? toastManager.GetStatic<int>(vibratorLengthLong) : toastManager.GetStatic<int>(vibratorLengthShort));
        toastObject.Call("show");
    }
    ////////////////////////////////////////////////////////////////////////////////
    // Send a SMS
    ////////////////////////////////////////////////////////////////////////////////
    public static void SendSMS(string _phoneNumber, string _textSMS)
    {
        if (!Application.isMobilePlatform) return;
        //AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaClass smsManager = new AndroidJavaClass("android.telephony.SmsManager");
        AndroidJavaObject smsObject = smsManager.CallStatic<AndroidJavaObject>("getDefault");
        smsObject.Call("sendTextMessage", _phoneNumber, null, _textSMS, null, null);
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Create an Audio Manager
    ////////////////////////////////////////////////////////////////////////////////
    public static void CreateAndroidAudioManager()
    {
        if (!Application.isMobilePlatform) return;
        audioObject = currentActivity.Call<AndroidJavaObject>("getSystemService", "audio");
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Detect the audio mode of the smartphone
    ////////////////////////////////////////////////////////////////////////////////
    public static void AudioModeAndroid()
    {
        if (!Application.isMobilePlatform) return;
        audioMode = audioObject.Call<int>("getRingerMode");
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Android Vibrator Class
    ////////////////////////////////////////////////////////////////////////////////
    public static void CreateAndroidVibratorClass()
    {
        if (!Application.isMobilePlatform) return;
        vibratorObject = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        if (AndroidVersion >= androidVersion26)
        {
            vibrationManager = new AndroidJavaClass("android.os.VibrationEffect");
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Create Vibrate from Handheld
    ////////////////////////////////////////////////////////////////////////////////
    public static void Vibrate ()
    {
        if (!Application.isMobilePlatform) return;
        Handheld.Vibrate ();
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Create Vibrate Function One Shot  
    ////////////////////////////////////////////////////////////////////////////////
    public static void Vibrate(long milliseconds)
    {
        if (AndroidVersion >= androidVersion26)
        {
            AndroidJavaObject oneShotVibrate = vibrationManager.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1);
            vibratorObject.Call("vibrate", oneShotVibrate);
        }
        else
        {
            vibratorObject.Call("vibrate", milliseconds);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Create Vibrate Function WaveForm
    ////////////////////////////////////////////////////////////////////////////////
    public static void Vibrate(long[] pattern, int repeat)
    {
        if (!Application.isMobilePlatform) return;
        if (AndroidVersion >= androidVersion26)
        {
            AndroidJavaObject waveform = vibrationManager.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
            vibratorObject.Call("vibrate", waveform);
        }
        else
        {
            vibratorObject.Call("vibrate", pattern, repeat);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Create Vibrate Function WaveForm + amplitude
    ////////////////////////////////////////////////////////////////////////////////
    public static void Vibrate(long[] pattern, int[] amplitude, int repeat)
    {
        if (!Application.isMobilePlatform) return;
        if (AndroidVersion >= androidVersion26)
        {
            AndroidJavaObject waveform = vibrationManager.CallStatic<AndroidJavaObject>("createWaveform", pattern, amplitude,repeat);
            vibratorObject.Call("vibrate", waveform);
        }
        else
        {
            vibratorObject.Call("vibrate", pattern, amplitude,repeat);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Create Bluetooth Android Java Class
    ////////////////////////////////////////////////////////////////////////////////
    public static void CreateBluetoothClass()
    {
        if (!Application.isMobilePlatform) return;
        //If not working display in two part
        bluetoothObject = currentActivity.Call<AndroidJavaObject>("getSystemService", "bluetooth").Call<AndroidJavaObject>("GetAdapter");
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Enable Bluetooth on Mobile
    ////////////////////////////////////////////////////////////////////////////////
    public static void EnableBluetooth()
    {
        bluetoothObject.Call<bool>("enable");
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Create WIFI Android Java Class
    ////////////////////////////////////////////////////////////////////////////////
    public static void CreateWifiClass()
    {
        if (!Application.isMobilePlatform) return;
        //If not working display in two part
        wifiObject = currentActivity.Call<AndroidJavaObject>("getSystemService", "wifi");
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Create a Request Access Checker for Camera
    ////////////////////////////////////////////////////////////////////////////////
    public static void RequestAccessToCamera()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Enable WIFI Module on Mobile
    ////////////////////////////////////////////////////////////////////////////////
    public static void EnableWifi()
    {
        wifiObject.Call<bool>("enable");
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Enable an Android Object
    ////////////////////////////////////////////////////////////////////////////////
    public static void CancelAndroidJavaClass(this AndroidJavaObject androidJavaObject)
    {
        androidJavaObject.Call<bool>("enable");
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Disable an Android Object 
    ////////////////////////////////////////////////////////////////////////////////
    public static void DisableAndroidObject(this AndroidJavaObject androidJavaObject)
    {
        androidJavaObject.Call<bool>("disable");
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Cancel an Android Java Class
    ////////////////////////////////////////////////////////////////////////////////
    public static void CancelAndroidJavaClass(this AndroidJavaClass androidJavaClass)
    {
        androidJavaClass.Call("cancel");
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // Cancel an Android Java Class
    ////////////////////////////////////////////////////////////////////////////////
}
