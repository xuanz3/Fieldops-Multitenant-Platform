using System.Security.Cryptography;
using FieldOps.Domain.Attachments;

namespace FieldOps.UnitTests;

public sealed class WorkOrderAttachmentTests
{
    [Fact]
    public void Attachment_records_size_and_sha256()
    {
        var content =
            "evidence"u8.ToArray();

        var attachment =
            new WorkOrderAttachment(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "completion.txt",
                "text/plain",
                content,
                Guid.NewGuid(),
                "Test Technician");

        Assert.Equal(
            content.LongLength,
            attachment.SizeBytes);

        Assert.Equal(
            Convert.ToHexString(
                SHA256.HashData(content)),
            attachment.Sha256);
    }

    [Fact]
    public void Attachment_strips_path_from_file_name()
    {
        var attachment =
            new WorkOrderAttachment(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "../unsafe/completion.txt",
                "text/plain",
                "safe"u8.ToArray(),
                Guid.NewGuid(),
                "Test Technician");

        Assert.Equal(
            "completion.txt",
            attachment.FileName);
    }

    [Fact]
    public void Attachment_rejects_content_over_five_megabytes()
    {
        var content =
            new byte[
                WorkOrderAttachment
                    .MaximumFileSizeBytes +
                1];

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new WorkOrderAttachment(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "large.pdf",
                        "application/pdf",
                        content,
                        Guid.NewGuid(),
                        "Test Technician"));

        Assert.Contains(
            "5 MB",
            exception.Message);
    }
}
