/*
 * @version: 2.0.0
 * @author: Ext.NET, Inc. http://www.ext.net/
 * @date: 2012-03-05
 * @copyright: Copyright (c) 2007-2012, Ext.NET, Inc. (http://www.ext.net/). All rights reserved.
 * @license: See license.txt and http://www.ext.net/licensetransformer/. 
 * @website: http://www.ext.net/
 */

using System.Collections.Generic;

namespace Transformer.NET
{
    public partial class TextTransformer : ITextTransformer
    {
        public delegate string BeforeParseHandler(ITextTransformer transformer, TransformerEventArgs e);
        public event BeforeParseHandler BeforeParse;

        protected virtual string OnBeforeParse(string text)
        {
            if (this.BeforeParse != null)
            {
                TransformerEventArgs e = new TransformerEventArgs(text);
                this.BeforeParse(this, e);

                return e.Text;
            }

            return text;
        }

        public delegate string AfterParseHandler(ITextTransformer transformer, TransformerEventArgs e);
        public event AfterParseHandler AfterParse;

        protected virtual string OnAfterParse(string text)
        {
            if (this.AfterParse != null)
            {
                TransformerEventArgs e = new TransformerEventArgs(text);
                this.AfterParse(this, e);
                return e.Text;
            }
            return text;
        }

        public delegate string TokenMatchHandler(ITextTransformer transformer, Token token);
        public event TokenMatchHandler TokenMatch;

        protected virtual void OnTokenMatch(Token token)
        {
            if (this.TokenMatch != null)
            {
                this.TokenMatch(this, token);
            }
        }

        public delegate string BeforeVariablesReplacingHandler(ITextTransformer transformer, TransformerEventArgs e);
        public event BeforeVariablesReplacingHandler BeforeVariablesReplacing;

        protected virtual string OnBeforeVariablesReplacing(string text)
        {
            if (this.BeforeVariablesReplacing != null)
            {
                TransformerEventArgs e = new TransformerEventArgs(text);
                this.BeforeVariablesReplacing(this, e);

                return e.Text;
            }

            return text;
        }

        public delegate string AfterVariablesReplacingHandler(ITextTransformer transformer, TransformerEventArgs e);
        public event AfterVariablesReplacingHandler AfterVariablesReplacing;

        protected virtual string OnAfterVariablesReplacing(string text)
        {
            if (this.AfterVariablesReplacing != null)
            {
                TransformerEventArgs e = new TransformerEventArgs(text);
                this.AfterVariablesReplacing(this, e);

                return e.Text;
            }

            return text;
        }

        public delegate string VariableMatchHandler(ITextTransformer transformer, VariableMatchEventArgs e);
        public event VariableMatchHandler VariableMatch;

        protected virtual string OnVariableMatch(string text, string variable, Dictionary<string, string> variables)
        {
            if (this.VariableMatch != null)
            {
                VariableMatchEventArgs e = new VariableMatchEventArgs(text, variable, variables);
                this.VariableMatch(this, e);

                return e.VariableValue;
            }

            return variables[variable];
        }
    }

    public class TransformerEventArgs
    {
        public TransformerEventArgs(string text)
        {
            this.text = text;
        }

        private string text;

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }
    }

    public class VariableMatchEventArgs : TransformerEventArgs
    {
        public VariableMatchEventArgs(string text, string variable, Dictionary<string, string> variables) : base(text)
        {
            this.variable = variable;
            this.variables = variables;
        }

        private string variable;

        public string Variable
        {
            get
            {
                return this.variable;
            }
        }

        private Dictionary<string, string> variables;

        public Dictionary<string, string> Variables
        {
            get
            {
                return variables;
            }
        }

        private string variableValue;

        public string VariableValue
        {
            get
            {
                if (this.variableValue == null)
                {
                    return this.Variables[this.variable];
                }

                return this.variableValue;
            }
            set
            {
                this.variableValue = value;
            }
        }
    }
}
