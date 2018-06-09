using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using TODO = System.NotImplementedException;
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

        private StringBuilder tokenBuilder = new StringBuilder();
        private char? previousCharacter = null;
        private bool IsPreviousSplitTypeBeforeAndAfter = false;
        public bool MoveNext()
        {
            if (IsPreviousSplitTypeBeforeAndAfter)
            {
                IsPreviousSplitTypeBeforeAndAfter = false;
                this.Current = new Token(tokenBuilder.ToString());
                tokenBuilder.Length = 0;
                return this.Current.ToString().Length > 0;
            }
            for (int read = reader.Read(); read >= 0; read = reader.Read())
            {
                char c = (char)read;
                switch (rule.ProcessCharacter(c, previousCharacter, tokenBuilder.ToString()))
                {
                    case TokenSplitType.Concat:
                        {
                            tokenBuilder.Append(c);
                            previousCharacter = c;
                            break;
                        }
                    case TokenSplitType.BeforeAndAfter:
                        if (tokenBuilder.Length == 0)
                        {
                            goto case TokenSplitType.After;
                        }
                        else
                        {
                            IsPreviousSplitTypeBeforeAndAfter = true;
                            goto case TokenSplitType.Before;
                        }
                    case TokenSplitType.Before:
                        {
                            if (tokenBuilder.Length > 0)
                            {
                                this.Current = new Token(tokenBuilder.ToString());
                                tokenBuilder.Length = 0;
                                tokenBuilder.Append(c);
                                previousCharacter = c;
                                return this.Current.ToString().Length > 0;
                            }
                            else
                            {
                                tokenBuilder.Append(c);
                                previousCharacter = c;
                                break;
                            }
                        }
                    case TokenSplitType.After:
                        {
                            tokenBuilder.Append(c);
                            this.Current = new Token(tokenBuilder.ToString());
                            tokenBuilder.Length = 0;
                            previousCharacter = null;
                            return this.Current.ToString().Length > 0;
                        }
                    case TokenSplitType.Skip:
                        {
                            if (tokenBuilder.Length == 0) continue;
                            this.Current = new Token(tokenBuilder.ToString());
                            tokenBuilder.Length = 0;
                            previousCharacter = null;
                            return this.Current.ToString().Length > 0;
                        }
                }
            }
            this.Current = new Token(tokenBuilder.ToString());
            tokenBuilder.Length = 0;
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
        public override string ToString() => token;
    }
}
