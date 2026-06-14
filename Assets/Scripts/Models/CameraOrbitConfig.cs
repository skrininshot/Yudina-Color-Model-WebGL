using UnityEngine;

namespace Models
{
    [CreateAssetMenu( menuName = "Camera Orbit Config", fileName = "CameraOrbitConfig" )]
    public class CameraOrbitConfig : ScriptableObject
    {
        [field: SerializeField] public float sensitivity { get; private set; }
        [field: SerializeField] public float minPitch { get; private set; }
        [field: SerializeField] public float maxPitch { get; private set; }

        private void Reset()
        {
            sensitivity = 0.5f;
            minPitch = -89f;
            maxPitch = 89f;
        }
    }
}