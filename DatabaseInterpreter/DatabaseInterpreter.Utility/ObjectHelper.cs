using Newtonsoft.Json;
using System.Linq;

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
    }
}
