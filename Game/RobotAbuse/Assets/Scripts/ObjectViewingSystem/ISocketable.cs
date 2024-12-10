namespace RobotAbuse
{
    //An interface to add the ability to use sockets using the ObjectViewer and an IViewableObject
    public interface ISocketable
    {
       public PartSocket PartSocket { get; }
    }
}