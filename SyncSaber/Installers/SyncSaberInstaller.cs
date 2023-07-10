using SyncSaber.UI;
using SyncSaber.Views;
using Zenject;

namespace SyncSaber.Installers
{
    public class SyncSaberInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<SyncSaberController>().FromNewComponentOnNewGameObject().AsSingle();
            this.Container.BindInterfacesAndSelfTo<NotificationViewController>().FromNewComponentAsViewController().AsSingle().NonLazy();
            this.Container.BindInterfacesAndSelfTo<SongListUtil>().AsCached();
            this.Container.BindInterfacesAndSelfTo<SettingViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}
