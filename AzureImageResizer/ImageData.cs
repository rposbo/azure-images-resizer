using System;

namespace AzureImageResizer
{
    public class ImageData
    {
        public string Name;
        public byte[] Data;
        public int Height;
        public int Width;
        public DateTime Timestamp;

        public string FormattedName
        {
            get { return string.Format("{0}_{1}-{2}", Height, Width, Name.Replace("/", string.Empty)); }
        }
    }
}