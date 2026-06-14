using Models;
using UnityEngine;

namespace Services
{
    public interface IMeshGenerator
    {
        public void Generate(Transform container, ColorModelConfig config);
    }
}