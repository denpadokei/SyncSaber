using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using SyncSaber.Configuration;
using System;
using System.Collections.Generic;
using Zenject;

namespace SyncSaber.UI
{
    [HotReload]
    internal class SettingViewController : BSMLAutomaticViewController, IInitializable
    {
        // For this method of setting the ResourceName, this class must be the first class in the file.
        public string ResourceName => string.Join(".", this.GetType().Namespace, "SettingView.bsml");

        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        [UIValue("auto-download-songs")]
        public virtual bool AutoDownloadSongs
        {
            get => PluginConfig.Instance.AutoDownloadSongs;
            set => PluginConfig.Instance.AutoDownloadSongs = value;
        }
        [UIValue("beastsaber-user-name")]
        public virtual string BeastSaberUsername
        {
            get => PluginConfig.Instance.BeastSaberUsername;
            set => PluginConfig.Instance.BeastSaberUsername = value;
        }
        [UIValue("max-followings-pages")]
        public virtual int MaxFollowingsPages
        {
            get => PluginConfig.Instance.MaxFollowingsPages;
            set => PluginConfig.Instance.MaxFollowingsPages = value;
        }
        [UIValue("max-boolmarls-pages")]
        public virtual int MaxBookmarksPages
        {
            get => PluginConfig.Instance.MaxBookmarksPages;
            set => PluginConfig.Instance.MaxBookmarksPages = value;
        }
        [UIValue("max-curator-recomended-pages")]
        public virtual int MaxCuratorRecommendedPages
        {
            get => PluginConfig.Instance.MaxCuratorRecommendedPages;
            set => PluginConfig.Instance.MaxCuratorRecommendedPages = value;
        }
        [UIValue("delete-old-versions")]
        public virtual bool DeleteOldVersions
        {
            get => PluginConfig.Instance.DeleteOldVersions;
            set => PluginConfig.Instance.DeleteOldVersions = value;
        }
        [UIValue("sync-bookmarks-feed")]
        public virtual bool SyncBookmarksFeed
        {
            get => PluginConfig.Instance.SyncBookmarksFeed;
            set => PluginConfig.Instance.SyncBookmarksFeed = value;
        }
        [UIValue("sync-creator-recomended-feed")]
        public virtual bool SyncCuratorRecommendedFeed
        {
            get => PluginConfig.Instance.SyncCuratorRecommendedFeed;
            set => PluginConfig.Instance.SyncCuratorRecommendedFeed = value;
        }
        [UIValue("sync-followings-feed")]
        public virtual bool SyncFollowingsFeed
        {
            get => PluginConfig.Instance.SyncFollowingsFeed;
            set => PluginConfig.Instance.SyncFollowingsFeed = value;
        }

        [UIValue("sync-pp-songs")]
        public virtual bool SyncPPSongs
        {
            get => PluginConfig.Instance.SyncPPSongs;
            set => PluginConfig.Instance.SyncPPSongs = value;
        }
        [UIValue("sort-modes")]
        public List<object> SortModes { get; set; } = new List<object>() { "DateRanked", "Difficurity" };
        [UIValue("current-mode")]
        public object CurrentMode
        {
#pragma warning disable CS0162 // 到達できないコードが検出されました
            get
            {
                switch (PluginConfig.Instance.RankSort) {
                    case ScoreSabers.ScoreSaberManager.RankSort.DateRanked:
                        return this.SortModes[0];
                        break;
                    case ScoreSabers.ScoreSaberManager.RankSort.StarDifficulity:
                        return this.SortModes[1];
                        break;
                    default:
                        return this.SortModes[0];
                        break;
                }
            }
#pragma warning restore CS0162 // 到達できないコードが検出されました
            set => PluginConfig.Instance.RankSort = value == this.SortModes[0]
                    ? ScoreSabers.ScoreSaberManager.RankSort.DateRanked
                    : ScoreSabers.ScoreSaberManager.RankSort.StarDifficulity;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // コマンド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // コマンド用メソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // オーバーライドメソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void Initialize()
        {
            try {
                BSMLSettings.instance.AddSettingsMenu("SYNC SABER", this.ResourceName, this);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        protected override void OnDestroy()
        {
            BSMLSettings.instance.RemoveSettingsMenu(this);
            base.OnDestroy();
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        #endregion
    }
}
