using UnityEngine;

namespace RobotAbuse
{
    public interface IHighlightable
    {
        public bool IsHighlighted { get; }
        void Highlight();
        void Unhighlight();
    }
}