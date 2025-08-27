using System.Drawing;
using System.Drawing.Imaging;

var exeDir = AppDomain.CurrentDomain.BaseDirectory;
// Walk up to find the repository root containing 'assets/logo.png'
string repoRoot = exeDir;
string? assetsDir = null;
for (int i = 0; i < 8; i++)
{
    var candidate = Path.Combine(repoRoot, "assets");
    if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "logo.png")))
    {
        assetsDir = candidate;
        break;
    }
    repoRoot = Path.GetFullPath(Path.Combine(repoRoot, ".."));
}

if (assetsDir == null)
{
    // Fallback to relative path
    assetsDir = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "assets"));
}

var png = Path.Combine(assetsDir, "logo.png");
var ico = Path.Combine(assetsDir, "logo.ico");

if (!File.Exists(png))
{
    Console.Error.WriteLine($"logo.png not found at {png}");
    return 1;
}

using var bmp = (Bitmap)Image.FromFile(png);
using var ms = new MemoryStream();

// Create a simple ICO with a single size (256x256) - Windows will scale as needed
var resized = new Bitmap(bmp, new Size(256, 256));
resized.Save(ms, ImageFormat.Png);
ms.Seek(0, SeekOrigin.Begin);

using var fs = new FileStream(ico, FileMode.Create, FileAccess.Write);

// Write an ICO header for one image
// ICONDIR
fs.WriteByte(0); fs.WriteByte(0); // reserved
fs.WriteByte(1); fs.WriteByte(0); // type = 1 (icon)
fs.WriteByte(1); fs.WriteByte(0); // count = 1

// ICONDIRENTRY (16 bytes)
fs.WriteByte(0); // width: 0 means 256
fs.WriteByte(0); // height: 0 means 256
fs.WriteByte(0); // color count
fs.WriteByte(0); // reserved
fs.WriteByte(1); fs.WriteByte(0); // planes
fs.WriteByte(32); fs.WriteByte(0); // bitcount

// image size
var imgBytes = ms.ToArray();
var sizeBytes = BitConverter.GetBytes(imgBytes.Length);
fs.Write(sizeBytes, 0, 4);
// offset (header 6 + direntry 16 = 22)
var offset = BitConverter.GetBytes(6 + 16);
fs.Write(offset, 0, 4);

// write image data
fs.Write(imgBytes, 0, imgBytes.Length);

Console.WriteLine($"Wrote {ico}");
return 0;
