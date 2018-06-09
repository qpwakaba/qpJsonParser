using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using qpwakaba.Utils;

using static qpwakaba.JsonSpecialCharacters;
namespace qpwakaba
{
    public enum JsonValueType
    {
        Object,
        Array,
        Number,
        String,
        Literal
    }
    public interface IDeepCopy<T>
    {
        T DeepCopy();
    }
    public interface IJsonValue : IJsonToken, IDeepCopy<IJsonValue>
    {
        new JsonValueType Type { get; }
        string ToString();
        string ToJsonCompatibleString(bool escapeUnicodeCharacter = false);
    }
    public class JsonBoolean : IJsonValue, IComparable, IComparable<bool>, IComparable<JsonBoolean>,
        IConvertible, IEquatable<bool>, IEquatable<JsonBoolean>
    {
        public bool Value { get; }
        #region constructors
        public JsonBoolean(bool boolean) => this.Value = boolean;
        #endregion
        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;

            var other = (JsonBoolean) obj;
            return this.Equals(other);
        }
        public override int GetHashCode() => this.Value.GetHashCode();
        #endregion

        public override string ToString() => this.Value ? jsonTrue : jsonFalse;
        public string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
            => ToString();

        #region implementation of interfaces
        JsonTokenType IJsonToken.Type => JsonTokenType.Value;
        JsonValueType IJsonValue.Type => JsonValueType.Literal;
        public int CompareTo(object obj) => this.Value.CompareTo(obj);
        public int CompareTo(bool other) => this.Value.CompareTo(other);
        TypeCode IConvertible.GetTypeCode() => TypeCode.Boolean;
        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible) this.Value).ToBoolean(provider);
        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible) this.Value).ToByte(provider);
        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible) this.Value).ToChar(provider);
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible) this.Value).ToDateTime(provider);
        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible) this.Value).ToDecimal(provider);
        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible) this.Value).ToDouble(provider);
        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible) this.Value).ToInt16(provider);
        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible) this.Value).ToInt32(provider);
        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible) this.Value).ToInt64(provider);
        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible) this.Value).ToSByte(provider);
        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible) this.Value).ToSingle(provider);
        string IConvertible.ToString(IFormatProvider provider) => this.Value.ToString(provider);
        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible) this.Value).ToType(conversionType, provider);
        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible) this.Value).ToUInt16(provider);
        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible) this.Value).ToUInt32(provider);
        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible) this.Value).ToUInt64(provider);
        public bool Equals(bool other) => this.Value.Equals(other);
        public bool Equals(JsonBoolean other) => Equals(other.Value);
        public int CompareTo(JsonBoolean other) => CompareTo(other.Value);
        public IJsonValue DeepCopy() => new JsonBoolean(this.Value);
        #endregion
    }
    public class JsonNull : IJsonValue
    {
        public JsonValueType Type => JsonValueType.Literal;
        public JsonNull() { }
        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            return obj.GetType() == this.GetType();
        }
        public override int GetHashCode() => -1;
        #endregion

        public override string ToString() => jsonNull;
        public string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
            => ToString();

        JsonTokenType IJsonToken.Type => JsonTokenType.Value;
        IJsonValue IDeepCopy<IJsonValue>.DeepCopy() => new JsonNull();
    }
    public class JsonNumber : IJsonValue, IComparable, IComparable<int>, IComparable<long>, IComparable<uint>,
        IComparable<ulong>, IComparable<float>, IComparable<double>, IComparable<decimal>, IComparable<JsonNumber>,
        IEquatable<int>, IEquatable<long>, IEquatable<uint>, IEquatable<ulong>, IEquatable<float>,
        IEquatable<double>, IEquatable<decimal>, IEquatable<JsonNumber>, IFormattable, IConvertible
    {
        #region constructors
        public JsonNumber(string value)
        {
            if (!JsonTokenRule.IsValidNumber(value))
                throw new ArgumentException($"{value} is not a valid number.");
            this.StringValue = value;
        }
        public JsonNumber(float value) => this.FloatValue = value;
        public JsonNumber(double value) => this.DoubleValue = value;
        public JsonNumber(int value) => this.IntegerValue = value;
        public JsonNumber(long value) => this.LongValue = value;
        public JsonNumber(uint value) => this.UIntValue = value;
        public JsonNumber(ulong value) => this.ULongValue = value;
        public JsonNumber(decimal value) => this.DecimalValue = value;
        #endregion

        public virtual float FloatValue
        {
            get => (float) this.DoubleValue;
            set => this.StringValue = value.ToString();
        }
        public virtual double DoubleValue
        {
            get => double.Parse(this.StringValue);
            set => this.StringValue = value.ToString();
        }
        public virtual int IntegerValue
        {
            get => (int) this.LongValue;
            set => this.StringValue = value.ToString();
        }
        public virtual long LongValue
        {
            get
            {
                const long Ten = 10;
                var split = this.StringValue.ToLower().Split(jsonExpSmall);
                var mantissa = long.Parse(split[0]);
                if (split.Length == 1)
                    return mantissa;

                int exponent = int.Parse(split[1]);
                if (exponent == 0)
                    return mantissa;

                if (exponent > 0)
                {
                    for (int i = 0; i < Math.Abs(exponent); i++)
                    {
                        mantissa *= Ten;
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Abs(exponent); i++)
                    {
                        mantissa /= Ten;
                    }
                }
                return mantissa;
            }
            set => this.StringValue = value.ToString();
        }
        public virtual uint UIntValue
        {
            get => (uint) this.ULongValue;
            set => this.StringValue = value.ToString();
        }
        public virtual ulong ULongValue
        {
            get
            {
                if (this.StringValue[0] == jsonMinus)
                {
                    return (ulong) this.LongValue;
                }
                const long Ten = 10;
                var split = this.StringValue.ToLower().Split(jsonExpSmall);
                var mantissa = ulong.Parse(split[0]);
                if (split.Length == 1)
                    return mantissa;

                int exponent = int.Parse(split[1]);
                if (exponent == 0)
                    return mantissa;

                if (exponent > 0)
                {
                    for (int i = 0; i < Math.Abs(exponent); i++)
                    {
                        mantissa *= Ten;
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Abs(exponent); i++)
                    {
                        mantissa /= Ten;
                    }
                }
                return mantissa;
            }
            set => this.StringValue = value.ToString();
        }
        public virtual decimal DecimalValue
        {
            get
            {
                const decimal Ten = 10;
                var split = this.StringValue.ToLower().Split(jsonExpSmall);
                var mantissa = decimal.Parse(split[0]);
                if (split.Length == 1)
                    return mantissa;

                int exponent = int.Parse(split[1]);
                if (exponent == 0)
                    return mantissa;

                decimal multiplier;
                if (exponent > 0)
                {
                    multiplier = Ten;
                }
                else
                {
                    multiplier = decimal.One / Ten;
                }
                for (int i = 0; i < Math.Abs(exponent); i++)
                {
                    mantissa *= multiplier;
                }
                return mantissa;
            }
            set => this.StringValue = value.ToString();
        }
        public string StringValue { get; set; }
        public override string ToString() => this.StringValue;
        public string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
            => ToString();

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;

            var other = (JsonNumber) obj;

            return this.Equals(other);
        }
        public bool Equals(JsonNumber other) => Equals(this.StringValue, other.StringValue);
        internal static bool Equals(string number1, string number2)
        {
            string[] split1 = number1.ToLower().Split(jsonExpSmall);
            string[] split2 = number2.ToLower().Split(jsonExpSmall);

            string mantissa1 = split1[0];
            string mantissa2 = split2[0];

            // check if digits aren't different
            {
                bool nonZero1 = false;
                bool nonZero2 = false;
                int i1 = 0;
                int i2 = 0;

                bool negative = false;
                // check if signs aren't different
                if ((negative = mantissa1[0] == jsonMinus) != (mantissa2[0] == jsonMinus))
                    return false;

                if (negative)
                {
                    i1++;
                    i2++;
                }

                if (mantissa1[i1] == '0')
                {
                    i1++;
                    if (i1 < mantissa1.Length && mantissa1[i1] == jsonDecimalPoint)
                    { 
                        i1++;
                        for (; i1 < mantissa1.Length; i1++)
                        {
                            if (mantissa1[i1] != '0')
                                break;
                        }
                    }
                }
                if (mantissa2[i2] == '0')
                {
                    i2++;
                    if (i2 < mantissa2.Length && mantissa2[i2] == jsonDecimalPoint)
                        i2++;
                    for (; i2 < mantissa2.Length; i2++)
                    {
                        if (mantissa2[i2] != '0')
                            break;
                    }
                }

                for (; i1 < mantissa1.Length || i2 < mantissa2.Length; i1++, i2++)
                {
                    if (i1 < mantissa1.Length && mantissa1[i1] == jsonDecimalPoint)
                        i1++;
                    if (i2 < mantissa2.Length && mantissa2[i2] == jsonDecimalPoint)
                        i2++;

                    if (i1 < mantissa1.Length && mantissa1[i1] != '0')
                        nonZero1 = true;
                    if (i2 < mantissa2.Length && mantissa2[i2] != '0')
                        nonZero2 = true;

                    if (i1 >= mantissa1.Length && i2 >= mantissa2.Length)
                    {
                        break;
                    }
                    else if (i1 < mantissa1.Length && i2 >= mantissa2.Length)
                    {
                        for (; i1 < mantissa1.Length; i1++)
                            if (mantissa1[i1] != '0')
                                return false;
                        break;
                    }
                    else if (i1 >= mantissa1.Length && i2 < mantissa2.Length)
                    {
                        for (; i2 < mantissa2.Length; i2++)
                            if (mantissa2[i2] != '0')
                                return false;
                        break;
                    }
                    else if (mantissa1[i1] != mantissa2[i2])
                    {
                        return false;
                    }
                }

                if (!nonZero1 && !nonZero2)
                    return true;
            }


            int e1 = Log10i(mantissa1);
            int e2 = Log10i(mantissa2);

            if (split1.Length == 2)
            {
                e1 += int.Parse(split1[1]);
            }
            if (split2.Length == 2)
            {
                e2 += int.Parse(split2[1]);
            }

            return e1 == e2;
        }

        internal static int Log10i(string stringValue)
        {
            int i = 0;
            if (stringValue[i] == jsonMinus)
                i++;

            // abs(stringValue) >= 1.0
            if (stringValue[i] != '0')
            {
                int d = 0;
                for (; i < stringValue.Length; d++, i++)
                {
                    if (stringValue[i] == jsonDecimalPoint)
                    {
                        break;
                    }
                }
                return d - 1;
            }
            else
            {
                i += 2; // "0."

                int d = 0;
                bool noZeroFlag = false;
                for (; i < stringValue.Length; i++)
                {
                    d--;
                    if (stringValue[i] != '0')
                    {
                        noZeroFlag = true;
                    }
                }
                return noZeroFlag ? d : 0;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 17;

            string[] split = this.StringValue.ToLower().Split(jsonExpSmall);
            string mantissa = split[0];

            int i = 0;
            if (mantissa[i] == jsonMinus)
            {
                hashCode += hashCode * 31 + (-1);
                i++;
            }

            if (mantissa[i] == '0')
            {
                i++;
                if (i < mantissa.Length && mantissa[i] == jsonDecimalPoint)
                {
                    i++;
                    for (; i < mantissa.Length; i++)
                    {
                        if (mantissa[i] != '0')
                            break;
                    }
                }
            }
            int tempHashCode = hashCode;
            for (; i < mantissa.Length; i++)
            {
                if (mantissa[i] == jsonDecimalPoint)
                {
                    continue;
                }

                if (mantissa[i] == '0')
                {
                    tempHashCode = tempHashCode * 31 + (mantissa[i] - '0');
                }
                else
                {
                    hashCode = tempHashCode * 31 + (mantissa[i] - '0');
                    tempHashCode = hashCode;
                }
            }

            return hashCode;
        }
        #endregion

        #region implementation of interfaces
        JsonTokenType IJsonToken.Type => JsonTokenType.Value;
        JsonValueType IJsonValue.Type => JsonValueType.Number;
        public int CompareTo(object obj) => this.DecimalValue.CompareTo(obj);
        public int CompareTo(int other) => this.IntegerValue.CompareTo(other);
        public int CompareTo(long other) => this.LongValue.CompareTo(other);
        public int CompareTo(float other) => this.FloatValue.CompareTo(other);
        public int CompareTo(double other) => this.DoubleValue.CompareTo(other);
        public int CompareTo(uint other) => this.UIntValue.CompareTo(other);
        public int CompareTo(ulong other) => this.ULongValue.CompareTo(other);
        public int CompareTo(decimal other) => this.DecimalValue.CompareTo(other);
        public string ToString(string format, IFormatProvider formatProvider) => this.DecimalValue.ToString(format, formatProvider);
        public TypeCode GetTypeCode() => TypeCode.Object;
        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible) this.StringValue).ToBoolean(provider);
        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible) this.StringValue).ToByte(provider);
        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible) this.StringValue).ToChar(provider);
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible) this.StringValue).ToDateTime(provider);
        decimal IConvertible.ToDecimal(IFormatProvider provider) => this.DecimalValue;
        double IConvertible.ToDouble(IFormatProvider provider) => this.DoubleValue;
        short IConvertible.ToInt16(IFormatProvider provider) => (short) this.IntegerValue;
        int IConvertible.ToInt32(IFormatProvider provider) => this.IntegerValue;
        long IConvertible.ToInt64(IFormatProvider provider) => this.LongValue;
        sbyte IConvertible.ToSByte(IFormatProvider provider) => (sbyte) this.IntegerValue;
        float IConvertible.ToSingle(IFormatProvider provider) => this.FloatValue;
        string IConvertible.ToString(IFormatProvider provider) => this.StringValue.ToString(provider);
        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible) this.StringValue).ToType(conversionType, provider);
        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible) this.StringValue).ToUInt16(provider);
        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible) this.StringValue).ToUInt32(provider);
        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible) this.StringValue).ToUInt64(provider);
        public bool Equals(int other) => this.IntegerValue.Equals(other);
        public bool Equals(long other) => this.LongValue.Equals(other);
        public bool Equals(uint other) => this.UIntValue.Equals(other);
        public bool Equals(ulong other) => this.ULongValue.Equals(other);
        public bool Equals(float other) => this.FloatValue.Equals(other);
        public bool Equals(double other) => this.DoubleValue.Equals(other);
        public bool Equals(decimal other) => this.DecimalValue.Equals(other);
        public int CompareTo(JsonNumber other) => this.CompareTo(other.DecimalValue);
        IJsonValue IDeepCopy<IJsonValue>.DeepCopy() => new JsonNumber(this.StringValue);
        #endregion
    }
    public class JsonString : IJsonValue, IComparable, IConvertible, IEnumerable,
        IComparable<string>, IComparable<JsonString>, IEquatable<string>, IEquatable<JsonString>, IEnumerable<char>
    {
        public string Value { get; set; }
        #region constructors
        public JsonString(string String) => this.Value = String;
        public JsonString(JsonString jsonString) => this.Value = jsonString.Value;
        #endregion
        public override string ToString() => this.Value;
        public string ToJsonCompatibleString(bool escapeUnicodeString = false)
        {
            var builder = new StringBuilder();
            foreach (var c in this.Value)
            {
                switch (c)
                {
                    case '\u0022':
                        builder.Append("\\\"");
                        break;
                    case '\u005C':
                        builder.Append("\\\\");
                        break;
                    case '\u002F':
                        builder.Append("\\/");
                        break;
                    case '\u0008':
                        builder.Append("\\b");
                        break;
                    case '\u000C':
                        builder.Append("\\f");
                        break;
                    case '\u000A':
                        builder.Append("\\n");
                        break;
                    case '\u000D':
                        builder.Append("\\r");
                        break;
                    case '\u0009':
                        builder.Append("\\t");
                        break;
                    default:
                        if (escapeUnicodeString || (0x00 <= c && c <= 0x1F))
                        {
                            builder.AppendFormat("\\u{0:X4}", (int) c);
                        }
                        else
                        {
                            builder.Append(c);
                        }
                        break;
                }
            }
            return $"{jsonQuotationMark}{builder.ToString()}{jsonQuotationMark}";
        }

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;

            var other = (JsonString) obj;
            return this.Value.Equals(other.Value);
        }
        public override int GetHashCode() => this.Value.GetHashCode();
        #endregion

        #region implementation of interfaces
        JsonTokenType IJsonToken.Type => JsonTokenType.Value;
        JsonValueType IJsonValue.Type => JsonValueType.String;
        public int CompareTo(object obj) => this.Value.CompareTo(obj);
        public object Clone() => new JsonString(this.Value);
        TypeCode IConvertible.GetTypeCode() => this.Value.GetTypeCode();
        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible) this.Value).ToBoolean(provider);
        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible) this.Value).ToByte(provider);
        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible) this.Value).ToChar(provider);
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible) this.Value).ToDateTime(provider);
        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible) this.Value).ToDecimal(provider);
        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible) this.Value).ToDouble(provider);
        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible) this.Value).ToInt16(provider);
        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible) this.Value).ToInt32(provider);
        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible) this.Value).ToInt64(provider);
        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible) this.Value).ToSByte(provider);
        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible) this.Value).ToSingle(provider);
        string IConvertible.ToString(IFormatProvider provider) => this.Value;
        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible) this.Value).ToType(conversionType, provider);
        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible) this.Value).ToUInt16(provider);
        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible) this.Value).ToUInt32(provider);
        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible) this.Value).ToUInt64(provider);
        public IEnumerator GetEnumerator() => ((IEnumerable) this.Value).GetEnumerator();
        public int CompareTo(string other) => this.Value.CompareTo(other);
        public bool Equals(string other) => this.Value.Equals(other);
        IEnumerator<char> IEnumerable<char>.GetEnumerator() => ((IEnumerable<char>) this.Value).GetEnumerator();
        public int CompareTo(JsonString other) => this.CompareTo(other.Value);
        public bool Equals(JsonString other) => this.Equals(other.Value);
        IJsonValue IDeepCopy<IJsonValue>.DeepCopy() => new JsonString(this);
        #endregion

        #region Parse
        public static JsonString Parse(string escapedString)
        {
            if (TryParse(escapedString, out JsonString result))
            {
                return result;
            }
            throw new InvalidDataException();
        }
        public static bool TryParse(string escapedString, out JsonString jsonString)
        {
            var builder = new StringBuilder(escapedString.Length);
            for (int i = 0; i < escapedString.Length; i++)
            {
                if (escapedString[i] == jsonEscape)
                {
                    if (i + 1 == escapedString.Length)
                    {
                        jsonString = default;
                        return false;
                    }
                    #region escapes
                    /*
                      %x22 /          ; "    quotation mark  U+0022
                      %x5C /          ; \    reverse solidus U+005C
                      %x2F /          ; /    solidus         U+002F
                      %x62 /          ; b    backspace       U+0008
                      %x66 /          ; f    form feed       U+000C
                      %x6E /          ; n    line feed       U+000A
                      %x72 /          ; r    carriage return U+000D
                      %x74 /          ; t    tab             U+0009
                      %x75 4HEXDIG )  ; uXXXX                U+XXXX
                     */
                    #endregion
                    switch (escapedString[++i])
                    {
                        case '\u0022':
                            builder.Append('\u0022');
                            break;
                        case '\u005C':
                            builder.Append('\u005C');
                            break;
                        case '\u002F':
                            builder.Append('\u002F');
                            break;
                        case '\u0062':
                            builder.Append('\u0008');
                            break;
                        case '\u0066':
                            builder.Append('\u000C');
                            break;
                        case '\u006E':
                            builder.Append('\u000A');
                            break;
                        case '\u0072':
                            builder.Append('\u000D');
                            break;
                        case '\u0074':
                            builder.Append('\u0009');
                            break;
                        case '\u0075':
                        {
                            ++i;
                            const int hexDigits = 4;
                            if (escapedString.Length < i + hexDigits)
                            {
                                // \u____
                                // 012345
                                // i     6
                                throw new InvalidDataException();
                            }
                            if (!TryGetCharacter(escapedString, i, out char decoded))
                            {
                                throw new InvalidDataException();
                            }
                            builder.Append(decoded);
                            i += hexDigits - 1;
                            break;
                        }
                        default:
                            jsonString = default;
                            return false;
                    }
                }
                else if (escapedString[i] == jsonQuotationMark)
                {
                    jsonString = default;
                    return false;
                }
                else if (0x00 <= escapedString[i] && escapedString[i] < 0x20)
                {
                    jsonString = default;
                    return false;
                }
                else
                    builder.Append(escapedString[i]);
            }
            jsonString = new JsonString(builder.ToString());
            return true;
        }

        private static int IntValueOfHexDigit(char hexDigit)
        {
            if ('0' <= hexDigit && hexDigit <= '9')
                return hexDigit - '0';
            if ('a' <= hexDigit && hexDigit <= 'f')
                return hexDigit - 'a' + 10;
            if ('A' <= hexDigit && hexDigit <= 'F')
                return hexDigit - 'A' + 10;
            throw new ArgumentException($"{hexDigit} is not a hex digit.");
        }
        internal static bool TryGetCharacter(string escaped, int offset, out char character)
        {
            char result = (char) 0;
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    result <<= 4;
                    result |= (char) IntValueOfHexDigit(escaped[offset + i]);
                }
                character = result;
                return true;
            }
            catch
            {
                character = default;
                return false;
            }
        }
        #endregion
    }
    public class JsonObject : IJsonValue, IDeepCopy<JsonObject>, IDictionary<string, IJsonValue>, IEnumerable<KeyValuePair<string, IJsonValue>>, IEnumerable
    {
        public IDictionary<string, IJsonValue> Parameters { get; }
        public IJsonValue this[string key] { get => this.Parameters[key]; set => this.Parameters[key] = value; }

        #region constructors
        public JsonObject(params KeyValuePair<string, IJsonValue>[] values)
            : this(values, false) { }
        public JsonObject(bool keepOrder, params KeyValuePair<string, IJsonValue>[] values)
            : this(values, keepOrder) { }
        public JsonObject(bool keepOrder = false) : this(new EmptyEnumerator<KeyValuePair<string, IJsonValue>>(), keepOrder) { }
        public JsonObject(IEnumerable<KeyValuePair<string, IJsonValue>> values, bool keepOrder = false)
            : this(values.GetEnumerator(), keepOrder) { }
        public JsonObject(IEnumerator<KeyValuePair<string, IJsonValue>> values, bool keepOrder = false)
        {
            this.Parameters = CreateDictionary<string, IJsonValue>(keepOrder);
            while (values.MoveNext())
            {
                if (!this.Parameters.ContainsKey(values.Current.Key))
                    this.Parameters.Add(values.Current);
            }
        }
        internal JsonObject(IEnumerable<IJsonToken> tokens, bool keepOrder) : this(tokens.GetEnumerator(), keepOrder) { }
        internal JsonObject(IEnumerator<IJsonToken> tokens, bool keepOrder)
        {
            this.Parameters = CreateDictionary<string, IJsonValue>(keepOrder);
            if (tokens.MoveNext())
            {
                if (tokens.Current.Type != JsonTokenType.BeginObject)
                    throw new InvalidDataException();
                if (!tokens.MoveNext())
                    throw new InvalidDataException();
                if (tokens.Current.Type != JsonTokenType.EndObject)
                    do
                    {
                        if (tokens.Current.Type != JsonTokenType.Value)
                            throw new InvalidDataException();
                        if (((IJsonValue) tokens.Current).Type != JsonValueType.String)
                            throw new InvalidDataException();
                        var key = (JsonString) tokens.Current;

                        if (!tokens.MoveNext())
                            throw new InvalidDataException();
                        if (tokens.Current.Type != JsonTokenType.NameSeparator)
                            throw new InvalidDataException();
                        if (!tokens.MoveNext())
                            throw new InvalidDataException();

                        if (tokens.Current.Type != JsonTokenType.Value)
                            throw new InvalidDataException();
                        var value = (IJsonValue) tokens.Current;

                        this.Parameters[key.Value] = value;

                        if (!tokens.MoveNext())
                            throw new InvalidDataException();
                        else if (tokens.Current.Type == JsonTokenType.EndObject)
                            break;
                        // disallow non value separator
                        else if (tokens.Current.Type != JsonTokenType.ValueSeparator)
                            throw new InvalidDataException();
                        // disallow trailing comma (value, })
                        else if (!tokens.MoveNext())
                            throw new InvalidDataException();
                    } while (true);
            }
        }
        #endregion

        #region ToString
        public override string ToString() => this.ToJsonCompatibleString(false);
        public string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
        {
            var builder = new StringBuilder();
            IEnumerator<KeyValuePair<string, IJsonValue>> enumerator = this.Parameters.GetEnumerator();
            builder.Append(jsonBeginObject);
            if (enumerator.MoveNext())
            {
                builder.Append(ToString(enumerator.Current, escapeUnicodeCharacter));
                while (enumerator.MoveNext())
                {
                    builder.Append($"{jsonValueSeparator} {ToString(enumerator.Current, escapeUnicodeCharacter)}");
                }
            }
            builder.Append(jsonEndObject);
            return builder.ToString();
        }

        private static string ToString(KeyValuePair<string, IJsonValue> pair, bool escapeUnicodeCharacter)
            => $"{new JsonString(pair.Key).ToJsonCompatibleString(escapeUnicodeCharacter)}{jsonNameSeparator} {pair.Value.ToJsonCompatibleString(escapeUnicodeCharacter)}";
        #endregion

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;

            var other = (JsonObject) obj;
            if (this.Parameters.Count != other.Parameters.Count)
                return false;

            var enumerator = this.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var thisValue = enumerator.Current.Value;
                var otherValue = other[enumerator.Current.Key];
                if (!thisValue.Equals(otherValue))
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            int hashCode = 17;
            var sortedParameters = new List<KeyValuePair<string, IJsonValue>>(this.Parameters);
            sortedParameters.Sort(KeyComparator.Instance);
            foreach (var parameter in this.Parameters)
            {
                hashCode = hashCode * 31 + (parameter.Key.GetHashCode() ^ parameter.Value.GetHashCode());
            }
            return hashCode;
        }

        private class KeyComparator : IComparer<KeyValuePair<string, IJsonValue>>
        {
            public static KeyComparator Instance { get; }
            static KeyComparator()
            {
                Instance = new KeyComparator();
            }
            private KeyComparator() { }

            public int Compare(KeyValuePair<string, IJsonValue> x, KeyValuePair<string, IJsonValue> y)
                => string.Compare(x.Key, y.Key);
        }
        #endregion

        public JsonObject DeepCopy()
        {
            bool keepOrder = this.Parameters.GetType() == typeof(OrderedDictionary<string, IJsonValue>);
            var instance = new JsonObject(this.Parameters, keepOrder);
            foreach (var parameter in this.Parameters)
                instance.Parameters[parameter.Key] = parameter.Value.DeepCopy();
            return instance;
        }

        #region implementation of interfaces
        JsonTokenType IJsonToken.Type => JsonTokenType.Value;
        JsonValueType IJsonValue.Type => JsonValueType.Object;
        public ICollection<string> Keys => this.Parameters.Keys;
        public ICollection<IJsonValue> Values => this.Parameters.Values;
        public int Count => this.Parameters.Count;
        public bool IsReadOnly => this.Parameters.IsReadOnly;
        public IEnumerator<KeyValuePair<string, IJsonValue>> GetEnumerator() => this.Parameters.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public void Add(string key, IJsonValue value) => this.Parameters.Add(key, value);
        public bool ContainsKey(string key) => this.Parameters.ContainsKey(key);
        public bool Remove(string key) => this.Parameters.Remove(key);
        public bool TryGetValue(string key, out IJsonValue value) => this.Parameters.TryGetValue(key, out value);
        public void Add(KeyValuePair<string, IJsonValue> item) => this.Parameters.Add(item);
        public void Clear() => this.Parameters.Clear();
        public bool Contains(KeyValuePair<string, IJsonValue> item) => this.Parameters.Contains(item);
        public void CopyTo(KeyValuePair<string, IJsonValue>[] array, int arrayIndex) => this.Parameters.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, IJsonValue> item) => this.Parameters.Remove(item);
        IJsonValue IDeepCopy<IJsonValue>.DeepCopy() => this.DeepCopy();
        #endregion

        private static IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(bool keepOrder)
            => keepOrder ? (IDictionary<TKey, TValue>) new OrderedDictionary<TKey, TValue>() : new Dictionary<TKey, TValue>();
        private static IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(int capacity, bool keepOrder)
            => keepOrder ? (IDictionary<TKey, TValue>) new OrderedDictionary<TKey, TValue>(capacity) : new Dictionary<TKey, TValue>(capacity);
    }
    public class JsonArray : IJsonValue, IDeepCopy<JsonArray>, IList<IJsonValue>, IEnumerable<IJsonValue>, IEnumerable
    {
        public IList<IJsonValue> Elements { get; }
        public IJsonValue this[int index] { get => this.Elements[index]; set => this.Elements[index] = value; }

        #region constructors
        public JsonArray() => this.Elements = new List<IJsonValue>();
        public JsonArray(params IJsonValue[] values) : this((IEnumerable<IJsonValue>) values) { }
        public JsonArray(IEnumerable<IJsonValue> values) => this.Elements = new List<IJsonValue>(values);
        public JsonArray(IEnumerator<IJsonValue> values) : this(values.ToEnumerable()) { }

        internal JsonArray(IEnumerable<IJsonToken> tokens) : this(tokens.GetEnumerator()) { }
        internal JsonArray(IEnumerator<IJsonToken> tokens)
        {
            // [ value *(, value) ]
            this.Elements = new List<IJsonValue>();
            if (tokens.MoveNext())
            {
                if (tokens.Current.Type != JsonTokenType.BeginArray)
                    throw new InvalidDataException();
                if (!tokens.MoveNext())
                    throw new InvalidDataException();
                if (tokens.Current.Type != JsonTokenType.EndArray)
                    do
                    {
                        var token = tokens.Current;
                        if (token.Type != JsonTokenType.Value)
                            throw new InvalidDataException();

                        this.Elements.Add((IJsonValue) token);

                        if (!tokens.MoveNext())
                            throw new InvalidDataException();
                        else if (tokens.Current.Type == JsonTokenType.EndArray)
                            break;
                        // disallow non value separator
                        else if (tokens.Current.Type != JsonTokenType.ValueSeparator)
                            throw new InvalidDataException();
                        // disallow trailing comma (value, ])
                        else if (!tokens.MoveNext())
                            throw new InvalidDataException();
                    } while (true);
            }
        }
        #endregion

        #region ToString
        public override string ToString() => this.ToJsonCompatibleString(false);
        public string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
        {
            var builder = new StringBuilder();
            IEnumerator<IJsonValue> enumerator = this.Elements.GetEnumerator();
            builder.Append(jsonBeginArray);
            if (enumerator.MoveNext())
            {
                builder.Append(enumerator.Current.ToJsonCompatibleString(escapeUnicodeCharacter));
                while (enumerator.MoveNext())
                {
                    builder.Append($"{jsonValueSeparator} {enumerator.Current.ToJsonCompatibleString(escapeUnicodeCharacter)}");
                }
            }
            builder.Append(jsonEndArray);
            return builder.ToString();
        }
        #endregion

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;

            var other = (JsonArray) obj;

            var thisEnumerator = this.GetEnumerator();
            var otherEnumerator = other.GetEnumerator();

            while (true)
            {
                bool thisMoveNext = thisEnumerator.MoveNext();
                if (thisMoveNext != otherEnumerator.MoveNext())
                    return false;
                if (!thisMoveNext)
                    break;
                if (!thisEnumerator.Current.Equals(otherEnumerator.Current))
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            int hashCode = 17;
            foreach (var element in this.Elements)
            {
                hashCode = hashCode * 31 + element.GetHashCode();
            }
            return hashCode;
        }
        #endregion

        public JsonArray DeepCopy()
        {
            var instance = new JsonArray(this.Elements);
            for (int i = 0; i < this.Elements.Count; i++)
                instance.Elements[i] = this.Elements[i];
            return instance;
        }

        #region implementation of interfaces
        JsonTokenType IJsonToken.Type => JsonTokenType.Value;
        JsonValueType IJsonValue.Type => JsonValueType.Array;
        public int Count => this.Elements.Count;
        public bool IsReadOnly => this.Elements.IsReadOnly;

        public IEnumerator<IJsonValue> GetEnumerator() => this.Elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public int IndexOf(IJsonValue item) => this.Elements.IndexOf(item);
        public void Insert(int index, IJsonValue item) => this.Elements.Insert(index, item);
        public void RemoveAt(int index) => this.Elements.RemoveAt(index);
        public void Add(IJsonValue item) => this.Elements.Add(item);
        public void Clear() => this.Elements.Clear();
        public bool Contains(IJsonValue item) => this.Elements.Contains(item);
        public void CopyTo(IJsonValue[] array, int arrayIndex) => this.Elements.CopyTo(array, arrayIndex);
        public bool Remove(IJsonValue item) => this.Elements.Remove(item);
        IJsonValue IDeepCopy<IJsonValue>.DeepCopy() => this.DeepCopy();
        #endregion
    }

    public static class JsonValueExtensions
    {
        public static T Cast<T>(this IJsonValue value) where T : IJsonValue
            => (T) value;
    }

}
