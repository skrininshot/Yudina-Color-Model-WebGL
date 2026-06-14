using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class TorusRingViewModel
    {
        public readonly TorusRingModel ringModel;
        public readonly GameObject gameObject;
        
        public TorusRingViewModel(TorusRingModel ringModel, GameObject gameObject)
        {
            this.ringModel = ringModel;
            this.gameObject = gameObject;
        }
    }
}