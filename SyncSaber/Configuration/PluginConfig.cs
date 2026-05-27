using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SyncSaber.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public virtual bool AutoDownloadSongs { get; set; } = true;
        public virtual string BeastSaberUsername { get; set; } = "";
        public virtual int MaxFollowingsPages { get; set; } = 3;
        public virtual int MaxBookmarksPages { get; set; } = 3;
        public virtual int MaxCuratorRecommendedPages { get; set; } = 3;
        public virtual int MaxPPSongsCount { get; set; } = 500;
        [UseConverter(typeof(EnumConverter<ScoreSabers.ScoreSaberManager.RankSort>))]
        public virtual ScoreSabers.ScoreSaberManager.RankSort RankSort { get; set; } = ScoreSabers.ScoreSaberManager.RankSort.DateRanked;
        public virtual bool DeleteOldVersions { get; set; } = false;
        public virtual bool SyncBookmarksFeed { get; set; } = true;
        public virtual bool SyncCuratorRecommendedFeed { get; set; } = false;
        public virtual bool SyncFollowingsFeed { get; set; } = true;
        public virtual bool SyncPPSongs { get; set; } = true;


        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other

            this.AutoDownloadSongs = other.AutoDownloadSongs;
            this.BeastSaberUsername = other.BeastSaberUsername;
            this.MaxFollowingsPages = other.MaxFollowingsPages;
            this.MaxBookmarksPages = other.MaxBookmarksPages;
            this.MaxCuratorRecommendedPages = other.MaxCuratorRecommendedPages;
            this.DeleteOldVersions = other.DeleteOldVersions;
            this.SyncBookmarksFeed = other.SyncBookmarksFeed;
            this.SyncCuratorRecommendedFeed = other.SyncCuratorRecommendedFeed;
            this.SyncFollowingsFeed = other.SyncFollowingsFeed;
            this.MaxPPSongsCount = other.MaxPPSongsCount;
            this.RankSort = other.RankSort;
        }
    }
}
