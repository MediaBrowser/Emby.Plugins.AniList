using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Entities.Movies;

//API v2
namespace Emby.Plugins.AniList
{
    public class AniListSeriesProvider : AniListMetadataProvider<Series, SeriesInfo>
    {
        public AniListSeriesProvider(IApplicationPaths appPaths, IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer) : base(appPaths, httpClient, logManager, jsonSerializer)
        {
            
        }

        protected override MetadataResult<Series> _GetMetadata(MetadataResult<Series> result, RootObject WebContent)
        {
            if (result.HasMetadata)
            {
                string status = WebContent.data.Media.status;
                if (status == Status.RELEASING || status == Status.NOT_YET_RELEASED)
                {
                    result.Item.Status = SeriesStatus.Continuing;
                }
                else
                {
                    result.Item.Status = SeriesStatus.Ended;
                }
            }
            return result;
        }
    }
}