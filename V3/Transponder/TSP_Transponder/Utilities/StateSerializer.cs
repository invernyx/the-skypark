using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TSP_Transponder.Attributes;

namespace TSP_Transponder.Utilities
{
    class StateSerializer<T>
    {
        private T Source;
        private Dictionary<string, dynamic> State = null;

        public StateSerializer(T source) {
            Source = source;
        }

        public StateSerializer(T source, Dictionary<string, dynamic> state)
        {
            Source = source;
            State = state;
        }

        public void Set(string entry, Func<dynamic, dynamic> modifier = null)
        {
            Func<Type, dynamic, dynamic> get_var_val = (type, val) =>
            {
                if(val != null)
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.String: { return val; }
                        case TypeCode.Boolean: { return val; }
                        case TypeCode.Double: { return Convert.ToDouble((double)val); }
                        case TypeCode.Single: { return Convert.ToSingle((float)val); }
                        case TypeCode.UInt16: { return Convert.ToUInt16((ushort)val); }
                        case TypeCode.UInt32: { return Convert.ToUInt32((uint)val); }
                        case TypeCode.UInt64: { return Convert.ToUInt64((ulong)val); }
                        case TypeCode.Int16: { return Convert.ToInt16((short)val); }
                        case TypeCode.Int32: { return Convert.ToInt32((int)val); }
                        case TypeCode.Int64: { return Convert.ToInt64((long)val); }
                        case TypeCode.DateTime: { return (DateTime)val != null ? DateTime.Parse((string)val, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null; }
                    }
                }

                return val;
            };
            
            if (State.ContainsKey(entry))
            {
                Type classType = typeof(T);
                PropertyInfo[] pis = classType.GetProperties();
                foreach (PropertyInfo fi in pis.Where(x =>
                {
                    return (x.GetCustomAttributes(typeof(StateSerializerField), false) as StateSerializerField[]).Where(x1 => x1.Name == entry).Count() > 0;
                }))
                {
                    var value = State[entry];
                    var type = fi.PropertyType;
                    if(modifier != null)
                    {
                        fi.SetValue(Source, modifier(State[entry]));
                    }
                    else
                    {
                        fi.SetValue(Source, get_var_val(type, value));
                    }
                }

                FieldInfo[] fis = classType.GetFields();
                foreach (FieldInfo fi in fis.Where(x =>
                {
                    return (x.GetCustomAttributes(typeof(StateSerializerField), false) as StateSerializerField[]).Where(x1 => x1.Name == entry).Count() > 0;
                }))
                {
                    var value = State[entry];
                    var type = fi.FieldType;
                    if (modifier != null)
                    {
                        fi.SetValue(Source, modifier(State[entry]));
                    }
                    else
                    {
                        fi.SetValue(Source, get_var_val(type, value));
                    }
                }

            }
            
            //GUID = new Guid(state["GUID"]);
            //LoadedOn = FleetService.GetAircraft((string)state["LoadedOn"]);
            //Delivered = state.ContainsKey("Delivered") ? state["Delivered"] : false;
            //TakenCharge = state.ContainsKey("TakenCharge") ? state["TakenCharge"] : false;
            //SetLocation(state["Location"] != null ? new GeoPosition((double)state["Location"][0], (double)state["Location"][1], (double)state["Location"][2], (double)state["Location"][3]) : null);
            //DestinationIndex = Convert.ToUInt16(state["DestinationIndex"]);
        }

        public void Get(string entry, Func<string, dynamic> modifier = null)
        {
            if(State == null)
            {
                State = new Dictionary<string, dynamic>();
            }

            dynamic val = null;
            if(modifier != null)
            {
                val = modifier(entry);
            }
            else
            {
                Type classType = typeof(T);
                PropertyInfo[] pis = classType.GetProperties();
                foreach (PropertyInfo fi in pis.Where(x =>
                {
                    return (x.GetCustomAttributes(typeof(StateSerializerField), false) as StateSerializerField[]).Where(x1 => x1.Name == entry).Count() > 0;
                }))
                {
                    val = fi.GetValue(Source);
                }

                FieldInfo[] fis = classType.GetFields();
                foreach (FieldInfo fi in fis.Where(x =>
                {
                    return (x.GetCustomAttributes(typeof(StateSerializerField), false) as StateSerializerField[]).Where(x1 => x1.Name == entry).Count() > 0;
                }))
                {
                    val = fi.GetValue(Source);
                }
            }


            if (!State.ContainsKey(entry))
            {
                State.Add(entry, val);
            }
            else
            {
                State[entry] = val;
            }
        }
        
        public Dictionary<string, dynamic> Get()
        {
            if (State == null)
                State = new Dictionary<string, dynamic>();

            return State;
        }
    }

    public class StateSerializerField : System.Attribute
    {
        public string Name;
        public bool IsKey;
        public StateSerializerField(string name, bool isKey = false)
        {
            Name = name;
            IsKey = isKey;
        }
    }
    
}
