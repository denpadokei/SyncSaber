using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using SyncSaber.Interfaces;
using SyncSaber.Utilities.PlaylistDownLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SyncSaber.Views
{
    [HotReload]
    internal class NotificationViewController : BSMLAutomaticViewController, IInitializable
    {
        /// <summary>説明 を取得、設定</summary>
        private string notificationText_;
        /// <summary>説明 を取得、設定</summary>
        [UIValue("notification-text")]
        public string NotificationText
        {
            get => this.notificationText_ ?? "";

            set
            {
                if (!this._floatingScreen.isActiveAndEnabled && !string.IsNullOrEmpty(notificationText_)) {
                    this._floatingScreen.gameObject.SetActive(true);
                }
                this.notificationText_ = value;
                this.NotifyPropertyChanged();
                if (string.IsNullOrEmpty(notificationText_)) {
                    this._floatingScreen.gameObject.SetActive(false);
                }
            }
        }
        private ISyncSaber _syncSaber;
        private DateTime _uiResetTime;
        private FloatingScreen _floatingScreen;

        [Inject]
        protected void Constractor(ISyncSaber syncSaber)
        {
            this._syncSaber = syncSaber;
        }

        private void Awake()
        {
            Logger.Debug("Awake call");
            DontDestroyOnLoad(this);
        }

        protected override void OnDestroy()
        {
            Logger.Debug("OnDestroy call");
            base.OnDestroy();
        }

        private void NotificationTextChange(string obj)
        {
            this.NotificationText = obj;
            this._uiResetTime = DateTime.Now.AddSeconds(5);
        }

        private void Update()
        {
            if (!string.IsNullOrEmpty(this.NotificationText) && this._uiResetTime <= DateTime.Now) {
                this.NotificationText = "";
            }
        }

        public async void Initialize()
        {
            Logger.Debug("Initialize call");
            try {
                this._floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 20f), false, new Vector3(0f, 0.3f, 2.8f), new Quaternion(0f, 0f, 0f, 0f));
                this._floatingScreen.SetRootViewController(this, AnimationType.None);
                foreach (var g in this._floatingScreen.gameObject.GetComponentsInChildren<Graphic>()) {
                    g.raycastTarget = false;
                }
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            try {
                this._syncSaber.NotificationTextChange -= this.NotificationTextChange;
                this._syncSaber.NotificationTextChange += this.NotificationTextChange;
                if (Utility.IsPlaylistDownLoaderInstalled()) {
                    this._syncSaber.SetEvent();
                }
                await this._syncSaber.Sync();
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            this._syncSaber.StartTimer();
        }
    }
}
