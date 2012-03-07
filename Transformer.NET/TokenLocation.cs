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
    public enum SelectorPosition
    {
        Before,
        After,
        Replace
    }

    public class PseudoMatch
    {
        public PseudoMatch(bool match, bool cancelSearch)
        {
            this.match = match;
            this.cancelSearch = cancelSearch;
        }
        
        private bool match;
        public bool Match
        {
            get
            {
                return this.match;
            }
        }

        private bool cancelSearch;
        public bool CancelSearch
        {
            get
            {
                return this.cancelSearch;
            }
        }
    }
    
    public class TokenSelector
    {
        /// <summary>
        /// Selector regex
        /// </summary>
        public virtual Regex Regex
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the group in the regex to determine token position
        /// </summary>
        public virtual string GroupName
        {
            get;
            set;
        }

        /// <summary>
        /// Index of the group in the regex to determine token position (GroupName has priority)
        /// </summary>
        public virtual int GroupIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Pseudo
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string PseudoArg
        {
            get;
            set;
        }

        private SelectorPosition position = SelectorPosition.After;
        /// <summary>
        /// Indicates the position to place the token
        /// </summary>
        public virtual SelectorPosition Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }

        /// <summary>
        /// Indicates the position to place the token
        /// </summary>
        public virtual RegexOptions Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Flags explanation:
        /// 
        /// i IgnoreCase Specifies case-insensitive matching. 
        /// m Multiline Multiline mode. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string. 
        /// x ExplicitCapture Specifies that the only valid captures are explicitly named or numbered groups of the form (?<name>…). This allows unnamed parentheses to act as noncapturing groups without the syntactic clumsiness of the expression (?:…). 
        /// s Singleline Specifies single-line mode. Changes the meaning of the dot (.) so it matches every character (instead of every character except \n). 
        /// w IgnorePatternWhitespace Eliminates unescaped white space from the pattern and enables comments marked with #. However, the RegexOptions..::.IgnorePatternWhitespace value does not affect or eliminate white space in character classes  
        /// r RightToLeft Specifies that the search will be from right to left instead of from left to right. 
        /// e ECMAScript Enables ECMAScript-compliant behavior for the expression. This value can be used only in conjunction with the IgnoreCase and Multiline values. The use of this value with any other values results in an exception. 
        /// c CultureInvariant Specifies that cultural differences in language are ignored. Ordinarily, the regular expression engine performs string comparisons based on the conventions of the current culture. If the CultureInvariant option is specified, it uses the conventions of the invariant culture.         
        /// </summary>

        static string selectorPattern = @"^(?:(?<position>before|after|replace)?(?:\[(?<selector>\w+)\])?(?:(?:\.)?(?:(?<pseudo>all|first|last|odd|even|nth|eq|gt|lt)(?:\((?<pseudoArg>[\w\W]+)\))?)?)?:)?(?:(?:/)?(?:(?:(?:(?<regex1>.+)/)(?<flags>[imxswrec]+))|(?<regex2>.+)))$";
        private static Regex selectorRegex = new Regex(selectorPattern, RegexOptions.Singleline);

        static string idSelectorPattern = @"^[\w]*$";
        private static Regex idSelectorRegex = new Regex(idSelectorPattern, RegexOptions.Singleline | RegexOptions.Compiled);

        private static bool IsIdSelector(string selector)
        {
            return idSelectorRegex.IsMatch(selector);
        }

        static string intPattern = @"^-?[\d]+$";
        private static Regex intRegex = new Regex(intPattern, RegexOptions.Singleline | RegexOptions.Compiled);

        private static bool IsInteger(string value)
        {
            return intRegex.IsMatch(value);
        }

        private static RegexOptions BuildFlags(string str)
        {
            RegexOptions ro = RegexOptions.None;

            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case 'i':
                        ro = ro | RegexOptions.IgnoreCase;
                        break;
                    case 'm':
                        ro = ro | RegexOptions.Multiline;
                        break;
                    case 'x':
                        ro = ro | RegexOptions.ExplicitCapture;
                        break;
                    case 's':
                        ro = ro | RegexOptions.Singleline;
                        break;
                    case 'w':
                        ro = ro | RegexOptions.IgnorePatternWhitespace;
                        break;
                    case 'r':
                        ro = ro | RegexOptions.RightToLeft;
                        break;
                    case 'e':
                        ro = ro | RegexOptions.ECMAScript;
                        break;
                    case 'c':
                        ro = ro | RegexOptions.CultureInvariant;
                        break;
                }
            }

            return ro;
        }

        public static TokenSelector ToTokenSelector(ITextTransformer parser, string selector)
        {
            if (IsIdSelector(selector))
            {
                if (!parser.Selectors.ContainsKey(selector))
                {
                    throw new Exception(string.Format("Selector '{0}' is not found in the predefined selectors", selector));
                }

                return parser.Selectors[selector];
            }

            selector = TrimTailingSlash(selector);

            TokenSelector tSelector = new TokenSelector();
            Match match = selectorRegex.Match(selector);

            if (!match.Success)
            {
                throw new Exception(string.Format("Selector regex ({0}) is invalid", selector));
            }

            if (match.Groups["position"].Success)
            {
                tSelector.Position = (SelectorPosition)Enum.Parse(typeof(SelectorPosition), match.Groups["position"].Value, true);
            }

            if (match.Groups["selector"].Success)
            {
                string group = match.Groups["selector"].Value;

                if (IsInteger(group))
                {
                    tSelector.GroupIndex = int.Parse(group);
                }
                else
                {
                    tSelector.GroupName = group;
                }
            }

            if (match.Groups["pseudo"].Success)
            {
                tSelector.Pseudo = match.Groups["pseudo"].Value;

                if (match.Groups["pseudoArg"].Success)
                {
                    tSelector.PseudoArg = match.Groups["pseudoArg"].Value;
                }
            }

            if (match.Groups["flags"].Success)
            {
                tSelector.Flags = BuildFlags(match.Groups["flags"].Value);
            }

            string regexStr;

            if (match.Groups["regex1"].Success)
            {
                regexStr = match.Groups["regex1"].Value;
            }
            else if (match.Groups["regex2"].Success)
            {
                regexStr = match.Groups["regex2"].Value;
            }
            else
            {
                throw new Exception("Regex is not found");
            }

            tSelector.Regex = new Regex(regexStr, tSelector.Flags);

            return tSelector;
        }

        public static string TrimTailingSlash(string pattern)
        {            
            return pattern.EndsWith(@"\/") ? pattern : pattern.EndsWith("/") ? pattern.Substring(0, pattern.Length-1) : pattern;
        }

        protected virtual PseudoMatch IsPseudoMatch(Group group, Token token, int index, int count)
        {
            if (this.Pseudo == null || this.Pseudo.Length == 0)
            {
                return new PseudoMatch(true, true);
            }

            switch (this.Pseudo)
            {
                case "all":
                    return new PseudoMatch(true, false);
                case "first":
                    return new PseudoMatch(index == 0, index == 0);
                case "last":
                    return new PseudoMatch(index == (count - 1), index == (count - 1));
                case "odd":
                    return new PseudoMatch((index & 1) != 0, false);
                case "even":
                    return new PseudoMatch((index & 1) == 0, false);
                case "nth":
                    int nth = int.Parse(this.PseudoArg);
                    return new PseudoMatch(((index + 1) % nth == 0), false);
                case "eq":
                    int eq = int.Parse(this.PseudoArg);
                    return new PseudoMatch(index == eq, index == eq);
                case "gt":
                    int gt = int.Parse(this.PseudoArg);
                    return new PseudoMatch(index > gt, false);
                case "lt":
                    int lt = int.Parse(this.PseudoArg);
                    return new PseudoMatch(index < lt, index >= lt);                
            }

            return new PseudoMatch(false, true);
        }

        public virtual List<string> Matches(string text, Token token)
        {
            List<string> list = new List<string>();
            MatchCollection matches = this.Regex.Matches(text);
            int count = matches.Count;
            for (int i = 0; i < count; i++)
            {
                Group group;
                if (this.GroupName != null && this.GroupName.Length > 0)
                {
                    group = matches[i].Groups[this.GroupName];
                }
                else
                {
                    group = matches[i].Groups[this.GroupIndex];
                }

                if (this.Pseudo == null || this.Pseudo.Length == 0)
                {
                    list.Add(group.Value);
                    break;
                }

                PseudoMatch pseudoMatch = this.IsPseudoMatch(group, token, i, count);
                if (pseudoMatch.Match)
                {
                    list.Add(group.Value);
                }

                if (pseudoMatch.CancelSearch)
                {
                    break;
                }
            }

            return list;
        }

        protected virtual bool HandleMatch(Match match, Token token, StringBuilder sb, int index, int count, ref int offset)
        {
            Group group;

            if (this.GroupName != null && this.GroupName.Length > 0)
            {
                group = match.Groups[this.GroupName];
            }
            else
            {
                group = match.Groups[this.GroupIndex];
            }

            PseudoMatch pseudoMatch = this.IsPseudoMatch(group, token, index, count);

            if (!pseudoMatch.Match)
            {
                return pseudoMatch.CancelSearch;
            }

            int pos;

            switch (this.Position)
            {
                case SelectorPosition.After:
                    pos = group.Index + group.Length;                    
                    break;
                case SelectorPosition.Before:
                    pos = group.Index;
                    break;
                case SelectorPosition.Replace:
                    pos = group.Index;
                    sb.Remove(pos+offset, group.Length);                    
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            sb.Insert(pos+offset, token.Output);
            if (this.Position == SelectorPosition.Replace)
            {
                offset -= group.Length;
            }
            offset += token.Output != null ? token.Output.Length : 0;

            return pseudoMatch.CancelSearch;
        }

        public virtual void Place(StringBuilder sb, Token token)
        {
            int offset = 0;
            if (this.Pseudo == null || this.Pseudo.Length == 0)
            {
                this.HandleMatch(this.Regex.Match(sb.ToString()), token, sb, 0, 0, ref offset);
            }
            else
            {
                MatchCollection matches = this.Regex.Matches(sb.ToString());
                int count = matches.Count;

                for (int i = 0; i < count; i++)
                {
                    if(this.HandleMatch(matches[i], token, sb, i, count, ref offset))
                    {
                        break;
                    }
                }
            }
        }
    }
}
