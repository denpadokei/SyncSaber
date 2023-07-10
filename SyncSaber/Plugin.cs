using BeatSaberMarkupLanguage.Settings;
using BS_Utils.Utilities;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using SyncSaber.Installers;
using SyncSaber.UI;
using SyncSaber.Utilities.PlaylistDownLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace SyncSaber
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public bool IsInGame { get; private set; }
        public static HashSet<string> SongDownloadHistory { get; } = new HashSet<string>();
        internal static Plugin Instance { get; private set; }
        public string Name => "SyncSaber";

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, IPA.Config.Config conf, Zenjector zenjector)
        {
            Instance = this;
            Logger.Log = logger;
            Logger.Log.Debug("Logger initialized.");
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Logger.Log.Debug("Config loaded");
            zenjector.Install<SyncSaberInstaller>(Location.Menu);
        }

        private async Task DelayedStartup()
        {
            await Task.Delay(500);
            if (Utility.IsModInstalled("MapperFeed")) {
                File.Move("Plugins\\BeatSaberMapperFeed.dll", "Plugins\\BeatSaberMapperFeed.dll.delete-me");
                //_mapperFeedNotification = Utilities.CreateNotificationText("Old version of MapperFeed detected! Restart the game now to enable SyncSaber!");
                Logger.Info("Old MapperFeed detected!");
            }
            //SyncSaber.OnLoad();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Instance = this;

            BSEvents.earlyMenuSceneLoadedFresh += this.BSEvents_earlyMenuSceneLoadedFresh;
            SceneManager.activeSceneChanged += this.SceneManagerOnActiveSceneChanged;
            _ = this.DelayedStartup();
        }

        private void BSEvents_earlyMenuSceneLoadedFresh(ScenesTransitionSetupDataSO obj)
        {
            
        }


        [OnExit]
        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= this.SceneManagerOnActiveSceneChanged;
            BSEvents.earlyMenuSceneLoadedFresh -= this.BSEvents_earlyMenuSceneLoadedFresh;
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            this.IsInGame = scene.name == "GameCore";
        }
    }
}
