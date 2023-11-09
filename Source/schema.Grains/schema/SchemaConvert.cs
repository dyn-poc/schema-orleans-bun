namespace schema.Grains;

using Json.Schema.Generation;

public record Field
{
    [Required]
    public string Type { get; init; }

    public bool? Required { get; set; }

    public string? Format { get; set; }

    public string WriteAccess { get; set; } = "serverOnly";


    public bool? Deleted { get; set; }

    public bool AllowNull { get; set; } = true;


    public bool? Dynamic { get; set; }

    public string? Encrypt { get; set; }

    public bool? EncryptedNonSearchable { get; set; }
}

