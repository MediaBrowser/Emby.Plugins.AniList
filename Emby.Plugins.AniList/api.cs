using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Common.Net;
using System.IO;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Logging;

namespace Emby.Plugins.AniList
{
    /// <summary>
    /// Based on the new API from AniList
    /// 🛈 This code works with the API Interface (v2) from AniList
    /// 🛈 https://anilist.gitbook.io/anilist-apiv2-docs
    /// 🛈 THIS IS AN UNOFFICAL API INTERFACE FOR EMBY
    /// </summary>
    public class Api
    {
        private static IJsonSerializer _jsonSerializer;
        private const string SearchLink = @"https://graphql.anilist.co/api/v2?query=
query ($query: String, $type: MediaType) {
  Page {
    media(search: $query, type: $type) {
      id
      title {
        romaji
        english
        native
      }
      coverImage {
        medium
        large
      }
      format
      type
      averageScore
      popularity
      episodes
      duration
      season
      hashtag
      isAdult
      startDate {
        year
        month
        day
      }
      endDate {
        year
        month
        day
      }
    }
  }
}&variables={ ""query"":""{0}"",""type"":""ANIME""}";
        public string AniList_anime_link = @"https://graphql.anilist.co/api/v2?query=
query($id: Int!, $type: MediaType) {
    Media(id: $id, type: $type) {
        id
        title {
            romaji
            english
            native
            userPreferred
        }
        startDate {
            year
            month
            day
        }
        endDate {
            year
            month
            day
        }
        coverImage {
            large
            medium
        }
        bannerImage
        format
        type
        status
        episodes
        duration
        chapters
        volumes
        season
        description
        averageScore
        meanScore
        genres
        synonyms
        nextAiringEpisode {
            airingAt
            timeUntilAiring
            episode
        }
        studios {
            edges {
                node {
                    name
                }
            }
        }
        trailer {
            id
            site
        }
        tags {
            name
        }
    }
}&variables={ ""id"":""{0}"",""type"":""ANIME""}";
        private const string AniList_anime_char_link = @"https://graphql.anilist.co/api/v2?query=
query($id: Int!, $type: MediaType, $staffLanguage: StaffLanguage, $page: Int = 1) {
  Media(id: $id, type: $type) {
    id
    characters(page: $page, sort: [ROLE]) {
      pageInfo {
        total
        perPage
        hasNextPage
        currentPage
        lastPage
      }
      edges {
        node {
          id
          name {
            first
            last
          }
          image {
            medium
            large
          }
        }
        role
        voiceActors(language: $staffLanguage, sort: [ROLE]) {
          id
          name {
            first
            last
            native
          }
          image {
            medium
            large
          }
          language
        }
      }
    }
  }
}&variables={ ""id"":""{0}"",""type"":""ANIME"",""staffLanguage"":""JAPANESE""}";

        private IHttpClient _httpClient;
        private ILogger _logger;

        public Api(ILogger logger, IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
        }
        /// <summary>
        /// API call to get the anime with the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RemoteSearchResult> GetAnime(string id, CancellationToken cancellationToken)
        {
            RootObject WebContent = await WebRequestAPI(AniList_anime_link.Replace("{0}", id, StringComparison.OrdinalIgnoreCase), cancellationToken);

            var result = new RemoteSearchResult
            {
                Name = ""
            };

            var media = WebContent.GetMedia();

            result.SearchProviderName = media.title.romaji;
            result.ImageUrl = media.coverImage.large;
            result.SetProviderId(ProviderNames.AniList, id);
            result.Overview = media.description;

            return result;
        }

        /// <summary>
        /// API call to select the lang
        /// </summary>
        /// <param name="WebContent"></param>
        /// <param name="preference"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string SelectName(BaseMedia media, string preferredLanguage)
        {
            if (string.IsNullOrEmpty(preferredLanguage) || preferredLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                var title = media.title?.english;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    return title;
                }
            }
            if (string.Equals(preferredLanguage, "ja", StringComparison.OrdinalIgnoreCase))
            {
                var title = media.title?.native;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    return title;
                }
            }

            return media.title?.romaji;
        }

        public async Task<List<PersonInfo>> GetPersonInfo(int id, AniListOptions config, CancellationToken cancellationToken)
        {
            List<PersonInfo> lpi = new List<PersonInfo>();
            RootObject WebContent = await WebRequestAPI(AniList_anime_char_link.Replace("{0}", id.ToString(), StringComparison.OrdinalIgnoreCase), cancellationToken);
            foreach (Edge edge in WebContent.GetMedia().characters.edges)
            {
                if (edge.voiceActors.Count > 0)
                {
                    VoiceActor va = edge.voiceActors[0];
                    PersonInfo actor = new PersonInfo();
                    actor.Name = va.name.first + " " + va.name.last;
                    actor.ImageUrl = va.image.large;
                    actor.Role = edge.node.name.first + " " + edge.node.name.last;
                    actor.Type = PersonType.Actor;
                    lpi.Add(actor);

                    if (config.ShouldDownloadCharacters)
                    {
                        PersonInfo character = new PersonInfo();
                        character.Name = actor.Role;
                        character.ImageUrl = edge.node.image.large;
                        character.Role = actor.Name;
                        character.Type = PersonType.GuestStar;
                        lpi.Add(character);
                    }
                }
            }
            return lpi;
        }

        /// <summary>
        /// API call to get the studios of the anime
        /// </summary>
        /// <param name="WebContent"></param>
        /// <returns></returns>
        public List<string> Get_Studio(BaseMedia media)
        {
            List<string> studios = new List<string>();
            media.studios.edges.ForEach(edge => studios.Add(edge.node.name));
            return studios;
        }

        /// <summary>
        /// API call to get the tags of the anime
        /// </summary>
        /// <param name="WebContent"></param>
        /// <returns></returns>
        public List<string> Get_Tag(BaseMedia media)
        {
            List<string> tags = new List<string>();
            media.tags.ForEach(tag => tags.Add(tag.name));
            return tags;
        }

        /// <summary>
        /// Convert int to Guid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async static Task<Guid> ToGuid(int value, CancellationToken cancellationToken)
        {
            byte[] bytes = new byte[16];
            await Task.Run(() => BitConverter.GetBytes(value).CopyTo(bytes, 0), cancellationToken);
            return new Guid(bytes);
        }

        /// <summary>
        /// API call to get the genre of the anime
        /// </summary>
        /// <param name="WebContent"></param>
        /// <returns></returns>
        public List<string> Get_Genre(BaseMedia media)
        {
            return media.genres ?? new List<string>();
        }

        /// <summary>
        /// API call to get the img url
        /// </summary>
        /// <param name="WebContent"></param>
        /// <returns></returns>
        public string Get_ImageUrl(BaseMedia media)
        {
            return media.coverImage?.large;
        }

        /// <summary>
        /// API call to get the description
        /// </summary>
        /// <param name="WebContent"></param>
        /// <returns></returns>
        public string Get_Overview(BaseMedia media)
        {
            return media.description;
        }

        /// <summary>
        /// API call to search a title and return the right one back
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> Search_GetSeries(string title, CancellationToken cancellationToken)
        {
            string result = null;
            RootObject WebContent = await WebRequestAPI(SearchLink.Replace("{0}", title, StringComparison.OrdinalIgnoreCase), cancellationToken);
            foreach (Medium media in WebContent.data.Page.media)
            {
                //get id

                try
                {

                    if (Equals_check.Compare_strings(media.title.romaji, title))
                    {
                        return media.id.ToString();
                    }
                    if (Equals_check.Compare_strings(media.title.english, title))
                    {
                        return media.id.ToString();
                    }
                    //Disabled due to false result.
                    /*if (await Task.Run(() => Equals_check.Compare_strings(media.title.native, title)))
                    {
                        return media.id.ToString();
                    }*/
                }

                catch (Exception) { }
            }

            return result;
        }

        /// <summary>
        /// API call to search a title and return a list back
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<string>> Search_GetSeries_list(string title, CancellationToken cancellationToken)
        {
            List<string> result = new List<string>();
            RootObject WebContent = await WebRequestAPI(SearchLink.Replace("{0}", title, StringComparison.OrdinalIgnoreCase), cancellationToken);
            foreach (Medium media in WebContent.data.Page.media)
            {
                //get id

                try
                {

                    if (Equals_check.Compare_strings(media.title.romaji, title))
                    {
                        result.Add(media.id.ToString());
                    }
                    if (Equals_check.Compare_strings(media.title.english, title))
                    {
                        result.Add(media.id.ToString());
                    }
                    //Disabled due to false result.
                    /*if (await Task.Run(() => Equals_check.Compare_strings(media.title.native, title)))
                    {
                        result.Add(media.id.ToString());
                    }*/
                }

                catch (Exception) { }
            }
            return result;
        }

        /// <summary>
        /// SEARCH Title
        /// </summary>
        public async Task<string> FindSeries(string title, CancellationToken cancellationToken)
        {
            string aid = await Search_GetSeries(title, cancellationToken);
            if (!string.IsNullOrEmpty(aid))
            {
                return aid;
            }
            aid = await Search_GetSeries(Equals_check.Clear_name(title), cancellationToken);
            if (!string.IsNullOrEmpty(aid))
            {
                return aid;
            }
            return null;
        }

        public async Task<RootObject> WebRequestAPI(string link, CancellationToken cancellationToken)
        {
            var options = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = link
            };

            options.SetPostData(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

            using (var response = await _httpClient.SendAsync(options, "POST").ConfigureAwait(false))
            {
                using (var stream = response.Content)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = await reader.ReadToEndAsync().ConfigureAwait(false);

                        //_logger.Info("Anilist response: {0}", json);

                        return _jsonSerializer.DeserializeFromString<RootObject>(json);
                    }
                }
            }
        }
    }
}