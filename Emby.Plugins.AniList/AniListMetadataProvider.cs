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
        protected readonly IConfigurationManager _config;
        protected readonly IApplicationPaths _paths;
        protected readonly ILogger _log;
        protected readonly Api _api;
        public int Order => 8;
        public string Name => "AniList";

        public AniListMetadataProvider(IApplicationPaths appPaths, IConfigurationManager config, IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer)
        {
            _log = logManager.GetLogger(Name);
            _httpClient = httpClient;
            _config = config;
            _api = new Api(_log, httpClient, jsonSerializer);
            _paths = appPaths;
        }

        protected virtual MetadataResult<T> _GetMetadata(MetadataResult<T> result, BaseMedia media)
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
                WebContent = await _api.WebRequestAPI(_api.AniList_anime_link.Replace("{0}", aid, StringComparison.OrdinalIgnoreCase), cancellationToken);
            }

            var result = new MetadataResult<T>();

            result.Item = new T();

            var media = WebContent?.GetMedia();

            if (media == null)
            {
                return result;
            }

            result.HasMetadata = true;

            result.Item.Name = _api.SelectName(media, info.MetadataLanguage);

            result.Item.OriginalTitle = media.title.native;

            result.People = await _api.GetPersonInfo(media.id, _config.GetAniListListOptions(), cancellationToken);
            foreach (var studio in _api.Get_Studio(media))
                result.Item.AddStudio(studio);
            foreach (var tag in _api.Get_Tag(media))
                result.Item.AddTag(tag);
            try {
                if (Equals_check.Compare_strings("youtube", media.trailer.site)) {
                    result.Item.AddTrailerUrl("https://youtube.com/watch?v=" + media.trailer.id);
                }
            } catch (Exception) { }
            result.Item.SetProviderId(ProviderNames.AniList, media.id.ToString());
            result.Item.Overview = media.description;
            try
            {
                StartDate startDate = media.startDate;
                DateTime date = new DateTime(startDate.year, startDate.month, startDate.day);
                date = date.ToUniversalTime();
                result.Item.PremiereDate = date;
                result.Item.ProductionYear = date.Year;
            }
            catch (Exception) { }
            try
            {
                EndDate endDate = media.endDate;
                DateTime date = new DateTime(endDate.year, endDate.month, endDate.day);
                date = date.ToUniversalTime();
                result.Item.EndDate = date;
            }
            catch (Exception) { }
            int episodes = media.episodes;
            int duration = media.duration;
            if (episodes > 0 && duration > 0){
                // minutes to microseconds, needs to x10 to display correctly for some reason
                result.Item.RunTimeTicks = episodes * duration * (long)600000000;
            }
            try
            {
                result.Item.CommunityRating = ((float)media.averageScore / 10);
            }
            catch (Exception) { }
            foreach (var genre in _api.Get_Genre(media))
                result.Item.AddGenre(genre);
            GenreHelper.CleanupGenres(result.Item);

            return _GetMetadata(result, media);
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
