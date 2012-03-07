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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TagNameAttribute : Attribute
    {
        public TagNameAttribute() { }

        public TagNameAttribute(string name)
        {
            this.name = name;
        }

        private string name = "";

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }

    public abstract class Token : IToken, IID
    {        
        public Token(Match match)
        {
            this.match = match;
        }

        public virtual ITextTransformer Transformer
        {
            get;
            set;
        }

        private string id;
        public virtual string ID
        {
            get
            {                
                if (this.id == null)
                {
                    this.id = this.GetAttribute("id");
                    if (this.id == null || this.id.Length == 0)
                    {
                        this.id = this.Transformer.ID(null);
                        this.Attributes["id"] = this.id;
                    }
                }

                return this.id;
            }
        }

        private string tpl;
        private bool tplChecked;
        public virtual string Template
        {
            get
            {
                if (!this.tplChecked)
                {
                    this.tpl = this.GetAttribute("tpl");
                    this.tplChecked = true;
                }
                return this.tpl;
            }
        }

        private Match match;

        public virtual Match Match
        {
            get
            {
                return this.match;
            }
        }

        private string value;
        private bool valueChecked;

        public virtual string Value
        {
            get
            {
                if (this.valueChecked)
                {
                    return this.value;
                }

                if (this is INoneValuableToken)
                {
                    return null;
                }

                if (this.value != null)
                {
                    return this.value;
                }

                Group group = this.Match.Groups["value"];

                this.value = (group != null && group.Success) ? group.Value : null;

                if (this.value == null || this.value.Length == 0)
                {
                    this.value = this.GetAttribute("value");
                }
                
                this.valueChecked = true;
                return this.value;
            }
            set
            {
                this.value = value;
                this.valueChecked = true;
            }
        }

        public virtual bool Handled
        {
            get;
            set;
        }

        private int index;
        private bool indexChecked;
        public virtual int Index
        {
            get
            {
                if (!this.indexChecked)
                {
                    string s = this.GetAttribute("index");
                    if (s == null || s.Length == 0)
                    {
                        this.index = int.MaxValue;
                    }
                    else
                    {
                        this.index = int.Parse(s);
                    }
                    
                    this.indexChecked = true;
                }

                return this.index;
            }
        }

        public virtual bool AnchorsHandled
        {
            get;
            set;
        }

        private string selector;
        private bool selectorChecked;
        public virtual string Selector
        {
            get
            {
                if (!this.selectorChecked)
                {
                    this.selector = this.GetAttribute("selector");
                    this.selectorChecked = true;
                }

                return this.selector;
            }
        }
        
        private List<AnchorTag> anchors;
        
        public virtual List<AnchorTag> Anchors
        {
            get
            {
                if (this is INoneValuableToken)
                {
                    return null;
                }

                this.ParseAnchors();

                return this.anchors;
            }
        }

        private List<Token> managedTokens;
        
        public virtual List<Token> ManagedTokens
        {
            get
            {
                if (this.managedTokens == null)
                {
                    this.managedTokens = new List<Token>();
                }

                return this.managedTokens;
            }
        }

        public virtual void ParseAnchors()
        {
            if (this is INoneValuableToken)
            {
                return;
            }

            if (this.anchors == null)
            {
                this.anchors = new List<AnchorTag>();
                string value = this.Value;

                if (!string.IsNullOrEmpty(value))
                {
                    MatchCollection matches = this.Transformer.AnchorsRegex.Matches(value);

                    foreach (Match match in matches)
                    {
                        AnchorTag token = new AnchorTag(match, this);
                        this.anchors.Add(token);
                        token.Transformer = this.Transformer;

                        if (this.Transformer != null)
                        {
                            //this.Parser.Tokens.Add(token);
                            this.Transformer.TokensById.Add(token.ID, token);
                            this.Transformer.Groups["anchor"].Add(token);
                        }
                    }
                }
            }
        }

        public virtual void HandleAnchors()
        {
            if (!this.AnchorsHandled)
            {
                if (this.Anchors.Count > 0)
                {
                    int counter = 0;

                    this.Value = this.Transformer.AnchorsRegex.Replace(this.Value, delegate(Match m)
                    {
                        AnchorTag anchor = this.Anchors[counter++];
            
                        List<ItemTag> items = anchor.ReferenceItems;

                        if (items.Count == 0)
                        {
                            return "";
                        }

                        StringBuilder sb = new StringBuilder();

                        foreach (ItemTag item in items)
                        {
                            if (item.Anchors.Count > 0 && !item.AnchorsHandled)
                            {
                                item.HandleAnchors();
                            }

                            sb.Append(item.Output);

                            item.Handled = true;                                
                        }

                        string tpl = anchor.Template;
                        return (tpl == null || tpl.Length == 0) ? sb.ToString() : anchor.GetTemplate(tpl).Apply(sb.ToString()); 
                        
                    });
                }
                
                this.AnchorsHandled = true;
            }
        }

        private Dictionary<string, string> attributes;

        public virtual Dictionary<string, string> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = this.BuildAttributes();
                }

                return this.attributes;    
            }
        }

        protected virtual string GetAttribute(string name)
        {
            //return this.Attributes.ContainsKey(name) ? this.Attributes[name] : null;
            return this.Attributes.ContainsKey(name) ? Utils.HtmlDecode(this.Attributes[name]) : null;
        }

        private static Regex attrsRegex = new Regex("(?<name>\\b\\w+\\b)\\s*=\\s*(?<value>\"(?<value1>[^\"]*)\"|'(?<value2>[^']*)'|(?<value3>[^\"'<>\\s]+))", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);

        protected virtual Dictionary<string, string> BuildAttributes()
        {
            return this.BuildAttributes(this.Match.Groups["attrs"].Value);
        }

        protected virtual Dictionary<string, string> BuildAttributes(string attrsStr)
        {            
            Dictionary<string, string> attrs = new Dictionary<string, string>();

            if(!string.IsNullOrEmpty(attrsStr))
            {
                MatchCollection matches = attrsRegex.Matches(attrsStr);

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        string key = match.Groups["name"].Value;
                        string groupName = "";

                        if (match.Groups["value1"].Success)
                        {
                            groupName = "value1";
                        }
                        else if (match.Groups["value2"].Success)
                        {
                            groupName = "value2";
                        }
                        else if (match.Groups["value3"].Success)
                        {
                            groupName = "value3";
                        }

                        string value = match.Groups[groupName].Value; ;

                        attrs.Add(key, value);
                    }
                }
            }

            return attrs;
        }

        #region IToken Members

        public virtual void BeginHandle()
        {
            // can be overriden in successors to apply own logic
        }

        public virtual void EndHandle()
        {
            // can be overriden in successors to apply own logic
            foreach (Token token in this.Transformer.Groups[this.TagName])
            {                
                token.Handled = true;
            }
        }

        private string tagName;

        public virtual string TagName
        {
            get
            {
                if (this.tagName == null)
                {
                    object[] attrs = this.GetType().GetCustomAttributes(typeof(TagNameAttribute), true);
                    this.tagName = ((TagNameAttribute)attrs[0]).Name;
                }

                return this.tagName;
            }
        }

        protected virtual string ManagedTokensToString()
        {
            if (this.managedTokens == null)
            {
                return "";
            }

            this.ManagedTokens.Sort(delegate(Token t1, Token t2)
            {
                return t1.Index.CompareTo(t2.Index);
            });

            StringBuilder sb = new StringBuilder();

            foreach (Token token in this.ManagedTokens)
            {
                sb.Append(token.Output);
            }

            return sb.ToString();
        }

        public virtual string ApplyTemplate(TemplateTag tpl)
        {
            string output;
            if (this.Selector != null && this.Selector.Length > 0)
            {
                TokenSelector selector = TokenSelector.ToTokenSelector(this.Transformer, this.Selector);
                List<string> list = selector.Matches(this.Transformer.Text, this);

                StringBuilder sb = new StringBuilder();
                foreach (string match in list)
                {
                    if (match != null && match.Length > 0)
                    {
                        sb.Append(tpl.Apply(match));
                    }
                }
                return sb.ToString();
            }
            
            output = this.ManagedTokensToString();
            return tpl.Apply(output != null && output.Length > 0 ? output : this.Value);
        }

        static string idTplPattern = @"^[\w]*$";
        private static Regex idTplRegex = new Regex(idTplPattern, RegexOptions.Singleline);

        public virtual bool IsTemplateInline(string tpl)
        {
            return !idTplRegex.IsMatch(tpl);
        }

        public virtual TemplateTag GetTemplate(string tplId)
        {
            if (this.IsTemplateInline(tplId))
            {
                return new DefaultTemplateTag("", tplId);
            }
            
            List<Token> tpls = this.Transformer.Groups["tpl"];
            TemplateTag tpl;

            for (int i = tpls.Count - 1; i >= 0; i--)
            {
                tpl = (TemplateTag)tpls[i];

                if (tpl.Override == tplId || tpl.ID == tplId)
                {
                    return tpl;
                }
            }

            if (this.Transformer.DefaultTemplates.ContainsKey(tplId))
            {
                return this.Transformer.DefaultTemplates[tplId];
            }

            return null;
        }

        public virtual string Output
        {
            get
            {                
                if(this.Template == null || this.Template.Length == 0)
                {
                    string output = this.ManagedTokensToString();
                    return output != null && output.Length > 0 ? output : this.Value;
                }
                
                TemplateTag tpl = this.GetTemplate(this.Template);

                if (tpl == null)
                {
                    throw new Exception("Template ("+this.Template+") is not found");
                }

                return this.ApplyTemplate(tpl);                
            }
        }

        public virtual Token GetTokenByID(string id)
        {
            return this.Transformer.TokensById[id];
        }

        public virtual void Validate() { }

        #endregion
    }
    
    [TagName("tpl")]
    public class TemplateTag :Token
    {
        public TemplateTag(Match match) : base(match) { }

        private string _override;
        private bool _overrideChecked;
        public virtual string Override
        {
            get
            {
                if (!this._overrideChecked)
                {
                    this._override = this.GetAttribute("override");
                    this._overrideChecked = true;
                }

                return this._override;
            }
        }

        public virtual string Apply(params string[] args)
        {
            return string.Format(this.Value, args);
        }

        public override void Validate()
        {
            string id = this.Override;

            if (!string.IsNullOrEmpty(id))
            {
                if (!this.Transformer.TokensById.ContainsKey(id) && !this.Transformer.DefaultTemplates.ContainsKey(id))
                {
                    throw new Exception(string.Format("TemplateTag validation ({0}): Override attribute contains unexisting template id ({1})", this.ID, id));
                }
            }
        }
    }
    
    public class DefaultTemplateTag : TemplateTag
    {
        public DefaultTemplateTag(string id, string value)
            : base(null)
        {
            this.attributes = new Dictionary<string, string>(1);
            this.attributes.Add("id", id);
            this.value = value;
        }
        
        private Dictionary<string, string> attributes;

        public override Dictionary<string, string> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        public override int Index
        {
            get
            {
                return int.MinValue;
            }
        }

        private string value;
        
        public override string Value
        {
            get
            {
                return this.value;
            }
        }
    }

    [TagName("anchor")]
    public class AnchorTag : Token, INoneValuableToken
    {
        public AnchorTag(Match match)
            : base(match)
        {
        }        

        private Token parentToken;

        public AnchorTag(Match match, Token parentToken)
            : base(match)
        {
            this.parentToken = parentToken;
        }        

        public virtual Token ParentToken
        {
            get
            {
                return this.parentToken;
            }
        }

        private List<ItemTag> referenceItems;

        public virtual List<ItemTag> ReferenceItems
        {
            get
            {
                if (this.referenceItems == null)
                {
                    this.referenceItems = this.Transformer.References.ContainsKey(this.ID) ? this.Transformer.References[this.ID] : new List<ItemTag>(0);
                }

                return this.referenceItems;
            }
        }

        public override void BeginHandle()
        {
            base.BeginHandle();

            foreach (AnchorTag anchor in this.Transformer.Groups[this.TagName])
            {
                if (anchor.ParentToken == null)
                {
                    foreach (ItemTag item in anchor.ReferenceItems)
                    {
                        anchor.ManagedTokens.Add(item);
                        item.Handled = true;
                    }  
                }
                else if (!anchor.ParentToken.AnchorsHandled)
                {
                    anchor.ParentToken.HandleAnchors();
                }                
            }
        }

        public override void EndHandle() { }
    }

    [TagName("group")]
    public class GroupTag : Token, INoneValuableToken
    {
        public GroupTag(Match match)
            : base(match)
        {
        }

        private string reference;
        
        private bool referenceChecked;

        public virtual string Reference
        {
            get
            {
                if (!this.referenceChecked)
                {
                    this.reference = this.GetAttribute("ref");
                    this.referenceChecked = true;
                }

                return this.reference;
            }
        }

        public override void EndHandle()
        {            
            foreach (GroupTag group in this.Transformer.Groups[this.TagName])
            {
                this.GetTokenByID(group.Reference).ManagedTokens.Add(group);
                group.Handled = true;
            }
        }

        public override void Validate()
        {
            string id = this.Reference;

            if (!string.IsNullOrEmpty(id))
            {
                if (!this.Transformer.TokensById.ContainsKey(id))
                {
                    throw new Exception(string.Format("GroupTag validation ({0}): Reference attribute contains unexisting reference id ({1})", this.ID, id));
                }
            }
        }
    }

    [TagName("item")]
    [CLSCompliant(true)]
    public class ItemTag : Token
    {
        public ItemTag(Match match)
            : base(match)
        {
        }

        private string reference;
        private bool referenceChecked;
        public virtual string Reference
        {
            get
            {
                if (!this.referenceChecked)
                {
                    this.reference = this.GetAttribute("ref");
                    this.referenceChecked = true;
                }

                return this.reference;
            }
        }

        public override void Validate()
        {
            string refId = this.Reference;

            if (!string.IsNullOrEmpty(refId))
            {
                if (!this.Transformer.TokensById.ContainsKey(refId))
                {
                    throw new Exception(string.Format("ItemTag validation ({0}): Reference token ({1}) was not found.", this.ID, refId));
                }
            }
        }

        public override void EndHandle() { }
    }
}
