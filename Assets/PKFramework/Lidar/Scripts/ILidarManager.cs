namespace PKFramework.Lidar
{
    public delegate void LidarDataReceived(LidarData data);
    public interface ILidarManager
    {
        event LidarDataReceived OnDataReceived;
    }
}