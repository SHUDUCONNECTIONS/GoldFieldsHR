namespace GoldFieldsHR.Application.Documents;

public interface IDocumentSigningService
{
    /// <summary>
    /// Stamps a signature image (plus a signer/date caption) onto a document.
    /// PDFs are stamped on every page in place; other supported file types
    /// (JPEG/PNG) are wrapped into a single-page PDF with the signature
    /// overlaid, so the result is always a PDF.
    /// </summary>
    byte[] StampSignature(byte[] sourceBytes, string sourceContentType, byte[] signaturePng, string signerCaption);
}
