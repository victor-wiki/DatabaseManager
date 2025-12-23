using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace DatabaseInterpreter.Utility
{
    public class ObjectHelper
    {
        public static T CloneObject<T>(object obj)
        {
            return (T)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj), typeof(T));
        }

        public static void CopyProperties(object source, object target)
        {
            var sourceProps = source.GetType().GetProperties().Where(x => x.CanRead).ToList();
            var targetProps = target.GetType().GetProperties().Where(x => x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (targetProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = targetProps.FirstOrDefault(x => x.Name == sourceProp.Name);
                    if (p != null && p.CanWrite)
                    {
                        p.SetValue(target, sourceProp.GetValue(source, null), null);
                    }
                }
            }
        }

        public static bool AreObjectsEqual(object object1, object object2)
        {
            if (object1 == null || object2 == null)
            {
                return object1 == object2;
            }

            Type type1 = object1.GetType();
            Type type2 = object2.GetType();

            PropertyInfo[] properties1 = type1.GetProperties();
            PropertyInfo[] properties2 = type1.GetProperties();

            if (properties1.Length != properties2.Length)
            {
                return false;
            }

            foreach (PropertyInfo property in properties1)
            {
                if (!Equals(property.GetValue(object1), property.GetValue(object2)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
