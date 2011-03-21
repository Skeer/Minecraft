using System.Text;

namespace Minecraft.Utilities
{
    class Base36
    {
        private const string Key = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string Parse(int i)
        {
            if (i == 0)
            {
                return "0";
            }

            StringBuilder builder = new StringBuilder();

            if (i < 0)
            {
                builder.Append('-');
                i = -i;
            }
            else
            {
                builder.Append(' ');
            }

            while (i > 0)
            {
                builder.Insert(1, Key[i % 36]);
                i /= 36;
            }
            return builder.ToString().Trim();
        }
    }
}
