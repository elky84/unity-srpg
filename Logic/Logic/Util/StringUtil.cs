using System;
using System.Text.RegularExpressions;

namespace Logic.Util
{
    public static class StringUtil
    {

        public static string OnlyDigit(this string str)
        {
            return Regex.Replace(str, @"\D", "");
        }

        public static int ToInt(this string str)
        {
            return int.Parse(str.OnlyDigit());
        }

        public static double ToDouble(this string str)
        {
            return double.Parse(str.OnlyDigit());
        }

        public static string ExtractKorean(this string str)
        {
            return Regex.Replace(str, "[^가-힣]", "");
        }

    }
}
