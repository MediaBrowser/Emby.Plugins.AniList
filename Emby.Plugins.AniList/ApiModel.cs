using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Emby.Plugins.AniList
{
    public class Title
    {
        public string romaji { get; set; }
        public string english { get; set; }
        public string native { get; set; }
    }

    public class CoverImage
    {
        public string medium { get; set; }
        public string large { get; set; }
    }

    public class StartDate
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
    }

    public class EndDate
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
    }

    public class Status
    {
        public static string FINISHED { get { return "FINISHED"; } }
        public static string RELEASING  { get { return "RELEASING"; } }
        public static string NOT_YET_RELEASED { get { return "NOT_YET_RELEASED"; } }
        public static string CANCELLED { get { return "CANCELLED"; } }
        public static string HIATUS { get { return "HIATUS"; } }
    }

    public class Medium : BaseMedia
    {
        public string format { get; set; }
        public string type { get; set; }
        public int popularity { get; set; }
        public string season { get; set; }
        public string hashtag { get; set; }
        public bool isAdult { get; set; }
        public object bannerImage { get; set; }
        public object chapters { get; set; }
        public object volumes { get; set; }
        public int meanScore { get; set; }
        public List<object> synonyms { get; set; }
        public object nextAiringEpisode { get; set; }
    }

    public class Page
    {
        public List<Medium> media { get; set; }
    }

    public class Data
    {
        public Page Page { get; set; }
        public Media Media { get; set; }
    }

    public class BaseMedia
    {
        public int id { get; set; }
        public Title title { get; set; }
        public CoverImage coverImage { get; set; }
        public string description { get; set; }
        public StartDate startDate { get; set; }
        public EndDate endDate { get; set; }
        public int episodes { get; set; }
        public int duration { get; set; }
        public int averageScore { get; set; }
        public Trailer trailer { get; set; }
        public string status { get; set; }
        public List<string> genres { get; set; }
        public Studios studios { get; set; }
        public List<Tag> tags { get; set; }
        public Characters characters { get; set; }
    }

    public class Media : BaseMedia
    {
        public int popularity { get; set; }
        public object hashtag { get; set; }
        public bool isAdult { get; set; }
        public object bannerImage { get; set; }
        public string format { get; set; }
        public string type { get; set; }
        public object chapters { get; set; }
        public object volumes { get; set; }
        public string season { get; set; }
        public int meanScore { get; set; }
        public List<object> synonyms { get; set; }
        public object nextAiringEpisode { get; set; }
    }
    public class PageInfo
    {
        public int total { get; set; }
        public int perPage { get; set; }
        public bool hasNextPage { get; set; }
        public int currentPage { get; set; }
        public int lastPage { get; set; }
    }

    public class Name
    {
        public string first { get; set; }
        public string last { get; set; }
    }

    public class Image
    {
        public string medium { get; set; }
        public string large { get; set; }
    }

    public class Node
    {
        public int id { get; set; }
        public Name name { get; set; }
        public Image image { get; set; }
    }

    public class Name2
    {
        public string first { get; set; }
        public string last { get; set; }
        public string native { get; set; }
    }

    public class Image2
    {
        public string medium { get; set; }
        public string large { get; set; }
    }

    public class VoiceActor
    {
        public int id { get; set; }
        public Name2 name { get; set; }
        public Image2 image { get; set; }
        public string language { get; set; }
    }

    public class Edge
    {
        public Node node { get; set; }
        public string role { get; set; }
        public List<VoiceActor> voiceActors { get; set; }
    }

    public class Characters
    {
        public PageInfo pageInfo { get; set; }
        public List<Edge> edges { get; set; }
    }

    public class StudioNode
    {
        public string name { get; set; }
    }

    public class StudioEdge
    {
        public StudioNode node { get; set; }
    }

    public class Studios
    {
        public PageInfo pageInfo { get; set; }
        public List<StudioEdge> edges { get; set; }
    }

    public class Trailer
    {
        public string id { get; set; }
        public string site { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
    }


    public class RootObject
    {
        public Data data { get; set; }

        public BaseMedia GetMedia()
        {
            return (data?.Media as BaseMedia) ?? data?.Page.media?.FirstOrDefault();
        }
    }
}
