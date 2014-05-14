namespace StompSharp.Messages
{
    internal class Header : IHeader
    {
        private readonly string _key;
        private readonly object _value;

        public Header(string key, object value)
        {
            _key = key;
            _value = value;
        }

        public string Key
        {
            get { return _key; }
        }

        public object Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Key, Value);
        }
    }
}