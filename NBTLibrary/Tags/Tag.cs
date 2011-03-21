using System.Collections.Generic;
namespace NBTLibrary.Tags
{
    public class Tag // struct? // no :)
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
            return Type.ToString() + ": " + _Name;
        }

    }
}

        //TODO:
        /*
         * just a thought... this works, but another way of doing it that I find is less confusing, but maybe a tiny bit more work...

is if yo uhave like


abstract class Tag {}
and then derive each tag type from it
class ByteTag: Tag
{
  byte Payload;
}
class ByteArrayTag: Tag
{
  byte[] Payload;
}
class Compound: Tag
{
}

...

...And you could implement it so that each tag knows how to load and save itself...

abstract class Tag
{
  static Tag Create(Stream s);
  protected (abstract?) Tag(Stream s);
  abstract void Save(Stream s);
}

That way... The static method can determine which type it is, and then call the appropriate constructor for ByteTag, ByteArrayTag, etc... and each Tag type knows how to save itself because it must implement Save(stream s);

I dunno, i find it helps :)  I did this for a few things, like GnutellaMessages... and for the various types in BEncoding (BDocument, BInteger, BList, BDictionary... all derived from an abstract BObject)

tHSOUGHTS? :P

Seems like a lot of work just to have Saves, clearer code, and to rid of castings.........

Well, ... castings do cost :P  but anyway... it'll speed up your code down the road though...  and it is safer and less error prone

Up to you though

NBT isn't really called that much... anyways, not really my priority right now...
I'll dully note it
lol:P
         */
