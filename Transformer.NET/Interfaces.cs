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
    /* INTERFACE */

    public interface IToken
    {
        string Value
        {
            get;
        }

        Match Match
        {
            get;
        }

        Dictionary<string, string> Attributes
        {
            get;
        }

        string Template
        {
            get;
        }

        /// <summary>
        /// This method is called by parser for first token in own group (Groups collection of Parser) after all tokens are found
        /// </summary>
        void BeginHandle();
        
        /// <summary>
        /// This method is called by parser for first token in own group after calling BegindHandle for tokens in all groups
        /// </summary>
        void EndHandle();

        void Validate();
    }

    public interface IID
    {
        string ID { get; }
    }

    public interface IIndexableToken
    {
        int Index { get; }
    }

    public interface INoneValuableToken { }

    public interface ITextTransformer
    {
        List<Type> TokensType { get; }
        List<Token> Tokens { get; }
        Dictionary<string, Token> TokensById { get; }
        Dictionary<string, List<Token>> Groups { get; }
        Dictionary<string, List<ItemTag>> References { get; }
        Dictionary<string, string> Variables { get; }
        Dictionary<string, TokenSelector> Selectors { get; }
        Dictionary<string, DefaultTemplateTag> DefaultTemplates { get; }

        string Transform();
        string Transform(List<Type> tokensType);
        string Transform(Dictionary<string, string> variables);
        string Transform(List<Type> tokensType, Dictionary<string, string> variables);
        string ID(string prefix);

        Regex TokensRegex { get; }
        Regex VarsRegex { get; }
        Regex AnchorsRegex { get; }
        string Text { get; }
    }
}
