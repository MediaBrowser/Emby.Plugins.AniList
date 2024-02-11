using System.Collections.Generic;
using MediaBrowser.Common.Configuration;

namespace Emby.Plugins.AniList
{
    public static class ConfigurationExtension
    {
        public static AniListOptions GetAniListListOptions(this IConfigurationManager manager)
        {
            return manager.GetConfiguration<AniListOptions>("anilist");
        }
    }

    public class AniListConfigurationFactory : IConfigurationFactory
    {
        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new ConfigurationStore[]
            {
                new ConfigurationStore
                {
                    Key = "anilist",
                    ConfigurationType = typeof (AniListOptions)
                }
            };
        }
    }

    public class AniListOptions
    {
        public bool ShouldDownloadCharacters { get; set; }

        public AniListOptions()
        {
            ShouldDownloadCharacters = true;
        }
    }
}