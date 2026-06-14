using UnityEngine;
using UnityEngine.EventSystems;

namespace Views
{
    public class SliceKnob : DragHandler
    {
        [SerializeField] private RectTransform needle;
        [SerializeField] private float sensitivity = 0.5f;

        private float accumulatedAngle;

        public float Value => NormalizeAngle(accumulatedAngle);

        private void Awake()
        {
            if (needle != null)
                accumulatedAngle = needle.localEulerAngles.z;
        }

        protected override void OnDragMoved(Vector2 delta, PointerEventData eventData)
        {
            if (Mathf.Abs(delta.x) < 0.1f)
                return;

            accumulatedAngle += delta.x * sensitivity;
            if (needle != null)
                needle.localRotation = Quaternion.Euler(0, 0, accumulatedAngle);
        }

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f)
                angle += 360f;
            return angle;
        }
    }
}