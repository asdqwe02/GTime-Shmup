namespace PKFramework.SerialPort.Scripts
{
    public delegate void PortOpened(string port);

    public delegate void PortClosed(string port);

    public delegate void MessageReceived(string port, string message);
    public interface ISerialPortManager
    {
        event PortOpened OnPortOpened;
        event PortClosed OnPortClosed;
        event MessageReceived OnMessageReceived;

        public void SendMessage(string message, string portName = default);
    }
}