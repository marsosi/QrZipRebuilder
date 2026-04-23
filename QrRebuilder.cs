using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using ZXing;
using ZXing.Common;

namespace QrZipRebuilder;

public sealed class RebuildResult
{
    public bool Success { get; init; }
    public string? OutputPath { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class OrderedImageRow
{
    public int Ordem { get; init; }
    public string NomeFicheiro { get; init; } = "";
    public string Tamanho { get; init; } = "";
    public string ChaveOrdenacao { get; init; } = "—";
}

public static class QrRebuilder
{
    private static readonly Regex DigitGroupRegex = new(@"\d+", RegexOptions.Compiled);

    public static IReadOnlyList<OrderedImageRow> ListarImagensOrdenadas(string? inputDir)
    {
        if (string.IsNullOrWhiteSpace(inputDir) || !Directory.Exists(inputDir))
            return Array.Empty<OrderedImageRow>();

        var fullInput = Path.GetFullPath(inputDir);
        var files = Directory.EnumerateFiles(fullInput)
            .Where(IsImageFile)
            .Select(p => new FileInfo(p))
            .OrderBy(f => GetNumericOrderKey(f.Name), new NumericListComparer())
            .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var ordem = 0;
        return files.Select(f =>
        {
            ordem++;
            var key = GetNumericOrderKey(f.Name);
            var chave = key.Count == 0 ? "—" : string.Join(" · ", key);
            return new OrderedImageRow
            {
                Ordem = ordem,
                NomeFicheiro = f.Name,
                Tamanho = FormatTamanho(f.Length),
                ChaveOrdenacao = chave
            };
        }).ToList();
    }

    private static string FormatTamanho(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    public static async Task<RebuildResult> RebuildAsync(
        string inputDir,
        IProgress<string>? log,
        CancellationToken cancellationToken = default)
    {
        void L(string s) => log?.Report(s);

        if (string.IsNullOrWhiteSpace(inputDir) || !Directory.Exists(inputDir))
        {
            return new RebuildResult
            {
                Success = false,
                ErrorMessage = "Invalid or missing folder."
            };
        }

        var fullInput = Path.GetFullPath(inputDir);
        var outputPath = Path.Combine(fullInput, "codigo_recuperado.zip");
        L($"Folder: {fullInput}");
        L($"Output:  {outputPath}");
        L("");

        var imageFiles = Directory.EnumerateFiles(fullInput)
            .Where(IsImageFile)
            .Select(path => new FileInfo(path))
            .OrderBy(f => GetNumericOrderKey(f.Name), new NumericListComparer())
            .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (imageFiles.Count == 0)
        {
            return new RebuildResult
            {
                Success = false,
                ErrorMessage = "No images found in the selected folder."
            };
        }

        var reader = new BarcodeReaderGeneric
        {
            Options = new DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                TryHarder = true,
                TryInverted = true
            }
        };

        var base64Builder = new StringBuilder();
        var total = imageFiles.Count;
        var sucesso = 0;
        var indice = 0;

        foreach (var file in imageFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            indice++;
            L($"[{indice}/{total}] {file.Name} ...");
            var texto = TryDecodeQrFromImage(file.FullName, reader, out var erro);
            if (texto is null)
            {
                L($"    Failed: {erro}");
                continue;
            }

            base64Builder.Append(texto);
            sucesso++;
            L("    OK");
        }

        L("");
        L($"Summary: {sucesso}/{total} read(s) OK.");

        if (sucesso < total)
        {
            return new RebuildResult
            {
                Success = false,
                ErrorMessage = "Cannot build the ZIP: all images must be read successfully."
            };
        }

        var base64Completo = base64Builder.ToString().Trim();
        if (string.IsNullOrEmpty(base64Completo))
        {
            return new RebuildResult
            {
                Success = false,
                ErrorMessage = "Resulting Base64 string is empty."
            };
        }

        try
        {
            var bytes = Convert.FromBase64String(base64Completo);
            await File.WriteAllBytesAsync(outputPath, bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (System.FormatException ex)
        {
            return new RebuildResult
            {
                Success = false,
                ErrorMessage = "Invalid Base64: " + ex.Message
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new RebuildResult
            {
                Success = false,
                ErrorMessage = "Error writing file: " + ex.Message
            };
        }

        var tamanhoMb = new FileInfo(outputPath).Length / (1024.0 * 1024.0);
        L("");
        L($"Saved: {tamanhoMb:F2} MB");

        return new RebuildResult
        {
            Success = true,
            OutputPath = outputPath
        };
    }

    private static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".tif" or ".tiff" or ".webp";
    }

    private static IReadOnlyList<int> GetNumericOrderKey(string fileName)
    {
        var name = Path.GetFileName(fileName);
        var list = new List<int>();
        foreach (Match m in DigitGroupRegex.Matches(name))
        {
            if (m.Success && int.TryParse(m.Value, out var n))
                list.Add(n);
        }

        return list;
    }

    private static string? TryDecodeQrFromImage(string fullPath, BarcodeReaderGeneric reader, out string? erro)
    {
        erro = null;

        using var bgr = Cv2.ImRead(fullPath, ImreadModes.Color);
        if (bgr.Empty())
        {
            erro = "Could not load image (empty or corrupt).";
            return null;
        }

        if (TentarDecodificarBgr(bgr, reader, out var texto) && !string.IsNullOrEmpty(texto))
            return texto;

        if (Math.Max(bgr.Cols, bgr.Rows) < 2200)
        {
            using var ampli = new Mat();
            Cv2.Resize(bgr, ampli, new OpenCvSharp.Size(), 2, 2, InterpolationFlags.Cubic);
            if (TentarDecodificarBgr(ampli, reader, out texto) && !string.IsNullOrEmpty(texto))
                return texto;
        }

        if (Math.Max(bgr.Cols, bgr.Rows) < 1800)
        {
            using var mid = new Mat();
            Cv2.Resize(bgr, mid, new OpenCvSharp.Size(), 1.5, 1.5, InterpolationFlags.Cubic);
            if (TentarDecodificarBgr(mid, reader, out texto) && !string.IsNullOrEmpty(texto))
                return texto;
        }

        erro = "QR not detected (tried several thresholds, CLAHE, Otsu, inversion, rotation, 2x upscale). Try closer, sharper focus, less screen glare.";
        return null;
    }

    private static bool TentarDecodificarBgr(Mat bgr, BarcodeReaderGeneric reader, out string? texto)
    {
        texto = null;
        using var cinza = new Mat();
        Cv2.CvtColor(bgr, cinza, ColorConversionCodes.BGR2GRAY);
        if (TentarVariantesEmCinza(reader, cinza, out texto) && !string.IsNullOrEmpty(texto))
            return true;
        for (var k = 1; k < 4; k++)
        {
            using var rodado = new Mat();
            if (k == 1)
                Cv2.Rotate(cinza, rodado, RotateFlags.Rotate90Clockwise);
            else if (k == 2)
                Cv2.Rotate(cinza, rodado, RotateFlags.Rotate180);
            else
            {
                using var a = new Mat();
                using var b2 = new Mat();
                Cv2.Rotate(cinza, a, RotateFlags.Rotate90Clockwise);
                Cv2.Rotate(a, b2, RotateFlags.Rotate90Clockwise);
                Cv2.Rotate(b2, rodado, RotateFlags.Rotate90Clockwise);
            }

            if (TentarVariantesEmCinza(reader, rodado, out texto) && !string.IsNullOrEmpty(texto))
                return true;
        }

        texto = null;
        return false;
    }

    private static string? QrSaida(Result? r) =>
        (r is not null && r.BarcodeFormat == BarcodeFormat.QR_CODE) ? r.Text : null;

    private static bool TentarVariantesEmCinza(BarcodeReaderGeneric reader, Mat cinza, out string? texto)
    {
        texto = null;
        using var bin = new Mat();

        var adaps = new (AdaptiveThresholdTypes At, int Bl, int C, ThresholdTypes Tt)[]
        {
            (AdaptiveThresholdTypes.GaussianC, 11, 2, ThresholdTypes.Binary),
            (AdaptiveThresholdTypes.GaussianC, 9, 2, ThresholdTypes.Binary),
            (AdaptiveThresholdTypes.GaussianC, 15, 3, ThresholdTypes.Binary),
            (AdaptiveThresholdTypes.GaussianC, 21, 4, ThresholdTypes.Binary),
            (AdaptiveThresholdTypes.GaussianC, 31, 5, ThresholdTypes.Binary),
            (AdaptiveThresholdTypes.MeanC, 11, 2, ThresholdTypes.Binary),
            (AdaptiveThresholdTypes.MeanC, 15, 3, ThresholdTypes.Binary),
            (AdaptiveThresholdTypes.GaussianC, 11, 2, ThresholdTypes.BinaryInv),
            (AdaptiveThresholdTypes.GaussianC, 15, 3, ThresholdTypes.BinaryInv),
        };

        foreach (var (at, bl, c, tt) in adaps)
        {
            Cv2.AdaptiveThreshold(cinza, bin, 255, at, tt, bl, c);
            var t = QrSaida(DecodeWithReader(reader, bin, cinza, out _));
            if (!string.IsNullOrEmpty(t)) { texto = t; return true; }
        }

        using (var o = new Mat())
        {
            Cv2.Threshold(cinza, o, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            var t = QrSaida(DecodeWithReader(reader, o, cinza, out _));
            if (!string.IsNullOrEmpty(t)) { texto = t; return true; }
        }

        using (var o = new Mat())
        {
            Cv2.Threshold(cinza, o, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.BinaryInv);
            var t = QrSaida(DecodeWithReader(reader, o, cinza, out _));
            if (!string.IsNullOrEmpty(t)) { texto = t; return true; }
        }

        using (var clahe = Cv2.CreateCLAHE(2.5, new OpenCvSharp.Size(8, 8)))
        using (var cj = new Mat())
        {
            clahe.Apply(cinza, cj);
            Cv2.AdaptiveThreshold(cj, bin, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);
            var t = QrSaida(DecodeWithReader(reader, bin, cj, out _));
            if (!string.IsNullOrEmpty(t)) { texto = t; return true; }
        }

        using (var suave = new Mat())
        {
            Cv2.GaussianBlur(cinza, suave, new OpenCvSharp.Size(3, 3), 0);
            Cv2.AdaptiveThreshold(suave, bin, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);
            var t = QrSaida(DecodeWithReader(reader, bin, suave, out _));
            if (!string.IsNullOrEmpty(t)) { texto = t; return true; }
        }

        using (var bilat = new Mat())
        {
            Cv2.BilateralFilter(cinza, bilat, 5, 50, 50);
            Cv2.AdaptiveThreshold(bilat, bin, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);
            var t = QrSaida(DecodeWithReader(reader, bin, bilat, out _));
            if (!string.IsNullOrEmpty(t)) { texto = t; return true; }
        }

        return false;
    }

    private static Result? DecodeWithReader(BarcodeReaderGeneric reader, Mat binary, Mat gray, out string? erro)
    {
        erro = null;

        var w = binary.Cols;
        var h = binary.Rows;
        if (w <= 0 || h <= 0)
        {
            erro = "Invalid dimensions after image processing.";
            return null;
        }

        var thBytes = MatToGrayscale8u(binary);
        if (thBytes is not null)
        {
            var th = reader.Decode(thBytes, w, h, RGBLuminanceSource.BitmapFormat.Gray8);
            if (th is not null && th.BarcodeFormat == BarcodeFormat.QR_CODE)
                return th;
        }

        if (!ReferenceEquals(gray, binary))
        {
            var gBytes = MatToGrayscale8u(gray);
            if (gBytes is not null)
            {
                var resG = reader.Decode(gBytes, w, h, RGBLuminanceSource.BitmapFormat.Gray8);
                if (resG is not null && resG.BarcodeFormat == BarcodeFormat.QR_CODE)
                    return resG;
            }
        }

        if (thBytes is not null)
        {
            var inv = new InvertedLuminanceSource(new RGBLuminanceSource(thBytes, w, h, RGBLuminanceSource.BitmapFormat.Gray8));
            var rInv = reader.Decode(inv);
            if (rInv is not null && rInv.BarcodeFormat == BarcodeFormat.QR_CODE)
                return rInv;
        }

        var rBmp = TryDecodeFromBitmapZxing(reader, binary) ?? TryDecodeFromBitmapZxing(reader, gray);
        if (rBmp is not null && rBmp.BarcodeFormat == BarcodeFormat.QR_CODE)
            return rBmp;

        erro = "QR not detected (Gray8, invert, and bitmap path).";
        return null;
    }

    private static Result? TryDecodeFromBitmapZxing(BarcodeReaderGeneric reader, Mat mat8u1)
    {
        if (mat8u1.Empty() || mat8u1.Cols < 1 || mat8u1.Rows < 1) return null;
        using var bitmap = mat8u1.ToBitmap();
        using var b32 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(b32))
        {
            g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }

        if (!LockBitsBgra32(b32, out var data, out var w, out var h) || data is null) return null;
        return reader.Decode(data, w, h, RGBLuminanceSource.BitmapFormat.BGRA32);
    }

    private static bool LockBitsBgra32(Bitmap b32, out byte[]? data, out int w, out int h)
    {
        data = null;
        w = b32.Width;
        h = b32.Height;
        if (w <= 0 || h <= 0) return false;

        var rect = new Rectangle(0, 0, w, h);
        var d = b32.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var len = w * h * 4;
            data = new byte[len];
            if (d.Stride == w * 4)
            {
                Marshal.Copy(d.Scan0, data, 0, len);
            }
            else
            {
                for (var y = 0; y < h; y++)
                {
                    var row = IntPtr.Add(d.Scan0, y * d.Stride);
                    Marshal.Copy(row, data, y * w * 4, w * 4);
                }
            }

            return true;
        }
        finally
        {
            b32.UnlockBits(d);
        }
    }

    private static byte[]? MatToGrayscale8u(Mat mat)
    {
        if (mat.Channels() != 1) return null;
        var w = mat.Cols;
        var h = mat.Rows;
        if (w <= 0 || h <= 0) return null;
        var buf = new byte[w * h];
        for (var r = 0; r < h; r++)
        {
            using var row = mat.Row(r);
            var ptr = row.Data;
            if (ptr == IntPtr.Zero) return null;
            Marshal.Copy(ptr, buf, r * w, w);
        }

        return buf;
    }
}

internal sealed class NumericListComparer : IComparer<IReadOnlyList<int>>
{
    public int Compare(IReadOnlyList<int>? x, IReadOnlyList<int>? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        var len = Math.Max(x.Count, y.Count);
        for (var i = 0; i < len; i++)
        {
            var vx = i < x.Count ? x[i] : 0;
            var vy = i < y.Count ? y[i] : 0;
            var c = vx.CompareTo(vy);
            if (c != 0) return c;
        }

        return 0;
    }
}
