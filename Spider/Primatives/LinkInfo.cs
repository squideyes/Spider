using System;
using System.IO;
using System.Diagnostics.Contracts;

namespace Spider
{
    public class LinkInfo : IComparable<LinkInfo>
    {
        public LinkInfo(Uri uri)
            : this(new Uri(uri.GetLeftPart(
                UriPartial.Authority)), uri.AbsoluteUri, LinkKind.HTML)
        {
        }

        public LinkInfo(Uri baseUri, string url, LinkKind kind)
        {
            Contract.Requires(Uri.IsWellFormedUriString(
                url, UriKind.RelativeOrAbsolute));

            Kind = kind;

            Uri uri;

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                uri = new Uri(url);
            else
                uri = new Uri(baseUri, url);

            if (kind == LinkKind.HTML)
                Uri = uri;
            else
                Uri = new Uri(uri.GetLeftPart(UriPartial.Path));
        }

        public Uri Uri { get; set; }
        public LinkKind Kind { get; set; }

        public string GetFileName(Uri uri, string basePath)
        {
            return Path.Combine(basePath, Path.GetFileName(Uri.AbsolutePath));
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Kind, Uri);
        }

        public int CompareTo(LinkInfo other)
        {
            if (Kind == other.Kind)
                return Uri.AbsoluteUri.CompareTo(other.Uri.AbsoluteUri);
            else
                return Kind.CompareTo(other.Kind);
        }
    }
}
