﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoopImage
{
    public class Mvc
    {
        public static string Go(object html, int height, int width, int maxheight, int maxwidth, bool constrain, int quality, string watermark)
        {
            string pattern = @"(<img.+?src=[\""'])(.+?)([\""'].*?>)";

            return System.Text.RegularExpressions.Regex.Replace(html.ToString(), pattern, delegate(System.Text.RegularExpressions.Match match)
            {
                var ss = match.Groups[2].Value.Split(new char[] { '?' });
                var group2 = String.Format("/CoopImage.ashx?imagepath={0}&amp;height={1}&amp;width={2}&amp;maxheight={3}&amp;maxwidth={4}&amp;constrain={5}&amp;quality={6}&amp;watermark={7}",
                    ss[0],
                    height,
                    width,
                    maxheight,
                    maxwidth,
                    constrain,
                    quality,
                    System.Web.HttpUtility.UrlEncode(watermark)
                );
                return match.Groups[1].Value + group2 + match.Groups[3].Value;
            });
        }
    }
}
