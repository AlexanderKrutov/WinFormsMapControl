using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents a tile server loading offline tiles from a ZIP file.
    /// </summary>
    public class ZipTileServer : ITileServer, IDisposable
    {
        /// <summary>
        /// Zip archive instance holding the offline tiles.
        /// </summary>
        private ZipArchive zipArchive;

        /// <summary>
        /// Displayable name of the tile server
        /// </summary>
        public string Name => "ZipTileServer";

        /// <summary>
        /// Attribution text that will be displayed in bottom-right corner of the map.
        /// </summary>
        public string AttributionText => "© <a href='https://www.openstreetmap.org/copyright\'>OpenStreetMap</a> Contributors";

        /// <summary>
        /// Gets minimal zoom level allowed for the tile server.
        /// </summary>
        public int MinZoomLevel
        {
            protected set; get;
        } = 0;

        /// <summary>
        /// Gets maximal zoom level allowed for the tile server.
        /// </summary>
        public int MaxZoomLevel
        {
            protected set; get;
        } = 20;

        /// <summary>
        /// Gets the image extension of the style loaded.
        /// </summary>
        public string ImageExtension
        {
            protected set; get;
        } = "jpg";

        /// <summary>
        /// Gets the style name of the style currently loaded.
        /// </summary>
        public string StyleName
        {
            protected set; get;
        } = "default";

        /// <summary>
        /// Constructor method.
        /// </summary>
        /// <param name="zipFileName">ZIP archive containing map tiles.</param>
        public ZipTileServer(string zipFileName)
        {
            this.zipArchive = ZipFile.OpenRead(zipFileName);
            this.LoadStyle(this.StyleName);
        }

        /// <summary>
        /// Constructor method.
        /// </summary>
        /// <param name="zipFileName">ZIP archive containing map tiles.</param>
        /// <param name="styleName">Name of the map style to be loaded.</param>
        public ZipTileServer(string zipFileName, string styleName)
        {
            this.zipArchive = ZipFile.OpenRead(zipFileName);
            this.LoadStyle(styleName);
        }

        /// <summary>
        /// Loads another map style at runtime.
        /// </summary>
        /// <param name="styleName">Name of the map style to be loaded.</param>
        public void LoadStyle(string styleName)
        {
            this.StyleName = styleName;

            // load info file - if not present or valid, we don't have a valid map archive
            string infoFilePath = string.Format("{0}/info.txt", this.StyleName);
            using (Stream infoFileStream = this.zipArchive.GetEntry(infoFilePath)?.Open())
            {
                try
                {
                    using (StreamReader infoFileReader = new StreamReader(infoFileStream))
                    {
                        this.MinZoomLevel = Convert.ToInt32(infoFileReader.ReadLine());
                        this.MaxZoomLevel = Convert.ToInt32(infoFileReader.ReadLine());
                        this.ImageExtension = infoFileReader.ReadLine()?.Trim();
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Invalid map data archive.");
                }
            }
        }

        /// <summary>
        /// Gets tile image by X and Y indices of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns>Tile image.</returns>
        public Image GetTile(int x, int y, int z)
        {
            string path = string.Format("{0}/{1}/{2}/{3}.{4}", this.StyleName, z, x, y, this.ImageExtension);

            using (Stream tileStream = this.zipArchive.GetEntry(path)?.Open())
            {
                if (tileStream != null)
                {
                    return new Bitmap(tileStream);
                }
                else
                {
                    throw new Exception("Tile image does not exist.");
                }
            }            
        }

        /// <summary>
        /// Disposes the virtual tile server.
        /// </summary>
        public void Dispose()
        {
            this.zipArchive?.Dispose();
        }
    }
}
