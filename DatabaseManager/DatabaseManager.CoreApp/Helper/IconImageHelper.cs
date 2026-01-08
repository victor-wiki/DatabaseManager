using FontAwesome.Sharp;
using System.Drawing;

namespace DatabaseManager.Helper
{
    public class IconImageHelper
    {
        public static readonly Color DefaultIconColor = ColorTranslator.FromHtml("#22C3E6");
        public static readonly Color TipColor = Color.DodgerBlue;

        public static int DefaultIconSize = 20;


        public static Bitmap GetImage(IconChar iconChar, Color? color=default(Color?),  int? size = default(int?))
        {
            return GetImageByFontType(iconChar, IconFont.Auto, color, size);
        }

        public static Bitmap GetImageByFontType(IconChar iconChar, IconFont font = IconFont.Auto, Color? color = default(Color?), int? size = default(int?))
        {
            return iconChar.ToBitmap(font, size.HasValue == false ? DefaultIconSize : size.Value, color.HasValue == false ? DefaultIconColor : color.Value);
        }
    }
}
