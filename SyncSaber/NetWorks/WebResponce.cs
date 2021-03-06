﻿using SyncSaber.SimpleJSON;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SyncSaber.NetWorks
{
    internal class WebResponse
    {
        public readonly HttpStatusCode StatusCode;
        public readonly string ReasonPhrase;
        public readonly HttpResponseHeaders Headers;
        public readonly HttpRequestMessage RequestMessage;
        public readonly bool IsSuccessStatusCode;

        private readonly byte[] _content;

        internal WebResponse(HttpResponseMessage resp, byte[] body)
        {
            this.StatusCode = resp.StatusCode;
            this.ReasonPhrase = resp.ReasonPhrase;
            this.Headers = resp.Headers;
            this.RequestMessage = resp.RequestMessage;
            this.IsSuccessStatusCode = resp.IsSuccessStatusCode;

            this._content = body;
        }

        public byte[] ContentToBytes() => this._content;
        public string ContentToString() => Encoding.UTF8.GetString(this._content);
        public JSONNode ConvertToJsonNode() => JSONNode.Parse(this.ContentToString());
    }
}
