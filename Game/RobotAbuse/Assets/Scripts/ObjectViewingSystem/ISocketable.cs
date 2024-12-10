namespace RobotAbuse
{
    //Allows IViewableObjects to use the ObjectViewer's socket system.
    public interface ISocketable
    {
       public PartSocket PartSocket { get; }
    }
}