# qpJsonParser


## Overview

qpJsonParser is a JSON parser licensed under NYSL Version 0.9982, wrriten in C#. 


## Example

### Example 1

#### Code
```C#
using qpwakaba;
/*
    {
      "number": 334,
      "string": "foo",
      "boolean": true,
      "null": null,
      "array": [1.0, 0.2, -3E+4, -5E-6, "33-4"],
      "object": {
        "name": "qpJsonParser",
        "version": "1.0.0.0"
      }
    }
*/

string json = "{\"number\":334,\"string\":\"foo\",\"boolean\":true,\"null\":null,\"array\":[1.0,0.2,-3E+4,-5E-6,\"33-4\"],\"object\":{\"name\":\"qpJsonParser\",\"version\":\"1.0.0.0\"}}";

IJsonValue deserialized = JsonParser.Parse(json);
// Write "JsonParser.Parse(json, true)" to keep key order

foreach(var value in (JsonObject)deserialized)
{
  Console.WriteLine(value.ToString());
}
```

#### Output

```
[number, 334]
[string, foo]
[boolean, true]
[null, null]
[array, [1.0, 0.2, -3E+4, -5E-6, "33-4"]]
[object, {"name": "qpJsonParser", "version": "1.0.0.0"}]
```


### Example 2

#### Code

```C#
using qpwakaba;
/*
    {
      "number": 334,
      "string": "foo",
      "boolean": true,
      "null": null,
      "array": [1.0, 0.2, -3E+4, -5E-6, "33-4"],
      "object": {
        "name": "qpJsonParser",
        "version": "1.0.0.0"
      }
    }
*/

string json = "{\"number\":334,\"string\":\"foo\",\"boolean\":true,\"null\":null,\"array\":[1.0,0.2,-3E+4,-5E-6,\"33-4\"],\"object\":{\"name\":\"qpJsonParser\",\"version\":\"1.0.0.0\"}}";

JsonObject deserialized = (JsonObject)JsonParser.Parse(json);

//IJsonObject.Cast<T> is a extension method.
var number = deserialized["number"].Cast<JsonNumber>();

double doubleValue = number.DoubleValue; // 334.0
int intValue = number.IntegerValue; // 334

var _string = deserialized["string"]; // "foo" (JsonString)

var boolean = deserialized["boolean"]; // true (JsonBoolean)

var _null = deserialized["null"]; // null (JsonNull)

var array = deserialized["array"]; // (JsonArray)
// ((JsonArray) array)[0] == 1.0 (JsonNumber)
// ((JsonArray) array)[1] == 0.2 (JsonNumber)
// ((JsonArray) array)[2] == -3E+4 (JsonNumber)
// ((JsonArray) array)[3] == -5E-6 (JsonNumber)
// ((JsonArray) array)[4] == "33-4" (JsonString)

var _object = deserialized["object"]; // (JsonObject)
```

### Example 3

#### Code

```C#
using qpwakaba;
/*
    {
      "number": 334,
      "string": "foo",
      "boolean": true,
      "null": null,
      "array": [1.0, 0.2, -3E+4, -5E-6, "33-4"],
      "object": {
        "name": "qpJsonParser",
        "version": "1.0.0.0"
      }
    }
*/

string json = "{\"number\":334,\"string\":\"foo\",\"boolean\":true,\"null\":null,\"array\":[1.0,0.2,-3E+4,-5E-6,\"33-4\"],\"object\":{\"name\":\"qpJsonParser\",\"version\":\"1.0.0.0\"}}";

JsonObject deserialized = JsonParser.Parse(json).Cast<JsonObject>();

string serialized = deserialized.ToJsonCompatibleString();
// Write "ToJsonCompatibleString(true)" to escape unicode characters.

Console.WriteLine(serialized);
```

#### Output

```
{"number": 334, "string": "foo", "boolean": true, "null": null, "array": [1.0, 0.2, -3E+4, -5E-6, "33-4"], "object": {"name": "qpJsonParser", "version": "1.0.0.0"}}
```


## Author

[@qpwakaba](https://twitter.com/qpwakaba)


## License

[NYSL Version 0.9982](http://www.kmonos.net/nysl/)

