using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Attributes
{
    class EnumAttr
    {
        internal static string GetDescription(Enum value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            EnumValue[] attrs = fi.GetCustomAttributes(typeof(EnumValue), false) as EnumValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        internal static object GetEnum(Type enumType, string value)
        {
            FieldInfo[] fis = enumType.GetFields();
            foreach (FieldInfo fi in fis)
            {
                EnumValue[] attrs = fi.GetCustomAttributes(typeof(EnumValue), false) as EnumValue[];
                if (attrs.Length > 0)
                {
                    if (attrs[0].Value == value)
                    {
                        return System.Enum.Parse(enumType, fi.Name);
                    }
                }
            }

            return null;
        }


        public class EnumValue : System.Attribute
        {
            private string _value;
            public EnumValue(string value)
            {
                _value = value;
            }
            public string Value
            {
                get { return _value; }
            }
        }
    }
}
