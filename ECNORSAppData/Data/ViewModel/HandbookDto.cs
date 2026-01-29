namespace ECNORSApp.Data.DTO;

public sealed class HandbookDto
{
    public long Id { get; set; }

    public string OriginalName { get; set; } = "";

    public string ContentType { get; set; } = "";

    public long SizeBytes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

}
public sealed class HandbookUploadResponseDto
{
    public long Id { get; set; }

    public string OriginalName { get; set; } = "";
}
