using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleForum.Helper
{
    public class PostFormatter : IPostFormatter
    {
        public string Prettify(string postContent)
        {
            //var postWithSpaces = TransformSpaces(postContent);
            //var postCodeFormatted = TransformCodeTags(postWithSpaces);
            //return postCodeFormatted;
            var result = postContent;
            if (!string.IsNullOrEmpty(result))
            {
                //Lazy loading images
                result = result.Replace(" src=\"", "data-src=\"").Replace("<img", "<img class=\"lazy\"");

                // Chen Youtube: [youtube:xyzAbc123]
                var video = "<div class=\"video\"><iframe width=\"560\" height=\"315\" title=\"YouTube embed\" src=\"https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&amp;hd=1&amp;rel=0&amp;theme=light\" allowfullscreen></iframe></div>";
                result = Regex.Replace(result, @"\[youtube:(.*?)\]", m => string.Format(video, m.Groups[1].Value));
            }
            return result;
        }

        private static string TransformSpaces(string post)
        {
            return post.Replace(Environment.NewLine, "<br />");
        }

        private static string TransformCodeTags(string post)
        {
            var head = post.Replace("[code]", "<pre>");
            return head.Replace(@"[/code]", "</pre>");
        }
    }
}
