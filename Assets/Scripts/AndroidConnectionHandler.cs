using System;
using System.Collections;
using UnityEngine;

public class AndroidConnectionHandler : MonoBehaviour, IUSBConnection
{
    private const string PluginClassName = "com.usb.library.SerialMotor";

   

  
    [Header("Settings")]
    [SerializeField] private int baudRate = 115200;
    [SerializeField] private float retryInterval = 2f;
    [SerializeField] private int maxRetries = 10;

    private static AndroidJavaClass _pluginClass;
    private AndroidJavaObject _activity;

    public bool IsConnected { get; private set; }

    private enum ConnectionState { Disconnected, Initializing, WaitingPermission, Connecting, Connected, Failed }
    private ConnectionState _state = ConnectionState.Disconnected;

    void Awake()
    {
#if UNITY_ANDROID && ! UNITY_EDITOR
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        try
        {
            _pluginClass = new AndroidJavaClass(PluginClassName);
            Debug.Log($"Plugin class loaded: {PluginClassName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load plugin class: {e.Message}");
            _pluginClass = null;
        }
#else
        Debug.LogWarning("Android USB only active on device");
#endif
    }

    void Start()
    {
#if UNITY_ANDROID && ! UNITY_EDITOR
        StartCoroutine(USBConnectionRoutine());
#endif
    }

    // PUBLIC METHOD:  Call this to manually retry connection
    public void ManualRetry()
    {
        Debug.Log("Manual retry triggered");
        IsConnected = false;
        StopAllCoroutines();
        StartCoroutine(USBConnectionRoutine());
    }

    private IEnumerator USBConnectionRoutine()
    {
        if (_pluginClass == null)
        {
            Debug.LogError("Plugin not loaded, cannot connect");
          
            yield break;
        }

        // Wait for Unity activity and XR to fully initialize
        Debug.Log("Waiting 1. 5s for activity initialization...");
        yield return new WaitForSeconds(1.5f);

        int attempts = 0;
        while (attempts < maxRetries && !IsConnected)
        {
            attempts++;
            Debug.Log($"=== USB Connection Attempt {attempts}/{maxRetries} ===");

            // Step 1: Initialize
          
            bool initOk = false;
            try
            {
                initOk = _pluginClass.CallStatic<bool>("initializeSerial", _activity, baudRate);
                Debug.Log($"Initialize result: {initOk}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Initialize exception: {e.Message}");
            }

            if (!initOk)
            {
                Debug.LogWarning("Init failed, retrying...");
                yield return new WaitForSeconds(retryInterval);
                continue;
            }

            // Check device count
            int deviceCount = -1;
            try
            {
                deviceCount = _pluginClass.CallStatic<int>("getDeviceCount");
                Debug.Log($"USB devices detected: {deviceCount}");
            }
            catch (Exception e)
            {
                Debug.LogError($"getDeviceCount exception: {e.Message}");
            }

            if (deviceCount <= 0)
            {
                Debug.LogWarning("No USB devices found, retrying...");
                yield return new WaitForSeconds(retryInterval);
                continue;
            }

 
            bool hasPermission = false;
            try
            {
                hasPermission = _pluginClass.CallStatic<bool>("requestPermission", _activity);
                Debug.Log($"Permission granted: {hasPermission}");
            }
            catch (Exception e)
            {
                Debug.LogError($"requestPermission exception: {e.Message}");
            }

            if (!hasPermission)
            {
                Debug.LogWarning("Permission denied or failed, retrying...");
                yield return new WaitForSeconds(retryInterval);
                continue;
            }

            // Wait longer after permission and re-init to refresh device state
            Debug.Log("Waiting 1s after permission grant...");
            yield return new WaitForSeconds(1f);

            // Re-initialize to refresh device list
            try
            {
                _pluginClass.CallStatic<bool>("initializeSerial", _activity, baudRate);
            }
            catch (Exception e)
            {
                Debug.LogError($"Re-init exception:  {e.Message}");
            }

            // Step 3: Open serial port
         
            try
            {
                IsConnected = _pluginClass.CallStatic<bool>("openSerial");
                Debug.Log($"Serial port open: {IsConnected}");
            }
            catch (Exception e)
            {
                Debug.LogError($"openSerial exception: {e.Message}");
                IsConnected = false;
            }

            if (IsConnected)
            {
                Debug.Log("*** USB CONNECTION SUCCESS ***");
           

                yield break;
            }
            else
            {
                Debug.LogWarning("Port open failed, retrying...");
                yield return new WaitForSeconds(retryInterval);
            }
        }

        if (!IsConnected)
        {
            Debug.LogError($"Failed to connect after {maxRetries} attempts");
            
        }
    }

  

    public void Write(byte[] data)
    {
#if UNITY_ANDROID && ! UNITY_EDITOR
        if (! IsConnected || _pluginClass == null)
        {
            Debug.LogWarning("Write called but not connected");
            return;
        }
        try
        {
            _pluginClass.CallStatic("writeRawBytes", data);
            Debug.Log($"Wrote {data.Length} bytes");
        }
        catch (Exception e)
        {
            Debug.LogError($"Write error: {e.Message}");
        }
#endif
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && ! UNITY_EDITOR
        try
        {
            _pluginClass?.CallStatic("closeSerial");
            IsConnected = false;
            Debug.Log("Serial port closed");
        }
        catch (Exception e)
        {
            Debug.LogError($"closeSerial error: {e.Message}");
        }
#endif
    }
}