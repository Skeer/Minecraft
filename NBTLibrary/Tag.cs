using System.Collections.Generic;
namespace NBTLibrary
{
    public class Tag // struct?
    {

        public TagType Type { get; set; }
        private string _Name = "";
        private Dictionary<string, Tag> Indices = new Dictionary<string, Tag>();

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private object _Payload = null;

        public object Payload
        {
            get
            {
                return _Payload;
            }
            set
            {
                if (value.GetType() == typeof(List<Tag>) && Type != TagType.List)
                {
                    foreach (Tag t in (List<Tag>)value)
                    {
                        Indices.Add(t.Name, t);
                    }
                }
                _Payload = value;
            }
        }

        public Tag this[string index]
        {
            get
            {
                return Indices[index];
            }
            set
            {
                Indices[index] = value;
            }
        }

        public override string ToString()
        {
            return Type.ToString() + ": "  + _Name;
        }
    }
}
