using System.Runtime.CompilerServices;
using System.Text;

namespace SharpSfv
{
    internal static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToUtf8(this string str) => Encoding.UTF8.GetBytes(str);
    }
}
