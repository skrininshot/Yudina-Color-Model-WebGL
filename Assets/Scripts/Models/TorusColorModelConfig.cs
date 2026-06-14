using System;
using UnityEngine;

namespace Models
{
    [CreateAssetMenu( menuName = "Torus Color Model Config", fileName = "TorusColorModelConfig" )]
    public class TorusColorModelConfig : ColorModelConfig
    {
        [field: SerializeField] public int ringCount { get; private set; }
        [field: SerializeField] public int slicesPerRing { get; private set; }
        [field: SerializeField] public float saturation { get; private set; }
        [field: SerializeField] public float value { get; private set; }


        private void Reset()
        {
            ringCount = 6;
            slicesPerRing = 12;
            saturation = 360;
            value = 360;
        }

        public float GetHue(int ring, int slice)
        {
            float huePerSlice = 360f / slicesPerRing;
            float hueShiftPerRing = 360f / ringCount;
            return (slice * huePerSlice + ring * hueShiftPerRing) % 360f;
        }
    }
}