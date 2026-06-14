using Controllers;
using Controllers.Controllers;
using Models;
using Services;
using UnityEngine;
using Views;
using Zenject;

namespace Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [Header("Camera Settings")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private CameraOrbitConfig cameraOrbitConfig;
        
        [Header("Mesh Settings")]
        [SerializeField] private Transform meshContainer;
        [SerializeField] private ColorModelConfig colorConfig;
        
        [Header("UI Settings")]
        [SerializeField] private DragArea cameraDragArea;
        [SerializeField] private SliceKnob sliceKnob;
        
        public override void InstallBindings()
        {
            Container.Bind<ColorModelConfig>().FromInstance(colorConfig).AsSingle();
            Container.Bind<TorusModel>().FromInstance(new TorusModel()).AsSingle();
            Container.BindInterfacesAndSelfTo<TorusMeshGenerator>().AsSingle();
            Container.BindInterfacesAndSelfTo<TorusMeshViewController>().AsSingle().WithArguments(meshContainer, sliceKnob);
            Container.BindInterfacesAndSelfTo<CameraOrbitController>().AsSingle().WithArguments(cameraPivot, cameraOrbitConfig, cameraDragArea);
        }
    }
}