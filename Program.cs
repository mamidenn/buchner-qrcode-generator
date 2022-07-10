using QR_Code_Generator;
using QRCoder;
using Spectre.Console;

AnsiConsole.MarkupLine("[bold underline cyan]QR Code Generator[/]");
AnsiConsole.MarkupLine("");
string bn = AnsiConsole.Prompt(
    new TextPrompt<string>("Für welche [bold yellow]BN[/] sollen QR Codes erstellt werden?")
);
uint amount = AnsiConsole.Prompt(
    new TextPrompt<uint>("[bold yellow]Wie viele[/] QR Codes sollen generiert werden?")
        .DefaultValue<uint>(50)
        .ValidationErrorMessage("[magenta]Bitte eine Zahl > 0 eingeben[/]")
);

bn = Uri.EscapeDataString(bn);
DirectoryInfo outputDir = Directory.CreateDirectory(bn);

const string format = "https://ccbuchner.de/clip_code-{0}-{1:D2}/";
const string filenameFormat = "{0}_{1:D2}.eps";
using QRCodeGenerator qRCodeGenerator = new();

AnsiConsole.Progress().Start(ctx =>
{
    var generator = ctx.AddTask("Erstelle QR Codes", true, amount);
    for (int k = 1; k <= amount; k++)
    {
        string payload = string.Format(format, bn, k);
        using QRCodeData qrCodeData = qRCodeGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using PostscriptQRCode qrCode = new(qrCodeData);
        string postscript = qrCode.GetGraphic(10, CMYKColor.Black, CMYKColor.White, epsFormat: true);
        string filename = Path.Combine(outputDir.FullName, string.Format(filenameFormat, bn, k));
        File.WriteAllText(filename, postscript);
        generator.Increment(1);
    }
});
AnsiConsole.MarkupLine("QR Codes generiert in [cyan]{0}[/]", outputDir.FullName);
Console.ReadKey();
