using System;
using Models;
using Services;
using UnityEngine;
using Views;
using Zenject;

namespace Controllers
{
    using System;
    using System.Collections.Generic;
    using Models;
    using UnityEngine;
    using Zenject;

    namespace Controllers
    {
        public class TorusMeshViewController : IInitializable, IDisposable, ITickable
        {
            private readonly TorusMeshGenerator generator;
            private readonly Transform meshContainer;
            private readonly ColorModelConfig colorConfig;
            private readonly SliceKnob sliceKnob;
            private readonly TorusModel torusModel;
            private readonly int ringCount;

            public TorusMeshViewController(
                TorusMeshGenerator generator,
                Transform meshContainer,
                ColorModelConfig colorConfig,
                SliceKnob sliceKnob,
                TorusModel torusModel,
                [InjectOptional] int? ringCount = null)
            {
                this.generator = generator;
                this.meshContainer = meshContainer;
                this.colorConfig = colorConfig;
                this.sliceKnob = sliceKnob;
                this.torusModel = torusModel;
                this.ringCount = ringCount ?? 6;
            }

            public void Initialize()
            {
                generator.Generate(meshContainer, colorConfig);
            }

            public void Dispose()
            {
                
            }

            public void Tick()
            {
                UpdateSliceVisibility();
            }

            private void UpdateSliceVisibility()
            {
                int actualRingCount = torusModel.Rings.Count;
                if (actualRingCount == 0)
                    return;

                float cutAngle = Mathf.Repeat(sliceKnob.Value, 360f);
                float angleStep = 360f / actualRingCount;
                int halfCount = actualRingCount / 2;

                // Индекс кольца, внутри которого находится линия среза (оно будет скрыто)
                int hiddenStart = Mathf.FloorToInt(cutAngle / angleStep) % actualRingCount;

                // Массив видимости размером с реальное число колец
                bool[] visibility = new bool[actualRingCount];
                for (int i = 0; i < halfCount; i++)
                {
                    int idx = (hiddenStart + 1 + i) % actualRingCount;
                    visibility[idx] = true;
                }

                for (int i = 0; i < actualRingCount; i++)
                {
                    torusModel.Rings[i].gameObject.SetActive(visibility[i]);
                }
            }
            
            private bool IsAngleBetween(float angle, float start, float halfRange)
            {
                float diff = Mathf.Repeat(angle - start, 360f);
                return diff > 0f && diff <= halfRange;
            }
        }
    }
}