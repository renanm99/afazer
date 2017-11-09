using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirstAppNetCore.Models;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Azure.Documents.Client;

namespace FirstAppNetCore.Controllers
{
    public class ApproveController : Controller
    {
        public async Task<IActionResult> Index()
        {
            Program Connection = new Program();
            string EndpointUrl = Connection.EndpointUrl;
            string PrimaryKey = Connection.PrimaryKey;
            DocumentClient client;

            var articles = new List<FeedModel>();

            articles.AddRange(await GetFeed("https://blogs.microsoft.com/iot/feed/"));
            articles.AddRange(await GetFeed("https://staceyoniot.com/feed/"));

            return View("Approve", articles.OrderByDescending((o => o.PublishDate)));

            //Bloco para gravar no banco
            /*
            using (client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey))
            {/*
                try
                {
                    foreach (var item in articles)
                    {
                        await client.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri("Teste", "CTeste"),
                        new FeedModel
                        {
                            Title = item.Title,
                            Content = item.Content,
                            Link = item.Link,
                            Img = item.Img,
                            PublishDate = item.PublishDate,
                        }
                        );
                    }

                    //Bloco para ler do banco (Aproveitando espaço e using)
                    IQueryable<FeedModel> queryable =
                        client.CreateDocumentQuery<FeedModel>(UriFactory.CreateDocumentCollectionUri("Teste", "CTeste"));
                    List<FeedModel> posts = queryable.ToList();
                    posts.RemoveRange(20, posts.Count - 20);

                    return View("Index", posts.OrderBy(o => o.PublishDate));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
            }
                */





            //Bloco para ler do banco (bloco seprado)
            /*
            using (client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey))
            {
                IQueryable<FeedModel> queryable =
                client.CreateDocumentQuery<FeedModel>(UriFactory.CreateDocumentCollectionUri("Teste", "CTeste"));
                List<FeedModel> posts = queryable.ToList();
                posts.RemoveRange(20, posts.Count - 20);

                return View("Index", posts.OrderBy(o => o.PublishDate));
            }
            */

        }

        public async Task<IEnumerable<FeedModel>> GetFeed(string feedUrl)
        {
            IEnumerable<FeedModel> feedItems;
            //var feedUrl = "https://staceyoniot.com/feed/";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(feedUrl);
                var responseMessage = await client.GetAsync(feedUrl);
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                //extract feed items
                XDocument doc = XDocument.Parse(responseString);

                feedItems = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                            select new FeedModel
                            {
                                Content = (item.DescendantNodes().OfType<XCData>().Last().Value),
                                Description = item.Elements().First(i => i.Name.LocalName == "description").Value,
                                Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                                Fonte = doc.Root.Descendants().Elements().First(i => i.Name.LocalName == "title").Value,
                                Img = ImgUrl(item.DescendantNodes().OfType<XCData>().Last().Value),
                                Ratio = ratio(item.DescendantNodes().OfType<XCData>().Last().Value),
                                PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                                Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                            };
            }

            return feedItems.ToList();

        }

        private DateTime ParseDate(string date)
        {
            DateTime result;
            if (DateTime.TryParse(date, out result))
                return result;
            else
                return DateTime.MinValue;
        }

        private string Conteudo(string content)
        {
            if (content.IndexOf("<") >= 0)
            {
                content = content.Remove(0, content.LastIndexOf("e>") + 2);
                return content;
            }
            return content;
        }

        private string ImgUrl(string url)
        {
            if (url.IndexOf("src=") > 0)
            {
                int a = url.IndexOf("src=\"") + 5;
                int b = url.IndexOf("alt=") - 2;
                url = url.Substring(a, b - a);

                if (url.Equals("https://mscorpmedia.azureedge.net/mscorpmedia/2018/03/ioytCTA_v4.png"))
                {
                    return "/img/single-post.jpg";
                }

                return url;
            }

            return "/img/single-post.jpg";
        }

        private bool ratio(string url)
        {
            string aspect = url;
            if (url.IndexOf("src=") > 0)
            {
                int a = url.IndexOf("src=\"") + 5;
                int b = url.IndexOf("alt=") - 2;
                url = url.Substring(a, b - a);

                a = aspect.IndexOf("width=");
                b = aspect.IndexOf("height=") + 13;
                aspect = aspect.Substring(a, b - a);
                aspect = aspect.Replace("'", "");
                aspect = aspect.Replace("\"", "");


                string width = "";
                string height = "";
                double ratio = 0;

                width = (aspect.Substring(aspect.IndexOf("=") + 1, aspect.IndexOf("h"))).ToString();
                width = width.Replace("h", "");
                width = width.Replace(" ", "");
                width = width.Replace("e", "");
                height = (aspect.Substring(aspect.LastIndexOf("=") + 1, aspect.Length - aspect.LastIndexOf("=") - 1)).ToString();
                height = height.Replace(" ", "");
                if (Int32.Parse(width) > 0 && Int32.Parse(height) > 0)
                {
                    ratio = (Int32.Parse(width) * 1.0 / Int32.Parse(height));
                    if (ratio > 1.4 && ratio < 2.2)
                    {
                        return true;
                    }
                    else if (ratio > 1.1 && ratio < 1.4)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}