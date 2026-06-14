using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class TorusModel
    {
        private List<TorusRingViewModel> rings = new ();
        public IReadOnlyList<TorusRingViewModel> Rings => rings;

        public void Add(TorusRingViewModel ring)
        {
            rings.Add(ring);
        }
    }
}