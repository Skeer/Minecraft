namespace NBTLibrary
{
    public class Tag // struct?
    {
        public TagType Type { get; set; }
        private string _Name = "";

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public object Payload { get; set; }
    }
}
