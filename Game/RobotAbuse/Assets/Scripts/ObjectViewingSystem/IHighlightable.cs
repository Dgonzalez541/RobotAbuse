namespace RobotAbuse
{
    //An interface to add the ability to Highlight or Unhighlight using the ObjectViewer and an IViewableObject
    public interface IHighlightable
    {
        public bool IsHighlighted { get; }
        void Highlight();
        void Unhighlight();
    }
}