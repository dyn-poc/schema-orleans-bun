namespace schema.server.UnitTest;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Grains;
using Json.Pointer;
using Json.Schema;
using Xunit.Abstractions;

public class SchemaConvertTest : Fixture
{
    /*
     * allowNull - A Boolean value ("true" by default), specifying whether null values are allowed.
     */

    /*
      * type - Defines the data type of this field. The supported values are:
            "integer" (range: -2,147,483,648 to 2,147,483,647, size: signed 32-bit integer).
            "long" (range: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807, size: signed 64-bit integer).
            "float" (approximate range: ±1.5e−45 to ±3.4e38, size: 7 digits).
            "string" (up to 16 KB in size - a multi-field with analyzed entries (searchable using partial case-insensitive searches)).
            "basic-string" (up to 16 KB in size - a field with non-analyzed entries (non-searchable)).
            "text" (up to 64 KB - an analyzed multi-field (only searchable using exact match)).
            "date" a date field, allows exact matching or range searches. The valid date format:
            Standard ISO 8601 time format (like "2011.07.14T11.42.32.123+3", can be specified without a time at all or without a time zone) "YYYY-MM-DDTHH.MM.SS.sssZ"
            "boolean" - true/false
            "binary" -   It can be used to hold large data objects such as base-64 images and large binary data. It isn't limited in size or data format (besides the overall limitation of 512K per JSON object).  .
          */


    /*

     * format - Allows assigning a regular expression (regex) that defines the patten of this  field.
        The format property accepts a string of the form: "regex('<regex-pattern>')".
        This property also accepts, for Boolean fields: true or false; e.g., "format":"true".
         This property also accepts, for numbers fields: range values. e.g. 0-10, also negative values -20-27

     */


    [Fact]
    public void Test_SchemaConvert_fromFieldsToSchema_With_Empty_Fields()
    {
        // Arrange
        var fields = new Dictionary<string, Field>();

        // Act
        var schema = SchemaConvert.Convert(fields, "profile");

        // Assert
        Assert.NotNull(schema);
        Assert.Null(schema.GetProperties());
    }

    [Fact]
    public void Test_SchemaConvert_fromFieldsToSchema_With_NonEmpty_Fields()
    {
        // Arrange
        var fields = new Dictionary<string, Field>()
        {
            ["field1"] = new Field() { Type = "string" },
            ["field2"] = new Field() { Type = "integer" }
        };

        // Act
        var schema = new Schema(fields, "profile").ToTree();
        // Assert
        Assert.NotNull(schema);
        Assert.Equal(2, schema.Count);
        Assert.Equal("string", schema["field1"].Field?.Type);
        Assert.Equal("integer", schema["field2"].Field?.Type);

        //JSON Schema Asserts
        var jsonSchema = schema.Apply(new JsonSchemaBuilder()).Build();
        Assert.NotNull(jsonSchema);
        Assert.True(jsonSchema.Evaluate(new JsonObject() { ["field1"] = "test", ["field2"] = 1 },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }).IsValid);

        Assert.False(jsonSchema.Evaluate(new JsonObject() { ["field1"] = 1, ["field2"] = "test" },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }).IsValid);

        Assert.Equal(2, jsonSchema.GetProperties()?.Count);
        Assert.Equal(jsonSchema.GetProperties()!.Keys, new[] { "field1", "field2" });
    }

    /* nested fields test including ids and anchor
        * e.g favorites.music.name
          should be converted to
          {
            "type": "object",
            "properties": {
              "favorites": {
                "type": "object",
                "$id": "favorites",
                "$anchor": "favorites",
                "properties": {
                  "music": {
                    "$id": "favorites.music",
                    "$anchor": "music",
                    "type": "object",
                    "properties": {
                      "name": {
                        "$id": "favorites.music.name",
                        "$anchor": "name",
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }

        *
        */
    [Fact]
    public void Test_SchemaConvert_fromFieldsToSchema_With_Nested_Fields()
    {
        // Arrange
        var fields = new Dictionary<string, Field>() { ["favorites.music.id"] = new Field() { Type = "string", AllowNull = false } };

        // Act
        var schema = this.WriteJson(SchemaConvert.Convert(fields, "profile"));

        // Assert
        Assert.NotNull(schema);
        Assert.NotNull(schema.GetProperties());
        Assert.Equal(1, schema.GetProperties()!.Count);
        Assert.NotNull(schema.GetProperties()!["favorites"]!.GetProperties());
        Assert.NotNull(schema.GetProperties()!["favorites"]!.GetProperties()!["music"]!.GetProperties());
        Assert.Equal(SchemaValueType.String,
            schema.GetProperties()!["favorites"]!.GetProperties()!["music"]!.GetProperties()!["id"]!.GetJsonType());

        //assert anchors and ids
        Assert.Equal("favorites", schema.GetProperties()!["favorites"]!.GetId().ToString());

        //TODO add anchor back
        // Assert.Equal("favorites", schema.GetProperties()!["favorites"]!.GetAnchor());

        // Assert.Equal("favorites.music", schema.GetProperties()!["favorites"]!.GetProperties()!["music"]!.Id);
        // Assert.Equal("music", schema.GetProperties()!["favorites"]!.GetProperties()!["music"]!.Anchor);
        // Assert.Equal("favorites.music.id",
        //     schema.GetProperties()!["favorites"]!.GetProperties()!["music"]!.GetProperties()!["id"]!.Id);
        // Assert.Equal("id",
        //     schema.GetProperties()!["favorites"]!.GetProperties()!["music"]!.GetProperties()!["id"]!.Anchor);




        // assert validate errors evaluation
        /* response should be
        Json.Schema.EvaluationResults: {
                "valid": false,
                "details": [
                    {
                    "valid": false,
                    "evaluationPath": "",
                    "schemaLocation": "https://json-everything.net/profile#",
                    "instanceLocation": ""
                    },
                    {
                    "valid": false,
                    "evaluationPath": "/properties/favorites",
                    "schemaLocation": "https://json-everything.net/favorites#/properties/favorites",
                    "instanceLocation": "/favorites"
                    },
                    {
                    "valid": false,
                    "evaluationPath": "/properties/favorites/properties/music",
                    "schemaLocation": "https://json-everything.net/favorites.music#/properties/favorites/properties/music",
                    "instanceLocation": "/favorites/music"
                    },
                    {
                    "valid": false,
                    "evaluationPath": "/properties/favorites/properties/music/properties/id",
                    "schemaLocation": "https://json-everything.net/favorites.music.id#/properties/favorites/properties/music/properties/id",
                    "instanceLocation": "/favorites/music/id",
                    "errors": {
                        "type": "Value is \u0022integer\u0022 but should be \u0022string\u0022"
                    }
                    }
                ]
            }
        */
        var result = this.WriteJson(
            schema.Evaluate(new JsonObject()
            {
                ["favorites"] = new JsonObject() { ["music"] = new JsonObject() { ["id"] = 1 } }
            },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Equal(4, result.Details!.Count);
        Assert.Collection(result.Details, item =>
        {
            Assert.Equal(JsonPointer.Parse(""), item.EvaluationPath);
            Assert.Equal(new Uri("https://json-everything.net/profile#"), item.SchemaLocation);
            Assert.Equal(JsonPointer.Parse(""), item.InstanceLocation);
        }, item =>
        {   //favorites
            Assert.Equal(JsonPointer.Parse("/properties/favorites"), item.EvaluationPath);
            Assert.Equal(new Uri("https://json-everything.net/favorites#/properties/favorites"), item.SchemaLocation);
            Assert.Equal(JsonPointer.Parse("/favorites"), item.InstanceLocation);
        }, item =>
        {   //favorites.music
            Assert.Equal(JsonPointer.Parse("/properties/favorites/properties/music"), item.EvaluationPath);
            Assert.Equal(new Uri("https://json-everything.net/favorites.music#/properties/favorites/properties/music"),
                item.SchemaLocation);
            Assert.Equal(JsonPointer.Parse("/favorites/music"), item.InstanceLocation);
        }, item =>
        {   //favorites.music.id
            Assert.Equal(JsonPointer.Parse("/properties/favorites/properties/music/properties/id"),
                item.EvaluationPath);
            Assert.Equal(new Uri("https://json-everything.net/favorites.music.id#/properties/favorites/properties/music/properties/id"),
                item.SchemaLocation);
            Assert.Equal(JsonPointer.Parse("/favorites/music/id"), item.InstanceLocation);
            Assert.NotNull(item.Errors);
            Assert.Equal(1, item.Errors!.Count);
            Assert.Equal("type", item.Errors.Keys.First());
            Assert.Equal("Value is \"integer\" but should be \"string\"", item.Errors.Values.First());
        });

    }

    /* type tests */
    [Theory]
    [InlineData("integer", "integer", 1, "string")]
    [InlineData("long", "number", 1, "string")]
    [InlineData("float", "number", 1.5, "string")]
    [InlineData("string", "string", "test", 1)]
    [InlineData("date", "string", "2021-01-01", 1)]
    [InlineData("boolean", "boolean", true, "string")]
    [InlineData("binary", "string", "test", 1)]
    public void Schema_Type_Test(string fieldType, string propertyType, object validInput, object invalidInput)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = fieldType, AllowNull = false }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        Assert.True(this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = JsonValue.Create(validInput) },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List })).IsValid);

        Assert.False(schema.Evaluate(new JsonObject() { ["field"] = JsonValue.Create(invalidInput)  },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }).IsValid);

        // assert error
        var result = this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = JsonValue.Create(invalidInput )},
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Contains(result.Details, detail => detail.HasErrors);
        var details = result.Details.First(detail => detail.HasErrors);
        Assert.Equal(JsonPointer.Parse("/properties/field"), details.EvaluationPath);
        Assert.Equal(JsonPointer.Parse("/field"), details.InstanceLocation);
        Assert.Collection(details.Errors!, item =>
        {
            Assert.Equal("type", item.Key);
            // match Value is "string" but should be "integer" string value can be any string
            Assert.Matches($"Value is \".+\" but should be \"{propertyType}\"", item.Value);
        });
    }

    [Fact]
    public void Schema_Date_Test()
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = "date", AllowNull = false }
        };

        var schema = this.WriteJson(SchemaConvert.Convert(fields, "profile"));


        Assert.True(this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = "2021-01-01T00:00:00.000Z" },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List, RequireFormatValidation = true }).IsValid));

        Assert.False(this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = "2021-01-01" },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List, RequireFormatValidation = true })).IsValid);


        // assert error
        var result = this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = "sdsd" },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List , RequireFormatValidation = true}));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Contains(result.Details, detail => detail.HasErrors);
        var details = result.Details.First(detail => detail.HasErrors);
        Assert.Equal(JsonPointer.Parse("/properties/field"), details.EvaluationPath);
        Assert.Equal(JsonPointer.Parse("/field"), details.InstanceLocation);
        Assert.Collection(details.Errors!, item =>
        {
            Assert.Equal("format", item.Key);
            Assert.Equal("Value does not match format \"date-time\"", item.Value);
         });

    }



    /*
     * format conversion tests
     */

    [Theory]
    [InlineData("regex('https://.*')", "https://www.google.com", "http://www.google.com")]
    [InlineData("regex('^[A-Z]{2}$')", "AS", "asds")]
    public void Schema_Format_String_Regex(string format, string validInput, string invalidInput)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Format = format, Type = "string", AllowNull = false }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.WriteJson(schema);

        Assert.True(this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = validInput },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List })).IsValid);

        Assert.False(schema.Evaluate(new JsonObject() { ["field"] = invalidInput },

            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }).IsValid);

        // assert error
        var result = this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = invalidInput },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Contains(result.Details, detail => detail.HasErrors);
        var details = result.Details.First(detail => detail.HasErrors);
        Assert.Equal(JsonPointer.Parse("/properties/field"), details.EvaluationPath);
        Assert.Equal(JsonPointer.Parse("/field"), details.InstanceLocation);
        Assert.Collection(details.Errors!, item =>
        {
            Assert.Equal("pattern", item.Key);
            Assert.Equal("The string value is not a match for the indicated regular expression", item.Value);
        });

      }






    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    [InlineData("False", false)]
    [InlineData("TRue", true)]
    [InlineData("FAlse", false)]
    [InlineData("TRUE", true)]
    [InlineData("FALSE", false)]
    [InlineData("true", "true", Skip = "TODO: fix this")]
    [InlineData("false", "false", Skip = "TODO: fix this")]
    [InlineData("true", "True", Skip = "TODO: fix this")]
    public void Test_Schema_Boolean_Format_ValidUseCases(string format, object input)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = "boolean", Format = format, AllowNull = false }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        Assert.True(this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = JsonValue.Create(input) },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List })).IsValid);
    }

    [Theory]
    [InlineData("true", false)]
    [InlineData("false", true)]
    [InlineData("True", false)]
    [InlineData("False", true)]
    [InlineData("TRue", false)]
    [InlineData("FAlse", true)]
    [InlineData("TRUE", false)]
    [InlineData("FALSE", true)]
    [InlineData("true", "false", Skip = "TODO: fix this")]
    [InlineData("false", "true", Skip = "TODO: fix this")]
    [InlineData("true", "anything", Skip = "TODO: fix this")]
    [InlineData("false", "anything", Skip = "TODO: fix this")]
    public void Test_Schema_Boolean_Format_InvalidUseCases(string format, object input)
    {
        var fields = new Dictionary<string, Field>() { ["field"] = new Field() { Type = "boolean", Format = format } };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        //assert error details
        var result = this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = JsonValue.Create(input) },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Contains(result.Details, detail => detail.HasErrors);
        var details = result.Details.First(detail => detail.HasErrors);
        Assert.Equal(JsonPointer.Parse("/properties/field"), details.EvaluationPath);
        Assert.Equal(JsonPointer.Parse("/field"), details.InstanceLocation);
        Assert.Collection(details.Errors!, item =>
        {
            Assert.Equal("enum", item.Key);
            Assert.Equal("Value should match one of the values specified by the enum", item.Value);
        });
    }

    /* range tests */
    [Theory]
    [InlineData("integer", "0-10", 5)]
    [InlineData("integer", "0-10", 0)]
    [InlineData("integer", "0-10", 10)]
    [InlineData("float", "1.2-1.7", 1.5)]
    [InlineData("float", "1.2-1.7", 1.2)]
    [InlineData("float", "1.2-1.7", 1.7)]
    [InlineData("float", "1.2-1.7", 1.2000000000000002)]
    [InlineData("float", "1.2-1.7", 1.6999999999999997)]
    [InlineData("float", "1.2-1.7", 1.2000000000000002)]
    [InlineData("long", "0-10", 5)]
    [InlineData("long", "0-10", 0)]
    [InlineData("long", "0-10", 10)]
    //long number
    [InlineData("long", "0-9223372036854775807", 9223372036854775807)]
    [InlineData("long", "0-9223372036854775807", 0)]
    //negative long number
    [InlineData("long", "-9223372036854775808-0", -9223372036854775808)]
    [InlineData("long", "-9223372036854775808-0", 0)]


    [InlineData("integer", "0-10", "5", Skip = "TODO: check this")]
    public void Test_Schema_Range_Valid(string type, string format, object input)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = type, Format = format, AllowNull = false }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        Assert.True(this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = JsonValue.Create(input) },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List })).IsValid);
    }


    [Theory]
    [InlineData("integer", "1-10", 0)]
    [InlineData("integer", "0-10", -1)]
    [InlineData("float", "1.2-1.7", 1.1)]
    [InlineData("long", "0-10", -1)]
    [InlineData("long", "0-9223372036854775807", -1)]
    [InlineData("long", "-977777709-0", -9777777099, Skip = "TODO: handle negetive ranges")]
    public void Test_Schema_Range_Invalid_Minimum(string type, string format, decimal input)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = type, Format = format, AllowNull = false }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        //assert error details
        var result = this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = input },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Contains(result.Details, detail => detail.HasErrors);
        var details = result.Details.First(detail => detail.HasErrors);
        Assert.Equal(JsonPointer.Parse("/properties/field"), details.EvaluationPath);
        Assert.Equal(JsonPointer.Parse("/field"), details.InstanceLocation);
        Assert.Collection(details.Errors!, item =>
        {
            Assert.Equal("minimum", item.Key);
            Assert.Matches($"{input} should be at least (.*)", item.Value);
        });
    }

    [Theory]
    [InlineData("integer", "0-10", 11)]
    [InlineData("float", "1.2-1.7", 1.8)]
    [InlineData("long", "0-10", 11)]
    [InlineData("long", "0-9223372036854775806", 9223372036854775807)]
    [InlineData("long", "-977777709-0", -9777777099, Skip = "TODO: handle negetive ranges")]
    public void Test_Schema_Range_Invalid_Maximum(string type, string format, decimal input)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = type, Format = format, AllowNull = false }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        //assert error details
        var result = this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = input },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Contains(result.Details, detail => detail.HasErrors);
        var details = result.Details.First(detail => detail.HasErrors);
        Assert.Equal(JsonPointer.Parse("/properties/field"), details.EvaluationPath);
        Assert.Equal(JsonPointer.Parse("/field"), details.InstanceLocation);
        Assert.Collection(details.Errors!, item =>
        {
            Assert.Equal("maximum", item.Key);
            Assert.Matches($"{input} should be at most (.*)", item.Value);
        });
    }

    /* allow null tests */

    [Theory]
    [InlineData("integer", "0-10")]
    [InlineData("integer", null)]
    [InlineData("long", "0-10")]
    [InlineData("long", null)]
    [InlineData("float", "0-10")]
    [InlineData("float", null)]
    [InlineData("string", "regex(.*)")]
    [InlineData("string", null)]
    [InlineData("date", null)]
    [InlineData("boolean", null)]
    [InlineData("boolean", "true")]
    [InlineData("boolean", "false")]
    [InlineData("boolean", "TRUE")]
    [InlineData("boolean", "FALSE")]
    public void Test_Schema_AllowNull_Valid(string type, string format)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = type, Format = format, AllowNull = true }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        Assert.True(this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = null },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List })).IsValid);
    }

    [Theory]
    [InlineData("boolean", "true")]
    [InlineData("boolean", "false")]
    [InlineData("boolean", "TRUE")]
    [InlineData("boolean", "FALSE")]
    [InlineData("boolean", null)]
    [InlineData("integer", "1-2")]
    [InlineData("integer", null)]
    [InlineData("float", "1.5-9.5")]
    [InlineData("float", null)]
    [InlineData("binary", null)]
    [InlineData("string", "regex(.*)")]
    [InlineData("string", null)]
    [InlineData("date", null)]
    public void Test_Schema_AllowNullFalse_Invalid(string type, string format)
    {
        var fields = new Dictionary<string, Field>()
        {
            ["field"] = new Field() { Type = type, Format = format, AllowNull = false }
        };

        var schema = SchemaConvert.Convert(fields, "profile");
        this.TestOutputHelper.WriteLine(JsonSerializer.Serialize(schema,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false, WriteIndented = true }));

        //assert error details

        var result = this.WriteJson(schema.Evaluate(new JsonObject() { ["field"] = null },
            new() { EvaluateAs = SpecVersion.Draft202012, OutputFormat = OutputFormat.List }));

        Assert.False(result.IsValid);
        Assert.NotNull(result.Details);
        Assert.Contains(result.Details, detail => detail.HasErrors);
        var details = result.Details.First(detail => detail.HasErrors);
        Assert.Equal(JsonPointer.Parse("/properties/field"), details.EvaluationPath);
        Assert.Equal(JsonPointer.Parse("/field"), details.InstanceLocation);

        Assert.Contains(details.Errors!, item => item.Key == "type");

        // TODO: fix this
        // Assert.Collection(details.Errors!, item =>
        // {
        //     Assert.Equal("type", item.Key);
        //     Assert.Equal("The value should be of type: boolean", item.Value);
        // });
    }


    private T WriteJson<T>(T json, [CallerArgumentExpression(nameof(json))] string name = "json")
    {
        this.TestOutputHelper.WriteLine(json + ": " + JsonSerializer.Serialize(json,
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }));

        return json;
    }


    /// <inheritdoc />
    public SchemaConvertTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }
}
