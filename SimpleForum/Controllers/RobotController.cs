using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.Data;
using WebEssentials.AspNetCore.OutputCaching;

namespace SimpleForum.Controllers
{
    public class RobotController : Controller
    {
        private readonly IPost postService;

        public RobotController(IPost postService)
        {
            this.postService = postService;
        }


        [Route("/robots.txt")]
        [OutputCache(Duration =3600)]
        public string Robots()
        {
            string host = Request.Scheme + "://" + Request.Host;
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Disallow: /Profile/");
            sb.AppendLine("Disallow: /Admin");
            sb.AppendLine("Disallow: /Identity/");
            sb.AppendLine("Disallow: /Moderator/");
            sb.AppendLine($"sitemap: {host}/sitemap.xml");

            return sb.ToString();
        }

        [Route("/sitemap.xml")]
        [OutputCache(Duration = 60)]
        public void SitemapXml()
        {
            string host = Request.Scheme + "://" + Request.Host;

            Response.ContentType = "application/xml";

            using (var xml = XmlWriter.Create(Response.Body, new XmlWriterSettings { Indent = true }))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                var posts = postService.GetAll();

                foreach (var p in posts)
                {
                    var lastMod = p.Created;

                    xml.WriteStartElement("url");
                    xml.WriteElementString("loc", host + "/Post/"+p.Slug);
                    xml.WriteElementString("lastmod", lastMod.ToString("yyyy-MM-ddThh:mmzzz"));
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }
        }
    }
}