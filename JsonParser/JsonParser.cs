using qpwakaba.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;

using static qpwakaba.JsonSpecialCharacters;
namespace qpwakaba
{
    public static class JsonParser
    {
        public static IJsonValue Parse(string json, bool keepOrder = false)
        {
            using (var reader = new StringReader(json))
            {
                return Parse(reader, keepOrder);
            }
        }
        public static IJsonValue Parse(Stream stream, bool keepOrder = false)
        {
            using (var reader = new StreamReader(stream))
            {
                return Parse(reader, keepOrder);
            }
        }

        public static IJsonValue Parse(TextReader reader, bool keepOrder = false)
        {
            var tokenizer = new Tokenizer(reader, new JsonTokenRule());
            var analyzer = new JsonAnalyzer(keepOrder);
            foreach (Token token in tokenizer)
            {
                try
                {
                    var jsonToken = Parse(token);
                    analyzer.Analyze(jsonToken);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidDataException("Invalid JSON format", ex);
                }
            }
            return analyzer.Finish();
        }
        internal static IJsonToken Parse(Token rawToken)
        {
            var tokenString = rawToken.ToString();
            switch (tokenString[0])
            {
                case jsonBeginArray:
                    return new JsonBeginArrayToken();
                case jsonBeginObject:
                    return new JsonBeginObjectToken();
                case jsonEndArray:
                    return new JsonEndArrayToken();
                case jsonEndObject:
                    return new JsonEndObjectToken();
                case jsonNameSeparator:
                    return new JsonNameSeparatorToken();
                case jsonValueSeparator:
                    return new JsonValueSeparatorToken();
            }
            switch (tokenString)
            {
                case jsonTrue:
                    return new JsonBoolean(true);
                case jsonFalse:
                    return new JsonBoolean(false);
                case jsonNull:
                    return new JsonNull();
            }

            if (tokenString.Length >= 2 &&
                tokenString[0] == jsonQuotationMark &&
                tokenString[tokenString.Length - 1] == jsonQuotationMark)
            {
                return JsonString.Parse(tokenString.Substring(1, tokenString.Length - 2));
            }

            if (JsonTokenRule.IsValidNumber(tokenString))
            {
                return new JsonNumber(tokenString);
            }

            throw new ArgumentException(rawToken.ToString());
        }
    }

    public static class JsonSpecialCharacters
    {
        public const char jsonBeginArray = '\u005B';     /* [ */
        public const char jsonBeginObject = '\u007B';    /* { */
        public const char jsonEndArray = '\u005D';       /* ] */
        public const char jsonEndObject = '\u007D';      /* } */
        public const char jsonNameSeparator = '\u003A';  /* : */
        public const char jsonValueSeparator = '\u002C'; /* , */

        public const char jsonQuotationMark = '\u0022';  /* " */
        public const char jsonEscape = '\u005C';         /* \ */

        public const char jsonDecimalPoint = '\u002E';   /* . */
        public const char jsonMinus = '\u002D';          /* - */
        public const char jsonPlus = '\u002B';           /* + */
        public const char jsonExpSmall = 'e';
        public const char jsonExpLarge = 'E';

        public const string jsonFalse = "false";
        public const string jsonTrue = "true";
        public const string jsonNull = "null";

        public static readonly char[] jsonWhiteSpaces = {
            '\u0020', /* Space */
            '\u0009', /* Horizontal tab */
            '\u000A', /* Line feed or New line */
            '\u000D'  /* Carriage return */
        };

        public static readonly char[] jsonSymbols = {
            jsonBeginArray,
            jsonBeginObject,
            jsonEndArray,
            jsonEndObject,
            jsonNameSeparator,
            jsonValueSeparator,
            jsonQuotationMark,
            jsonEscape,
        };
    }

    internal class JsonAnalyzer
    {
        private readonly Stack<IJsonToken> tokens = new Stack<IJsonToken>();
        private readonly bool keepOrder;
        public JsonAnalyzer(bool keepOrder) => this.keepOrder = keepOrder;
        public void Analyze(IJsonToken current)
        {
            switch (current.Type)
            {
                case JsonTokenType.BeginArray:
                case JsonTokenType.BeginObject:
                {
                    this.tokens.Push(current);
                    break;
                }
                case JsonTokenType.EndArray:
                {
                    var arrayTokens = new Stack<IJsonToken>();
                    arrayTokens.Push(current);
                    while (true)
                    {
                        if (this.tokens.Count == 0) throw new InvalidDataException();
                        var token = this.tokens.Pop();
                        arrayTokens.Push(token);
                        if (token.Type == JsonTokenType.BeginArray) break;
                    }
                    this.tokens.Push(new JsonArray(arrayTokens));
                    break;
                }
                case JsonTokenType.EndObject:
                {
                    var objectTokens = new Stack<IJsonToken>();
                    objectTokens.Push(current);
                    while (true)
                    {
                        if (this.tokens.Count == 0) throw new InvalidDataException();
                        var token = this.tokens.Pop();
                        objectTokens.Push(token);
                        if (token.Type == JsonTokenType.BeginObject) break;
                    }
                    this.tokens.Push(new JsonObject(objectTokens, this.keepOrder));
                    break;
                }
                case JsonTokenType.Value:
                case JsonTokenType.NameSeparator:
                case JsonTokenType.ValueSeparator:
                {
                    this.tokens.Push(current);
                    break;
                }
                default:
                    throw new ArgumentException($"Unknown token type ({current.Type})");
            }
        }
        public IJsonValue Finish()
        {
            if (this.tokens.Count != 1)
            {
                throw new InvalidDataException();
            }
            var token = this.tokens.Pop();
            if (token.Type != JsonTokenType.Value) throw new InvalidDataException();
            var value = (IJsonValue)token;

            switch (value.Type)
            {
                case JsonValueType.Number:
                case JsonValueType.String:
                case JsonValueType.Literal:
                    throw new InvalidDataException();
            }
            this.tokens.Clear();

            return value;
        }
    }

    internal class JsonTokenRule : ITokenRule
    {

        private bool IsInString { get; set; } = false;
        internal static bool IsDigit(char c) => '0' <= c && c <= '9';
        internal static bool IsValidNumber(string token)
        {
            if (token.Length == 0) return false;
            int i = 0;
            if (token[i] == jsonMinus) i++;

            // at least 1 digit
            if (i == token.Length) return false;
            if (!IsDigit(token[i])) return false;
            bool isZero = false;
            if (token[i] == '0') isZero = true;
            bool isFrac = false;
            for (i++; i < token.Length; i++)
            {
                // 0# (#: '0'-'9')
                if (isZero && IsDigit(token[i]) && !isFrac) return false;

                // c is not digit or decimal point
                if (!IsDigit(token[i]) && token[i] != jsonDecimalPoint) break;

                // end with decimal point
                if (token[i] == jsonDecimalPoint && i + 1 == token.Length) return false;

                if (token[i] == jsonDecimalPoint)
                {
                    if (isFrac) return false;
                    isFrac = true;
                }
            }

            if (i == token.Length) return true;
            if (token[i] != jsonExpSmall && token[i] != jsonExpLarge) return false;
            if (++i == token.Length) return false;
            if (token[i] == jsonMinus || token[i] == jsonPlus)
            {
                if (++i == token.Length) return false;
            }
            for (; i < token.Length; i++)
            {
                if (!IsDigit(token[i])) return false;
            }
            return true;
        }
        public Tokenizer.TokenSplitType ProcessCharacter(char c, char? previousOfToken, string token)
        {
            // if (previousOfToken == null && IsInString) throw AssertionError;
            Debug.Assert(previousOfToken != null || !this.IsInString);
            if (this.IsInString)
            {
                if (c == jsonQuotationMark && previousOfToken != jsonEscape)
                {
                    this.IsInString = false;
                    return Tokenizer.TokenSplitType.After;
                }
                else
                {
                    return Tokenizer.TokenSplitType.Concat;
                }
            }
            else if (Array.IndexOf(jsonWhiteSpaces, c) >= 0)
            {
                return Tokenizer.TokenSplitType.Skip;
            }
            else if (c == jsonQuotationMark)
            {
                this.IsInString = true;
                return Tokenizer.TokenSplitType.Before;
            }
            else if (Array.IndexOf(jsonSymbols, c) >= 0)
            {
                return Tokenizer.TokenSplitType.BeforeAndAfter;
            }
            else
            {
                return Tokenizer.TokenSplitType.Concat;
            }

        }
    }

    public enum JsonTokenType
    {
        BeginArray,
        EndArray,
        BeginObject,
        EndObject,
        NameSeparator,
        ValueSeparator,
        Value
    }
    public interface IJsonToken
    {
        JsonTokenType Type { get; }
    }

    internal class JsonBeginArrayToken : IJsonToken
    {
        internal JsonBeginArrayToken() { }
        public override string ToString() => jsonBeginArray.ToString();
        JsonTokenType IJsonToken.Type => JsonTokenType.BeginArray;
    }
    internal class JsonBeginObjectToken : IJsonToken
    {
        internal JsonBeginObjectToken() { }
        public override string ToString() => jsonBeginObject.ToString();
        JsonTokenType IJsonToken.Type => JsonTokenType.BeginObject;
    }
    internal class JsonEndArrayToken : IJsonToken
    {
        internal JsonEndArrayToken() { }
        public override string ToString() => jsonEndArray.ToString();
        JsonTokenType IJsonToken.Type => JsonTokenType.EndArray;
    }
    internal class JsonEndObjectToken : IJsonToken
    {
        internal JsonEndObjectToken() { }
        public override string ToString() => jsonEndObject.ToString();
        JsonTokenType IJsonToken.Type => JsonTokenType.EndObject;
    }
    internal class JsonNameSeparatorToken : IJsonToken
    {
        internal JsonNameSeparatorToken() { }
        public override string ToString() => jsonNameSeparator.ToString();
        JsonTokenType IJsonToken.Type => JsonTokenType.NameSeparator;
    }
    internal class JsonValueSeparatorToken : IJsonToken
    {
        internal JsonValueSeparatorToken() { }
        public override string ToString() => jsonValueSeparator.ToString();
        JsonTokenType IJsonToken.Type => JsonTokenType.ValueSeparator;
    }
}
