using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Spider
{
    public class Crawler
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        public event EventHandler<GenericArgs<LogItem>> OnLog;
        public event EventHandler OnFinished;

        public void Cancel()
        {
            cts.Cancel();
        }

        public async void Crawl()
        {
            var scraper = new TransformManyBlock<LinkInfo, LinkInfo>(
                async linkInfo =>
                {
                    var linkInfos = new List<LinkInfo>();

                    try
                    {
                        var response = await linkInfo.Uri.GetResponse();

                        if (!IsSuccessStatus(response, linkInfo))
                            return linkInfos;

                        var html = await response.Content.ReadAsStringAsync();

                        var doc = new HtmlDocument();

                        doc.LoadHtml(html);

                        linkInfos = doc.ToLinkInfos(linkInfo.Uri);

                        Log(Context.GoodHTML, linkInfo.Uri.AbsoluteUri);
                    }
                    catch (Exception error)
                    {
                        Log(Context.BadHTML, "Error: {0} (URL: {1})", error.Message, linkInfo.Uri);
                    }

                    return linkInfos;
                },
                new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = cts.Token
                });

            var fetcher = new ActionBlock<LinkInfo>(
                async linkInfo =>
                {
                    try
                    {
                        var fileName = linkInfo.GetFileName(linkInfo.Uri, "Downloads");

                        if (File.Exists(fileName))
                        {
                            Log(Context.DupMedia, linkInfo.Uri.AbsoluteUri);

                            return;
                        }

                        var response = await linkInfo.Uri.GetResponse();

                        if (!IsSuccessStatus(response, linkInfo))
                            return;

                        var webStream = await response.Content.ReadAsStreamAsync();

                        fileName.EnsurePathExists();

                        using (var fileStream = File.OpenWrite(fileName))
                            await webStream.CopyToAsync(fileStream);

                        Log(Context.GoodMedia, linkInfo.Uri.AbsoluteUri);
                    }
                    catch (Exception error)
                    {
                        Log(Context.BadMedia, "Error: {0} (URL: {1})", error.Message, linkInfo.Uri);
                    }
                },
                new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = cts.Token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 12
                });

            scraper.Completion.SetOnlyOnFaultedCompletion(error => HandleErrors(error));
            fetcher.Completion.SetOnlyOnFaultedCompletion(error => HandleErrors(error));

            scraper.LinkTo(scraper, new Predicate<LinkInfo>(li => li.Kind == LinkKind.HTML));
            scraper.LinkTo(fetcher, new Predicate<LinkInfo>(li => li.Kind == LinkKind.Media));

            scraper.Post(new LinkInfo(new Uri("http://www.bbc.com/news/")));

            try
            {
                await Task.WhenAll(scraper.Completion, fetcher.Completion);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                Log(Context.Failure, "Error: " + error.Message);
            }

            if (OnFinished != null)
                OnFinished(this, EventArgs.Empty);
        }

        private void HandleErrors(AggregateException errors)
        {
            int count = 0;

            foreach (var error in errors.InnerExceptions)
                Log(Context.Failure, "Failure #{0}: {1}", ++count, error.Message);

            cts.Cancel();
        }

        private bool IsSuccessStatus(HttpResponseMessage response, LinkInfo linkInfo)
        {
            if (response.IsSuccessStatusCode)
                return true;

            Log(Context.BadStatus, "{0} (Kind; {1}, URL: {2})",
                response.StatusCode, linkInfo.Kind, linkInfo.Uri);

            return false;
        }

        private void Log(Context context, string format, params object[] args)
        {
            if (OnLog != null)
            {
                OnLog(this, new GenericArgs<LogItem>(
                    new LogItem(context, string.Format(format, args))));
            }
        }
    }
}
