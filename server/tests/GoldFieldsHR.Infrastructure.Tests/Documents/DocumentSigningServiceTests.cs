using GoldFieldsHR.Infrastructure.Documents;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Documents;

public class DocumentSigningServiceTests
{
    // 1x1 transparent PNG.
    private static readonly byte[] SamplePng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

    [Fact]
    public void StampSignature_OnExistingPdf_ProducesLargerValidPdf()
    {
        var sourcePdf = BuildSamplePdf();
        var service = new DocumentSigningService();

        var stamped = service.StampSignature(sourcePdf, "application/pdf", SamplePng, "Signed by Test on 1 Jan 2026");

        Assert.NotEmpty(stamped);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(stamped, 0, 4));
    }

    [Fact]
    public void StampSignature_OnImageAttachment_WrapsIntoAPdf()
    {
        var service = new DocumentSigningService();

        var stamped = service.StampSignature(SamplePng, "image/png", SamplePng, "Signed by Test on 1 Jan 2026");

        Assert.NotEmpty(stamped);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(stamped, 0, 4));
    }

    private static byte[] BuildSamplePdf()
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        using var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawString("Sample document", new XFont("Arial", 12, XFontStyleEx.Regular), XBrushes.Black, 20, 20);

        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }
}
