/*
 * @version: 2.0.0
 * @author: Ext.NET, Inc. http://www.ext.net/
 * @date: 2012-03-05
 * @copyright: Copyright (c) 2007-2012, Ext.NET, Inc. (http://www.ext.net/). All rights reserved.
 * @license: See license.txt and http://www.ext.net/licensetransformer/. 
 * @website: http://www.ext.net/
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Transformer.NET
{
    public static class Utils
    {
        private static System.Collections.Generic.Dictionary<string, string> replacements;
        private static Regex decodeRegex;
        
        public static string HtmlDecode(string text)
        {
            if (decodeRegex == null)
            {                
                replacements = new System.Collections.Generic.Dictionary<string, string>(4);
                replacements.Add("&quot;", @"""");
                replacements.Add("&lt;", "<");
                replacements.Add("&gt;", ">");
                replacements.Add("&amp;", "&");

                decodeRegex = new Regex("(" + String.Join("|", replacements.Keys.ToArray()) + ")", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);
            }

            return decodeRegex.Replace
                (
                    text,                                    
                    delegate(Match m) { return replacements[m.Value]; }
                );            
        }
    }
}
