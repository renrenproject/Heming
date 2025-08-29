
using Heming;
using Heming.Console;
using Microsoft.Extensions.Configuration;
using NAudio.Wave;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

[DllImport("user32.dll")]
static extern void keybd_event(byte bVk, byte bScan, UpDown dwFlags, int dwExtraInfo);
[DllImport("user32.dll")]
static extern bool SetCursorPos(int X, int Y);
[DllImport("user32.dll")]
static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);

double CalculateAvgColor(Bitmap bitmap)
{
    double avgColor = 0;
    for (int i = 0; i < bitmap.Height; i++)
    {
        for (int j = 0; j < bitmap.Width; j++)
        {
            Color p = bitmap.GetPixel(j, i);
            avgColor += p.R + p.G + p.B;
        }
    }
    return avgColor / bitmap.Height / bitmap.Width;
}

void Click(LeftRightMedium mef, UpDownClick udc)
{
    switch (mef)
    {
        case LeftRightMedium.Right:
            if ((udc == UpDownClick.Down) || (udc == UpDownClick.Click))
            {
                mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
            }
            if ((udc == UpDownClick.Up) || (udc == UpDownClick.Click))
            {
                mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
            }
            return;

        case LeftRightMedium.Middle:
            if ((udc == UpDownClick.Down) || (udc == UpDownClick.Click))
            {
                mouse_event(MouseEventFlag.MiddleDown, 0, 0, 0, UIntPtr.Zero);
            }
            if ((udc == UpDownClick.Up) || (udc == UpDownClick.Click))
            {
                mouse_event(MouseEventFlag.MiddleUp, 0, 0, 0, UIntPtr.Zero);
            }
            return;
    }
    if ((udc == UpDownClick.Down) || (udc == UpDownClick.Click))
    {
        mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
    }
    if ((udc == UpDownClick.Up) || (udc == UpDownClick.Click))
    {
        mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
    }
}

Console.WriteLine("Loading settings...");
bool debug = false;
bool pressShift = false;
byte hookDetectMethod;
int hookingDelay = 0;
int tested = 0;
int x;
int y;
int fished = 0;
int succeed = 0;
int undetedted = 0;
double?[] avgarray = new double?[20];
double?[] avgcolors = new double?[Int16.MaxValue];
double?[] avgmaxcolors = new double?[byte.MaxValue];
float?[] succeedrecord = new float?[10];
double targetColorThreshold = 0;
double targetSoundThreshold = 0;
double maxcolor = 0;
double targetcolor = 0;
double currentSoundLevel = 0;
Size size;
string configName = "appsettings";
const string imgpath = "cache.jpg";
string remoteurl;
Graphics captureGraphics;
Bitmap captureBmp;
Bitmap hookbmp;
Graphics hookGraphic;
Rectangle hookposition;
Predicting predicting = new Predicting();
predicting.Session = new Microsoft.ML.OnnxRuntime.InferenceSession("HemingModel.onnx");
PredictionResult predres;
HootDetectMethod method = new HootDetectMethod();
WasapiLoopbackCapture capture;

if (args.Length > 0)
{
    configName = args[0];
}

try
{
    IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile(configName + ".json").Build();
    Resolution.Initilize();
    debug = bool.Parse(configuration["Debug"]);
    x = int.Parse(configuration["X"]);
    y = int.Parse(configuration["Y"]);
    targetColorThreshold = double.Parse(configuration["TargetColorThreshold"]);
    targetSoundThreshold = double.Parse(configuration["TargetSoundThreshold"]);
    size = new System.Drawing.Size(int.Parse(configuration["W"]), int.Parse(configuration["H"]));
    hookingDelay = int.Parse(configuration["StartHookingDelayMs"]);
    pressShift = bool.Parse(configuration["PressShift"]);
    while (true)
    {
        Console.WriteLine("Please hook detect method[1-3]:\r\n1:Visual; 2:Audio; 3:Both");
        if (byte.TryParse(Console.ReadLine(), out hookDetectMethod))
        {
            if (hookDetectMethod > 0 && hookDetectMethod <= 3)
            {
                method = (HootDetectMethod)hookDetectMethod;
                Console.WriteLine($"Selected {method} detecting.");
                break;
            }
        }
        Console.WriteLine("Wrong input.");
    }
    captureBmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
    captureGraphics = Graphics.FromImage(captureBmp);
    avgmaxcolors[0] = 0;
    succeedrecord[0] = 0;
    if (((int)method & 2) == 2)    //Audio or Both
    {
        capture = new WasapiLoopbackCapture();
        capture.DataAvailable += OnDataAvailable;
        capture.StartRecording();
    }
    Console.WriteLine("Load settings succeed.");
}
catch (Exception e)
{
    Console.WriteLine("Load settings failed: {0}, {1}", e.Message, e.StackTrace);
    return;
}

Console.WriteLine("Please make sure your game is on and your fish pole is equiped");
for (int i = 10; i > 0; i--)
{
    Console.Write("\rHeming is starts in {0}...\r", i);
    Thread.Sleep(1000);
}

Console.WriteLine("\rHeming started.            ");
while (!(fished > 20 && succeedrecord.Average() < 0.3))
{
    SetCursorPos(0, 0);
    Thread.Sleep(100);
    //press 9
    keybd_event(0x39, 0x0A, UpDown.Down, 0);
    Thread.Sleep(100);
    keybd_event(0x39, 0x0A, UpDown.Up, 0);
    Thread.Sleep(500);
    //press 0
    keybd_event(0x30, 0x0B, UpDown.Down, 0);
    Thread.Sleep(100);
    keybd_event(0x30, 0x0B, UpDown.Up, 0);
    Thread.Sleep(5000);
    captureGraphics.CopyFromScreen(x, y, 0, 0, size);
    captureBmp.Save(".\\" + imgpath, ImageFormat.Jpeg);
    succeedrecord[fished % succeedrecord.Length] = 0;
    try
    {
        IList<PredictionResult> predictions;
        predictions = predicting.LocalPredicting(imgpath);

        if (predictions == null || predictions.Count == 0)
        {
            if (debug)
                Console.WriteLine($"No prediction, continue.");
            continue;
        }
        predres = predictions[0];
        if (debug)
        {
            Console.WriteLine($"Prediction result: Probability:{predres.Probability}, x:{predres.BoundingBox.Left}, y:{predres.BoundingBox.Top}, w:{predres.BoundingBox.Width}, h:{predres.BoundingBox.Height}");
        }
    }
    catch (Exception e)
    {
        if (debug)
            Console.WriteLine($"Predicting error: {e}");
        continue;
    }
    //Detected hook
    if (predres.Probability > 0.02)
    {
        hookposition = new Rectangle(x + (int)(predres.BoundingBox.Left * size.Width)
            , y + (int)(predres.BoundingBox.Top * size.Height)
            , (int)(size.Width * predres.BoundingBox.Width)
            , (int)(size.Height * predres.BoundingBox.Height));
        if (((int)method & 1) == 1)
        {
            hookbmp = new Bitmap(hookposition.Width, hookposition.Height, PixelFormat.Format32bppArgb);
            hookGraphic = Graphics.FromImage(hookbmp);
            hookGraphic.CopyFromScreen(hookposition.Left, hookposition.Top, 0, 0, hookposition.Size);
            double origcolor = CalculateAvgColor(hookbmp);
            maxcolor = 0;
            if (debug)
            {
                hookbmp.Save("hook.jpg");
                Console.WriteLine("\rorigcolor: " + origcolor);
            }
        }
    
        DateTime fishtime = DateTime.Now;
        if (hookingDelay > 0)
            Thread.Sleep(hookingDelay);
        for (int i = 0; i < avgarray.Length; i++)
            avgarray[i] = null;
        while (DateTime.Now - fishtime < new TimeSpan(0, 0, 25))
        {
            if (((int)method & 1) == 1)
            {
                hookGraphic.CopyFromScreen(hookposition.Left, hookposition.Top, 0, 0, hookposition.Size);
                targetcolor = CalculateAvgColor(hookbmp);
                if (targetcolor > maxcolor)
                {
                    maxcolor = targetcolor;
                    if (debug)
                        Console.WriteLine("\rmaxcolor: " + maxcolor);
                }
                avgarray[tested % avgarray.Length] = targetcolor;
                avgcolors[tested] = targetcolor;
            }

            //Hooked
            if (((((int)method & 1) == 1) && (targetcolor > avgarray.Average() * double.Max(avgmaxcolors.Average().Value * 0.85 / avgcolors.Average().Value, targetColorThreshold)))
                || ((((int)method & 2) == 2) && currentSoundLevel > targetSoundThreshold))
            {
                if (debug)
                {
                    if (tested == Int16.MaxValue)
                        hookbmp.Save("hooked.jpg");
                    Console.WriteLine("hooked");
                }
                Thread.Sleep(1500);
                SetCursorPos((hookposition.Left + hookposition.Width / 2) * Resolution.ScreenWidth / Resolution.DeviceWidth, (hookposition.Top + hookposition.Height / 2) * Resolution.ScreenHeight / Resolution.DeviceHeight);
                //Shift down
                if (pressShift)
                {
                    Thread.Sleep(100);
                    keybd_event(0x10, 0x2A, UpDown.Down, 0);
                }
                Thread.Sleep(100);
                Click(LeftRightMedium.Right, UpDownClick.Click);
                //Shift up
                if (pressShift)
                {
                    Thread.Sleep(100);
                    keybd_event(0x10, 0x2A, UpDown.Up, 0);
                }
                succeed++;
                succeedrecord[fished % succeedrecord.Length] = 1;
                break;
            }

            Thread.Sleep(100);
            tested++;
            if (tested == Int16.MaxValue)
                tested = 0;
        }
        if (((int)method & 1) == 1)
            avgmaxcolors[fished % avgmaxcolors.Length] = maxcolor;
    }
    else
        undetedted++;
    fished++;
    string statusmsg = $"\rFished: {fished}, hooked: {succeed}, percentage: {succeed * 100 / fished}, undetected: {undetedted}                 ";
    if (debug)
        Console.WriteLine(statusmsg);
    else
        Console.Write(statusmsg);
    Thread.Sleep(1000);
}
Console.WriteLine('\r');
Console.WriteLine("\rHeming exit due to low hook percentage.                                                                   ");

void OnDataAvailable(object? sender, WaveInEventArgs e)
{
    int length = e.BytesRecorded / 4;     // 4 bytes per sample (32 bit stereo)
    double[] result = new double[length];

    for (int i = 0; i < length; i++)
    {
        float tmp = BitConverter.ToSingle(e.Buffer, i * 4);
        result[i] = tmp < 0 ? -tmp : tmp;
    }

    if (e.BytesRecorded > 0)
        currentSoundLevel = result.Average();
    else
        currentSoundLevel = 0;
}

enum UpDown
{
    Down = 0x0000,
    Up = 0x0002,
}

enum LeftRightMedium
{
    Left,
    Right,
    Middle
}
enum UpDownClick
{
    Up,
    Down,
    Click
}
enum HootDetectMethod
{
    Visual = 1,
    Audio = 2,
    Both = 3
}

[Flags]
internal enum MouseEventFlag : uint
{
    Absolute = 0x8000,
    LeftDown = 2,
    LeftUp = 4,
    MiddleDown = 0x20,
    MiddleUp = 0x40,
    Move = 1,
    RightDown = 8,
    RightUp = 0x10,
    VirtualDesk = 0x4000,
    Wheel = 0x800,
    XDown = 0x80,
    XUp = 0x100
}