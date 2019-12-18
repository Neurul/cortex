using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Application
{
    internal class Helper
    {
        internal static string GetAvatarUrl(string baseUrl, string storeId)
        {
            string result = string.Empty;
            if (Uri.TryCreate(new Uri(baseUrl), storeId, out Uri storeUrl))
                result = storeUrl.ToString() + "/";
            return result;
        }        
    }
}
