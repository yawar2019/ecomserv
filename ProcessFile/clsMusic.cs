using FFMpegSharp;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using YoutubeSearch;

namespace bomoserv.ProcessFile
{
    public class clsMusic
    {
        public bool GetMusic(string url, out Data.MPData res)
        {
            string log_key = "GetMusic Go6";
            res = new Data.MPData();
            bool stat = false;
            try
            { 
                System.Collections.Generic.List<YoutubeExtractor.VideoInfo> items = new List<YoutubeExtractor.VideoInfo>();
                int id = 0; 
                string download_url = "";
                int res1 = 0;
                try
                {
                    items = (List<YoutubeExtractor.VideoInfo>)YoutubeExtractor.DownloadUrlResolver.GetDownloadUrls(url);
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].VideoType != YoutubeExtractor.VideoType.Mp4 || items[i].Resolution == 0)
                            continue;
                        if (res1 == 0)
                        {
                            res1 = items[i].Resolution;
                            id = i;
                            download_url = items[i].DownloadUrl;
                        }
                        if (items[i].Resolution < res1)
                        {
                            res1 = items[i].Resolution;
                            id = i;
                            download_url = items[i].DownloadUrl;
                        }
                    } 
                }
                catch
                {

                }
                if (download_url == "")
                {
                    WebClient client = new WebClient();
                    string data = client.DownloadString(url);
                    data = splt_n_get(data, "url_encoded_fmt_stream_map", 1);
                    data = splt_n_get(data, "&", 0);
                    data = HttpUtility.UrlDecode(data);
                    data = splt_n_get(data, "url=", 1);
                    data = splt_n_get(data, "&", 0);
                    data = HttpUtility.UrlDecode(data);
                    download_url = data;
                }
               // if (DownloadVideo(download_url, out arr))
                {
                    //    string path=  EmergenceGuardian.FFmpeg.FFmpegConfig.FFmpegPath;
                }
                stat = true;
            }
            catch (Exception exp)
            {
                clsCommon common = new clsCommon();
                common.Log(log_key, exp.Message, true, exp);
            }
            return stat;
        }
        private string splt_n_get(string val, string plitter, int num)
        {
            var splt = split(val, plitter);
            return splt[num];
        }
        public bool GetDataList(string querystring, out List<Data.MPData> res)
        {
            string log_key = "GetDataList Go6";
            res = new List<Data.MPData>();
            bool stat = false;
            try
            { 
                int querypages = 1;
                var items = new VideoSearch();
                int count = 0;
                List<VideoInformation> items_all = new List<VideoInformation>(); 
                foreach (VideoInformation item in items.SearchQuery(querystring, querypages))
                {
                    try
                    {
                        Data.MPData this_data = new Data.MPData();  
                        this_data.videoId = item.Url;
                        this_data.thumbnail = item.Thumbnail;
                        this_data.title_lengthy = item.Title;
                        this_data.simpleText = item.Title;
                        this_data.lengthText = item.Duration.ToString();
                        this_data.viewCountText = "0";
                        this_data.publishedTimeText = "";
                        if (this_data.videoId != "" && this_data.simpleText != "")
                            res.Add(this_data);
                    }
                    catch (Exception expp)
                    {

                    }
                    count++;
                }
                stat = true;
            }
            catch (Exception exp)
            {
                clsCommon common = new clsCommon();
                common.Log(log_key, exp.Message, true, exp);
            }
            return stat;
        }
        public bool GetList(string keyword,out  List<Data.MPData> res)
        {
            string log_key = "GetList Go6"; 
            res = new List<Data.MPData>();
            bool stat = false;
            try
            {
                string url = "https://www.youtube.com/results?search_query=" + keyword;
                var hw = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = hw.Load(url);
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//script"))
                {
                    if (!node.InnerHtml.Contains("window[\"ytInitialData\"]"))
                    {
                        continue;
                    }
                    string html_data = node.InnerHtml;
                    html_data = split(html_data, "ytInitialData")[1].ToString().Trim();
                    html_data = html_data.Replace("\"] = ", "");
                    html_data = split(html_data, "ytInitialPlayerResponse")[0].ToString().Trim();
                    html_data = html_data.Replace("window[\"", "").ToString().Trim();
                    html_data = html_data.Remove(html_data.Length - 1, 1);
                    dynamic parsedJson = JsonConvert.DeserializeObject(html_data);
                    foreach (var item in parsedJson.contents.twoColumnSearchResultsRenderer.primaryContents.sectionListRenderer.contents[0].itemSectionRenderer.contents)
                    {
                        try
                        {
                            Data.MPData this_data = new Data.MPData();
                            var videoRenderer = item.videoRenderer;
                            this_data.videoId = "";
                            this_data.thumbnail = "";
                            this_data.title_lengthy = "";
                            this_data.simpleText = "";
                            this_data.lengthText = "";
                            this_data.viewCountText = "";
                            this_data.publishedTimeText = "";
                            this_data.videoId = videoRenderer.videoId; 
                            try
                            {
                                if (videoRenderer.thumbnail.thumbnails.Count > 0)
                                {
                                    this_data.thumbnail = videoRenderer.thumbnail.thumbnails[0].url;
                                }

                            }
                            catch
                            {

                            }
                            try
                            {
                                this_data.title_lengthy = videoRenderer.title.accessibility.accessibilityData.label; 

                            }
                            catch
                            {

                            }
                            try
                            {
                                this_data.simpleText = videoRenderer.title.simpleText;
                            }
                            catch
                            {

                            }
                            try
                            {
                                this_data.lengthText = videoRenderer.lengthText.accessibility.accessibilityData.label;
                            }
                            catch
                            {

                            }

                            try
                            {
                                this_data.viewCountText = videoRenderer.viewCountText.simpleText;
                            }
                            catch
                            {

                            }
                            try
                            {
                                this_data.publishedTimeText = videoRenderer.publishedTimeText.simpleText;
                            }
                            catch
                            {

                            }
                            if (this_data.videoId != "" && this_data.simpleText != "")
                                res.Add(this_data);

                        }
                        catch (Exception exp)
                        {
                            clsCommon common = new clsCommon();
                            common.Log(log_key, exp.Message, true, exp);
                        }
                    }
                    stat = true;
                }
            }
            catch (Exception exp)
            {
                clsCommon common = new clsCommon();
                common.Log(log_key, exp.Message, true, exp);
            }
            return stat;
        }
        public static string[] split(string val, string pattern)
        {
            return System.Text.RegularExpressions.Regex.Split(val, pattern);
        }
    }
  
}