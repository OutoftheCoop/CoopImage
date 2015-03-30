using System;
using System.Web;

namespace CoopImage
{
    public class Handler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context = HttpContext.Current;

            string imagepath = context.Server.MapPath(context.Request.QueryString["imagepath"]);
            int height, width, maxheight, maxwidth, quality = 0;
            bool constrain = false;
            int.TryParse(context.Request.QueryString["height"], out height);
            int.TryParse(context.Request.QueryString["width"], out width);
            int.TryParse(context.Request.QueryString["maxheight"], out maxheight);
            int.TryParse(context.Request.QueryString["maxwidth"], out maxwidth);
            if (!int.TryParse(context.Request.QueryString["quality"], out quality))
            {
                quality = 90;
            }
            bool.TryParse(context.Request.QueryString["constrain"], out constrain);
            string watermark = context.Request.QueryString["watermark"];
            if (!String.IsNullOrEmpty(watermark))
            {
                watermark = context.Server.MapPath(watermark);
            }

            var req = new ImageRequest()
            {
                ImagePath = imagepath,
                Constrain = constrain,
                Quality = quality,
                Height = height,
                Width = width,
                MaxHeight = maxheight,
                MaxWidth = maxwidth,
                Watermark = watermark
            };

            context.Response.Clear();
            context.Response.ContentType = GetContentType(req.ImagePath);

            var cache = context.Response.Cache;
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(DateTime.Now.AddYears(1));
            cache.SetMaxAge(TimeSpan.FromDays(365));
            cache.AppendCacheExtension("");

            byte[] buffer = ImageRepository.Get(req);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.End();
        }

        public string GetContentType(string imagepath)
        {
            switch (System.IO.Path.GetExtension(imagepath))
            {
                case ".bmp": return "image/bmp";
                case ".gif": return "image/gif";
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                default: break;
            }
            return "";
        }
    }
}
