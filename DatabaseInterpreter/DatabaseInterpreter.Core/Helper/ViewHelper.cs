using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DatabaseInterpreter.Core
{
    public class ViewHelper
    {
        public static List<View> ResortViews(List<View> views)
        {
            for (int i = 0; i < views.Count - 1; i++)
            {
                for (int j = i + 1; j < views.Count - 1; j++)
                {
                    if (!string.IsNullOrEmpty(views[i].Definition))
                    {                       
                        Regex nameRegex = new Regex($"\\b({views[j].Name})\\b", RegexOptions.IgnoreCase);

                        if(nameRegex.IsMatch(views[i].Definition))
                        {
                            var temp = views[j];
                            views[j] = views[i];
                            views[i] = temp;
                        }              
                    }
                }
            }
            return views;
        }
    }
}
