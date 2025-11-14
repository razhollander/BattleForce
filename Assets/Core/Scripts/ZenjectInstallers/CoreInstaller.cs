using CoreDomain.Scripts.Audio;
using CoreDomain.Scripts.Mvc.LoadingScreen;
using CoreDomain.Scripts.Mvc.UICamera;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.AddressablesLoader;
using CoreDomain.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.DataPersistence;
using CoreDomain.Scripts.Services.InitiatorInvokerService;
using CoreDomain.Scripts.Services.Logger;
using CoreDomain.Scripts.Services.ResourcesLoaderService;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.Serializers.Serializer;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.ZenjectInstallers
{
    public class CoreInstaller : MonoInstaller
    {
        [SerializeField] private CoreAudioClipsScriptableObject _coreAudioClipsScriptableObject;
        [SerializeField] private UpdateSubscriptionService _updateSubscriptionService;
        [SerializeField] private AudioService _audioService;
        [SerializeField] private LoadingScreenView _loadingScreenView;
        [SerializeField] private UICameraView _uiCameraView;
        [SerializeField] private WorldCameraView _worldCameraView;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<UnityLogger>().AsSingle().NonLazy();
            Container.BindInterfacesTo<SceneLoaderService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<AddressablesLoaderService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ResourcesLoaderService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<StateMachineService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<UpdateSubscriptionService>().FromInstance(_updateSubscriptionService).AsSingle().NonLazy();
            Container.BindInterfacesTo<AudioService>().FromInstance(_audioService).AsSingle().NonLazy();
            Container.BindInterfacesTo<SerializerService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<PlayerPrefsDataPersistence>().AsSingle().NonLazy();
            Container.BindInterfacesTo<SceneInitiatorsService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<CommandFactory>().AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.BindInterfacesTo<LoadingScreenController>().AsSingle().WithArguments(_loadingScreenView).NonLazy();
            Container.BindInterfacesTo<UICameraController>().AsSingle().WithArguments(_uiCameraView).NonLazy();
            Container.BindInterfacesTo<WorldCameraController>().AsSingle().WithArguments(_worldCameraView).NonLazy();
            Container.Bind<CoreAudioClipsScriptableObject>().FromScriptableObject(_coreAudioClipsScriptableObject).AsSingle().NonLazy();
            Container.Bind<GameInputActions>().AsSingle().NonLazy();
        }
    }
}
