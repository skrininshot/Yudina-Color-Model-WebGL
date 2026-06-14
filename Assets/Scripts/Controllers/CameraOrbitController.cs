using Models;
using UnityEngine;
using Views;
using Zenject;

namespace Controllers
{
    public class CameraOrbitController : IInitializable, ITickable
    {
        private readonly Transform cameraPivot;
        private readonly CameraOrbitConfig cameraOrbitConfig;
        private readonly DragArea dragArea;

        private float yaw;
        private float pitch;
        
        private Vector2 lastInputPosition;

        public CameraOrbitController(
            Transform cameraPivot,
            CameraOrbitConfig cameraOrbitConfig,
            [InjectOptional] DragArea dragArea = null)
        {
            this.cameraPivot = cameraPivot;
            this.cameraOrbitConfig = cameraOrbitConfig;
            this.dragArea = dragArea;
        }

        public void Initialize()
        {
            Vector3 initialEuler = cameraPivot.rotation.eulerAngles;
            yaw = initialEuler.y;
            pitch = initialEuler.x;

            if (dragArea != null)
            {
                dragArea.OnDragMove += HandleDragMove;
            }
        }

        public void Tick()
        {
            if (dragArea == null)
                HandleDirectInput();

            ApplyRotation();
        }

        private void HandleDirectInput()
        {
            Vector2 currentInputPos = Vector2.zero;
            bool inputActive = false;

            if (Input.touchSupported && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    currentInputPos = touch.position;
                    inputActive = true;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                currentInputPos = Input.mousePosition;
                inputActive = true;
            }

            if (inputActive)
            {
                Vector2 delta = currentInputPos - lastInputPosition;
                yaw += delta.x * cameraOrbitConfig.sensitivity * 0.1f;
                pitch -= delta.y * cameraOrbitConfig.sensitivity * 0.1f;
                pitch = Mathf.Clamp(pitch, cameraOrbitConfig.minPitch, cameraOrbitConfig.maxPitch);
                lastInputPosition = currentInputPos;
            }
        }

        private void HandleDragMove(Vector2 delta)
        {
            yaw += delta.x * cameraOrbitConfig.sensitivity * 0.1f;
            pitch -= delta.y * cameraOrbitConfig.sensitivity * 0.1f;
            pitch = Mathf.Clamp(pitch, cameraOrbitConfig.minPitch, cameraOrbitConfig.maxPitch);
        }

        private void ApplyRotation()
        {
            cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}