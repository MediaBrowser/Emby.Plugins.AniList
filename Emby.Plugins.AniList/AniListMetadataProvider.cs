using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Serialization;

//API v2
namespace Emby.Plugins.AniList
{
    public class AniListMetadataProvider<T, U> : IRemoteMetadataProvider<T, U>, IHasOrder where T : BaseItem,IHasLookupInfo<U>,new() where U : ItemLookupInfo,new()
    {
        protected readonly IHttpClient _httpClient;
        protected readonly IApplicationPaths _paths;
        protected readonly ILogger _log;
        protected readonly Api _api;
        public int Order => 8;
        public string Name => "AniList";

        public AniListMetadataProvider(IApplicationPaths appPaths, IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer)
        {
            _log = logManager.GetLogger(Name);
            _httpClient = httpClient;
            _api = new Api(_log, httpClient, jsonSerializer);
            _paths = appPaths;
        }

        protected virtual MetadataResult<T> _GetMetadata(MetadataResult<T> result, RootObject WebContent)
        {
            return result;
        }

        public async Task<MetadataResult<T>> GetMetadata(U info, CancellationToken cancellationToken)
        {
            RootObject WebContent = null;

            var aid = info.GetProviderId(ProviderNames.AniList);
            if (string.IsNullOrEmpty(aid))
            {
                _log.Info("Start AniList... Searching(" + info.Name + ")");
                aid = await _api.FindSeries(info.Name, cancellationToken);
            }

            if (!string.IsNullOrEmpty(aid))
            {
                WebContent = await _api.WebRequestAPI(_api.AniList_anime_link.Replace("{0}", aid), cancellationToken);
            }

            var result = new MetadataResult<T>();

            result.Item = new T();
            result.HasMetadata = true;

            result.Item.Name = _api.SelectName(WebContent, info.MetadataLanguage);
            result.Item.OriginalTitle = WebContent.data.Media.title.native;

            result.People = await _api.GetPersonInfo(WebContent.data.Media.id, cancellationToken);
            foreach (var studio in _api.Get_Studio(WebContent))
                result.Item.AddStudio(studio);
            foreach (var tag in _api.Get_Tag(WebContent))
                result.Item.AddTag(tag);
            try {
                if (Equals_check.Compare_strings("youtube", WebContent.data.Media.trailer.site)) {
                    result.Item.AddTrailerUrl("https://youtube.com/watch?v=" + WebContent.data.Media.trailer.id);
                }
            } catch (Exception) { }
            result.Item.SetProviderId(ProviderNames.AniList, WebContent.data.Media.id.ToString());
            result.Item.Overview = WebContent.data.Media.description;
            try
            {
                StartDate startDate = WebContent.data.Media.startDate;
                DateTime date = new DateTime(startDate.year, startDate.month, startDate.day);
                date = date.ToUniversalTime();
                result.Item.PremiereDate = date;
                result.Item.ProductionYear = date.Year;
            }
            catch (Exception) { }
            try
            {
                EndDate endDate = WebContent.data.Media.endDate;
                DateTime date = new DateTime(endDate.year, endDate.month, endDate.day);
                date = date.ToUniversalTime();
                result.Item.EndDate = date;
            }
            catch (Exception) { }
            int episodes = WebContent.data.Media.episodes;
            int duration = WebContent.data.Media.duration;
            if (episodes > 0 && duration > 0){
                // minutes to microseconds, needs to x10 to display correctly for some reason
                result.Item.RunTimeTicks = episodes * duration * (long)600000000;
            }
            try
            {
                //AniList has a max rating of 5
                result.Item.CommunityRating = (WebContent.data.Media.averageScore / 10);
            }
            catch (Exception) { }
            foreach (var genre in _api.Get_Genre(WebContent))
                result.Item.AddGenre(genre);
            GenreHelper.CleanupGenres(result.Item);

            return _GetMetadata(result, WebContent);
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(U searchInfo, CancellationToken cancellationToken)
        {
            var results = new Dictionary<string, RemoteSearchResult>();

            var aid = searchInfo.GetProviderId(ProviderNames.AniList);
            if (!string.IsNullOrEmpty(aid))
            {
                if (!results.ContainsKey(aid))
                    results.Add(aid, await _api.GetAnime(aid, cancellationToken));
            }

            if (!string.IsNullOrEmpty(searchInfo.Name))
            {
                List<string> ids = await _api.Search_GetSeries_list(searchInfo.Name, cancellationToken);
                foreach (string a in ids)
                {
                    results.Add(a, await _api.GetAnime(a, cancellationToken));
                }
            }

            return results.Values;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url
            });
        }
    }
}
