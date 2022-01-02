using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using qpwakaba.Utils;

using static qpwakaba.JsonParser;
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
    public abstract class JsonValue : DynamicObject, IDeepCopy<JsonValue>
    {
        public abstract JsonValueType Type { get; }
        public abstract override string ToString();
        public abstract string ToJsonCompatibleString(bool escapeUnicodeCharacter = false);
        public abstract JsonValue DeepCopy();

        internal static bool TryToJsonValue(Array arr, [NotNullWhen(true)] out JsonArray? ja)
        {
            JsonValue[] buf = new JsonValue[arr.Length];
            int i = 0;
            foreach (var e in arr)
            {
                if (!TryToJsonValue(e, out buf[i++]!))
                {
                    ja = default;
                    return false;
                }
            }
            ja = new JsonArray(buf);
            return true;
        }
        internal static bool TryToJsonValue(IList arr, [NotNullWhen(true)] out JsonArray? ja)
        {
            JsonValue[] buf = new JsonValue[arr.Count];
            int i = 0;
            foreach (var e in arr)
            {
                if (!TryToJsonValue(e, out buf[i++]!))
                {
                    ja = default;
                    return false;
                }
            }
            ja = new JsonArray(buf);
            return true;
        }
        internal static bool TryToJsonValue(IEnumerable enumerable, [NotNullWhen(true)] out JsonArray? ja)
        {
            ja = new();
            foreach (var e in enumerable)
            {
                if (!TryToJsonValue(e, out JsonValue? jv))
                    return false;
                ja.Add(jv);
            }
            return true;
        }
        internal static bool TryToJsonValue(object obj, [NotNullWhen(true)] out JsonObject? jo)
        {
            const BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = obj.GetType().GetFields(flag);
            var props = obj.GetType().GetProperties(flag);

            jo = new();
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<NonSerializedAttribute>() is not null)
                    continue;
                if (!prop.CanWrite)
                    continue;
                if (prop.GetIndexParameters().Length > 0)
                    continue;
                if (!TryToJsonValue(prop.GetValue(obj), out JsonValue? jv))
                    return false;
                jo.Add(prop.Name, jv);
            }
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<NonSerializedAttribute>() is not null)
                    continue;
                if (field.GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
                    continue;
                if (!TryToJsonValue(field.GetValue(obj), out JsonValue? jv))
                    return false;
                jo.Add(field.Name, jv);
            }
            return true;
        }
        internal static bool TryToJsonValue(IDictionary dic, [NotNullWhen(true)] out JsonObject? jo)
        {
            var enumerator = dic.GetEnumerator();
            jo = new();
            while (enumerator.MoveNext())
            {
                if (!TryToJsonValue(enumerator.Value, out JsonValue? jv))
                    return false;
                switch (enumerator.Key)
                {
                    case string s:
                        jo.Add(s, jv);
                        continue;
                    case JsonString js:
                        jo.Add(js.Value, jv);
                        continue;
                    default:
                        return false;
                }
            }
            return true;
        }
        [return: NotNullIfNotNull("obj")]
        public static JsonValue? ToJsonValue<T>(T obj)
        {
            if (TryToJsonValue(obj, out JsonValue? value)) return value;
            else throw new ArgumentException();
        }
        public static bool TryToJsonValue<T>(T? obj, out JsonValue? value)
            where T : struct
        {
            switch (obj)
            {
                case int i:
                    value = new JsonNumber(i);
                    return true;
                case float f:
                    value = new JsonNumber(f);
                    return true;
                case double d:
                    value = new JsonNumber(d);
                    return true;
                case long l:
                    value = new JsonNumber(l);
                    return true;
                case uint i:
                    value = new JsonNumber(i);
                    return true;
                case ulong l:
                    value = new JsonNumber(l);
                    return true;
                case decimal d:
                    value = new JsonNumber(d);
                    return true;
                case null:
                    value = null;
                    return true;
                default:
                {
                    bool result = TryToJsonValue(obj, out JsonObject? jo);
                    value = jo;
                    return result;
                }
            }
        }
        public static bool TryToJsonValue<T>(T? obj, out JsonValue? value)
            where T : class
        {
            switch (obj)
            {
                case string str:
                    value = new JsonString(str);
                    return true;
                case Array arr:
                {
                    bool result = TryToJsonValue(arr, out JsonArray? ja);
                    value = ja;
                    return result;
                }
                case IList lst:
                {
                    bool result = TryToJsonValue(lst, out JsonArray? ja);
                    value = ja;
                    return result;
                }
                case null:
                    value = null;
                    return true;
                case JsonValue jv:
                    value = jv;
                    return true;
                default:
                {
                    bool result = TryToJsonValue(obj, out JsonObject? jo);
                    value = jo;
                    return result;
                }
            }
        }
        public static bool TryToJsonValue(object? obj, out JsonValue? value)
        {
            switch (obj)
            {
                case int i:
                    value = new JsonNumber(i);
                    return true;
                case float f:
                    value = new JsonNumber(f);
                    return true;
                case double d:
                    value = new JsonNumber(d);
                    return true;
                case long l:
                    value = new JsonNumber(l);
                    return true;
                case string str:
                    value = new JsonString(str);
                    return true;
                case uint i:
                    value = new JsonNumber(i);
                    return true;
                case ulong l:
                    value = new JsonNumber(l);
                    return true;
                case byte b:
                    value = new JsonNumber(b);
                    return true;
                case sbyte b:
                    value = new JsonNumber(b);
                    return true;
                case short s:
                    value = new JsonNumber(s);
                    return true;
                case ushort s:
                    value = new JsonNumber(s);
                    return true;
                case decimal d:
                    value = new JsonNumber(d);
                    return true;
                case Array arr:
                {
                    bool result = TryToJsonValue(arr, out JsonArray? ja);
                    value = ja;
                    return result;
                }
                case bool b:
                    value = new JsonBoolean(b);
                    return true;
                case null:
                    value = null;
                    return true;
                case JsonValue jv:
                    value = jv;
                    return true;
                default:
                {
                    var type = obj.GetType();
                    if (type.IsGenericType) {
                        if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                            type.GenericTypeArguments[0] == typeof(string))
                        {
                            value = new JsonObject(ToObject((IDictionary)obj));
                            return true;
                        }
                        else if (type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            value = new JsonArray(ToArray((IEnumerable)obj));
                            return true;
                        }
                    }
                    bool result = TryToJsonValue(obj, out JsonObject? jo);
                    value = jo;
                    return result;
                }
            }

            IEnumerator<KeyValuePair<string, JsonValue?>> ToObject(IDictionary dict)
            {
                var enumerator = dict.GetEnumerator();
                while (enumerator.MoveNext())
                    yield return new((string)enumerator.Key, ToJsonValue(enumerator.Value));
            }

            IEnumerable<JsonValue?> ToArray(IEnumerable list)
                => list.Cast<object>().Select(ToJsonValue);
        }
        public T ToObject<T>() => (T) ToObject(typeof(T));

        public abstract object ToObject(Type type);
        //public object ToObject(Type type)
        //{
        //    if (type == typeof(int))
        //    {
        //    }
        //    switch (obj)
        //    {
        //        case int i:
        //            value = new JsonNumber(i);
        //            return true;
        //        case float f:
        //            value = new JsonNumber(f);
        //            return true;
        //        case double d:
        //            value = new JsonNumber(d);
        //            return true;
        //        case long l:
        //            value = new JsonNumber(l);
        //            return true;
        //        case string str:
        //            value = new JsonString(str);
        //            return true;
        //        case uint i:
        //            value = new JsonNumber(i);
        //            return true;
        //        case ulong l:
        //            value = new JsonNumber(l);
        //            return true;
        //        case decimal d:
        //            value = new JsonNumber(d);
        //            return true;
        //        case Array arr:
        //        {
        //            bool result = TryToJsonValue(arr, out JsonArray? ja);
        //            value = ja;
        //            return result;
        //        }
        //        case IList lst:
        //        {
        //            bool result = TryToJsonValue(lst, out JsonArray? ja);
        //            value = ja;
        //            return result;
        //        }
        //        case bool b:
        //            value = new JsonBoolean(b);
        //            return true;
        //        case null:
        //            value = null;
        //            return true;
        //        case JsonValue jv:
        //            value = jv;
        //            return true;
        //        default:
        //        {
        //            bool result = TryToJsonValue(obj, out JsonObject? jo);
        //            value = jo;
        //            return result;
        //        }
        //    }
            
        //    const BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        //    var fields = type.GetFields(flag);
        //    var props = type.GetProperties(flag);


        //    foreach (var prop in props)
        //    {
        //        if (prop.GetCustomAttribute<NonSerializedAttribute>() is not null)
        //            continue;
        //        prop.SetValue(instance, );
        //        if (!TryToJsonValue(prop.GetValue(obj), out JsonValue? jv))
        //            return false;
        //        jo.Add(prop.Name, jv);
        //    }
        //    foreach (var field in fields)
        //    {
        //        if (field.GetCustomAttribute<NonSerializedAttribute>() is not null)
        //            continue;
        //        if (field.GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
        //            continue;
        //        if (!TryToJsonValue(field.GetValue(obj), out JsonValue? jv))
        //            return false;
        //        jo.Add(field.Name, jv);
        //    }
        //}
    }
    public class JsonBoolean : JsonValue, IComparable, IComparable<bool>, IComparable<JsonBoolean>,
        IConvertible, IEquatable<bool>, IEquatable<JsonBoolean>
    {
        public bool Value { get; }
        #region constructors
        public JsonBoolean(bool boolean) => this.Value = boolean;
        #endregion
        #region Equals
        public override bool Equals(object? obj)
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
        public override string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
            => ToString();

        public override object ToObject(Type type)
        {
            if (type != typeof(bool))
                throw new ArgumentException();
            return this.Value;
        }
        #region implementation of interfaces
        public override JsonValueType Type => JsonValueType.Literal;
        public int CompareTo(object? obj) => this.Value.CompareTo(obj);
        public int CompareTo(bool other) => this.Value.CompareTo(other);
        TypeCode IConvertible.GetTypeCode() => TypeCode.Boolean;
        bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible) this.Value).ToBoolean(provider);
        byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible) this.Value).ToByte(provider);
        char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible) this.Value).ToChar(provider);
        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible) this.Value).ToDateTime(provider);
        decimal IConvertible.ToDecimal(IFormatProvider? provider) => ((IConvertible) this.Value).ToDecimal(provider);
        double IConvertible.ToDouble(IFormatProvider? provider) => ((IConvertible) this.Value).ToDouble(provider);
        short IConvertible.ToInt16(IFormatProvider? provider) => ((IConvertible) this.Value).ToInt16(provider);
        int IConvertible.ToInt32(IFormatProvider? provider) => ((IConvertible) this.Value).ToInt32(provider);
        long IConvertible.ToInt64(IFormatProvider? provider) => ((IConvertible) this.Value).ToInt64(provider);
        sbyte IConvertible.ToSByte(IFormatProvider? provider) => ((IConvertible) this.Value).ToSByte(provider);
        float IConvertible.ToSingle(IFormatProvider? provider) => ((IConvertible) this.Value).ToSingle(provider);
        string IConvertible.ToString(IFormatProvider? provider) => this.Value.ToString(provider);
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible) this.Value).ToType(conversionType, provider);
        ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible) this.Value).ToUInt16(provider);
        uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible) this.Value).ToUInt32(provider);
        ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible) this.Value).ToUInt64(provider);
        public bool Equals(bool other) => this.Value.Equals(other);
        public bool Equals(JsonBoolean? other) => other is not null ? Equals(other.Value) : false;
        public int CompareTo(JsonBoolean? other) => other is not null ?CompareTo(other.Value) : 1;
        public override JsonValue DeepCopy() => new JsonBoolean(this.Value);
        #endregion
    }
    public class JsonNumber : JsonValue, IComparable, IComparable<int>, IComparable<long>, IComparable<uint>,
        IComparable<ulong>, IComparable<float>, IComparable<double>, IComparable<decimal>, IComparable<JsonNumber>,
        IEquatable<int>, IEquatable<long>, IEquatable<uint>, IEquatable<ulong>, IEquatable<float>,
        IEquatable<double>, IEquatable<decimal>, IEquatable<JsonNumber>, IFormattable, IConvertible
    {
        #region constructors
        public JsonNumber(string value)
        {
            if (!JsonParser.IsValidNumber(value))
                throw new ArgumentException($"{value} is not a valid number.");
            this.StringValue = value;
        }
        public JsonNumber(float value)
        {
            this.FloatValue = value;
            this.StringValue = value.ToString("r");
        }

        public JsonNumber(double value)
        {
            this.DoubleValue = value;
            this.StringValue = value.ToString("r");
        }

        public JsonNumber(int value)
        {
            this.IntegerValue = value;
            this.StringValue = value.ToString();
        }

        public JsonNumber(long value)
        {
            this.LongValue = value;
            this.StringValue = value.ToString();
        }

        public JsonNumber(uint value)
        {
            this.UIntValue = value;
            this.StringValue = value.ToString();
        }
        public JsonNumber(ulong value)
        {
            this.ULongValue = value;
            this.StringValue = value.ToString();
        }
        public JsonNumber(decimal value)
        {
            this.DecimalValue = value;
            this.StringValue = value.ToString();
        }
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
        public override string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
            => ToString();

        public override object ToObject(Type type)
        {
            if (type == typeof(int)) return IntegerValue;
            if (type == typeof(double)) return DoubleValue;
            if (type == typeof(long)) return LongValue;
            if (type == typeof(float)) return FloatValue;
            if (type == typeof(ulong)) return ULongValue;
            if (type == typeof(uint)) return UIntValue;
            if (type == typeof(byte)) return (byte) IntegerValue;
            if (type == typeof(sbyte)) return (sbyte) IntegerValue;
            if (type == typeof(short)) return (short) IntegerValue;
            if (type == typeof(ushort)) return (ushort) IntegerValue;
            if (type == typeof(decimal)) return DecimalValue;
            if (type == typeof(string)) return StringValue;
            throw new ArgumentException();
        }

        #region Equals
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;

            var other = (JsonNumber) obj;

            return this.Equals(other);
        }
        public bool Equals(JsonNumber? other) => other is not null && Equals(this.StringValue, other.StringValue);
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
        public override JsonValueType Type => JsonValueType.Number;
        public int CompareTo(object? obj) => this.DecimalValue.CompareTo(obj);
        public int CompareTo(int other) => this.IntegerValue.CompareTo(other);
        public int CompareTo(long other) => this.LongValue.CompareTo(other);
        public int CompareTo(float other) => this.FloatValue.CompareTo(other);
        public int CompareTo(double other) => this.DoubleValue.CompareTo(other);
        public int CompareTo(uint other) => this.UIntValue.CompareTo(other);
        public int CompareTo(ulong other) => this.ULongValue.CompareTo(other);
        public int CompareTo(decimal other) => this.DecimalValue.CompareTo(other);
        public string ToString(string? format, IFormatProvider? formatProvider) => this.DecimalValue.ToString(format, formatProvider);
        public TypeCode GetTypeCode() => TypeCode.Object;
        bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible) this.StringValue).ToBoolean(provider);
        byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible) this.StringValue).ToByte(provider);
        char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible) this.StringValue).ToChar(provider);
        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible) this.StringValue).ToDateTime(provider);
        decimal IConvertible.ToDecimal(IFormatProvider? provider) => this.DecimalValue;
        double IConvertible.ToDouble(IFormatProvider? provider) => this.DoubleValue;
        short IConvertible.ToInt16(IFormatProvider? provider) => (short) this.IntegerValue;
        int IConvertible.ToInt32(IFormatProvider? provider) => this.IntegerValue;
        long IConvertible.ToInt64(IFormatProvider? provider) => this.LongValue;
        sbyte IConvertible.ToSByte(IFormatProvider? provider) => (sbyte) this.IntegerValue;
        float IConvertible.ToSingle(IFormatProvider? provider) => this.FloatValue;
        string IConvertible.ToString(IFormatProvider? provider) => this.StringValue.ToString(provider);
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible) this.StringValue).ToType(conversionType, provider);
        ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible) this.StringValue).ToUInt16(provider);
        uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible) this.StringValue).ToUInt32(provider);
        ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible) this.StringValue).ToUInt64(provider);
        public bool Equals(int other) => this.IntegerValue.Equals(other);
        public bool Equals(long other) => this.LongValue.Equals(other);
        public bool Equals(uint other) => this.UIntValue.Equals(other);
        public bool Equals(ulong other) => this.ULongValue.Equals(other);
        public bool Equals(float other) => this.FloatValue.Equals(other);
        public bool Equals(double other) => this.DoubleValue.Equals(other);
        public bool Equals(decimal other) => this.DecimalValue.Equals(other);
        public int CompareTo(JsonNumber? other) => other is not null ? this.CompareTo(other.DecimalValue) : 1;
        public override JsonValue DeepCopy() => new JsonNumber(this.StringValue);
        #endregion
    }
    public class JsonString : JsonValue, IComparable, IConvertible, IEnumerable,
        IComparable<string>, IComparable<JsonString>, IEquatable<string>, IEquatable<JsonString>, IEnumerable<char>
    {
        public string Value { get; set; }
        #region constructors
        public JsonString(string String) => this.Value = String;
        public JsonString(JsonString jsonString) => this.Value = jsonString.Value;
        #endregion
        public override string ToString() => this.Value;
        public override string ToJsonCompatibleString(bool escapeUnicodeString = false)
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
        public override object ToObject(Type type)
        {
            if (type == typeof(string)) return this.Value;
            if (type == typeof(int)) return int.Parse(this.Value);
            if (type == typeof(uint)) return uint.Parse(this.Value);
            if (type == typeof(long)) return long.Parse(this.Value);
            if (type == typeof(ulong)) return ulong.Parse(this.Value);
            if (type == typeof(byte)) return byte.Parse(this.Value);
            if (type == typeof(sbyte)) return sbyte.Parse(this.Value);
            if (type == typeof(short)) return short.Parse(this.Value);
            if (type == typeof(ushort)) return ushort.Parse(this.Value);
            if (type.IsEnum) return Enum.Parse(type, this.Value);
            throw new ArgumentException(type.ToString());
        }

        #region Equals
        public override bool Equals(object? obj)
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
        public override JsonValueType Type => JsonValueType.String;
        public int CompareTo(object? obj) => this.Value.CompareTo(obj);
        public object Clone() => new JsonString(this.Value);
        TypeCode IConvertible.GetTypeCode() => this.Value.GetTypeCode();
        bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible) this.Value).ToBoolean(provider);
        byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible) this.Value).ToByte(provider);
        char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible) this.Value).ToChar(provider);
        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible) this.Value).ToDateTime(provider);
        decimal IConvertible.ToDecimal(IFormatProvider? provider) => ((IConvertible) this.Value).ToDecimal(provider);
        double IConvertible.ToDouble(IFormatProvider? provider) => ((IConvertible) this.Value).ToDouble(provider);
        short IConvertible.ToInt16(IFormatProvider? provider) => ((IConvertible) this.Value).ToInt16(provider);
        int IConvertible.ToInt32(IFormatProvider? provider) => ((IConvertible) this.Value).ToInt32(provider);
        long IConvertible.ToInt64(IFormatProvider? provider) => ((IConvertible) this.Value).ToInt64(provider);
        sbyte IConvertible.ToSByte(IFormatProvider? provider) => ((IConvertible) this.Value).ToSByte(provider);
        float IConvertible.ToSingle(IFormatProvider? provider) => ((IConvertible) this.Value).ToSingle(provider);
        string IConvertible.ToString(IFormatProvider? provider) => this.Value;
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible) this.Value).ToType(conversionType, provider);
        ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible) this.Value).ToUInt16(provider);
        uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible) this.Value).ToUInt32(provider);
        ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible) this.Value).ToUInt64(provider);
        public IEnumerator GetEnumerator() => ((IEnumerable) this.Value).GetEnumerator();
        public int CompareTo(string? other) => this.Value.CompareTo(other);
        public bool Equals(string? other) => this.Value.Equals(other);
        IEnumerator<char> IEnumerable<char>.GetEnumerator() => ((IEnumerable<char>) this.Value).GetEnumerator();
        public int CompareTo(JsonString? other) => other is not null ? this.CompareTo(other.Value) : 1;
        public bool Equals(JsonString? other) => other is not null && this.Equals(other.Value);
        public override JsonValue DeepCopy() => new JsonString(this);
        #endregion

        #region Parse
        public static JsonString Parse(string escapedString)
        {
            if (TryParse(escapedString, out JsonString? result))
            {
                return result;
            }
            throw new InvalidDataException();
        }
        public static bool TryParse(string escapedString, [NotNullWhen(true)] out JsonString? jsonString)
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
    public class JsonObject : JsonValue, IDictionary<string, JsonValue?>, IEnumerable<KeyValuePair<string, JsonValue?>>, IEnumerable
    {
        public IDictionary<string, JsonValue?> Parameters { get; }
        public JsonValue? this[string key]
        {
            get => this.Parameters.ContainsKey(key) ? this.Parameters[key] : null;
            set => this.Parameters[key] = value;
        }

        #region constructors
        public JsonObject(params KeyValuePair<string, JsonValue?>[] values)
            : this(values, false) { }
        public JsonObject(bool keepOrder, params KeyValuePair<string, JsonValue?>[] values)
            : this(values, keepOrder) { }
        public JsonObject(bool keepOrder = false)
        {
            this.Parameters = CreateDictionary<string, JsonValue?>(keepOrder);
        }
        public JsonObject(IEnumerable<KeyValuePair<string, JsonValue?>> values, bool keepOrder = false)
            : this(values.GetEnumerator(), keepOrder)
        {
        }
        public JsonObject(IEnumerator<KeyValuePair<string, JsonValue?>> values, bool keepOrder = false)
            : this(keepOrder)
        {
            while (values.MoveNext())
            {
                this.Parameters[values.Current.Key] = values.Current.Value;
            }
        }
        #endregion

        #region ToString
        public override string ToString() => this.ToJsonCompatibleString(false);
        public override string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
        {
            var builder = new StringBuilder();
            IEnumerator<KeyValuePair<string, JsonValue?>> enumerator = this.Parameters.GetEnumerator();
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

        private static string ToString(KeyValuePair<string, JsonValue?> pair, bool escapeUnicodeCharacter)
            => $"{new JsonString(pair.Key).ToJsonCompatibleString(escapeUnicodeCharacter)}{jsonNameSeparator} {pair.Value?.ToJsonCompatibleString(escapeUnicodeCharacter) ?? jsonNull}";
        #endregion

        #region Equals
        public override bool Equals(object? obj)
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
                if (thisValue is null && otherValue is null)
                    continue;
                if (thisValue is null || otherValue is null)
                    return false;
                if (!thisValue.Equals(otherValue))
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            int hashCode = 17;
            var sortedParameters = new List<KeyValuePair<string, JsonValue?>>(this.Parameters);
            sortedParameters.Sort(KeyComparator.Instance);
            foreach (var parameter in this.Parameters)
            {
                hashCode = hashCode * 31 + (parameter.Key.GetHashCode() ^ parameter.Value?.GetHashCode() ?? 0);
            }
            return hashCode;
        }

        private class KeyComparator : IComparer<KeyValuePair<string, JsonValue?>>
        {
            public static KeyComparator Instance { get; }
            static KeyComparator()
            {
                Instance = new KeyComparator();
            }
            private KeyComparator() { }

            public int Compare(KeyValuePair<string, JsonValue?> x, KeyValuePair<string, JsonValue?> y)
                => string.Compare(x.Key, y.Key);
        }
        #endregion

        public override JsonValue DeepCopy()
        {
            bool keepOrder = this.Parameters.GetType() == typeof(OrderedDictionary<string, JsonValue>);
            var instance = new JsonObject(this.Parameters, keepOrder);
            foreach (var parameter in this.Parameters)
                instance.Parameters[parameter.Key] = parameter.Value?.DeepCopy();
            return instance;
        }

        public override object ToObject(Type type)
        {
            const BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                object?[] param = new object?[2];
                object dict = Activator.CreateInstance(type) ?? throw new Exception();
                var set = type.GetMethod(nameof(Dictionary<object, object>.Add)) ?? throw new Exception();
                foreach ((var key, var value) in this)
                {
                    param[0] = new JsonString(key).ToObject(type.GenericTypeArguments[0]);
                    param[1] = value?.ToObject(type.GenericTypeArguments[1]);
                    set.Invoke(dict, param);
                }
                return dict;
            }
            else
            {
                var fields = type.GetFields(flag);
                var props = type.GetProperties(flag);
                var obj = Activator.CreateInstance(type) ?? throw new Exception();
                foreach (var prop in props)
                {
                    if (!this.ContainsKey(prop.Name))
                        continue;
                    if (prop.GetCustomAttribute<NonSerializedAttribute>() is not null)
                        continue;
                    if (!prop.CanWrite)
                        continue;
                    if (prop.GetIndexParameters().Length > 0)
                        continue;
                    prop.SetValue(obj, this[prop.Name]?.ToObject(prop.PropertyType));
                }
                foreach (var field in fields)
                {
                    if (!this.ContainsKey(field.Name))
                        continue;
                    if (field.GetCustomAttribute<NonSerializedAttribute>() is not null)
                        continue;
                    if (field.GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
                        continue;
                    field.SetValue(obj, this[field.Name]?.ToObject(field.FieldType));
                }
                return obj;
            }

        }

        #region implementation of interfaces
        public override JsonValueType Type => JsonValueType.Object;
        public ICollection<string> Keys => this.Parameters.Keys;
        public ICollection<JsonValue?> Values => this.Parameters.Values;
        public int Count => this.Parameters.Count;
        public bool IsReadOnly => this.Parameters.IsReadOnly;
        public IEnumerator<KeyValuePair<string, JsonValue?>> GetEnumerator() => this.Parameters.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public void Add(string key, JsonValue? value) => this.Parameters.Add(key, value);
        public bool ContainsKey(string key) => this.Parameters.ContainsKey(key);
        public bool Remove(string key) => this.Parameters.Remove(key);
        public bool TryGetValue(string key, out JsonValue? value) => this.Parameters.TryGetValue(key, out value);
        public void Add(KeyValuePair<string, JsonValue?> item) => this.Parameters.Add(item);
        public void Clear() => this.Parameters.Clear();
        public bool Contains(KeyValuePair<string, JsonValue?> item) => this.Parameters.Contains(item);
        public void CopyTo(KeyValuePair<string, JsonValue?>[] array, int arrayIndex) => this.Parameters.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, JsonValue?> item) => this.Parameters.Remove(item);
        #endregion

        #region Dynamic
        public override IEnumerable<string> GetDynamicMemberNames() => this.Parameters.Keys;
        public override bool TryDeleteMember(DeleteMemberBinder binder)
            => this.Parameters.Remove(binder.Name);
        private bool TryGetMember(string name, bool ignoreCase, out object? result)
        {
            string? lookupName = name;
            if (!this.Parameters.ContainsKey(lookupName))
            {
                if (!ignoreCase) goto NOT_FOUND;

                lookupName = this.Parameters.Keys
                    .FirstOrDefault(s => s.ToLower(CultureInfo.InvariantCulture) == name.ToLower(CultureInfo.InvariantCulture));
                if (lookupName is null)
                    goto NOT_FOUND;
            }

            result = this.Parameters[lookupName];
            return true;
NOT_FOUND:
            result = null;
            return false;
        }
        private bool TrySetMember(string name, bool ignoreCase, object? value)
        {
            if (!TryToJsonValue(value, out JsonValue? jsonValue))
                return false;
            string? lookupName = name;
            if (ignoreCase)
            {
                if (!this.Parameters.ContainsKey(lookupName))
                {
                    lookupName = this.Parameters.Keys
                        .FirstOrDefault(s => s.ToLower(CultureInfo.InvariantCulture) == name.ToLower(CultureInfo.InvariantCulture)) ?? name;
                }
            }

            this.Parameters[lookupName] = jsonValue;
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
            => TryGetMember(binder.Name, binder.IgnoreCase, out result);
        public override bool TrySetMember(SetMemberBinder binder, object? value)
            => TrySetMember(binder.Name, binder.IgnoreCase, value);
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
            => TryGetMember((string) indexes[0], false, out result);
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
            => TrySetMember((string) indexes[0], false, value);
        #endregion
        private static IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(bool keepOrder)
            where TKey : notnull
            => keepOrder ? (IDictionary<TKey, TValue>) new OrderedDictionary<TKey, TValue>() : new Dictionary<TKey, TValue>();
        private static IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(int capacity, bool keepOrder)
            where TKey : notnull
            => keepOrder ? (IDictionary<TKey, TValue>) new OrderedDictionary<TKey, TValue>(capacity) : new Dictionary<TKey, TValue>(capacity);
    }
    public class JsonArray : JsonValue, IList<JsonValue?>, IEnumerable<JsonValue?>, IEnumerable
    {
        public IList<JsonValue?> Elements { get; }
        public JsonValue? this[int index] { get => this.Elements[index]; set => this.Elements[index] = value; }

        #region constructors
        public JsonArray() => this.Elements = new List<JsonValue?>();
        public JsonArray(params JsonValue?[] values) : this((IEnumerable<JsonValue?>) values) { }
        public JsonArray(IEnumerable<JsonValue?> values) => this.Elements = new List<JsonValue?>(values);

        #endregion

        #region ToString
        public override string ToString() => this.ToJsonCompatibleString(false);
        public override string ToJsonCompatibleString(bool escapeUnicodeCharacter = false)
        {
            var builder = new StringBuilder();
            IEnumerator<JsonValue?> enumerator = this.Elements.GetEnumerator();
            builder.Append(jsonBeginArray);
            if (enumerator.MoveNext())
            {
                builder.Append(enumerator.Current?.ToJsonCompatibleString(escapeUnicodeCharacter) ?? jsonNull);
                while (enumerator.MoveNext())
                {
                    builder.Append($"{jsonValueSeparator} {enumerator.Current?.ToJsonCompatibleString(escapeUnicodeCharacter) ?? jsonNull}");
                }
            }
            builder.Append(jsonEndArray);
            return builder.ToString();
        }
        #endregion

        #region Equals
        public override bool Equals(object? obj)
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
                if (thisEnumerator.Current is null && otherEnumerator.Current is null)
                    continue;
                if (thisEnumerator.Current is null || otherEnumerator.Current is null)
                    return false;
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
                hashCode = hashCode * 31 + element?.GetHashCode() ?? 0;
            }
            return hashCode;
        }
        #endregion

        public override JsonValue DeepCopy()
        {
            var instance = new JsonArray(this.Elements);
            for (int i = 0; i < this.Elements.Count; i++)
                instance.Elements[i] = this.Elements[i];
            return instance;
        }

        public override object ToObject(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var cnt = this.Count;
                var etype = type.GenericTypeArguments[0];
                var list = Activator.CreateInstance(type, cnt) ?? throw new Exception();
                var add = type.GetMethod(nameof(List<object>.Add)) ?? throw new Exception();
                object?[] param = new object?[1];
                foreach (var v in this)
                {
                    param[0] = v?.ToObject(etype);
                    add.Invoke(list, param);
                }
                return list;
            }
            else if (type.IsArray)
            {
                var etype = type.GetElementType() ?? throw new Exception();
                var arr = Array.CreateInstance(etype, this.Count);
                for (int i = 0; i < arr.Length; ++i)
                    arr.SetValue(this[i]?.ToObject(etype), i);
                return arr;
            }
            throw new ArgumentException();
        }

        #region implementation of interfaces
        public override JsonValueType Type => JsonValueType.Array;
        public int Count => this.Elements.Count;
        public bool IsReadOnly => this.Elements.IsReadOnly;

        public IEnumerator<JsonValue?> GetEnumerator() => this.Elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public int IndexOf(JsonValue? item) => this.Elements.IndexOf(item);
        public void Insert(int index, JsonValue? item) => this.Elements.Insert(index, item);
        public void RemoveAt(int index) => this.Elements.RemoveAt(index);
        public void Add(JsonValue? item) => this.Elements.Add(item);
        public void Clear() => this.Elements.Clear();
        public bool Contains(JsonValue? item) => this.Elements.Contains(item);
        public void CopyTo(JsonValue?[] array, int arrayIndex) => this.Elements.CopyTo(array, arrayIndex);
        public bool Remove(JsonValue? item) => this.Elements.Remove(item);
        #endregion
        #region Dynamic
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
        {
            uint index = (uint) indexes[0];
            if (index < this.Elements.Count)
            {
                result = this.Elements[(int) indexes[0]];
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
        {
            int index = (int)indexes[0];
            if ((uint)index >= this.Elements.Count)
                return false;
            if (TryToJsonValue(value, out JsonValue? jsonValue))
            {
                this.Elements[index] = jsonValue;
                return true;
            }
            else
                return false;
        }
        #endregion
    }

    public static class JsonValueExtensions
    {
        public static T? Cast<T>(this JsonValue? value) where T : JsonValue?
            => (T?) value;
        [return: NotNullIfNotNull("obj")]
        public static dynamic? AsDynamic(this object? obj) => obj;
    }

}
