using System.Collections.Generic;

namespace Models
{
    public class TorusRingModel
    {
        public IReadOnlyList<TorusRingSliceModel> Slices => _slices;
        private List<TorusRingSliceModel> _slices = new();

        public void AddSlice(TorusRingSliceModel slice)
        {
            _slices.Add(slice);
        }
    }
}