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

namespace Transformer.NET.Html
{
    public class HtmlTransformer : TextTransformer
    {
        private static Regex headStartRegex = new Regex(@"<head[.\w\W]*?>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static Regex headEndRegex = new Regex(@"</head>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static Regex bodyStartRegex = new Regex(@"<body[.\w\W]*?>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static Regex bodyEndRegex = new Regex(@"</body>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public HtmlTransformer(string text) : base(text) { }

        public HtmlTransformer(string text, TextTransformerConfig config) : base(text, config) { }

        public override List<Type> StandardTokens
        {
            get
            {
                List<Type> tokens = base.StandardTokens;

                tokens.AddRange(new List<Type>
                {
                    typeof(JsTag),
                    typeof(CssTag),
                    typeof(ScriptTag),
                    typeof(StyleTag)
                });

                return tokens;
            }
        }

        Dictionary<string, TokenSelector> selectors;

        public override Dictionary<string, TokenSelector> Selectors
        {
            get
            {
                if (this.selectors == null)
                {
                    this.selectors = new Dictionary<string, TokenSelector>();

                    this.selectors.Add("headstart", new TokenSelector { Regex = headStartRegex });
                    this.selectors.Add("headend", new TokenSelector { Regex = headEndRegex, Position = SelectorPosition.Before });
                    this.selectors.Add("bodystart", new TokenSelector { Regex = bodyStartRegex });
                    this.selectors.Add("bodyend", new TokenSelector { Regex = bodyEndRegex, Position = SelectorPosition.Before });
                }

                return this.selectors;
            }
        }

        private Dictionary<string, DefaultTemplateTag> defaultTemplates;

        public override Dictionary<string, DefaultTemplateTag> DefaultTemplates
        {
            get
            {
                if (this.defaultTemplates == null)
                {
                    this.defaultTemplates = new Dictionary<string, DefaultTemplateTag>();
                    this.defaultTemplates.Add("js", new DefaultTemplateTag("js", "<script type=\"text/javascript\" src=\"{0}\"></script>"));
                    this.defaultTemplates.Add("script", new DefaultTemplateTag("script", "<script type=\"text/javascript\">{0}</script>"));
                    this.defaultTemplates.Add("cdata", new DefaultTemplateTag("cdata", "<script type=\"text/javascript\">\n\t//<![CDATA[\n\t\t{0}\n\t//]]>\n\t</script>"));
                    this.defaultTemplates.Add("css", new DefaultTemplateTag("css", "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />"));
                    this.defaultTemplates.Add("style", new DefaultTemplateTag("style", "<style type=\"text/css\">\n{0}\t</style>"));
                }

                return this.defaultTemplates;
            }
        }

        new public static string Transform(string text)
        {
            return new Transformer.NET.Html.HtmlTransformer(text).Transform();
        }

        new public static string Transform(string text, TextTransformerConfig config)
        {
            return new Transformer.NET.Html.HtmlTransformer(text, config).Transform();
        }

        new public static string Transform(string text, List<Type> tokensType)
        {
            return new Transformer.NET.Html.HtmlTransformer(text).Transform(tokensType);
        }

        new public static string Transform(string text, Dictionary<string, string> variables)
        {
            return new Transformer.NET.Html.HtmlTransformer(text).Transform(variables);
        }

        new public static string Transform(string text, List<Type> tokensType, Dictionary<string, string> variables)
        {
            return new Transformer.NET.Html.HtmlTransformer(text).Transform(tokensType, variables);
        }
    }

    [TagName("js")]
    public class JsTag : ItemTag
    {
        public JsTag(Match match) : base(match) { }

        public override string Template
        {
            get
            {
                return "js";
            }
        }

        public virtual string Path
        {
            get
            {
                return this.GetAttribute("path");
            }
        }

        public override string ApplyTemplate(TemplateTag tpl)
        {
            return tpl.Apply(this.Path);
        }
    }

    [TagName("script")]
    public class ScriptTag : ItemTag
    {
        public ScriptTag(Match match) : base(match) { }

        public override string Template
        {
            get
            {
                return "script";
            }
        }
    }

    [TagName("css")]
    public class CssTag : JsTag
    {
        public CssTag(Match match) : base(match) { }

        public override string Template
        {
            get
            {
                return "css";
            }
        }
    }

    [TagName("style")]
    public class StyleTag : ItemTag
    {
        public StyleTag(Match match) : base(match) { }

        public override string Template
        {
            get
            {
                return "style";
            }
        }
    }
}
