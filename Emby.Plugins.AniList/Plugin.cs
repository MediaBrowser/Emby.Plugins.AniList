using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using MediaBrowser.Model.Drawing;
using System.IO;

namespace Emby.Plugins.AniList
{
    public class Plugin : BasePlugin, IHasWebPages, IHasThumbImage
    {
        public ILogger Logger { get; private set; }

        public Plugin(ILogManager logManager)
        {
            Instance = this;
            Logger = logManager.GetLogger(Name);
        }

        public override string Name
        {
            get { return "AniList"; }
        }

        public static Plugin Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "anilist",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.anilist.html",
                    EnableInMainMenu = true,
                    MenuIcon = "dvr"
                },

                new PluginPageInfo
                {
                    Name = "anilistjs",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.anilist.js"
                }
            };
        }

        private Guid _id = new Guid("043B75CD-FC2C-49CC-ACCC-4D1D60A297C5");

        public override Guid Id
        {
            get { return _id; }
        }

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }

        public ImageFormat ThumbImageFormat
        {
            get
            {
                return ImageFormat.Png;
            }
        }
    }
}