using IPA.Utilities;
using SongCore;
using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace SyncSaber
{
    public class SongListUtil : IDisposable
    {
        private LevelCollectionViewController _levelCollectionViewController;
        private LevelCollectionTableView levelCollectionTableView;
        private bool disposedValue;

        public BeatmapLevel CurrentPreviewBeatmapLevel { get; private set; }
        private BeatmapLevel _lastSelectedBeatmap;
        [Inject]
        public void Constractor(LevelCollectionViewController levelView)
        {
            this._levelCollectionViewController = levelView;

            this.levelCollectionTableView = this._levelCollectionViewController.GetField<LevelCollectionTableView, LevelCollectionViewController>("_levelCollectionTableView");
            this.levelCollectionTableView.didSelectLevelEvent += this.TableView_didSelectLevelEvent;
            Loader.OnLevelPacksRefreshed += this.Loader_OnLevelPacksRefreshed;
        }

        private void Loader_OnLevelPacksRefreshed()
        {
            if (this._lastSelectedBeatmap != null) {
                this.SelectBeatMapLevel(this._lastSelectedBeatmap);
            }
        }
        private void TableView_didSelectLevelEvent(LevelCollectionTableView arg1, BeatmapLevel arg2)
        {
            this.CurrentPreviewBeatmapLevel = arg2;
        }

        public IEnumerator RefreshSongs(bool fullRefresh = false)
        {
            yield return new WaitWhile(() => !Loader.AreSongsLoaded || Loader.AreSongsLoading || Plugin.Instance.IsInGame);
            this._lastSelectedBeatmap = this.CurrentPreviewBeatmapLevel;
            Loader.Instance.RefreshSongs(fullRefresh);
            yield return new WaitWhile(() => !Loader.AreSongsLoaded || Loader.AreSongsLoading || Plugin.Instance.IsInGame);

        }

        public void SelectBeatMapLevel(BeatmapLevel beatmapLevel)
        {
            if (!this.levelCollectionTableView) {
                return;
            }
            this.levelCollectionTableView.SelectLevel(beatmapLevel);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this.levelCollectionTableView.didSelectLevelEvent -= this.TableView_didSelectLevelEvent;
                    Loader.OnLevelPacksRefreshed -= this.Loader_OnLevelPacksRefreshed;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                this.disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~SongListUtils()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
