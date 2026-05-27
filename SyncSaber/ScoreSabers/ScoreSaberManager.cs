using SyncSaber.NetWorks;
using SyncSaber.SimpleJSON;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SyncSaber.ScoreSabers
{
    public static class ScoreSaberManager
    {
        public const string BASEURL = "https://scoresaber.com/api.php";
        public const string BASEURL_V2 = "https://scoresaber.com/api/v2";

        public static async Task<JSONArray> Ranked(int songcouts, RankSort sort)
        {
            var pageCount = 0;
            var results = new JSONArray();
            do {
                // https://scoresaber.com/api/v2/maps?page=2&limit=100&status=RANKED&sortBy=latestRankedAt
                var url = $"/maps?page={++pageCount}&limit=100&status=RANKED&sortBy={GetEnumDescription(sort)}";
                var buff = await WebClient.GetAsync($"{BASEURL_V2}{url}", new CancellationTokenSource().Token);
                if (buff == null) {
                    return null;
                }
                var json = JSON.Parse(buff.ContentToString());
                if (json["data"] == null || !json["data"].IsArray) {
                    return null;
                }
                var rankSongs = json["data"].AsArray;
                foreach (var song in rankSongs.Values) {
                    results.Add(song.AsObject);
                    if (songcouts <= results.Count) {
                        break;
                    }
                }
                await Task.Delay(2000);
            } while (results.Count < songcouts);
            return results;
        }
        /// <summary>
        /// Which category to sort by (0 = trending, date ranked = 1, scores set = 2, star difficulty = 3, author = 4)
        /// </summary>
        public enum RankSort
        {
            [Description("latestRankedAt")]
            LatestRankedAt,
            [Description("createdAt")]
            CreatedAt,
            [Description("highestStars")]
            HighestStars,
            [Description("totalScores")]
            TotalScores,
            [Description("trending")]
            Trending,
        }

        private static string GetEnumDescription(RankSort value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attribute.Length > 0 ? (attribute[0] as DescriptionAttribute).Description : value.ToString();
        }
    }
}
