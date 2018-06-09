using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qpwakaba.Utils;

namespace qpwakaba.Tests
{
    [TestClass]
    public class JsonParserTest
    {
        [TestMethod]
        public void ValidationTest()
        {
            #region validation test
            InvalidJson("");
            InvalidJson("0");
            InvalidJson("0.5");
            InvalidJson("true");
            InvalidJson("false");
            InvalidJson("null");
            InvalidJson("{");
            InvalidJson("}");
            InvalidJson("{{}");
            InvalidJson("{[}");
            InvalidJson("[{]");
            InvalidJson("{]]");
            InvalidJson("][");
            InvalidJson("}{");

            ValidJson("{}");
            ValidJson("[]");
            ValidJson("[0.0]");
            ValidJson("[0.0, 0.1]");
            InvalidJson("[0.0,]");
            ValidJson("{\"out\": \"safe\"}");
            InvalidJson("{out: \"safe\"}");
            InvalidJson("{true: \"true\"}");
            InvalidJson("{false: \"false\"}");
            InvalidJson("{null: \"null\"}");
            InvalidJson("{0.5: \"0.5\"}");
            InvalidJson("[\name\": \"value\"]");
            ValidJson("{\"1\": \"value1\", \"2\": \"value2\"}");
            InvalidJson("{\"1\": \"value1\", \"2\": \"value2\", }");
            InvalidJson("{\"1\": \"value1\", \"2\": }");
            InvalidJson("{\"1\": , \"2\": \"value2\"}");
            ValidJson("{\"1\": 0.5, \"2\": -1.0E6}");
            ValidJson("{\"1\":0.5, \"2\":-1.0E6}");
            ValidJson("{\"1\":0.5, \"2\":-1.0E+6}");
            ValidJson("{\"1\":0.5, \"2\":-1.0E-6}");
            InvalidJson("{\"1\":0.5, \"2\":+1.0E6}");
            InvalidJson("{\"1\":0.5, \"2\":+1.0E-6}");

            ValidJson("[{}]");
            InvalidJson("{{}}");
            ValidJson("[[]]");
            ValidJson("[{}, {}]");
            ValidJson("[[], {}]");
            ValidJson("{\"key1\": {\"key\": \"name\"}}");
            ValidJson("{\"key1\": {\"key\": \"name\"}, \"key2\": {\"key\": \"name\"}}");
            ValidJson("{\"key1\": {\"key\": \"name\"}, \"key1\": {\"key\": \"name\"}}");
            ValidJson("{\"key1\": [[], {\"key2\": null}]}");

            #region InvalidJson("{\n  \"key1\": {\n    \"key\": \"name\" //it is a comment\n  },\n  \"key2\": {\n    \"key\": \"name\"\n  }\n}");
            /*
            * { 
            *   "key1": {
            *     "key": "name" //it is a comment
            *   },
            *   "key2": {
            *     "key": "name"
            *   }
            * }
            */
            InvalidJson("{\n  \"key1\": {\n    \"key\": \"name\" //it is a comment\n  },\n  \"key2\": {\n    \"key\": \"name\"\n  }\n}");
            #endregion

            #region InvalidJson("{\n  \"key1\": {\n    \"key\": \"name\" /* it is also a comment */\n  },\n  \"key2\": {\n    \"key\": \"name\"\n  }\n}");
            /*
            * { 
            *   "key1": {
            *     "key": "name" /* it is also a comment */
            /*   },
            *   "key2": {
            *     "key": "name"
            *   }
            * }
            */
            InvalidJson("{\n  \"key1\": {\n    \"key\": \"name\" /* it is also a comment */\n  },\n  \"key2\": {\n    \"key\": \"name\"\n  }\n}");
            #endregion
            #endregion
        }
        [TestMethod]
        public void ParseTest()
        {
            #region Check if it can parse correctly
            Assert.AreEqual
            (
                new JsonObject(),
                JsonParser.Parse("{}")
            );
            Assert.AreEqual
            (
                new JsonArray(),
                JsonParser.Parse("[]")
            );
            Assert.AreEqual
            (
                new JsonArray(new JsonNumber("0.0")),
                JsonParser.Parse("[0.0]")
            );

            Assert.AreEqual
            (
                new JsonArray
                (
                    new JsonNumber("0.0"),
                    new JsonNumber("0.1")
                ),
                JsonParser.Parse("[0.0, 0.1]")
            );

            Assert.AreEqual
            (
                new JsonObject
                (
                    new KeyValuePair<string, IJsonValue>("out", new JsonString("safe"))
                ),
                JsonParser.Parse("{\"out\": \"safe\"}")
            );
            Assert.AreEqual
            (
                new JsonObject
                (
                    new KeyValuePair<string, IJsonValue>("1", new JsonString("value1")),
                    new KeyValuePair<string, IJsonValue>("2", new JsonString("value2"))
                ),
                JsonParser.Parse("{\"1\": \"value1\", \"2\": \"value2\"}")
            );
            {
                Assert.AreEqual
                (
                    new JsonObject
                    (
                        new KeyValuePair<string, IJsonValue>("1", new JsonNumber(0.5)),
                        new KeyValuePair<string, IJsonValue>("2", new JsonNumber(-1.0E6))
                    ),
                    JsonParser.Parse("{\"1\": 0.5, \"2\": -1.0E6}")
                );
                Assert.AreEqual
                (
                    new JsonObject
                    (
                        new KeyValuePair<string, IJsonValue>("1", new JsonNumber((decimal) 0.5)),
                        new KeyValuePair<string, IJsonValue>("2", new JsonNumber((decimal) -1.0E6))
                    ),
                    JsonParser.Parse("{\"1\": 0.5, \"2\": -1.0E6}")
                );
                Assert.AreEqual
                (
                    new JsonObject
                    (
                        new KeyValuePair<string, IJsonValue>("1", new JsonNumber((float) 0.5)),
                        new KeyValuePair<string, IJsonValue>("2", new JsonNumber((float) -1.0E6))
                    ),
                    JsonParser.Parse("{\"1\": 0.5, \"2\": -1.0E6}")
                );
            }
            JsonParser.Parse("{\"1\":0.5, \"2\":-1.0E6}");
            JsonParser.Parse("{\"1\":0.5, \"2\":-1.0E+6}");
            JsonParser.Parse("{\"1\":0.5, \"2\":-1.0E-6}");


            Assert.AreEqual
            (
                new JsonArray(new JsonObject()),
                JsonParser.Parse("[{}]")
            );
            Assert.AreEqual
            (
               new JsonArray((IJsonValue) new JsonArray()),
                JsonParser.Parse("[[]]")
            );
            Assert.AreEqual
            (
                new JsonArray(new JsonObject(), new JsonObject()),
                JsonParser.Parse("[{}, {}]")
            );
            Assert.AreEqual
            (
                new JsonArray(new JsonArray(), new JsonObject()),
                JsonParser.Parse("[[], {}]")
            );
            Assert.AreEqual(
                new JsonObject
                (
                    new KeyValuePair<string, IJsonValue>
                    (
                        "key1", new JsonObject
                        (
                            new KeyValuePair<string, IJsonValue>("key", new JsonString("name"))
                        )
                    )
                ),
                JsonParser.Parse("{\"key1\": {\"key\": \"name\"}}")
            );
            Assert.AreEqual(
                new JsonObject
                (
                    new KeyValuePair<string, IJsonValue>
                    (
                        "key1", new JsonObject
                        (
                            new KeyValuePair<string, IJsonValue>("key", new JsonString("name"))
                        )
                    ),
                    new KeyValuePair<string, IJsonValue>
                    (
                        "key2", new JsonObject
                        (
                            new KeyValuePair<string, IJsonValue>("key", new JsonString("name"))
                        )
                    )
                ),
                JsonParser.Parse("{\"key1\": {\"key\": \"name\"}, \"key2\": {\"key\": \"name\"}}")
            );
            Assert.AreEqual(
                new JsonObject
                (
                    new KeyValuePair<string, IJsonValue>
                    (
                        "key1", new JsonObject
                        (
                            new KeyValuePair<string, IJsonValue>("key", new JsonString("name"))
                        )
                    ),
                    new KeyValuePair<string, IJsonValue>
                    (
                        "key1", new JsonObject
                        (
                            new KeyValuePair<string, IJsonValue>("key", new JsonString("name"))
                        )
                    )
                ),
                JsonParser.Parse("{\"key1\": {\"key\": \"name\"}, \"key1\": {\"key\": \"name\"}}")
            );
            Assert.AreEqual(
                new JsonObject
                (
                    new KeyValuePair<string, IJsonValue>
                    (
                        "key1", new JsonArray
                        (
                            new JsonArray(),
                            new JsonObject
                            (
                                new KeyValuePair<string, IJsonValue>("key2", new JsonNull())
                            )
                        )
                    )
                ),
                JsonParser.Parse("{\"key1\": [[], {\"key2\": null}]}")
            );
            #endregion
            #region Check if it has correct value
            #region parse an array which has only 0
            Assert.AreEqual(
                (int) 0,
                (JsonParser.Parse("[0]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().IntValue
            );
            Assert.AreEqual(
                (long) 0,
                (JsonParser.Parse("[0]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().LongValue
            );
            Assert.AreEqual(
                (uint) 0,
                (JsonParser.Parse("[0]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().UIntValue
            );
            Assert.AreEqual(
                (ulong) 0,
                (JsonParser.Parse("[0]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().ULongValue
            );
            Assert.AreEqual(
                (float) 0,
                (JsonParser.Parse("[0]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().FloatValue
            );
            Assert.AreEqual(
                (double) 0,
                (JsonParser.Parse("[0]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DoubleValue
            );
            Assert.AreEqual(
                (decimal) 0,
                (JsonParser.Parse("[0]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DecimalValue
            );
            #endregion
            #region parse an array which has only 0.5
            Assert.AreEqual(
                (float) 0.5,
                (JsonParser.Parse("[0.5]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().FloatValue
            );
            Assert.AreEqual(
                (double) 0.5,
                (JsonParser.Parse("[0.5]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DoubleValue
            );
            Assert.AreEqual(
                (decimal) 0.5,
                (JsonParser.Parse("[0.5]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DecimalValue
            );
            #endregion
            #region parse an array which has only 5E-3
            Assert.AreEqual(
                (float) 0.005,
                (JsonParser.Parse("[5E-3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().FloatValue
            );
            Assert.AreEqual(
                (double) 0.005,
                (JsonParser.Parse("[5E-3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DoubleValue
            );
            Assert.AreEqual(
                (decimal) 0.005,
                (JsonParser.Parse("[5E-3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DecimalValue
            );
            #endregion
            #region parse an array which has only 5E+3
            Assert.AreEqual(
                (int) 5000,
                (JsonParser.Parse("[5E+3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().IntValue
            );
            Assert.AreEqual(
                (long) 5000,
                (JsonParser.Parse("[5E+3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().LongValue
            );
            Assert.AreEqual(
                (uint) 5000,
                (JsonParser.Parse("[5E+3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().UIntValue
            );
            Assert.AreEqual(
                (ulong) 5000,
                (JsonParser.Parse("[5E+3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().ULongValue
            );
            Assert.AreEqual(
                (float) 5000,
                (JsonParser.Parse("[5E+3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().FloatValue
            );
            Assert.AreEqual(
                (double) 5000,
                (JsonParser.Parse("[5E+3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DoubleValue
            );
            Assert.AreEqual(
                (decimal) 5000,
                (JsonParser.Parse("[5E+3]").Cast<JsonArray>())[0]
                    .Cast<JsonNumber>().DecimalValue
            );
            #endregion
            #endregion
            const string dummyJson = "[{\"_id\":\"5b1ba9ce93fe7f2c376613a6\",\"index\":0,\"guid\":\"7f2238b4-33fb-4da1-a847-09201bf18c4e\",\"isActive\":true,\"balance\":\"$2,498.90\",\"picture\":\"http://placehold.it/32x32\",\"age\":29,\"eyeColor\":\"brown\",\"name\":\"Rosanna Curry\",\"gender\":\"female\",\"company\":\"PAPRICUT\",\"email\":\"rosannacurry@papricut.com\",\"phone\":\"+1 (921) 570-3611\",\"address\":\"828 Chase Court, Ryderwood, Colorado, 7491\",\"about\":\"Est exercitation ea duis culpa aliquip nulla dolor qui. Reprehenderit irure laborum ullamco id culpa aliqua reprehenderit aliquip commodo. Velit in ea elit do. Sint elit commodo voluptate do consectetur et pariatur officia ullamco excepteur. Ut ut proident proident deserunt Lorem nulla amet duis deserunt. Ullamco in velit ipsum consequat nulla esse.\\r\\n\",\"registered\":\"2015-10-14T02:50:18 -09:00\",\"latitude\":81.009721,\"longitude\":-6.631632,\"tags\":[\"tempor\",\"non\",\"consectetur\",\"proident\",\"non\",\"enim\",\"mollit\"],\"friends\":[{\"id\":0,\"name\":\"Reyes Branch\"},{\"id\":1,\"name\":\"Reva Grimes\"},{\"id\":2,\"name\":\"Vicky Whitfield\"}],\"greeting\":\"Hello, Rosanna Curry! You have 9 unread messages.\",\"favoriteFruit\":\"apple\"},{\"_id\":\"5b1ba9ceeaa9ca32ba9dbf27\",\"index\":1,\"guid\":\"e7f0ce04-b8af-4403-aa53-fbdf4ceb57da\",\"isActive\":true,\"balance\":\"$1,819.55\",\"picture\":\"http://placehold.it/32x32\",\"age\":39,\"eyeColor\":\"green\",\"name\":\"Georgia Shepherd\",\"gender\":\"female\",\"company\":\"ZENTILITY\",\"email\":\"georgiashepherd@zentility.com\",\"phone\":\"+1 (957) 435-3060\",\"address\":\"689 Polar Street, Axis, Minnesota, 2665\",\"about\":\"Sunt ut sunt reprehenderit sit labore nisi duis velit adipisicing et ad sit. Nulla est commodo labore proident est non amet proident irure elit. Nulla non dolore minim sunt minim nulla nulla dolore in. Aliquip magna elit fugiat esse aliquip ad elit occaecat eu enim commodo. Eiusmod incididunt fugiat non minim.\\r\\n\",\"registered\":\"2017-07-26T09:23:26 -09:00\",\"latitude\":65.917926,\"longitude\":82.650233,\"tags\":[\"sunt\",\"cupidatat\",\"nostrud\",\"nisi\",\"amet\",\"eiusmod\",\"commodo\"],\"friends\":[{\"id\":0,\"name\":\"Norman Munoz\"},{\"id\":1,\"name\":\"Lara Dominguez\"},{\"id\":2,\"name\":\"Salinas Santana\"}],\"greeting\":\"Hello, Georgia Shepherd! You have 10 unread messages.\",\"favoriteFruit\":\"strawberry\"},{\"_id\":\"5b1ba9ce262806f5842e531b\",\"index\":2,\"guid\":\"0f4fc07a-a393-415e-9cbb-bdffce18a5bb\",\"isActive\":true,\"balance\":\"$1,344.84\",\"picture\":\"http://placehold.it/32x32\",\"age\":25,\"eyeColor\":\"brown\",\"name\":\"Jeri Harper\",\"gender\":\"female\",\"company\":\"BALOOBA\",\"email\":\"jeriharper@balooba.com\",\"phone\":\"+1 (826) 581-3763\",\"address\":\"231 Boynton Place, Belmont, Maryland, 5018\",\"about\":\"Eu laborum cillum fugiat consequat. Tempor labore pariatur nulla mollit duis excepteur nostrud. Non esse id proident laboris.\\r\\n\",\"registered\":\"2017-05-30T11:26:46 -09:00\",\"latitude\":21.54895,\"longitude\":13.878004,\"tags\":[\"aliqua\",\"id\",\"dolor\",\"tempor\",\"excepteur\",\"et\",\"pariatur\"],\"friends\":[{\"id\":0,\"name\":\"Holmes Ryan\"},{\"id\":1,\"name\":\"Soto Zimmerman\"},{\"id\":2,\"name\":\"Katherine Maddox\"}],\"greeting\":\"Hello, Jeri Harper! You have 7 unread messages.\",\"favoriteFruit\":\"banana\"},{\"_id\":\"5b1ba9ceb711e221ee6a0e51\",\"index\":3,\"guid\":\"727062c2-c6c2-4f16-8665-850f0280c3a5\",\"isActive\":false,\"balance\":\"$2,147.83\",\"picture\":\"http://placehold.it/32x32\",\"age\":25,\"eyeColor\":\"blue\",\"name\":\"Sonya Hamilton\",\"gender\":\"female\",\"company\":\"AUTOMON\",\"email\":\"sonyahamilton@automon.com\",\"phone\":\"+1 (808) 440-2471\",\"address\":\"768 Batchelder Street, Welda, Washington, 7247\",\"about\":\"Minim fugiat qui fugiat dolore id qui dolor amet ipsum reprehenderit anim cillum sit. Laboris ad proident ipsum sunt id laboris cillum do duis dolore eiusmod proident. Aute nulla ut qui velit aute velit. Velit exercitation mollit ad ad consectetur ut nisi non fugiat Lorem aliquip do. Do consectetur exercitation commodo consectetur et. Sint quis aliquip minim magna enim excepteur Lorem anim officia sint veniam nostrud officia. Labore eu nostrud Lorem ipsum nulla aute ea irure sit nisi minim.\\r\\n\",\"registered\":\"2015-09-30T03:00:34 -09:00\",\"latitude\":-70.924781,\"longitude\":-83.966263,\"tags\":[\"sint\",\"in\",\"irure\",\"ullamco\",\"amet\",\"minim\",\"reprehenderit\"],\"friends\":[{\"id\":0,\"name\":\"Livingston Barton\"},{\"id\":1,\"name\":\"Finch Chen\"},{\"id\":2,\"name\":\"Margo Nicholson\"}],\"greeting\":\"Hello, Sonya Hamilton! You have 2 unread messages.\",\"favoriteFruit\":\"apple\"},{\"_id\":\"5b1ba9cedb7ea73a261d5070\",\"index\":4,\"guid\":\"63aa4a67-2de5-49ee-b0bd-f71944b61398\",\"isActive\":true,\"balance\":\"$1,625.28\",\"picture\":\"http://placehold.it/32x32\",\"age\":36,\"eyeColor\":\"green\",\"name\":\"Harriett Parrish\",\"gender\":\"female\",\"company\":\"BRAINQUIL\",\"email\":\"harriettparrish@brainquil.com\",\"phone\":\"+1 (978) 502-2817\",\"address\":\"921 Empire Boulevard, Brandermill, Guam, 6691\",\"about\":\"Deserunt voluptate laborum mollit minim veniam dolor aliqua esse ullamco reprehenderit culpa non. Esse duis enim qui laboris consectetur dolore commodo tempor amet. Lorem id commodo do elit. Minim ipsum proident esse ea sit. Fugiat esse velit esse laboris officia.\\r\\n\",\"registered\":\"2014-11-28T01:05:44 -09:00\",\"latitude\":84.691056,\"longitude\":85.326845,\"tags\":[\"veniam\",\"minim\",\"labore\",\"occaecat\",\"dolor\",\"sunt\",\"nulla\"],\"friends\":[{\"id\":0,\"name\":\"Taylor Logan\"},{\"id\":1,\"name\":\"Knapp Leach\"},{\"id\":2,\"name\":\"Melinda Hicks\"}],\"greeting\":\"Hello, Harriett Parrish! You have 9 unread messages.\",\"favoriteFruit\":\"apple\"}]";

            Assert.AreEqual(
                JsonParser.Parse(dummyJson),
                JsonParser.Parse(dummyJson)
            );
        }

        private static IEnumerable<IJsonValue> ParseTestCases()
        {
            yield return new JsonNull();
            yield return new JsonBoolean(true);
            yield return new JsonBoolean(false);
            yield return new JsonString("");
            yield return new JsonString("test");
            yield return new JsonNumber("1.0");
            yield return new JsonNumber(1.0);
            yield return new JsonNumber(decimal.MaxValue);
            yield return new JsonNumber(decimal.MinValue);
            yield return new JsonNumber(double.MaxValue);
            yield return new JsonNumber(double.MinValue);
            yield return new JsonNumber(double.Epsilon);
            yield return new JsonArray();
            yield return new JsonArray(new JsonBoolean(true));
            yield return new JsonArray(new JsonBoolean(true), new JsonBoolean(false));
            yield return (
                new JsonArray(
                    (IJsonValue) new JsonArray(
                        new JsonBoolean(true)
                    )
                )
            );
            yield return (
                new JsonArray(
                    new JsonArray(
                        new JsonBoolean(true)
                    ),
                    new JsonArray(
                        new JsonBoolean(false)
                    )
                )
            );
            yield return (
                new JsonArray(
                    new JsonArray(
                        new JsonBoolean(true),
                        new JsonBoolean(false)
                    ),
                    new JsonArray(
                        new JsonBoolean(false),
                        new JsonBoolean(true)
                    )
                )
            );
            yield return new JsonObject();
            yield return (
                new JsonObject(
                    new KeyValuePair<string, IJsonValue>("key1", new JsonString("value1"))
                )
            );
            yield return (
                new JsonObject(
                    new KeyValuePair<string, IJsonValue>("key1", new JsonString("value1")),
                    new KeyValuePair<string, IJsonValue>("key2", new JsonString("value2"))
                )
            );
            yield return (
                new JsonObject(
                    new KeyValuePair<string, IJsonValue>("key1", new JsonString("value1")),
                    new KeyValuePair<string, IJsonValue>("key2",
                        new JsonObject(
                            new KeyValuePair<string, IJsonValue>("key2.key1", new JsonString("value2")),
                            new KeyValuePair<string, IJsonValue>("key2.key2", new JsonString("value3"))
                        )
                    )
                )
            );
            yield return JsonParser.Parse("[{}]");
            yield return JsonParser.Parse("[[]]");
            yield return JsonParser.Parse("[{}, {}]");
            yield return JsonParser.Parse("[[], {}]");
            yield return JsonParser.Parse("{\"key1\": {\"key\": \"name\"}, \"key2\": {\"key\": \"name\"}}");
            yield return JsonParser.Parse("{\"key1\": {\"key\": \"name\"}, \"key1\": {\"key\": \"name\"}}");
            yield return JsonParser.Parse("{\"key1\": [[], {\"key2\": null}]}");
        }

        [TestMethod]
        public void DeepCopyTest()
        {
            foreach (var value in ParseTestCases())
            {
                Assert.AreEqual(
                    value,
                    value
                );
                Assert.AreEqual(
                    value,
                    value.DeepCopy()
                );
                Assert.AreEqual(
                    value,
                    value.DeepCopy().DeepCopy()
                );
            }
        }

        [TestMethod]
        public void SerializeTest()
        {
            foreach (var value in ParseTestCases())
            {
                // continue if invalid JSON which has illegal top-level object type
                switch (value.Type)
                {
                    case JsonValueType.Array:
                    case JsonValueType.Object:
                        break;
                    default:
                        continue;
                }
                Assert.AreEqual(value, JsonParser.Parse(value.ToJsonCompatibleString()));
            }
        }

        private static void ValidJson(string json) => JsonParser.Parse(json);
        private static void InvalidJson(string json) => Assert.ThrowsException<InvalidDataException>(() => JsonParser.Parse(json));

        [TestMethod]
        public void NumberEqualsTest()
        {
            Assert.IsTrue(JsonNumber.Equals("0", "0"));
            Assert.IsTrue(JsonNumber.Equals("0.00", "0"));
            Assert.IsTrue(JsonNumber.Equals("1234", "1234"));
            Assert.IsTrue(JsonNumber.Equals("-1234", "-1234"));
            Assert.IsTrue(JsonNumber.Equals("0E1", "0"));
            Assert.IsFalse(JsonNumber.Equals("0", "-0"));
            Assert.IsTrue(JsonNumber.Equals("1.0E1", "10"));
            Assert.IsTrue(JsonNumber.Equals("1.0E2", "100"));
            Assert.IsTrue(JsonNumber.Equals("1.0E-1", "0.1"));
            Assert.IsTrue(JsonNumber.Equals("1.0E-2", "0.01"));
            Assert.IsTrue(JsonNumber.Equals("-1.0E1", "-10"));
            Assert.IsTrue(JsonNumber.Equals("-1.0E2", "-100"));
            Assert.IsTrue(JsonNumber.Equals("-1.0E-1", "-0.1"));
            Assert.IsTrue(JsonNumber.Equals("-1.0E-2", "-0.01"));
            Assert.IsFalse(JsonNumber.Equals("1.0E1", "-10"));
            Assert.IsFalse(JsonNumber.Equals("1.0E2", "-100"));
            Assert.IsFalse(JsonNumber.Equals("1.0E-1", "-0.1"));
            Assert.IsFalse(JsonNumber.Equals("1.0E-2", "-0.01"));
            Assert.IsTrue(JsonNumber.Equals("1.0E0", "1"));
            Assert.IsTrue(JsonNumber.Equals("1.00000E0", "1"));
            Assert.IsTrue(JsonNumber.Equals("1.00000E0", "1E0"));
            Assert.IsTrue(JsonNumber.Equals("1.23E0", "123E-2"));
            Assert.IsTrue(JsonNumber.Equals("1.23E0", "12300E-4"));
            Assert.IsFalse(JsonNumber.Equals("1.2345E0", "123E-2"));
            Assert.IsTrue(JsonNumber.Equals("-1.23E0", "-123E-2"));
            Assert.IsTrue(JsonNumber.Equals("-1.23E0", "-12300E-4"));
            Assert.IsFalse(JsonNumber.Equals("-1.2345E0", "-123E-2"));
            Assert.IsFalse(JsonNumber.Equals("1E-2", "1.0E2"));
            Assert.IsFalse(JsonNumber.Equals("1E-2", "1.0E2"));


            Assert.AreEqual(new JsonNumber("0").GetHashCode(), new JsonNumber("0").GetHashCode());
            Assert.AreEqual(new JsonNumber("0.00").GetHashCode(), new JsonNumber("0").GetHashCode());
            Assert.AreEqual(new JsonNumber("1234").GetHashCode(), new JsonNumber("1234").GetHashCode());
            Assert.AreEqual(new JsonNumber("-1234").GetHashCode(), new JsonNumber("-1234").GetHashCode());
            Assert.AreEqual(new JsonNumber("0E1").GetHashCode(), new JsonNumber("0").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.0E1").GetHashCode(), new JsonNumber("10").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.0E2").GetHashCode(), new JsonNumber("100").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.0E-1").GetHashCode(), new JsonNumber("0.1").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.0E-2").GetHashCode(), new JsonNumber("0.01").GetHashCode());
            Assert.AreEqual(new JsonNumber("-1.0E1").GetHashCode(), new JsonNumber("-10").GetHashCode());
            Assert.AreEqual(new JsonNumber("-1.0E2").GetHashCode(), new JsonNumber("-100").GetHashCode());
            Assert.AreEqual(new JsonNumber("-1.0E-1").GetHashCode(), new JsonNumber("-0.1").GetHashCode());
            Assert.AreEqual(new JsonNumber("-1.0E-2").GetHashCode(), new JsonNumber("-0.01").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.0E0").GetHashCode(), new JsonNumber("1").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.00000E0").GetHashCode(), new JsonNumber("1").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.00000E0").GetHashCode(), new JsonNumber("1E0").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.23E0").GetHashCode(), new JsonNumber("123E-2").GetHashCode());
            Assert.AreEqual(new JsonNumber("1.23E0").GetHashCode(), new JsonNumber("12300E-4").GetHashCode());
            Assert.AreEqual(new JsonNumber("-1.23E0").GetHashCode(), new JsonNumber("-123E-2").GetHashCode());
            Assert.AreEqual(new JsonNumber("-1.23E0").GetHashCode(), new JsonNumber("-12300E-4").GetHashCode());

        }

        [TestMethod]
        public void Log10iTest()
        {
            Assert.AreEqual(0, JsonNumber.Log10i("0"));
            Assert.AreEqual(0, JsonNumber.Log10i("1"));
            Assert.AreEqual(1, JsonNumber.Log10i("10"));
            Assert.AreEqual(1, JsonNumber.Log10i("20"));
            Assert.AreEqual(1, JsonNumber.Log10i("-20"));
            Assert.AreEqual(0, JsonNumber.Log10i("-0"));
            Assert.AreEqual(0, JsonNumber.Log10i("3.14"));
            Assert.AreEqual(0, JsonNumber.Log10i("-3.14"));
            Assert.AreEqual(1, JsonNumber.Log10i("33.4"));
            Assert.AreEqual(3, JsonNumber.Log10i("-2411"));
            Assert.AreEqual(5, JsonNumber.Log10i("-114514"));
            Assert.AreEqual(7, JsonNumber.Log10i("12345678"));
            Assert.AreEqual(0, JsonNumber.Log10i("0.000"));
            Assert.AreEqual(-3, JsonNumber.Log10i("0.001"));
            Assert.AreEqual(-3, JsonNumber.Log10i("-0.001"));
            Assert.AreEqual(-1, JsonNumber.Log10i("-0.1"));
        }

        [TestMethod]
        public void IsValidNumberTest()
        {
            Assert.IsTrue(JsonTokenRule.IsValidNumber("1E400"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("3.141592653589793238462643383279"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber(""));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("{"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("}"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("+1"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("+0"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("iosdajnvx"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("true"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("false"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("null"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("INF"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("inf"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("NaN"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("1.8"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("35248.9"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-3541.5"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("0"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-0"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-0.0"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("0.0"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1..0"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("0..0"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("-0..0"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("-1..0"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("03"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("08"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("0x1F"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("-002"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("00"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("1E3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("1e3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("1E+3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("1e+3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-1E+3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-1e+3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-1E-3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-1e-3"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("1e03"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-1e03"));
            Assert.IsTrue(JsonTokenRule.IsValidNumber("-1e0000003"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1E"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1e"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1Ee"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1eE"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1E1.0"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1e0.3"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1."));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1.E"));
            Assert.IsFalse(JsonTokenRule.IsValidNumber("1.e"));
        }

        [TestMethod]
        public void IsDigitTest()
        {
            for (char c = (char) 0; c < '0'; c++)
                Assert.IsFalse(JsonTokenRule.IsDigit(c));
            for (char c = '0'; c <= '9'; c++)
                Assert.IsTrue(JsonTokenRule.IsDigit(c));

            //(char)0xFFFF + (char)0x0001 = (char)0x0000
            for (char c = (char) ('9' + 1); c != (char) 0; c++)
                Assert.IsFalse(JsonTokenRule.IsDigit(c));
        }

        [TestMethod]
        public void ValidateStringTest()
        {
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
            ValidString("");
            ValidString("114514");
            ValidString("alphabet");
            ValidString("ALPHABET");
            ValidString("aLpHaBeT");
            ValidString("AlPhAbEt");
            ValidString("`~!@#$%^&*()_-=[]{};':,.<>/? (signs)");
            ValidString("This is a sentence.");
            ValidString("two words");
            /*        /* {like: {a: {Mojangson: \"data\"}}} */
            ValidString("{like: {a: {Mojangson: \\\"data\\\"}}}");
            ValidString("\\\"");      /* \" */
            ValidString("\\\"\\\"");  /* \"\" */
            ValidString("\\\\");      /* \\ */
            ValidString("\\/");       /* \/ */
            ValidString("\\b");
            ValidString("\\f");
            ValidString("\\n");
            ValidString("\\r");
            ValidString("\\t");
            ValidString("\\u0000");
            ValidString("\\u0020");
            ValidString("\\u0022");
            ValidString("\\u005C");
            ValidString("\\uFFFF");
            ValidString("\\u10000");  /* \u10000 ('\u1000' + '0') */

            InvalidString("\\");      /* \ */
            InvalidString("\"");      /* " */
            InvalidString("\\u");     /* \u */
            InvalidString("\\u000");  /* \u000 */
            InvalidString("\\uFFF");  /* \uFFF */
            InvalidString("\\uIOEN"); /* \uTOEN */
            InvalidString("\\uAAAZ"); /* \uAAAZ */
            InvalidString("\\uAAZZ"); /* \uAAZZ */
            InvalidString("\\uAAZZ"); /* \uAAZZ */
            InvalidString("\\uZZZZ"); /* \uZZZZ */
            InvalidString("\\uZZZA"); /* \uZZZA */
            InvalidString("\\uZZAA"); /* \uZZAA */
            InvalidString("\\uZAAA"); /* \uZAAA */
            InvalidString("\\B");     /* \B */
            InvalidString("\\F");     /* \F */
            InvalidString("\\N");     /* \N */
            InvalidString("\\R");     /* \R */
            InvalidString("\\T");     /* \T */
            InvalidString("\\a");     /* \a */
            InvalidString("\\c");     /* \c */

            InvalidString("\\n\\");   /* \n\ */
            InvalidString("\\n\"");   /* \n" */
            InvalidString("\\n\\aa"); /* \n\aa */
            InvalidString("\\n\"aa"); /* \n"aa */

        }

        private static void ValidString(string value) => JsonString.Parse(value);
        private static void InvalidString(string value) => Assert.ThrowsException<InvalidDataException>(() => JsonString.Parse(value));
    }
}
