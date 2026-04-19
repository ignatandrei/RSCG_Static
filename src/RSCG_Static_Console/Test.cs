using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RSCG_Static_Console;

public class DateTimeTest : IDateTime
{
    public static DateTime Now => new DateTime(1970,4,16);

    public static DateTime Today => throw new NotImplementedException();

    public static DateTime UtcNow => throw new NotImplementedException();

    public static int Compare(DateTime t1, DateTime t2)
    {
        throw new NotImplementedException();
    }

    public static int DaysInMonth(int year, int month)
    {
        throw new NotImplementedException();
    }

    public static bool Equals(DateTime t1, DateTime t2)
    {
        throw new NotImplementedException();
    }

    public static DateTime FromBinary(long dateData)
    {
        throw new NotImplementedException();
    }

    public static DateTime FromFileTime(long fileTime)
    {
        throw new NotImplementedException();
    }

    public static DateTime FromFileTimeUtc(long fileTime)
    {
        throw new NotImplementedException();
    }

    public static DateTime FromOADate(double d)
    {
        throw new NotImplementedException();
    }

    public static bool IsLeapYear(int year)
    {
        throw new NotImplementedException();
    }

    public static DateTime Parse(ReadOnlySpan<char> s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static DateTime Parse(ReadOnlySpan<char> s, IFormatProvider provider, DateTimeStyles styles)
    {
        throw new NotImplementedException();
    }

    public static DateTime Parse(string s)
    {
        throw new NotImplementedException();
    }

    public static DateTime Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static DateTime Parse(string s, IFormatProvider provider, DateTimeStyles styles)
    {
        throw new NotImplementedException();
    }

    public static DateTime ParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider provider, DateTimeStyles style)
    {
        throw new NotImplementedException();
    }

    public static DateTime ParseExact(ReadOnlySpan<char> s, string[] formats, IFormatProvider provider, DateTimeStyles style)
    {
        throw new NotImplementedException();
    }

    public static DateTime ParseExact(string s, string format, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style)
    {
        throw new NotImplementedException();
    }

    public static DateTime ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style)
    {
        throw new NotImplementedException();
    }

    public static DateTime SpecifyKind(DateTime value, DateTimeKind kind)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider provider, DateTimeStyles style, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParseExact(ReadOnlySpan<char> s, string[] formats, IFormatProvider provider, DateTimeStyles style, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out DateTime result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out DateTime result)
    {
        throw new NotImplementedException();
    }
}
public class TestData
{
    public static string GetData() => "Hello World";
    
}

public interface ITestData{

    static abstract string GetData();
    public static string GetData<T>() where T : ITestData
    {
        return T.GetData();
    }
 }

public class TestDataImplDefault : ITestData
{
    public static string GetData()
    {
        return TestData.GetData();
    }
}
