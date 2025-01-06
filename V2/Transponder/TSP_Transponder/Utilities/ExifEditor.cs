using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Utilities
{
    internal class ExifEditor
    {
        /// <summary>  
        /// Some of the EXIF values for setting. To expand use complete list of EXIF values http://www.exiv2.org/tags.html  
        /// </summary>  
        public enum MetaProperty
        {
            Title = 40091,
            Comment = 40092,
            Author = 40093,
            Keywords = 40094,
            Subject = 40095,
            Copyright = 33432,
            Software = 11,
            DateTime = 36867
        }

        public enum MetaType
        {
            Byte = 1,
            ASCII = 2,
            Short = 3,
            Long = 4,
            Rational = 5,
            Undefined = 6,
            SLong = 7,
            SRational = 10
        }

        // https://www.exiv2.org/tags.html
        public static Bitmap SetMetaValue(Bitmap SourceBitmap, MetaProperty property, MetaType type, string value)
        {
            PropertyItem prop = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
            
            prop.Id = (int)property;
            prop.Type = (short)type;
            prop.Value = System.Text.Encoding.UTF8.GetBytes(value);
            prop.Len = prop.Value.Length;

            SourceBitmap.SetPropertyItem(prop);

            return SourceBitmap;
        }
        /// <summary>  
        /// Returns meta value from the bitmap  
        /// </summary>  
        /// <param name="SourceBitmap">Bitmap to which changes will be applied to</param>  
        /// <param name="property">Property enum value containing the id of the property to be changed</param>  
        /// <returns>Returns value of the bitmap meta property</returns>  
        public static string GetMetaValue(Bitmap SourceBitmap, MetaProperty property)
        {
            PropertyItem[] propItems = SourceBitmap.PropertyItems;
            var prop = propItems.FirstOrDefault(p => p.Id == (int)property);
            if (prop != null)
            {
                return Encoding.UTF8.GetString(prop.Value);
            }
            else
            {
                return null;
            }
        }
    }
}
