using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Attributes
{
    class ObjAttr
    {
        internal static string GetDescription(object value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            ObjValue[] attrs = fi.GetCustomAttributes(typeof(ObjValue), false) as ObjValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        internal static object GetObj(object obj, object parent, string value)
        {
            FieldInfo[] fis = parent.GetType().GetFields();
            foreach (FieldInfo fi in fis)
            {
                ObjValue[] attrs = fi.GetCustomAttributes(typeof(ObjValue), false) as ObjValue[];
                if (attrs.Length > 0)
                {
                    if (attrs[0].Value == value)
                    {
                        return fi.GetValue(parent); //fi.GetValue(obj); //.GetValue(fi.Name) System.Enum.Parse(enumType, fi.Name);
                    }
                }
            }

            return null;
        }


        public class ObjValue : System.Attribute
        {
            private string _value;
            public ObjValue(string value)
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
