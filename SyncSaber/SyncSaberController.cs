using SongCore;
using SyncSaber.Configuration;
using SyncSaber.Extentions;
using SyncSaber.Interfaces;
using SyncSaber.NetWorks;
using SyncSaber.ScoreSabers;
using SyncSaber.SimpleJSON;
using SyncSaber.Statics;
using SyncSaber.Utilities;
using SyncSaber.Utilities.PlaylistDownLoader;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SyncSaber
{
    public class SyncSaberController : MonoBehaviour, ISyncSaber
    {
        private SemaphoreSlim _semaphoreSlim;
        private System.Timers.Timer SyncTimer { get; set; }

        private static readonly string s_historyPath = Path.Combine(Environment.CurrentDirectory, "UserData", "SyncSaberHistory.txt");
        private static readonly string s_favoriteMappersPath = Path.Combine(Environment.CurrentDirectory, "UserData", "FavoriteMappers.ini");
        private static readonly string s_customLevelsPath = Path.Combine(Environment.CurrentDirectory, "Beat Saber_Data", "CustomLevels");

        private static readonly Regex s_invalidDirectoryAndFileChars = new Regex($@"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))}]");

        private ConcurrentStack<string> AuthorDownloadQueue { get; } = new ConcurrentStack<string>();
        public static HashSet<string> SongDownloadHistory => Plugin.SongDownloadHistory;
        private readonly Playlist _syncSaberSongs = new Playlist("SyncSaberPlaylist", "SyncSaber Playlist", "brian91292", "1");
        private readonly Playlist _curatorRecommendedSongs = new Playlist("SyncSaberCuratorRecommendedPlaylist", "BeastSaber Curator Recommended", "brian91292", "1");
        private readonly Playlist _followingsSongs = new Playlist("SyncSaberFollowingsPlaylist", "BeastSaber Followings", "brian91292", "1");
        private readonly Playlist _bookmarksSongs = new Playlist("SyncSaberBookmarksPlaylist", "BeastSaber Bookmarks", "brian91292", "1");

        public event Action<string> NotificationTextChange;
        private bool _initialized = false;
        private volatile bool _didDownloadAnySong = false;
        /// <summary>
        /// ほんとはボックス化しちゃうからEnumをDictionaryに放り込みたくない
        /// </summary>
        private readonly Dictionary<DownloadFeed, string> _beastSaberFeeds = new Dictionary<DownloadFeed, string>();
        private DiContainer _diContainer;
        private SongListUtil _utils;

#pragma warning disable IDE1006 // 命名スタイル
        public const string ROOT_BEATSAVER_URL = "https://api.beatsaver.com";
        public const string DOWNLOAD_URL = "https://cdn.beatsaver.com";
        public const string ROOT_BEASTSABER_URL = "https://bsaber.com/wp-json/bsaber-api/songs";
#pragma warning restore IDE1006 // 命名スタイル

        [Inject]
        protected void Constractor(DiContainer container, SongListUtil songListUtil)
        {
            this._diContainer = container;
            this._utils = songListUtil;
        }

        #region UinityMethod
        private void Awake()
        {
            Logger.Info("Start Awake.");
            DontDestroyOnLoad(this);
            if (this._semaphoreSlim == null) {
                this._semaphoreSlim = new SemaphoreSlim(1, 1);
            }
            if (this.SyncTimer == null) {
#if DEBUG
                this.SyncTimer = new System.Timers.Timer(new TimeSpan(0, 0, 30).TotalMilliseconds);
#else
                this.SyncTimer = new System.Timers.Timer(new TimeSpan(0, 30, 0).TotalMilliseconds);
#endif
            }
            this._beastSaberFeeds.Add(DownloadFeed.Followings, $"{ROOT_BEASTSABER_URL}/?followed_by=%BeastSaberUserName%&count=200&page=%PageNumber%");
            this._beastSaberFeeds.Add(DownloadFeed.Bookmarks, $"{ROOT_BEASTSABER_URL}/?bookmarked_by=%BeastSaberUserName%&count=200&page=%PageNumber%");
            this._beastSaberFeeds.Add(DownloadFeed.CuratorRecommended, $"{ROOT_BEASTSABER_URL}/?bookmarked_by=curatorrecommended&count=50&page=%PageNumber%");

            if (File.Exists(s_historyPath + ".bak")) {
                // Something went wrong when the history file was being written previously, restore it from backup
                if (File.Exists(s_historyPath)) {
                    File.Delete(s_historyPath);
                }

                File.Move(s_historyPath + ".bak", s_historyPath);
            }
            if (File.Exists(s_historyPath)) {
                SongDownloadHistory.Clear();
                SongDownloadHistory.AddRange(File.ReadAllLines(s_historyPath));
            }
            if (!Directory.Exists(s_customLevelsPath)) {
                Directory.CreateDirectory(s_customLevelsPath);
            }

            if (!File.Exists(s_favoriteMappersPath)) {
#if DEBUG
                File.WriteAllLines(s_favoriteMappersPath, new string[] { "denpadokei", "ejiejidayo", "fefy" });
#else
                File.WriteAllLines(s_favoriteMappersPath, new string[] { "" });
#endif
            }
            this.SyncTimer.Elapsed -= this.Timer_Elapsed;
            this.SyncTimer.Elapsed += this.Timer_Elapsed;
            Logger.Info("Finish Awake.");
        }
        #endregion
        public void Initialize()
        {
            Logger.Debug("initialize call.");
            this.FinishInitialization();
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender is System.Timers.Timer timer) {
                timer.Stop();
                await this.Sync();
                timer.Start();
            }
        }

        private void FinishInitialization()
        {
            this.DisplayNotification("SyncSaber Initialized!");
            this._initialized = true;
        }

        private void DisplayNotification(string text)
        {
            if (Plugin.Instance?.IsInGame == true && !string.IsNullOrEmpty(text)) {
                return;
            }
            Logger.Info(text);
            MainThreadInvoker.Instance.Enqueue(() =>
            {
                NotificationTextChange?.Invoke("SyncSaber - " + text);
            });
        }

        public async Task Sync()
        {
            var tasks = new List<Task>();
            foreach (var mapper in File.ReadAllLines(s_favoriteMappersPath)) {
                Logger.Info($"Mapper: {mapper}");
                this.AuthorDownloadQueue.Push(mapper);
            }
            try {
                await this._semaphoreSlim.WaitAsync();
                this._didDownloadAnySong = false;
                while (!this._initialized || !Loader.AreSongsLoaded) {
                    await Task.Delay(200);
                }

                while (this.AuthorDownloadQueue.TryPop(out var author)) {
                    tasks.Add(this.DownloadAllSongsByAuthor(author));
                }
                foreach (var feed in Enum.GetValues(typeof(DownloadFeed)).OfType<DownloadFeed>()) {
                    switch (feed) {
                        case DownloadFeed.Followings:
                            if (!PluginConfig.Instance.SyncFollowingsFeed || string.IsNullOrWhiteSpace(PluginConfig.Instance.BeastSaberUsername)) {
                                continue;
                            }
                            break;
                        case DownloadFeed.Bookmarks:
                            if (!PluginConfig.Instance.SyncBookmarksFeed || string.IsNullOrWhiteSpace(PluginConfig.Instance.BeastSaberUsername)) {
                                continue;
                            }
                            break;
                        case DownloadFeed.CuratorRecommended:
                            if (!PluginConfig.Instance.SyncCuratorRecommendedFeed) {
                                continue;
                            }
                            break;
                        default:
                            break;
                    }
                    tasks.Add(this.DownloadBeastSaberFeeds(feed));
                }
                if (PluginConfig.Instance.SyncPPSongs) {
                    tasks.Add(this.DownloadPPSongs());
                }
                while (!Loader.AreSongsLoaded || Loader.AreSongsLoading) {
                    await Task.Delay(200);
                }
                await Task.WhenAll(tasks);
                this.StartCoroutine(this.BeforeDownloadSongs());
                if (Utility.IsPlaylistDownLoaderInstalled()) {
                    this.StartCoroutine(this.CheckPlaylist());
                }
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            finally {
                this._semaphoreSlim.Release();
            }
        }

        public void StartTimer()
        {
            this.SyncTimer.Start();
        }

        private int GetMaxBeastSaberPages(DownloadFeed feedToDownload)
        {
            switch (feedToDownload) {
                case DownloadFeed.Followings:
                    return PluginConfig.Instance.MaxFollowingsPages;
                case DownloadFeed.Bookmarks:
                    return PluginConfig.Instance.MaxBookmarksPages;
                case DownloadFeed.CuratorRecommended:
                    return PluginConfig.Instance.MaxCuratorRecommendedPages;
                default:
                    return 0;
            }
        }

        private IEnumerator BeforeDownloadSongs()
        {
            yield return new WaitWhile(() => !Loader.AreSongsLoaded || Loader.AreSongsLoading || Plugin.Instance.IsInGame);
            if (this._didDownloadAnySong) {
                yield return this.StartCoroutine(this._utils.RefreshSongs());
            }
            yield return new WaitWhile(() => !Loader.AreSongsLoaded || Loader.AreSongsLoading || Plugin.Instance.IsInGame);
            this.DisplayNotification("Finished checking for new songs!");
            yield return null;
            this._didDownloadAnySong = false;
        }

        private Playlist GetPlaylistForFeed(DownloadFeed feedToDownload)
        {
            switch (feedToDownload) {
                case DownloadFeed.Followings:
                    return this._followingsSongs;
                case DownloadFeed.Bookmarks:
                    return this._bookmarksSongs;
                case DownloadFeed.CuratorRecommended:
                    return this._curatorRecommendedSongs;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="hash"></param>
        /// <param name="songName"></param>
        private void UpdatePlaylist(Playlist playlist, string hash, string songName)
        {
            // Update our playlist with the new song if it doesn't exist, or replace the old song id/name with the updated info if it does
            var playlistSongFound = playlist.Songs.Any(x => x.hash.ToUpper() == hash.ToUpper());

            if (!playlistSongFound) {
                playlist.Add(hash, songName);
                Logger.Info($"Success adding new song \"{songName}\" with BeatSaver index {hash} to playlist {playlist.Title}!");
            }
        }



        private void RemoveOldVersions(string hash)
        {
            if (!PluginConfig.Instance.DeleteOldVersions) {
                return;
            }

            var levelMap = Loader.CustomLevels.FirstOrDefault(x => x.Value.levelID.ToUpper() == $"CUSTOM_LEVEL_{hash.ToUpper()}");

            if (levelMap.Key != null) {
                try {
                    Loader.Instance.RemoveSongWithLevelID(levelMap.Value.levelID);
                    var directoryName = levelMap.Key;
                    Directory.Delete(directoryName, true);
                }
                catch (Exception e) {
                    Logger.Info($"Exception when trying to remove old versions {e}");
                }
            }
        }

        private async Task DownloadAllSongsByAuthor(string author)
        {
            if (string.IsNullOrEmpty(author)) {
                return;
            }

            Logger.Info($"Downloading all songs from {author}");
            JSONNode result;
            var stopWatch = new Stopwatch();
            var pageCount = 0;
            var docsCount = 0;
            while (Plugin.Instance?.IsInGame == true) {
                await Task.Delay(200);
            }
            this.DisplayNotification($"Checking {author}'s maps. ({pageCount} page)");
            var res = await WebClient.GetAsync($"{ROOT_BEATSAVER_URL}/search/text/0?q={author}", new CancellationTokenSource().Token).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode) {
                Logger.Info($"{res.StatusCode}");
                return;
            }
            result = JSON.Parse(res.ContentToString());
            if (!result["user"].IsObject) {
                return;
            }
            var user = result["user"].AsObject;
            var userid = user["id"].AsInt;
            try {
                stopWatch.Start();
                do {
                    res = await WebClient.GetAsync($"{ROOT_BEATSAVER_URL}/maps/uploader/{userid}/{pageCount}", new CancellationTokenSource().Token).ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode) {
                        Logger.Info($"{res.StatusCode}");
                        return;

                    }
                    var docs = result["docs"].AsArray;
                    docsCount = docs.Count;
                    result = JSON.Parse(res.ContentToString());


                    foreach (var keyvalue in docs) {
                        var song = keyvalue.Value.AsObject;
                        var versionNode = song["versions"].AsArray.Children.FirstOrDefault(x => string.Equals(x["state"].Value, "Published", StringComparison.InvariantCultureIgnoreCase));
                        if (versionNode == null) {
                            continue;
                        }
                        var version = versionNode.AsObject;
                        var hash = version["hash"].Value;
                        var key = song["id"].Value;
                        var songName = song["name"].Value;
                        var downloadSucess = false;
                        if (SongDownloadHistory.Contains(hash.ToLower()) || Loader.GetLevelByHash(hash.ToUpper()) != null) {
                            SongDownloadHistory.Add(hash.ToLower());
                            // Update our playlist with the latest song info
                            this.UpdatePlaylist(this._syncSaberSongs, hash, songName);
                            continue;
                        }

                        if (PluginConfig.Instance.AutoDownloadSongs) {
                            try {
                                var metaData = song["metadata"].AsObject;
                                Logger.Debug($"{hash} : {songName}");
                                var currentSongDirectory = this.CreateSongDirectory(song);
                                Logger.Debug($"{songName} : {currentSongDirectory}");
                                this.DisplayNotification($"Downloading {songName}");
                                var url = $"{version["downloadURL"].Value}";
                                if (string.IsNullOrEmpty(url)) {
                                    continue;
                                }
                                Logger.Info(url);
                                this.DisplayNotification($"Download - {songName}");
                                while (Plugin.Instance?.IsInGame == true) {
                                    await Task.Delay(200);
                                }
                                var buff = await WebClient.DownloadSong(url, new CancellationTokenSource().Token);
                                if (buff == null) {
                                    Logger.Notice($"Failed to download song : {songName}");
                                    continue;
                                }
                                while (Plugin.Instance?.IsInGame == true) {
                                    await Task.Delay(200);
                                }
                                using (var st = new MemoryStream(buff)) {
                                    IO_Util.ExtractZip(st, currentSongDirectory);
                                }
                                downloadSucess = true;
                                this._didDownloadAnySong = true;
                            }
                            catch (Exception e) {
                                Logger.Error(e);
                            }
                        }
                        Logger.Info("Finish download");
                        // Keep a history of all the songs we download- count it as downloaded even if the user already had it downloaded previously so if they delete it it doesn't get redownloaded
                        if (downloadSucess && !SongDownloadHistory.Contains(hash.ToLower())) {
                            SongDownloadHistory.Add(hash.ToLower());

                            // Update our playlist with the latest song info
                            this.UpdatePlaylist(this._syncSaberSongs, hash, songName);

                            // Delete any duplicate songs that we've downloaded
                            this.RemoveOldVersions(hash);
                        }
                    }
                    pageCount++;

                } while (docsCount != 0);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            finally {
                stopWatch.Stop();
            }

            // Write our download history to file
            Utility.WriteStringListSafe(s_historyPath, SongDownloadHistory.ToList());

            // Write to the SyncSaber playlist
            this._syncSaberSongs.WritePlaylist();

            Logger.Info($"Downloaded downloadCount songs from mapper \"{author}\" in {stopWatch.Elapsed.Seconds} seconds.");
        }

        private async Task DownloadBeastSaberFeeds(DownloadFeed feedToDownload)
        {
            var startTime = DateTime.Now;
            var downloadCount = 0;
            var totalSongs = 0;
            var pageIndex = 1;

            while (true) {
                if (!this._beastSaberFeeds.TryGetValue(feedToDownload, out var url)) {
                    return;
                }
                this.DisplayNotification($"Checking page {pageIndex} of {feedToDownload} feed from BeastSaber!");
                try {
                    while (Plugin.Instance?.IsInGame == true) {
                        await Task.Delay(200);
                    }
                    var res = await WebClient.GetAsync($"{url.Replace("%BeastSaberUserName%", PluginConfig.Instance.BeastSaberUsername).Replace("%PageNumber%", $"{pageIndex}")}", new CancellationTokenSource().Token);
                    if (!res.IsSuccessStatusCode) {
                        return;
                    }
                    var beastSaberFeed = res.ConvertToJsonNode();
                    var songs = beastSaberFeed["songs"].AsArray;
                    var downloadSucess = false;
                    foreach (var songNode in songs.Children) {
                        var currentSongDirectory = this.CreateSongDirectory(songNode);
                        var songName = songNode["title"].Value;
                        var hash = songNode["hash"].Value;
                        totalSongs++;
                        if (SongDownloadHistory.Contains(hash.ToLower()) || Loader.GetLevelByHash(hash.ToUpper()) != null) {
                            SongDownloadHistory.Add(hash.ToLower());
                            // Update our playlist with the latest song info
                            this.UpdatePlaylist(this._syncSaberSongs, hash, songName);
                            this.UpdatePlaylist(this.GetPlaylistForFeed(feedToDownload), hash, songName);
                            continue;
                        }
                        if (PluginConfig.Instance.AutoDownloadSongs) {
                            this.DisplayNotification($"Downloading {songName}");
                            if (string.IsNullOrEmpty(hash)) {
                                continue;
                            }
                            var dlUrl = $"{DOWNLOAD_URL}/{hash}.zip";

                            Logger.Info(dlUrl);
                            this.DisplayNotification($"Download - {songName}");
                            while (Plugin.Instance?.IsInGame == true) {
                                await Task.Delay(200);
                            }
                            var buff = await WebClient.DownloadSong(dlUrl, new CancellationTokenSource().Token);
                            if (buff == null) {
                                Logger.Notice($"Failed to download song : {songName}");
                                continue;
                            }
                            while (Plugin.Instance?.IsInGame == true) {
                                await Task.Delay(200);
                            }
                            using (var st = new MemoryStream(buff)) {
                                IO_Util.ExtractZip(st, currentSongDirectory);
                            }
                            downloadSucess = true;
                            downloadCount++;
                        }

                        // Keep a history of all the songs we download- count it as downloaded even if the user already had it downloaded previously so if they delete it it doesn't get redownloaded
                        if (downloadSucess) {
                            SongDownloadHistory.Add(hash.ToLower());

                            // Update our playlist with the latest song info
                            this.UpdatePlaylist(this._syncSaberSongs, hash, songName);
                            this.UpdatePlaylist(this.GetPlaylistForFeed(feedToDownload), hash, songName);

                            // Delete any duplicate songs that we've downloaded
                            this.RemoveOldVersions(hash);
                        }
                        this._didDownloadAnySong = true;
                    }

                    var nextPageNumText = beastSaberFeed["next_page"].Value;
                    if (string.IsNullOrEmpty(nextPageNumText) || !int.TryParse(nextPageNumText, out pageIndex)) {
                        break;
                    }
                }
                catch (Exception e) {
                    Logger.Error(e);
                }

                //Logger.Info($"Reached end of page! Found {totalSongsForPage.ToString()} songs total, downloaded {downloadCountForPage.ToString()}!");
                if (pageIndex > this.GetMaxBeastSaberPages(feedToDownload) + 1 && this.GetMaxBeastSaberPages(feedToDownload) != 0) {
                    break;
                }
            }
            // Write our download history to file
            Utility.WriteStringListSafe(s_historyPath, SongDownloadHistory.ToList());
            // Write to the SynCSaber playlist
            this._syncSaberSongs.WritePlaylist();
            this.GetPlaylistForFeed(feedToDownload).WritePlaylist();
            Logger.Info($"Downloaded {downloadCount} songs from BeastSaber {feedToDownload} feed in {((DateTime.Now - startTime).Seconds)} seconds. Checked {pageIndex} page{(pageIndex > 1 ? "s" : "")}, skipped {(totalSongs - downloadCount)} songs.");
        }

        private async Task DownloadPPSongs()
        {
            this.DisplayNotification("Download PPSongs.");

            var songlist = await ScoreSaberManager.Ranked(PluginConfig.Instance.MaxPPSongsCount, PluginConfig.Instance.RankSort);
            if (songlist == null) {
                return;
            }
            foreach (var ppMap in songlist.Values) {
                try {
                    while (Plugin.Instance?.IsInGame != false) {
                        await Task.Delay(200);
                    }
                    var hash = ppMap["songHash"].Value.ToLower();
                    var beatmap = Loader.GetLevelByHash(hash);
                    if (beatmap != null) {
                        this.UpdatePlaylist(this._syncSaberSongs, hash, beatmap.songName);
                        SongDownloadHistory.Add(hash.ToLower());
                        continue;
                    }
                    if (SongDownloadHistory.Contains(hash.ToLower())) {
                        continue;
                    }
                    var songInfo = await WebClient.GetAsync($"{ROOT_BEATSAVER_URL}/maps/hash/{hash}", new CancellationTokenSource().Token);
                    var jsonObject = JSON.Parse(songInfo?.ContentToString());
                    if (jsonObject == null) {
                        Logger.Info($"missing pp song : {ROOT_BEATSAVER_URL}/maps/hash/{hash}");
                        continue;
                    }
                    var key = jsonObject["id"].Value.ToLower();
                    var version = jsonObject["versions"].AsArray.Children.FirstOrDefault(x => string.Equals(x["state"].Value, "Published", StringComparison.InvariantCultureIgnoreCase));
                    if (version == null) {
                        Logger.Debug($"{hash} is not published.");
                        continue;
                    }
                    var dlUrl = version.AsObject["downloadURL"].Value;
                    var author = ppMap["levelAuthorName"].Value;

                    this.DisplayNotification($"Downloading {jsonObject["name"].Value}");
                    var buff = await WebClient.DownloadSong($"{dlUrl}", new CancellationTokenSource().Token);
                    if (buff == null) {
                        Logger.Notice($"Failed to download song : {jsonObject["name"].Value}");
                        continue;
                    }
                    while (Plugin.Instance?.IsInGame == true) {
                        await Task.Delay(200);
                    }
                    var songDirectory = this.CreateSongDirectory(jsonObject);
                    using (var st = new MemoryStream(buff)) {
                        IO_Util.ExtractZip(st, songDirectory);
                    }
                    this._didDownloadAnySong = true;
                    SongDownloadHistory.Add(hash.ToLower());
                    this.UpdatePlaylist(this._syncSaberSongs, hash, jsonObject["name"].Value);
                }
                catch (Exception e) {
                    Logger.Error($"{e}\r\n{ppMap["name"]} : {ppMap["id"]}");
                    Logger.Debug($"{ppMap}");
                }
            }
            try {
                // Write our download history to file
                Utility.WriteStringListSafe(s_historyPath, SongDownloadHistory.ToList());
                // Write to the SyncSaber playlist
                this._syncSaberSongs.WritePlaylist();
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }
        public void SetEvent()
        {
            try {
                Utility.RemoveEventHandler(_diContainer, this.RaiseEvent);
                Utility.AddEventHandler(_diContainer, this.RaiseEvent);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }

        private IEnumerator CheckPlaylist()
        {
            yield return new WaitWhile(() => !Loader.AreSongsLoaded || Loader.AreSongsLoading);
            try {
                _ = Utility.CheckPlaylistsSong(_diContainer);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }

        private string CreateSongDirectory(JSONNode songNode)
        {
            var key = songNode["id"].Value?.ToLower();
            if (string.IsNullOrEmpty(key)) {
                key = songNode["song_key"].Value.ToLower();
                var author = songNode["level_author_name"].Value;
                var result = Path.Combine(s_customLevelsPath, s_invalidDirectoryAndFileChars.Replace($"{key} ({songNode["title"].Value} - {author})", "_"));

                var count = 1;
                var resultLength = result.Length;
                while (Directory.Exists(result)) {
                    result = $"{result.Substring(0, resultLength)}({count})";
                    count++;
                }
                return result;
            }
            else {
                var meta = songNode["metadata"].AsObject;
                var author = meta["levelAuthorName"].Value;
                var result = Path.Combine(s_customLevelsPath, s_invalidDirectoryAndFileChars.Replace($"{key} ({songNode["name"].Value} - {author})", "_"));

                var count = 1;
                var resultLength = result.Length;
                while (Directory.Exists(result)) {
                    result = $"{result.Substring(0, resultLength)}({count})";
                    count++;
                }
                return result;
            }
        }

        private void RaiseEvent(string text)
        {
            this.NotificationTextChange?.Invoke(text);
        }

        //private string CreateSongDirectory(XmlNode songNode)
        //{
        //    var key = songNode["SongKey"].InnerText;
        //    var author = songNode["LevelAuthorName"].InnerText;
        //    var result = Path.Combine(_customLevelsPath, Regex.Replace($"{key} ({songNode["SongTitle"].InnerText} - {author})", "[\\\\:*/?\"<>|]", "_"));

        //    var count = 1;
        //    var resultLength = result.Length;
        //    while (Directory.Exists(result)) {
        //        result = $"{result.Substring(0, resultLength)}({count})";
        //        count++;
        //    }
        //    return result;
        //}

        public void Dispose()
        {
            Logger.Debug("Dispose call!");
            this.SyncTimer.Elapsed -= this.Timer_Elapsed;
            ((IDisposable)this.SyncTimer).Dispose();
            this.SyncTimer = null;
            this._semaphoreSlim?.Dispose();
            this._semaphoreSlim = null;
        }
    }
}
