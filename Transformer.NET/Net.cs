/*
 * @version: 2.0.0
 * @author: Ext.NET, Inc. http://www.ext.net/
 * @date: 2012-03-05
 * @copyright: Copyright (c) 2007-2012, Ext.NET, Inc. (http://www.ext.net/). All rights reserved.
 * @license: See license.txt and http://www.ext.net/licensetransformer/. 
 * @website: http://www.ext.net/
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Transformer.NET
{
    public static class Net
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Transform(string text)
        {
            return Transformer.NET.TextTransformer.Transform(text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string Transform(string text, TextTransformerConfig config)
        {
            return Transformer.NET.TextTransformer.Transform(text, config);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Transform(string text, Dictionary<string, string> variables)
        {
            return Transformer.NET.TextTransformer.Transform(text, variables);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Transform(string text, List<Type> tokensType)
        {
            return Transformer.NET.TextTransformer.Transform(text, tokensType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Transform(string text, List<Type> tokensType, Dictionary<string, string> variables)
        {
            return Transformer.NET.TextTransformer.Transform(text, tokensType, variables);
        }

        /* Helpers */

        private static string GetTokenName(Type tokenType)
        {
            object[] attrs = tokenType.GetCustomAttributes(typeof(TagNameAttribute), true);
            return ((TagNameAttribute)attrs[0]).Name;
        }

        public static string CreateToken(Type tokenType, Dictionary<string, string> attributes)
        {
            return Transformer.NET.Net.CreateToken(Transformer.NET.Net.GetTokenName(tokenType), attributes);
        }

        public static string CreateToken(Type tokenType, Dictionary<string, string> attributes, string value)
        {
            return Transformer.NET.Net.CreateToken(Transformer.NET.Net.GetTokenName(tokenType), attributes, value);
        }

        public static string CreateToken(string token, Dictionary<string, string> attributes)
        {
            return Transformer.NET.Net.CreateToken(token, attributes, null);
        }

        public static string CreateToken(string token, Dictionary<string,string> attributes, string value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<#:");
            sb.Append(token);

            if (attributes != null && attributes.Count > 0)
            {
                foreach (string key in attributes.Keys)
                {
                    sb.AppendFormat(" {0}=\"{1}\"", key, attributes[key]);
                }
            }

            if (value != null && value.Length > 0)
            {
                sb.AppendFormat(">{0}</#:{1}>", value, token);
            }
            else
            {
                sb.Append(" />");
            }

            return sb.ToString();
        }
    }
}
