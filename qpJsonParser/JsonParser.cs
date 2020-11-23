using qpwakaba.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace qpwakaba
{
    public class JsonParser
    {
        internal const char jsonBeginArray = '\u005B';     /* [ */
        internal const char jsonBeginObject = '\u007B';    /* { */
        internal const char jsonEndArray = '\u005D';       /* ] */
        internal const char jsonEndObject = '\u007D';      /* } */
        internal const char jsonNameSeparator = '\u003A';  /* : */
        internal const char jsonValueSeparator = '\u002C'; /* , */

        internal const char jsonQuotationMark = '\u0022';  /* " */
        internal const char jsonEscape = '\u005C';         /* \ */

        internal const char jsonDecimalPoint = '\u002E';   /* . */
        internal const char jsonMinus = '\u002D';          /* - */
        internal const char jsonPlus = '\u002B';           /* + */
        internal const char jsonExpSmall = 'e';
        internal const char jsonExpLarge = 'E';

        internal const string jsonFalse = "false";
        internal const string jsonTrue = "true";
        internal const string jsonNull = "null";

        internal static readonly char[] jsonWhiteSpaces = {
            '\u0020', /* Space */
            '\u0009', /* Horizontal tab */
            '\u000A', /* Line feed or New line */
            '\u000D'  /* Carriage return */
        };

        internal static readonly char[] jsonSymbols = {
            jsonBeginArray,
            jsonBeginObject,
            jsonEndArray,
            jsonEndObject,
            jsonNameSeparator,
            jsonValueSeparator,
            jsonQuotationMark,
            jsonEscape,
        };
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
        internal static bool IsWhiteSpace(char c) => c switch
        {
            '\u0020' => true, /* Space */
            '\u0009' => true, /* Horizontal tab */
            '\u000A' => true, /* Line feed or New line */
            '\u000D' => true, /* Carriage return */
            _ => false
        };
        internal static bool IsLiteralChar(char c) => c is >= 'a' and <= 'z';

        private readonly StringBuilder buf = new();
        private bool IsInString { get; set; } = false;
        public IJsonValue Parse(string json, bool keepOrder = false)
        {
            using (var reader = new StringReader(json))
            {
                return Parse(reader, keepOrder);
            }
        }
        public IJsonValue Parse(TextReader reader) => Parse(reader, false);
        public IJsonValue Parse(TextReader reader, bool keepOrder)
        {
            var value = ParseValue();
            return (peekOrReadNonWhiteSpaceNullable()) switch
            {
                jsonEndArray or jsonEndObject or jsonValueSeparator or null => value,
                _ => throw new InvalidDataException(),
            };

            IJsonValue ParseValue()
            {
                return peekOrReadNonWhiteSpaceNullable() switch
                {
                    jsonBeginArray => ParseArray(),
                    jsonBeginObject => ParseObject(),
                    jsonQuotationMark => ParseString(),
                    jsonMinus => ParseNumber(),
                    >= '0' and <= '9' => ParseNumber(),
                    char nc when IsLiteralChar(nc) => ParseLiteral(),
                    _ => throw new InvalidDataException()
                };
            }

            JsonArray ParseArray()
            {
                if (readNonWhiteSpace() is not jsonBeginArray)
                    throw new InvalidDataException();

                JsonArray arr = new();
                if (peekOrReadNonWhiteSpace() is jsonEndArray)
                {
                    read();
                    return arr;
                }
                while (true) { 
                    arr.Elements.Add(ParseValue());
                    switch (peekOrReadNonWhiteSpace())
                    {
                        case jsonEndArray:
                            read();
                            return arr;
                        case jsonValueSeparator:
                            read();
                            continue;
                        default:
                            throw new InvalidDataException();
                    }
                }
            }
            JsonObject ParseObject()
            {
                if (readNonWhiteSpace() is not jsonBeginObject)
                    throw new InvalidDataException();

                JsonObject obj = new();
                if (peekOrReadNonWhiteSpace() is jsonEndObject)
                {
                    read();
                    return obj;
                }
                while (true)
                {
                    JsonString name = ParseString();
                    if (readNonWhiteSpace() != jsonNameSeparator)
                        throw new InvalidDataException();
                    IJsonValue value = ParseValue();
                    obj[name.Value] = value;
                    switch (peekOrReadNonWhiteSpace())
                    {
                        case jsonEndObject:
                            read();
                            return obj;
                        case jsonValueSeparator:
                            read();
                            continue;
                        default:
                            throw new InvalidDataException();
                    }
                }
            }
            JsonNumber ParseNumber()
            {
                // TODO: 構文解析が JsonNumber にもあって気持ち悪いのでなんとかする
                buf.Clear();
                if (peekOrReadNonWhiteSpace() == jsonMinus)
                    buf.Append(read());
 
                // int
                switch (peek())
                {
                    case '0':
                        buf.Append(read());
                        break;
                    case char c when '1' <= c && c <= '9':
                        buf.Append(read());
                        while (IsDigit(peek()))
                            buf.Append(read());
                        break;
                    default:
                        throw new InvalidDataException();
                }

                // frac?
                if (peekNullable() == jsonDecimalPoint)
                {
                    buf.Append(read());
                    if (!IsDigit(peek()))
                        throw new InvalidDataException();

                    do
                    {
                        buf.Append(read());
                    } while (peekNullable() is char c && IsDigit(c));
                }

                if (peekNullable() is jsonExpSmall or jsonExpLarge)
                {
                    buf.Append(read());
                    if (peekNullable() is (jsonMinus or jsonPlus))
                    {
                        buf.Append(read());
                    }
                    {
                        if (peekNullable() is char c && !IsDigit(c))
                            throw new InvalidDataException();
                    }
                    // c is DIGIT
                    do
                    {
                        buf.Append(read());
                    } while (peekNullable() is char c && IsDigit(c));
                }
                return new(buf.ToString());
            }
            JsonString ParseString()
            {
                if (readNonWhiteSpace() is not jsonQuotationMark)
                    throw new InvalidDataException();
                buf.Clear();
                while (true)
                {
                    char c = readChecked();
                    switch (c)
                    {
                        case jsonQuotationMark:
                            return new(buf.ToString());
                        case jsonEscape:
                            buf.Append(readNonWhiteSpace());
                            continue;
                        default:
                            buf.Append(c);
                            continue;
                    }
                }
            }
            IJsonValue ParseLiteral()
            {
                switch (peek())
                {
                    case 'f':
                    {
                        read();
                        if (readChecked() != 'a') break;
                        if (readChecked() != 'l') break;
                        if (readChecked() != 's') break;
                        if (readChecked() != 'e') break;
                        if (peekNullable() is char c && IsLiteralChar(c)) break;
                        return new JsonBoolean(false);
                    }
                    case 't':
                    {
                        read();
                        if (readChecked() != 'r') break;
                        if (readChecked() != 'u') break;
                        if (readChecked() != 'e') break;
                        if (peekNullable() is char c && IsLiteralChar(c)) break;
                        return new JsonBoolean(true);
                    }
                    case 'n':
                    {
                        read();
                        if (readChecked() != 'u') break;
                        if (readChecked() != 'l') break;
                        if (readChecked() != 'l') break;
                        if (peekNullable() is char c && IsLiteralChar(c)) break;
                        return new JsonNull();
                    }
                }
                throw new InvalidDataException();
            }

            char? peekNullable()
            {
                if ((reader.Peek() is int i) && i != -1)
                    return (char) i;
                else
                    return null;
            }
            char peek() => peekNullable() ?? throw new Exception();

            char? readCheckedNullable() => reader.Read() is not -1 and int i ? (char) i : null;
            char readChecked() => readCheckedNullable() ?? throw new InvalidDataException();
            char read() => (char)reader.Read();
            char? readNonWhiteSpaceNullable()
            {
                int r;
                while (true)
                {
                    r = reader.Read();
                    if (r == -1) return null;
                    if (IsWhiteSpace((char) r)) continue;
                    return (char) r;
                }
            }
            char readNonWhiteSpace() => readNonWhiteSpaceNullable() ?? throw new InvalidDataException();
            char? peekOrReadNonWhiteSpaceNullable()
            {
                while (true)
                {
                    switch (peekNullable())
                    {
                        case char c when !IsWhiteSpace(c):
                            return c;
                        case null:
                            return null;
                        default:
                            read();
                            continue;
                    }
                }
            }

            char peekOrReadNonWhiteSpace() => peekOrReadNonWhiteSpaceNullable() ?? throw new InvalidDataException();
        }

        public static IJsonValue ParseOnce(string json) => ParseOnce(new StringReader(json));
        public static IJsonValue ParseOnce(string json, bool keepOrder) => ParseOnce(new StringReader(json), keepOrder);
        public static IJsonValue ParseOnce(TextReader reader)
            => new JsonParser().Parse(reader);
        public static IJsonValue ParseOnce(TextReader reader, bool keepOrder)
            => new JsonParser().Parse(reader, keepOrder);
    }

}
