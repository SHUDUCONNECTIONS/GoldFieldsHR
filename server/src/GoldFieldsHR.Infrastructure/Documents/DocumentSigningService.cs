using GoldFieldsHR.Application.Documents;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace GoldFieldsHR.Infrastructure.Documents;

public class DocumentSigningService : IDocumentSigningService
{
    private const double MarginPoints = 24;
    private const double SignatureWidthPoints = 140;

    public byte[] StampSignature(byte[] sourceBytes, string sourceContentType, byte[] signaturePng, string signerCaption)
    {
        using var document = sourceContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            ? OpenExistingPdf(sourceBytes)
            : WrapImageInPdf(sourceBytes);

        using var signatureStream = ToPubliclyVisibleStream(signaturePng);
        using var signatureImage = XImage.FromStream(signatureStream);
        var signatureHeight = SignatureWidthPoints * signatureImage.PointHeight / signatureImage.PointWidth;

        var font = new XFont("Arial", 8, XFontStyleEx.Regular);

        foreach (var page in document.Pages.Cast<PdfPage>())
        {
            using var gfx = XGraphics.FromPdfPage(page);
            var x = page.Width.Point - MarginPoints - SignatureWidthPoints;
            var y = page.Height.Point - MarginPoints - signatureHeight - 12;

            gfx.DrawImage(signatureImage, x, y, SignatureWidthPoints, signatureHeight);
            gfx.DrawString(
                signerCaption, font, XBrushes.Black,
                new XRect(x, y + signatureHeight + 2, SignatureWidthPoints, 12),
                XStringFormats.TopLeft);
        }

        using var output = new MemoryStream();
        document.Save(output);
        return output.ToArray();
    }

    private static PdfDocument OpenExistingPdf(byte[] sourceBytes)
    {
        using var input = ToPubliclyVisibleStream(sourceBytes);
        return PdfReader.Open(input, PdfDocumentOpenMode.Modify);
    }

    private static PdfDocument WrapImageInPdf(byte[] sourceBytes)
    {
        var document = new PdfDocument();
        using var imageStream = ToPubliclyVisibleStream(sourceBytes);
        using var image = XImage.FromStream(imageStream);

        var page = document.AddPage();
        page.Width = XUnit.FromPoint(image.PointWidth);
        page.Height = XUnit.FromPoint(image.PointHeight);

        using var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawImage(image, 0, 0, image.PointWidth, image.PointHeight);

        return document;
    }

    // PdfSharp's XImage.FromStream needs GetBuffer() access, which the plain
    // new MemoryStream(byte[]) constructor doesn't grant by default.
    private static MemoryStream ToPubliclyVisibleStream(byte[] bytes) =>
        new(bytes, 0, bytes.Length, writable: false, publiclyVisible: true);
}
