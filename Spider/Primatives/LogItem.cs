using System;

namespace Spider
{
    public class LogItem
    {
        public LogItem(Context context, string message)
        {
            AddedOn = DateTime.Now;
            Context = context;
            Message = message;
        }

        public DateTime AddedOn { get; private set; }
        public Context Context { get; private set; }
        public string Message { get; private set; }
    }
}
