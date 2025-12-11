using UnityEngine;

public class MotorControl : MonoBehaviour
{
    [SerializeField] private AndroidConnectionHandler usb;

    void Start()
    {
        Debug.Log($"MotorControl started, USB handler assigned: {usb != null}");
    }

    public void SpinForward()
    {

        if (usb == null)
        {
            Debug.LogError("USB handler not assigned!");
            return;
        }

        if (!usb.IsConnected)
        {
            Debug.LogWarning("USB not connected, cannot spin motor");
            return;
        }

        byte[] command = new byte[] { 0x01 };
        usb.Write(command);
    }

    public void SpinBackward()
    {

        if (usb == null)
        {
            Debug.LogError("USB handler not assigned!");
            return;
        }

        if (!usb.IsConnected)
        {
            Debug.LogWarning("USB not connected, cannot spin motor");
            return;
        }

        byte[] command = new byte[] { 0x02 };
        usb.Write(command);
    }

    public void SpinStop()
    {

        if (usb == null)
        {
            Debug.LogError("USB handler not assigned!");
            return;
        }

        if (!usb.IsConnected)
        {
            Debug.LogWarning("USB not connected, cannot spin motor");
            return;
        }

        byte[] command = new byte[] { 0x00 };
        usb.Write(command);
    }
}