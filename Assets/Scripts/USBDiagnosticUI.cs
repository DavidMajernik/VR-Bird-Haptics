using UnityEngine;
using TMPro; 

public class USBDiagnosticUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI diagnosticText; 
    [SerializeField] private AndroidConnectionHandler usbHandler;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 1f;

    private AndroidJavaClass _pluginClass;
    private float _lastUpdate;

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            _pluginClass = new AndroidJavaClass("com.usb.library.SerialMotor");
        }
        catch (System.Exception e)
        {
            UpdateText("Failed to load plugin:\n" + e.Message);
        }
#else
        UpdateText("USB diagnostics only available on Android device");
#endif
    }

    void Update()
    {
#if UNITY_ANDROID && ! UNITY_EDITOR
        if (Time.time - _lastUpdate >= updateInterval)
        {
            _lastUpdate = Time.time;
            UpdateDiagnostics();
        }
#endif
    }

    private void UpdateDiagnostics()
    {
        if (_pluginClass == null)
        {
            UpdateText("Plugin not loaded");
            return;
        }

        try
        {
            string diag = _pluginClass.CallStatic<string>("getDiagnosticInfo");
            bool isConnected = usbHandler != null && usbHandler.IsConnected;
            bool isOpen = _pluginClass.CallStatic<bool>("isOpen");

            string status = $"=== USB STATUS ===\n";
            status += $"Connected:  {isConnected}\n";
            status += $"Port Open: {isOpen}\n";
            status += $"Time: {System.DateTime.Now:HH:mm:ss}\n\n";
            status += diag;

            UpdateText(status);
        }
        catch (System.Exception e)
        {
            UpdateText("Error getting diagnostics:\n" + e.Message);
        }
    }

    private void UpdateText(string text)
    {
        if (diagnosticText != null)
        {
            diagnosticText.text = text;
        }
    }

    // Call this from a button to force refresh
    public void RefreshDiagnostics()
    {
        UpdateDiagnostics();
    }

    // Call this from a button to retry connection
    public void RetryConnection()
    {
        if (usbHandler != null)
        {
            usbHandler.ManualRetry();
        }
        UpdateText("Connection retry triggered.. .");
    }
}