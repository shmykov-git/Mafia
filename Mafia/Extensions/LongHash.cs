using System.Linq;

namespace Mafia.Extensions;

public static class LongHash
{
    public static ulong GetULongHash(this IEnumerable<ulong> vs)
    {
        return vs.Aggregate(37ul, GetU);
    }

    public static ulong GetULongHash(this IEnumerable<IEnumerable<ulong>> vss)
    {
        return vss.Select(GetULongHash).Aggregate(37ul, GetU);
    }

    public static ulong GetULongHash(this string s)
    {
        return GetU(s);
    }

    public static ulong GetULongHash(this IEnumerable<string> ss)
    {
        return ss.Select(GetULongHash).Aggregate(37ul, GetU);
    }

    public static ulong GetULongHash(this IEnumerable<IEnumerable<string>> sss)
    {
        return sss.Select(GetULongHash).Aggregate(37ul, GetU);
    }

    public static ulong GetULongHash(this int value) 
    { 
        unchecked 
        { 
            return (ulong)value; 
        } 
    }

    private static long Get(long v1, long v2)
    {
        unchecked
        {
            long hash = 17;
            hash = hash * 31 + v1;
            hash = hash * 31 + v2;
            return hash;
        }
    }

    private static ulong GetU(ulong v1, ulong v2) => (ulong)Get((long)v1, (long)v2);

    private static long Get(this IEnumerable<long> list)
    {
        return list.Aggregate(37L, Get);
    }

    private static long Get(string s)
    {
        return s.Select(c => (long)c).Get();
    }

    private static ulong GetU(string s)
    {
        return s.Select(c => (ulong)c).GetULongHash();
    }
}