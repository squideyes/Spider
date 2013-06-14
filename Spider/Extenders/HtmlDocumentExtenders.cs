using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spider
{
    public static class HtmlDocumentExtenders
    {
        public static List<LinkInfo> ToLinkInfos(this HtmlDocument doc, Uri uri)
        {
            var linkInfos = new List<LinkInfo>();

            var baseUri = new Uri(uri.GetLeftPart(UriPartial.Authority));

            linkInfos.AddRange(doc.GetHrefUrls(baseUri).
                Select(url => new LinkInfo(baseUri, url, LinkKind.HTML)));

            linkInfos.AddRange(doc.GetSrcUrls(baseUri).
                Select(url => new LinkInfo(baseUri, url, LinkKind.Media)));

            return linkInfos.OrderBy(li => li).ToList();
        }

        private static List<string> GetHrefUrls(this HtmlDocument doc, Uri baseUri)
        {
            return (from link in doc.DocumentNode.Descendants("a")
                    where link.Attributes.Contains("href")
                    let url = link.GetAttributeValue("href", "")
                    where IsGoodUrl(baseUri, url, false)
                    select url).Distinct().ToList();
        }

        private static List<string> GetSrcUrls(this HtmlDocument doc, Uri baseUri)
        {
            return (from link in doc.DocumentNode.Descendants("img")
                    where link.Attributes.Contains("src")
                    let url = link.GetAttributeValue("src", "")
                    where IsGoodUrl(baseUri, url, true)
                    select url).Distinct().ToList();
        }

        private static bool IsGoodUrl(Uri baseUri, string url, bool isFileUrl)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                return false;

            Uri uri = null;

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                uri = new Uri(url);
            else
                uri = new Uri(baseUri, url);

            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    break;
                default:
                    return false;
            }

            if (!isFileUrl)
                return true;

            var leftPart = uri.GetLeftPart(UriPartial.Path);

            var ext = Path.GetExtension(leftPart);

            switch (Path.GetExtension(leftPart).ToLower())
            {
                case ".jpg":
                case ".png":
                case ".gif":
                    return true;
                default:
                    return false;
            }
        }
    }
}
