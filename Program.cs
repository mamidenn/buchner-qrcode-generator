using Microsoft.Extensions.Configuration;
using QR_Code_Generator;
using QRCoder;
using Spectre.Console;

IConfigurationRoot configurationRoot = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddInMemoryCollection(new Dictionary<string, string> {
        { "UrlFormat", "https://ccbuchner.de/clip_code-{0}-{1:D2}/" },
        { "FilenameFormat", "{0}_{1:D2}.eps" },
        { "DefaultAmount", "50" }
    })
    .AddJsonFile("appsettings.json", true, true)
    .Build();

AnsiConsole.MarkupLine("[bold underline cyan]QR Code Generator[/]");
AnsiConsole.MarkupLine("");
string bn = AnsiConsole.Prompt(
    new TextPrompt<string>("Für welche [bold yellow]BN[/] sollen QR Codes erstellt werden?")
);
uint amount = AnsiConsole.Prompt(
    new TextPrompt<uint>("[bold yellow]Wie viele[/] QR Codes sollen generiert werden?")
        .DefaultValue(uint.Parse(configurationRoot["DefaultAmount"]))
        .ValidationErrorMessage("[magenta]Bitte eine Zahl > 0 eingeben[/]")
);

bn = Uri.EscapeDataString(bn);
DirectoryInfo outputDir = Directory.CreateDirectory(bn);

string format = configurationRoot["UrlFormat"];
string filenameFormat = configurationRoot["FilenameFormat"];
using QRCodeGenerator qRCodeGenerator = new();

AnsiConsole.Progress().Start(ctx =>
{
    var generator = ctx.AddTask("Erstelle QR Codes", true, amount);
    for (int k = 1; k <= amount; k++)
    {
        string payload = string.Format(format, bn, k);
        string filename = string.Format(filenameFormat, bn, k);
        CreateQrCode(filename, payload);
        generator.Increment(1);
    }
});
AnsiConsole.MarkupLine("QR Codes generiert in [cyan]{0}[/]", outputDir.FullName);
Console.ReadKey();

void CreateQrCode(string filename, string payload)
{
    using QRCodeData qrCodeData = qRCodeGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
    using PostscriptQRCode qrCode = new(qrCodeData);
    string postscript = qrCode.GetGraphic(10, CMYKColor.Black, CMYKColor.White, epsFormat: true);
    postscript = postscript.Replace("%%Title: QRCode", "%%Title: " + filename);
    string fullname = Path.Combine(outputDir.FullName, filename + ".eps");
    File.WriteAllText(fullname, postscript);
}
