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
using System.Text.RegularExpressions;

namespace Transformer.NET
{
    [CLSCompliant(true)]
    public class TextTransformerConfig
    {
        public virtual Regex TokensRegex
        {
            get;
            set;
        }

        public virtual Regex VariablesRegex
        {
            get;
            set;
        }

        public virtual Regex AnchorsRegex
        {
            get;
            set;
        }

        public virtual Dictionary<string, TokenSelector> Selectors
        {
            get;
            set;
        }

        public virtual List<Type> TokensType
        {
            get;
            set;
        }

        public virtual bool? SkipValidation
        {
            get;
            set;
        }

        public virtual Dictionary<string, string> Variables
        {
            get;
            set;
        }

        public virtual Dictionary<string, DefaultTemplateTag> DefaultTemplates
        {
            get;
            set;
        }
    }
}
