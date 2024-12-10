namespace RobotAbuse
{
    //Allows IVieweableObjects to Highlight and UnHighlight in ObjectViewer.
    public interface IHighlightable
    {
        public bool IsHighlighted { get; }
        void Highlight();
        void Unhighlight();
    }
}