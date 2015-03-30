using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoopImage
{
    public class ImageRequest
    {
        public string ImagePath = string.Empty;
        public bool Constrain = false;
        public int Height = 0;
        public int Width = 0;
        public int MaxHeight = 0;
        public int MaxWidth = 0;
        public int Quality = 0;
        public String Watermark = string.Empty;
    }
}
