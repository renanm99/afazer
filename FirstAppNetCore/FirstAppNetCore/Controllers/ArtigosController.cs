using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using FirstAppNetCore.Models;
using System.Xml;
using FirstAppNetCore;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml.Linq;
using System.Linq;

namespace FirstAppNetCore.Controllers
{
    public class ArtigosController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var articles = new List<FeedModel>();
            var feedUrl = "http://rss.iotevolutionworld.com/rss/grss.ashx?cnl=519";
            //var feedUrl = "http://g1.globo.com/dynamo/rss2.xml";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(feedUrl);
                var responseMessage = await client.GetAsync(feedUrl);
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                //extract feed items
                XDocument doc = XDocument.Parse(responseString);

                /*
                var cdataElement = doc.DescendantNodes().OfType<XCData>().FirstOrDefault();
                if (cdataElement != null)
                    cdataContent = cdataElement.Value;

                
    */

                var feedItems = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                                select new FeedModel
                                {
                                    Content = Conteudo(item.Elements().First(i => i.Name.LocalName == "description").Value),
                                    Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                                    Img = ImgUrl(item.Elements().First(i => i.Name.LocalName == "description").Value),
                                    PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                                    Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                                };
                articles = feedItems.ToList();
            }

            return View("Artigos", articles);

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
                content = content.Remove(0, content.LastIndexOf(">") + 2);
                return content;
            }
            return content;
        }

        private string ImgUrl(string url)
        {
            url += "";
            if (url.IndexOf("<") >= 0)
            {
                url = url.Remove(0, url.IndexOf("\"") + 1);
                url = url.Remove(url.LastIndexOf("\""));
                url += "";
                return url;
            }
            return "";
        }


    }
}