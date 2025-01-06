using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace TSP_Transponder.Utilities
{
    class ClassSerializer<T>
    {
        private T Source;
        private bool All = false;
        private Dictionary<string, dynamic> OutputFields = null;
        private Dictionary<string, dynamic> State = new Dictionary<string, dynamic>();

        public ClassSerializer(T obj, Dictionary<string, dynamic> outputFields)
        {
            Source = obj;
            OutputFields = outputFields;

            All = outputFields != null ? outputFields.ContainsKey("_all") : false;
        }
        
        public void Generate(Type classType, Dictionary<string, dynamic> outputFields)
        {
            PropertyInfo[] pis = classType.GetProperties();
            foreach (PropertyInfo fi in pis)
            {
                ClassSerializerField[] attrs = fi.GetCustomAttributes(typeof(ClassSerializerField), false) as ClassSerializerField[];
                if(attrs != null ? attrs.Length > 0 : false)
                {
                    if ((outputFields != null ? outputFields.ContainsKey(attrs[0].Name) : true) || All)
                        State.Add(attrs[0].Name, fi.GetValue(Source));
                }
            }

            FieldInfo[] fis = classType.GetFields();
            foreach (FieldInfo fi in fis)
            {
                ClassSerializerField[] attrs = fi.GetCustomAttributes(typeof(ClassSerializerField), false) as ClassSerializerField[];
                if (attrs != null ? attrs.Length > 0 : false)
                {
                    if ((outputFields != null ? outputFields.ContainsKey(attrs[0].Name) : true) || All)
                        State.Add(attrs[0].Name, fi.GetValue(Source));
                }
            }

        }
        
        public void Get(string field, Dictionary<string, dynamic> outputFields, Func<Dictionary<string, dynamic>, dynamic> value)
        {
            if(All)
            {
                State.Add(field, value(null));
            }
            else
            {
                if (OutputFields != null)
                {
                    if (OutputFields.ContainsKey(field))
                    {
                        if (outputFields[field] is Boolean)
                        {
                            State.Add(field, value(null));
                        }
                        else if (outputFields[field] is Dictionary<string, dynamic>)
                        {
                            State.Add(field, value(outputFields[field]));
                        }
                        else
                        {
                            State.Add(field, outputFields[field]);
                        }
                    }
                }
                else
                {
                    State.Add(field, value(null));
                }
            }
        }
        
        public Dictionary<string, dynamic> Get()
        {
            return State;
        }
    }

    public class ClassSerializerField : System.Attribute
    {
        public string Name;
        public bool IsKey;
        public ClassSerializerField(string name, bool isKey = false)
        {
            Name = name;
            IsKey = isKey;
        }
    }
    
}
