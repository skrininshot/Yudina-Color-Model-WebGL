using UnityEngine;
using UnityEngine.EventSystems;

namespace Views
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        protected Vector2 lastDragPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            lastDragPosition = eventData.position;
            OnDragStarted(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - lastDragPosition;
            lastDragPosition = eventData.position;
            OnDragMoved(delta, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragEnded(eventData);
        }

        protected virtual void OnDragStarted(PointerEventData eventData) { }
        protected virtual void OnDragMoved(Vector2 delta, PointerEventData eventData) { }
        protected virtual void OnDragEnded(PointerEventData eventData) { }
    }
}