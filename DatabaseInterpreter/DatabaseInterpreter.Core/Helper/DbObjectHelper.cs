using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DatabaseInterpreter.Core
{
    public class DbObjectHelper
    {
        public static void Resort<T>(List<T> dbObjects)
            where T: ScriptDbObject
        {
            for (int i = 0; i < dbObjects.Count - 1; i++)
            {
                for (int j = i + 1; j < dbObjects.Count - 1; j++)
                {
                    if (!string.IsNullOrEmpty(dbObjects[i].Definition))
                    {                       
                        Regex nameRegex = new Regex($"\\b({dbObjects[j].Name})\\b", RegexOptions.IgnoreCase);

                        if(nameRegex.IsMatch(dbObjects[i].Definition))
                        {
                            var temp = dbObjects[j];
                            dbObjects[j] = dbObjects[i];
                            dbObjects[i] = temp;
                        }              
                    }
                }
            }           
        }
    }
}
