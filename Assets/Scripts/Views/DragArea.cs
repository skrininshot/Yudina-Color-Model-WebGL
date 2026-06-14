using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Views
{
    public class DragArea : DragHandler
    {
        public event Action<Vector2> OnDragStart;
        public event Action<Vector2> OnDragMove;
        public event Action OnDragEnd;

        protected override void OnDragStarted(PointerEventData eventData)
        {
            OnDragStart?.Invoke(eventData.position);
        }

        protected override void OnDragMoved(Vector2 delta, PointerEventData eventData)
        {
            OnDragMove?.Invoke(delta);
        }

        protected override void OnDragEnded(PointerEventData eventData)
        {
            OnDragEnd?.Invoke();
        }
    }
}