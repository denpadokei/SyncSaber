# Description
This mod keeps all your songs up to date, and additionally downloads all the newest releases from your favorite mappers, as well as content from your followings/bookmarks feed on [BeastSaber](https://www.bsaber.com/)
### Features:
- Automatic song updating
   * SyncSaber automatically checks for updates for all of your songs whenever you select a song from the menu! This means if a beatmap gets updated, you'll automatically receive the newest version without having to do anything manually!
- MapperFeed
   * SyncSaber can download songs from any of your favorite mappers! All you have to do is create a file called FavoriteMappers.ini in the UserData folder inside your Beat saber director, then enter your favorite mappers BeatSaver usernames (one mapper per line) into FavoriteMappers.ini.
- BeastSaberFeed
   * If you want SyncSaber to automatically download content from your BeastSaber bookmarks/followings feeds, all you have to do is enter your BeastSaber username in the Beat Saber settings menu under the SyncSaber submenu. You can also enable the "Curator Recommended" BeastSaber feed, which automatically downloads the best songs recently recommended by curators on BeastSaber!  
- AutoDownload Rank Songs  
   * Download the rank maps.  
  
# Dependencies
SyncSaber requires [CustomUI](https://github.com/brian91292/BeatSaber-CustomUI/releases) and [SongLoader](https://github.com/Kylemc1413/BeatSaberSongLoader/releases), both of which are available on the [BeatMods](https://beatmods.com/).

# Getting Started
You can manage almost all SyncSaber settings via the Beat Saber settings menu! Just look for the SyncSaber menu! Keep reading to find out how to setup additional features of SyncSaber that cannot be configured from the menu!

# Config
SyncSaber settings are stored in the modprefs.ini which you can find in the UserData folder inside your Beat Saber install directory. The following config options are available:

| Option                     | Description                                                                                                                  |
|----------------------------|------------------------------------------------------------------------------------------------------------------------------|
| **AutoDownloadSongs** | Whether or not SyncSaber should automatically download any new songs it finds, or just add them to the SyncSaber playlist. |
| **AutoUpdateSongs**| Whether or not SyncSaber will attempt to auto-update songs when you click on them in the song browser. |
| **DeleteOldVersions**| Whether or not SyncSaber should keep or delete old versions of songs upon updating them, or when a new release is downloaded from one of your favorite mappers. |
| **BeastSaberUsername**| Your username from [BeastSaber](https://bsaber.com), this config option is optional. Only enter this if you want to automatically download songs posted to your followings/bookmarks feed on BeastSaber. |
| **SyncBookmarksFeed**| Whether or not to automatically sync your bookmarks feed from BeastSaber. |
| **SyncFollowingsFeed**| Whether or not to automatically sync your followings feed from BeastSaber. |
| **SyncCuratorRecommendedFeed**| Whether or not to automatically sync the curator recommended feed from BeastSaber. |
| **SyncPPSongs**| Whether or not to automatically sync rank maps. |
| **MaxBookmarksPages**| The maximum number of bookmarks pages to scan for new songs on BeastSaber, 0 is unlimited. |
| **MaxFollowingsPages**| The maximum number of followings pages to scan for new songs on BeastSaber, 0 is unlimited. |
| **MaxCuratorRecommendedPages**| The maximum number of curator recommended pages to scan for new songs on BeastSaber, 0 is unlimited. |
| **MaxPPSongsCount**| The maximum number of curator ranked songs. |

# Download
[Click here to download the latest version of SyncSaber!](https://github.com/denpadokei/SyncSaber/releases/latest)
