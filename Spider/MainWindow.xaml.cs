using System;
using System.ComponentModel;
using System.Windows;

namespace Spider
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool crawling;
        private Crawler crawler;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            LogItems = new BindingList<LogItem>();

            Crawling = false;
        }

        public BindingList<LogItem> LogItems { get; private set; }
        public string CloseOrCancelCaption { get; private set; }
        public int Scraped { get; private set; }
        public int Fetched { get; private set; }
        public int DupMedia { get; private set; }
        public int BadStatus { get; private set; }
        public int Errors { get; private set; }

        public bool Crawling
        {
            get
            {
                return crawling;
            }
            set
            {
                crawling = value;

                CloseOrCancelCaption = value ? "Cancel" : "Close";

                NotifyPropertyChanged("CrawlCommand");
                NotifyPropertyChanged("CloseOrCancelCaption");
            }
        }

        private void Dispatch(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        public DelegateCommand CrawlCommand
        {
            get
            {
                return new DelegateCommand(
                    () =>
                    {
                        crawler = new Crawler();

                        crawler.OnLog += (s, e) =>
                            {
                                Dispatch(() =>
                                {
                                    LogItems.Insert(0, e.Value);

                                    UpdateCounter(e.Value.Context);
                                });
                            };

                        crawler.OnFinished += (s, e) =>
                            {
                            };

                        Crawling = true;

                        crawler.Crawl();
                    },
                    () => !Crawling);
            }
        }

        private void UpdateCounter(Context context)
        {
            switch (context)
            {
                case Context.BadHTML:
                case Context.BadMedia:
                    Errors++;
                    NotifyPropertyChanged("Errors");
                    break;
                case Context.BadStatus:
                    BadStatus++;
                    NotifyPropertyChanged("BadStatus");
                    break;
                case Context.DupMedia:
                    DupMedia++;
                    NotifyPropertyChanged("DupMedia");
                    break;
                case Context.GoodHTML:
                    Scraped++;
                    NotifyPropertyChanged("Scraped");
                    break;
                case Context.GoodMedia:
                    Fetched++;
                    NotifyPropertyChanged("Fetched");
                    break;
            };
        }

        public DelegateCommand CloseOrCancelCommand
        {
            get
            {
                return new DelegateCommand(
                    () =>
                    {
                        if (Crawling)
                        {
                            crawler.Cancel();

                            Crawling = false;
                        }
                        else
                        {
                            Close();
                        }
                    });
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
