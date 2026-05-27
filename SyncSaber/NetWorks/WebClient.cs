using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SyncSaber.NetWorks
{
    public static class WebClient
    {
        private static readonly HttpClient s_client;
        private static readonly int s_retryCount = 5;
        private static readonly SemaphoreSlim s_semaphoreSlim = new SemaphoreSlim(2, 2);

        static WebClient()
        {
            s_client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 15)
            };
            s_client.DefaultRequestHeaders.UserAgent.TryParseAdd($"SyncSaber/{Assembly.GetExecutingAssembly().GetName().Version}");
        }

        internal static async Task<WebResponse> GetAsync(string url, CancellationToken token)
        {
            try {
                return await SendAsync(HttpMethod.Get, url, token);
            }
            catch (Exception e) {
                Logger.Error($"{e}");
                return null;
            }
        }

        internal static async Task<byte[]> DownloadImage(string url, CancellationToken token)
        {
            try {
                var response = await SendAsync(HttpMethod.Get, url, token);
                if (response?.IsSuccessStatusCode == true) {
                    return response.ContentToBytes();
                }
                return null;
            }
            catch (Exception e) {
                Logger.Error($"{e}");
                return null;
            }
        }

        internal static async Task<byte[]> DownloadSong(string url, CancellationToken token, IProgress<double> progress = null)
        {
            // check if beatsaver url needs to be pre-pended
            //if (!url.StartsWith(@"https://beatsaver.com/")) {
            //    url = $"https://beatsaver.com/{url}";
            //}
            try {
                var response = await SendAsync(HttpMethod.Get, url, token, progress: progress);

                if (response?.IsSuccessStatusCode == true) {
                    return response.ContentToBytes();
                }
                return null;
            }
            catch (Exception e) {
                Logger.Error($"{e}");
                return null;
            }
        }

        internal static async Task<WebResponse> SendAsync(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress = null)
        {
            Logger.Info($"{methodType}: {url}");

            // send request
            try {
                await s_semaphoreSlim.WaitAsync();

                HttpResponseMessage resp = null;
                var retryCount = 0;
                do {
                    try {
                        // create new request messsage
                        var req = new HttpRequestMessage(methodType, url);
                        if (retryCount != 0) {
                            await Task.Delay(1000);
                        }
                        retryCount++;
                        resp = await s_client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                        Logger.Info($"resp code : {resp.StatusCode}");
                    }
                    catch (Exception e) {
                        Logger.Error($"Error : {e}");
                        Logger.Error($"{resp?.StatusCode}");
                    }
                } while (resp?.StatusCode != HttpStatusCode.NotFound && resp?.IsSuccessStatusCode != true && retryCount <= s_retryCount);


                if (token.IsCancellationRequested) {
                    throw new TaskCanceledException();
                }

                using (var memoryStream = new MemoryStream())
                using (var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                    var buffer = new byte[8192];
                    var bytesRead = 0; ;

                    var contentLength = resp?.Content.Headers.ContentLength;
                    var totalRead = 0;

                    // send report
                    progress?.Report(0);

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0) {
                        if (token.IsCancellationRequested) {
                            throw new TaskCanceledException();
                        }

                        if (contentLength != null) {
                            progress?.Report(totalRead / (double)contentLength);
                        }

                        await memoryStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                        totalRead += bytesRead;
                    }

                    progress?.Report(1);
                    var bytes = memoryStream.ToArray();

                    return new WebResponse(resp, bytes);
                }
            }
            catch (Exception e) {
                Logger.Error($"{e}");
                throw;
            }
            finally {
                s_semaphoreSlim.Release();
            }
        }
    }
}
