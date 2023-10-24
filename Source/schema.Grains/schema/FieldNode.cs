namespace schema.Grains;

using Json.Schema;

public class FieldNode : Dictionary<string, FieldNode>
{
    /// <inheritdoc />
    public FieldNode(IEnumerable<KeyValuePair<string, FieldNode>> collection, string name, string id) : base(collection)
    {
        this.Name = name;
        this.Id = id;
    }

    public Field? Field { get; init; }
    public string Name { get; init; }
    public string Id { get; init; }

    public Field[] Descends => this.Values.SelectMany(x => x.Descends)
        .Concat(this.Field is not null ? new[] { this.Field } : Enumerable.Empty<Field>()).ToArray();

    public JsonSchemaBuilder Apply(JsonSchemaBuilder schemaBuilder)
    {
        schemaBuilder.Title(this.Name);
        schemaBuilder.Description(this.Name);
        schemaBuilder.Id(this.Id);
        schemaBuilder.Anchor(this.Name);
        if (this.Field is not null)
        {
            ApplyFieldSchema(this.Field, schemaBuilder);
            ApplyUnrecognizedFields(this.Field, schemaBuilder);
        }

        if (this.Values.Any())
        {
            schemaBuilder
                .Properties(this.Values.Select(v => (v.Name, v.Apply(new JsonSchemaBuilder()).Build()))
                .ToArray());
            // TODO: Required fields
            // schemaBuilder.Required(
            //     this.Values.Where(v => v.Descends.Any(x => x.Required)).Select(v => v.Name).ToArray());
        }

        return schemaBuilder;

        static void ApplyUnrecognizedFields(Field field, JsonSchemaBuilder schemaBuilder)
        {
            if (field?.Required is not null)
                schemaBuilder.Unrecognized("x-field-required-for-login", field.Required);
            if (field?.WriteAccess is not null)
                schemaBuilder.Unrecognized("x-field-write-access", field.WriteAccess);
            if (field?.Deleted is not null)
                schemaBuilder.Unrecognized("x-field-deleted", field.Deleted);
            if (field?.Encrypt is not null)
                schemaBuilder.Unrecognized("x-field-encrypt", field.Encrypt);
            if (field?.EncryptedNonSearchable is not null)
                schemaBuilder.Unrecognized("x-field-encrypted-non-searchable", field.EncryptedNonSearchable);
        }

        static JsonSchema ApplyFieldSchema(Field field, JsonSchemaBuilder schemaBuilder)
        {
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
             * format - Allows assigning a regular expression (regex) that defines the format of this data field.
             * SAP Customer Data Cloud will check the value passed by users to this data field to make sure it answers to the defined format.
             *  The format property accepts a string of the form: "regex('<regex-pattern>')".
             * This property also accepts, for Boolean fields: true or false; e.g., "format":"true".
             * This property also accepts, for numbers fields: range values. e.g. 0-10, also negative values -20-27
             */
            _ = field switch
            {
                { Type: "boolean" } => BooleanSchema(),
                { Type: "date" } => DateSchema(),
                { Type: "string" or "basic-string" or "text" } => StringSchema(),
                { Type: "float" or "long" or "integer" } => NumberSchema(),
                { Type: "object" } => ObjectSchema(),
                {Type: "binary"} => BinarySchema(),
                _ => ObjectSchema(),
            };

            return schemaBuilder.Build();


            JsonSchemaBuilder DateSchema() => schemaBuilder.Type(AddNull(SchemaValueType.String)).Format(Formats.DateTime);

            JsonSchemaBuilder NumberSchema()
            {
                _ = field switch
                {
                    { Type: "float" or "long" } => schemaBuilder.Type(AddNull(SchemaValueType.Number)),
                    { Type: "integer" } => schemaBuilder.Type(AddNull(SchemaValueType.Integer)),
                    _ => schemaBuilder,
                };

                if (field.Format?.Split('-') is [var minimumString, var maximumString])
                {
                    _ = decimal.TryParse(minimumString, out var minimum);
                    _ = decimal.TryParse(maximumString, out var maximum);
                    schemaBuilder.Minimum(minimum).Maximum(maximum);
                }

                return schemaBuilder;
            }

            JsonSchemaBuilder BinarySchema() => schemaBuilder.Type(AddNull(SchemaValueType.String)).Format("base64");


            JsonSchemaBuilder ObjectSchema()
            {
                return schemaBuilder
                    .Type(AddNull(SchemaValueType.Object))
                    .AdditionalProperties(field.Dynamic ?? false);
                ;
            }


            JsonSchemaBuilder StringSchema()
            {
                schemaBuilder.Type(AddNull(SchemaValueType.String));

                if (field.Format is not null and var format)
                {
                    schemaBuilder.Pattern(RemoveSuffix(RemovePrefix(format,"regex('"),"')"));
                }

                if (field.Format is ['(', '\'', 'r', 'e', 'g', 'e', 'x', '\'', .. var pattern, '\'', ')'])
                {
                    schemaBuilder.Pattern(pattern);
                }

                static string RemovePrefix(string str, string prefix, StringComparison comparison = StringComparison.Ordinal) => str.StartsWith(prefix, comparison) ? str[prefix.Length..] : str;

                static string RemoveSuffix(string str, string suffix, StringComparison comparison = StringComparison.Ordinal) => str.EndsWith(suffix, comparison) ? str.Remove(str.Length - suffix.Length) : str;


                return schemaBuilder;
            }

            JsonSchemaBuilder BooleanSchema()
            {
                schemaBuilder.Type(AddNull(SchemaValueType.Boolean));
                _ = (field.Format?.ToLowerInvariant(), field.AllowNull) switch
                {
                    ("true", false) => schemaBuilder.Const(true),
                    ("false", false) => schemaBuilder.Const(false),
                    ("true", true) => schemaBuilder.Enum(true, null),
                    ("false", true) => schemaBuilder.Enum(false, null),
                    _ => schemaBuilder,
                };

                return schemaBuilder;
            }

            SchemaValueType AddNull(SchemaValueType type) =>
                field switch
                {
                    { AllowNull: false } => type,
                    _ => type | SchemaValueType.Null, // allow null by default
                };


        }
    }
}
