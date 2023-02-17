using System.Globalization;
using QRCoder;

namespace QR_Code_Generator;

public struct Size
{
    public int Width;
    public int Height;
    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
public struct CMYKColor
{
    public byte C;
    public byte M;
    public byte Y;
    public byte K;
    public static readonly CMYKColor Black = new() { C = 0, M = 0, Y = 0, K = 255 };
    public static readonly CMYKColor White = new() { C = 0, M = 0, Y = 0, K = 0 };
}

public class PostscriptQRCode : AbstractQRCode, IDisposable
{
    /// <summary>
    /// Constructor without params to be used in COM Objects connections
    /// </summary>
    public PostscriptQRCode() { }
    public PostscriptQRCode(QRCodeData data) : base(data) { }

    public string GetGraphic(int pointsPerModule, CMYKColor darkColor, CMYKColor lightColor, bool drawQuietZones = true, bool epsFormat = false)
    {
        Size viewBox = new(pointsPerModule * QrCodeData.ModuleMatrix.Count, pointsPerModule * QrCodeData.ModuleMatrix.Count);
        return GetGraphic(viewBox, darkColor, lightColor, drawQuietZones, epsFormat);
    }

    public string GetGraphic(Size viewBox, CMYKColor darkColor, CMYKColor lightColor, bool drawQuietZones = true, bool epsFormat = false)
    {

        int offset = drawQuietZones ? 0 : 4;
        int drawableModulesCount = QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : offset * 2);
        double pointsPerModule = Math.Min(viewBox.Width, viewBox.Height) / (double)drawableModulesCount;

        string psFile = string.Format(CultureInfo.InvariantCulture, psHeader,
            DateTime.Now.ToString("s"), viewBox.Width, pointsPerModule,
            epsFormat ? "EPSF-3.0" : string.Empty
        );
        psFile += string.Format(CultureInfo.InvariantCulture, psFunctions,
            darkColor.C / 255.0, darkColor.M / 255.0, darkColor.Y / 255.0, darkColor.K / 255.0,
            lightColor.C / 255.0, lightColor.M / 255.0, lightColor.Y / 255.0, lightColor.K / 255.0,
            drawableModulesCount
        );

        for (int xi = offset; xi < offset + drawableModulesCount; xi++)
        {
            if (xi > offset)
            {
                psFile += "nl\n";
            }

            for (int yi = offset; yi < offset + drawableModulesCount; yi++)
            {
                psFile += QrCodeData.ModuleMatrix[xi][yi] ? "f " : "b ";
            }
            psFile += "\n";
        }
        return psFile + psFooter;
    }

    private const string psHeader = @"%!PS-Adobe-3.0 {3}
%%Creator: QRCoder.NET
%%Title: QRCode
%%CreationDate: {0}
%%DocumentData: Clean7Bit
%%Origin: 0
%%DocumentMedia: Default {1} {1} 0 () ()
%%BoundingBox: 0 0 {1} {1}
%%LanguageLevel: 2 
%%Pages: 1
%%Page: 1 1
%%DocumentProcessColors: Cyan Magenta Yellow Black
%%EndComments
%%BeginConstants
/sz {1} def
/sc {2} def
%%EndConstants
%%BeginFeature: *PageSize Default
<< /PageSize [ sz sz ] /ImagingBBox null >> setpagedevice
%%EndFeature
";

    private const string psFunctions = @"%%BeginFunctions 
/csquare {{
    newpath
    0 0 moveto
    0 1 rlineto
    1 0 rlineto
    0 -1 rlineto
    closepath
    setcmykcolor
    fill
}} def
/f {{ 
    gsave
    true setoverprint
    {0} {1} {2} {3} csquare
    grestore
    1 0 translate
}} def
/b {{ 
    1 0 translate
}} def 
/background {{ 
    {4} {5} {6} {7} csquare 
}} def
/nl {{
    -{8} -1 translate
}} def
%%EndFunctions
%%BeginBody
0 0 moveto
gsave
sz sz scale
background
grestore
gsave
sc sc scale
0 {8} 1 sub translate
";

    private const string psFooter = @"%%EndBody
grestore
showpage   
%%EOF
";
}

