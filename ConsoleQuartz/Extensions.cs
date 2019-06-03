using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleQuartz
{
    public static class Extensions
    {

        public static List<string> ParseList(this string str, char splitTag)
        {
            var data = new List<string>();

            var strSplit = splitTag.ToString();
            if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(strSplit))
            {
                data = str.Split(new char[] { splitTag }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return data;
        }
    }
}
