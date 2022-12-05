using DatabaseInterpreter.Utility;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmImageViewer : Form
    {
        static readonly List<byte> jpg = new List<byte> { 0xFF, 0xD8 };
        static readonly List<byte> bmp = new List<byte> { 0x42, 0x4D };
        static readonly List<byte> gif = new List<byte> { 0x47, 0x49, 0x46 };
        static readonly List<byte> png = new List<byte> { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        static readonly List<byte> svg_xml_small = new List<byte> { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }; 
        static readonly List<byte> svg_xml_capital = new List<byte> { 0x3C, 0x3F, 0x58, 0x4D, 0x4C }; 
        static readonly List<byte> svg_small = new List<byte> { 0x3C, 0x73, 0x76, 0x67 }; 
        static readonly List<byte> svg_capital = new List<byte> { 0x3C, 0x53, 0x56, 0x47 }; 
        static readonly List<byte> intel_tiff = new List<byte> { 0x49, 0x49, 0x2A, 0x00 };
        static readonly List<byte> motorola_tiff = new List<byte> { 0x4D, 0x4D, 0x00, 0x2A };

        static readonly List<(List<byte> magic, string extension)> imageFormats = new List<(List<byte> magic, string extension)>()
        {
            (jpg, "jpg"),
            (bmp, "bmp"),
            (gif, "gif"),
            (png, "png"),
            (svg_small, "svg"),
            (svg_capital, "svg"),
            (intel_tiff,"tif"),
            (motorola_tiff, "tif"),
            (svg_xml_small, "svg"),
            (svg_xml_capital, "svg")
        };

        public frmImageViewer()
        {
            InitializeComponent();
        }

        private void frmImageViewer_Load(object sender, EventArgs e)
        {

        }

        private void btnView_Click(object sender, EventArgs e)
        {
            string content = this.txtContent.Text.Trim();

            if(string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Content can't be empty!");
                return;
            }

            try
            {
                var bytes = ValueHelper.HexStringToBytes(content);

                string extension = TryGetExtension(bytes);

                if(!string.IsNullOrEmpty(extension))
                {
                    this.lblExtension.Text = extension;
                }

                //ole bytes array
                if(content.StartsWith("0x15", StringComparison.OrdinalIgnoreCase))
                {
                    bytes = bytes.Skip(78).ToArray();
                }

                using(MemoryStream ms =new MemoryStream(bytes))
                {                    
                    this.pictureBox.Image = Image.FromStream(ms);                
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex));
            }
        }

        /// <summary>
        /// https://www.c-sharpcorner.com/blogs/auto-detecting-image-type-and-extension-from-byte-in-c-sharp
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string TryGetExtension(Byte[] array)
        {
            foreach (var imageFormat in imageFormats)
            {
                if (IsImage(array, imageFormat.magic))
                {
                    if (imageFormat.magic != svg_xml_small && imageFormat.magic != svg_xml_capital)
                        return imageFormat.extension;

                    // special handling for SVGs starting with XML tag
                    int readCount = imageFormat.magic.Count; // skip XML tag
                    int maxReadCount = 1024;

                    do
                    {
                        if (IsImage(array, svg_small, readCount) || IsImage(array, svg_capital, readCount))
                        {
                            return imageFormat.extension;
                        }

                        readCount++;
                    }
                    while (readCount < maxReadCount && readCount < array.Length - 1);

                    return null;
                }
            }
            return null;
        }

        private static bool IsImage(Byte[] array, List<byte> comparer, int offset = 0)
        {
            int arrayIndex = offset;
            foreach (byte c in comparer)
            {
                if (arrayIndex > array.Length - 1 || array[arrayIndex] != c)
                    return false;
                ++arrayIndex;
            }
            return true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
