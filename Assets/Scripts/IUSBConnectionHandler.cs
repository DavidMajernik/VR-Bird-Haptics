public interface IUSBConnection
{
    bool IsConnected { get; }
    void Write(byte[] data);
   
}
