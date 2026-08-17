namespace GoldFieldsHR.Application.Common;

/// <summary>
/// Signatures travel over the wire as PNG data URIs (what a &lt;canvas&gt;
/// signature pad exports) and are stored in the database as raw bytes.
/// </summary>
public static class SignatureImageCodec
{
    private const string DataUriPrefix = "base64,";

    public static byte[] Decode(string signaturePngBase64)
    {
        var index = signaturePngBase64.IndexOf(DataUriPrefix, StringComparison.Ordinal);
        var raw = index >= 0 ? signaturePngBase64[(index + DataUriPrefix.Length)..] : signaturePngBase64;
        return Convert.FromBase64String(raw);
    }

    public static string Encode(byte[] signatureImageData) =>
        $"data:image/png;base64,{Convert.ToBase64String(signatureImageData)}";
}
