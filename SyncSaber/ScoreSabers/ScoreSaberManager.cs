using SyncSaber.NetWorks;
using SyncSaber.SimpleJSON;
using System.Threading;
using System.Threading.Tasks;

namespace SyncSaber.ScoreSabers
{
    public static class ScoreSaberManager
    {
        public const string BASEURL = "https://scoresaber.com";
        public static async Task<JSONArray> Ranked(int songcouts, RankSort sort)
        {
            var pageCount = 0;
            var results = new JSONArray();
            do {
                var url = $"/api/leaderboards?ranked=true&category={(int)sort}&sort=0&unique=true&page={pageCount}";
                var buff = await WebClient.GetAsync($"{BASEURL}{url}", new CancellationTokenSource().Token);
                if (buff == null) {
                    return null;
                }
                var json = JSON.Parse(buff.ContentToString());
                if (json["leaderboards"] == null || !json["leaderboards"].IsArray) {
                    return null;
                }
                var rankSongs = json["leaderboards"].AsArray;
                foreach (var song in rankSongs.Values) {
                    results.Add(song.AsObject);
                    if (songcouts <= results.Count) {
                        break;
                    }
                }
                pageCount++;
            } while (results.Count < songcouts);
            return results;
        }
        /// <summary>
        /// Which category to sort by (0 = trending, date ranked = 1, scores set = 2, star difficulty = 3, author = 4)
        /// </summary>
        public enum RankSort
        {
            Trending,
            DateRanked,
            ScoreSet,
            StarDifficulity,
            Author
        }
    }
}
