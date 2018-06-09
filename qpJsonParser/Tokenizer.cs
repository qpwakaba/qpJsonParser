using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace qpwakaba
{
    public class Tokenizer : IEnumerator<Token>, IEnumerable<Token>
    {
        private readonly TextReader reader;
        private readonly ITokenRule rule;
        public Tokenizer(TextReader reader, ITokenRule rule)
        {
            this.reader = reader;
            this.rule = rule;
        }

        public Token Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public enum TokenSplitType
        {
            Concat,
            Before,
            After,
            BeforeAndAfter,
            Skip
        }

        private readonly StringBuilder tokenBuilder = new StringBuilder();
        private char? previousCharacter = null;
        private bool IsPreviousSplitTypeBeforeAndAfter = false;
        public bool MoveNext()
        {
            if (this.IsPreviousSplitTypeBeforeAndAfter)
            {
                this.IsPreviousSplitTypeBeforeAndAfter = false;
                this.Current = new Token(this.tokenBuilder.ToString());
                this.tokenBuilder.Length = 0;
                return this.Current.ToString().Length > 0;
            }
            for (int read = this.reader.Read(); read >= 0; read = this.reader.Read())
            {
                char c = (char)read;
                switch (this.rule.ProcessCharacter(c, this.previousCharacter, this.tokenBuilder.ToString()))
                {
                    case TokenSplitType.Concat:
                    {
                        this.tokenBuilder.Append(c);
                        this.previousCharacter = c;
                        break;
                    }
                    case TokenSplitType.BeforeAndAfter:
                        if (this.tokenBuilder.Length == 0)
                        {
                            goto case TokenSplitType.After;
                        }
                        else
                        {
                            this.IsPreviousSplitTypeBeforeAndAfter = true;
                            goto case TokenSplitType.Before;
                        }
                    case TokenSplitType.Before:
                    {
                        if (this.tokenBuilder.Length > 0)
                        {
                            this.Current = new Token(this.tokenBuilder.ToString());
                            this.tokenBuilder.Length = 0;
                            this.tokenBuilder.Append(c);
                            this.previousCharacter = c;
                            return this.Current.ToString().Length > 0;
                        }
                        else
                        {
                            this.tokenBuilder.Append(c);
                            this.previousCharacter = c;
                            break;
                        }
                    }
                    case TokenSplitType.After:
                    {
                        this.tokenBuilder.Append(c);
                        this.Current = new Token(this.tokenBuilder.ToString());
                        this.tokenBuilder.Length = 0;
                        this.previousCharacter = null;
                        return this.Current.ToString().Length > 0;
                    }
                    case TokenSplitType.Skip:
                    {
                        if (this.tokenBuilder.Length == 0) continue;
                        this.Current = new Token(this.tokenBuilder.ToString());
                        this.tokenBuilder.Length = 0;
                        this.previousCharacter = null;
                        return this.Current.ToString().Length > 0;
                    }
                }
            }
            this.Current = new Token(this.tokenBuilder.ToString());
            this.tokenBuilder.Length = 0;
            return this.Current.ToString().Length > 0;
        }

        public IEnumerator<Token> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        public void Dispose() { }

        public void Reset() => throw new NotSupportedException();
    }

    public interface ITokenRule
    {
        Tokenizer.TokenSplitType ProcessCharacter(char c, char? previousOfToken, string token);
    }

    public class Token
    {
        private readonly string token;
        public Token(string token) => this.token = token;
        public override string ToString() => this.token;
    }
}
