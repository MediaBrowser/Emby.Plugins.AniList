﻿using MediaBrowser.Common.Configuration;
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
    public class AniListSeriesProvider : AniListMetadataProvider<Series, SeriesInfo>, IHasSupportedExternalIdentifiers
    {
        public AniListSeriesProvider(IApplicationPaths appPaths, IConfigurationManager config, IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer) : base(appPaths, config, httpClient, logManager, jsonSerializer)
        {

        }

        public string[] GetSupportedExternalIdentifiers()
        {
            return new[] {

                ProviderNames.AniList
            };
        }

        protected override MetadataResult<Series> _GetMetadata(MetadataResult<Series> result, BaseMedia media)
        {
            if (result.HasMetadata)
            {
                string status = media.status;
                if (!string.IsNullOrEmpty(status))
                {
                    if (string.Equals(status, Status.RELEASING, StringComparison.OrdinalIgnoreCase) || string.Equals(status, Status.NOT_YET_RELEASED, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Item.Status = SeriesStatus.Continuing;
                    }
                    else
                    {
                        result.Item.Status = SeriesStatus.Ended;
                    }
                }
            }
            return result;
        }
    }
}