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
using System.Text.RegularExpressions;

namespace Transformer.NET
{
    public partial class TextTransformer : ITextTransformer
    {
        private string originalText;
        private string newText;
        private List<Type> tokensType;
        private List<Token> tokens;
        private Dictionary<string, Token> tokensById;
        private Dictionary<string, List<Token>> groups;
        private Dictionary<string, string> variables;
        private Dictionary<string, TokenSelector> selectors;
        private Dictionary<string, List<ItemTag>> references;
        
        static string pattern = @"<#:(?<tag>[\w]+)\s*(?<attrs>.*?)(?:(?:\s+/>)|(?:\s*>(?:(?<value>[.\w\W]*?)</#:\k<tag>>)))";
        static string varsPattern = @"<#:(?<tag>\w+):?(?<func>\w+)?\s+#?/>";
        static string anchorPattern = @"<#:anchor\s*(?<attrs>.*?)(?:(?:\s+/>)|(?:\s*>(?:(?<value>[.\w\W]*?)</#:anchor>)))";        

        private static Regex varsRegex = new Regex(varsPattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);
        private static Regex tokensRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);
        private static Regex anchorsRegex = new Regex(anchorPattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);

        public TextTransformer(string text)
        {
            this.originalText = text;
        }

        public TextTransformer(string text, TextTransformerConfig config) : this(text)
        {
            this.ApplyConfig(config);
        }

        protected virtual void ApplyConfig(TextTransformerConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (config.TokensRegex != null)
            {
                this.TokensRegex = config.TokensRegex;
            }

            if (config.VariablesRegex != null)
            {
                this.VarsRegex = config.VariablesRegex;
            }

            if (config.AnchorsRegex != null)
            {
                this.AnchorsRegex = config.AnchorsRegex;
            }

            if (config.Selectors != null)
            {
                foreach (string key in config.Selectors.Keys)
                {
                    this.Selectors.Add(key, config.Selectors[key]);
                }
            }

            if (config.Variables != null)
            {
                foreach (string key in config.Variables.Keys)
                {
                    this.Variables.Add(key, config.Variables[key]);
                }
            }

            if (config.DefaultTemplates != null)
            {
                foreach (string key in config.DefaultTemplates.Keys)
                {
                    this.DefaultTemplates.Add(key, config.DefaultTemplates[key]);
                }
            }

            if (config.TokensType != null)
            {
                this.TokensType.AddRange(config.TokensType);
            }

            if (config.SkipValidation.HasValue)
            {
                this.SkipValidation = config.SkipValidation.Value;
            }
        }

        private Regex _tokensRegex = TextTransformer.tokensRegex;

        public virtual Regex TokensRegex
        {
            get
            {
                return this._tokensRegex;
            }
            set
            {
                this._tokensRegex = value;
            }
        }

        private Regex _varsRegex = TextTransformer.varsRegex;

        public virtual Regex VarsRegex
        {
            get
            {
                return this._varsRegex;
            }
            set
            {
                this._varsRegex = value;
            }
        }

        private Regex _anchorsRegex = TextTransformer.anchorsRegex;

        public virtual Regex AnchorsRegex
        {
            get
            {
                return this._anchorsRegex;
            }
            set
            {
                this._anchorsRegex = value;
            }
        }

        int counter = 0;

        public virtual string ID(string prefix)
        {
            return (string.IsNullOrEmpty(prefix) ? "_tkn_" : prefix) + (++this.counter);
        }

        private Dictionary<string, DefaultTemplateTag> defaultTemplates;

        public virtual Dictionary<string, DefaultTemplateTag> DefaultTemplates
        {
            get
            {
                if (this.defaultTemplates == null)
                {
                    this.defaultTemplates = new Dictionary<string, DefaultTemplateTag>();
                }

                return this.defaultTemplates;
            }
        }

        public virtual Dictionary<string, TokenSelector> Selectors
        {
            get
            {
                if (this.selectors == null)
                {
                    this.selectors = new Dictionary<string, TokenSelector>();
                }

                return this.selectors;
            }
        }

        public virtual List<Type> StandardTokens
        {
            get
            {
                return new List<Type>
                {
                    typeof(TemplateTag),
                    typeof(AnchorTag),
                    typeof(GroupTag),
                    typeof(ItemTag)
                };
            }
        }

        public virtual bool SkipValidation
        {
            get;
            set;
        }

        public virtual List<Type> TokensType
        {
            get
            {
                return this.tokensType;
            }
        }

        public virtual List<Token> Tokens
        {
            get
            {
                return this.tokens;
            }
        }

        public virtual Dictionary<string, Token> TokensById
        {
            get
            {
                return this.tokensById;
            }
        }

        public virtual Dictionary<string, string> Variables
        {
            get
            {
                return this.variables;
            }
        }

        public virtual Dictionary<string, List<Token>> Groups
        {
            get
            {
                return this.groups;
            }
        }

        public virtual Dictionary<string, List<ItemTag>> References
        {
            get
            {
                return this.references;
            }
        }

        public static string Transform(string text)
        {
            return new Transformer.NET.TextTransformer(text).Transform();
        }

        public static string Transform(string text, TextTransformerConfig config)
        {
            return new Transformer.NET.TextTransformer(text, config).Transform();
        }

        public static string Transform(string text, List<Type> tokensType)
        {
            return new Transformer.NET.TextTransformer(text).Transform(tokensType);
        }

        public static string Transform(string text, Dictionary<string, string> variables)
        {
            return new Transformer.NET.TextTransformer(text).Transform(variables);
        }

        public static string Transform(string text, List<Type> tokensType, Dictionary<string, string> variables)
        {
            return new Transformer.NET.TextTransformer(text).Transform(tokensType, variables);
        }

        public virtual List<Token> Parse() 
        {
            return this.Parse(this.StandardTokens, null);
        }

        public virtual List<Token> Parse(Dictionary<string, string> variables)
        {
            return this.Parse(this.StandardTokens, variables);
        }

        public virtual List<Token> Parse(List<Type> tokensType)
        {
            return this.Parse(tokensType, null);
        }

        public virtual string Text
        {
            get
            {
                return this.newText;
            }
        }

        public virtual List<Token> Parse(List<Type> tokensType, Dictionary<string, string> variables)
        {
            this.variables = variables;
            this.newText = this.originalText;

            if (this.Variables != null && this.Variables.Count > 0)
            {
                this.newText = this.OnBeforeVariablesReplacing(this.newText);
                this.newText = this.VarsRegex.Replace(this.originalText, delegate(Match m)
                {
                    string variable = m.Groups["tag"].Success ? m.Groups["tag"].Value : m.Groups["func"].Value;

                    if (!this.Variables.ContainsKey(variable))
                    {
                        throw new Exception(string.Format("Variable with name '{0}' is not found", variable));
                    }

                    return this.OnVariableMatch(this.newText, variable, this.Variables);
                });

                this.newText = this.OnAfterVariablesReplacing(this.newText);
            }
            
            this.tokensType = tokensType;
            this.tokens = new List<Token>();
            this.tokensById = new Dictionary<string, Token>();
            this.groups = new Dictionary<string, List<Token>>();

            this.newText = this.OnBeforeParse(this.newText);
            MatchCollection matches = this.TokensRegex.Matches(this.newText);

            foreach (Match match in matches)
            {
                string tagName = match.Groups["tag"].Value;
                Token token = this.CreateToken(tagName, match);

                if (token != null)
                {
                    token.Transformer = this;
                    this.Tokens.Add(token);
                    this.TokensById.Add(token.ID, token);
                    this.Groups[tagName].Add(token);
                    token.ParseAnchors();
                }

                this.OnTokenMatch(token);
            }

            this.OnAfterParse(this.newText);

            return this.tokens;
        }

        protected virtual void GroupReferences()
        {
            this.references = new Dictionary<string, List<ItemTag>>();

            foreach (string key in this.Groups.Keys)
            {
                var group = this.Groups[key];
                if (group.Count > 0 && group[0] is ItemTag)
                {
                    foreach (ItemTag item in group)
                    {
                        var reference = item.Reference;
                        if (reference != null && reference.Length != 0)
                        {
                            if(!this.references.ContainsKey(reference))
                            {
                                this.references.Add(reference, new List<ItemTag>());
                            }
                            this.references[reference].Add(item);
                        }
                    }
                }
            }

            foreach (var key in this.references.Keys)
            {
                this.references[key].Sort(delegate(ItemTag t1, ItemTag t2)
                {
                    return t1.Index.CompareTo(t2.Index);
                });
            }
        }

        public virtual void Handle()
        {
            if (!this.SkipValidation)
            {
                foreach (Token token in this.Tokens)
                {
                    token.Validate();
                }
            }

            this.GroupReferences();
            
            foreach (string tagName in this.Groups.Keys)
            {
                List<Token> tokens = this.Groups[tagName];

                if (tokens.Count > 0)
                {
                    tokens[0].BeginHandle();
                }
            }

            foreach (string tagName in this.Groups.Keys)
            {
                List<Token> tokens = this.Groups[tagName];

                if (tokens.Count > 0)
                {
                    tokens[0].EndHandle();
                }
            }

            int counter = 0;
            List<Token> selectorTokens = new List<Token>();

            this.newText = this.TokensRegex.Replace(this.newText, delegate(Match m) 
            {
                Token token = this.Tokens[counter++];

                if(!token.Handled && token.Selector != null && token.Selector.Length > 0)
                {
                    selectorTokens.Add(token);
                    return "";
                }

                return token.Handled ? "" : token.Output;
            });

            if (selectorTokens.Count > 0)
            {
                this.HandleSelectorTokens(selectorTokens);
            }
        }

        protected virtual void HandleSelectorTokens(List<Token> selectorTokens)
        {
 	        StringBuilder sb = new StringBuilder(this.newText);

            selectorTokens.Sort(delegate(Token t1, Token t2)
            {
                return t1.Selector == t2.Selector ? t2.Index.CompareTo(t1.Index) : string.Compare(t1.Selector,t2.Selector);
            });

            foreach (Token token in selectorTokens)
	        {
                string strSelector = token.Selector;
                TokenSelector selector = TokenSelector.ToTokenSelector(this, strSelector);
                selector.Place(sb, token);                
	        }

            this.newText = sb.ToString();
        }

        public virtual string Transform()
        {
            return this.Transform(this.StandardTokens);
        }

        public virtual string Transform(Dictionary<string, string> variables)
        {
            return this.Transform(this.StandardTokens, variables);
        }

        public virtual string Transform(List<Type> tokensType)
        {
            return this.Transform(tokensType, null);
        }

        public virtual string Transform(List<Type> tokensType, Dictionary<string, string> variables)
        {
            this.Parse(tokensType, variables);
            this.Handle();

            return this.ToString();
        }

        private Dictionary<string, Type> tagsMap;

        protected virtual Token CreateToken(string tagName, Match match)
        {
            if (!this.TagsMap.ContainsKey(tagName))
            {
                return null;
            }

            return (Token)Activator.CreateInstance(this.TagsMap[tagName], match);
        }

        protected virtual Dictionary<string, Type> TagsMap
        {
            get
            {
                this.InitTagsMap();
                return this.tagsMap;
            }
        }

        protected virtual void InitTagsMap()
        {
            if (this.tagsMap == null)
            {
                this.tagsMap = new Dictionary<string, Type>(this.tokensType.Count);

                foreach (Type tokenType in this.tokensType)
                {
                    object[] attrs = tokenType.GetCustomAttributes(typeof(TagNameAttribute), true);
                    string tn = ((TagNameAttribute)attrs[0]).Name;
                    this.tagsMap.Add(tn, tokenType);
                    this.Groups.Add(tn, new List<Token>());
                }
            }
        }

        public override string ToString()
        {
            return this.newText;
        }
    }
}
