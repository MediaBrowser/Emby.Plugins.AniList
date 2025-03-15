using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Controller.Entities.Movies;

//API v2
namespace Emby.Plugins.AniList
{
    public class AniListMovieProvider : AniListMetadataProvider<Movie, MovieInfo>, IHasSupportedExternalIdentifiers
    {
        public AniListMovieProvider(IApplicationPaths appPaths, IConfigurationManager config, IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer) : base(appPaths, config, httpClient, logManager, jsonSerializer)
        {

        }

        public string[] GetSupportedExternalIdentifiers()
        {
            return new[] {

                ProviderNames.AniList
            };
        }
    }
}