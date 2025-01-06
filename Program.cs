using NAudio.Wave;
using WindowsInput;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
public static class ColorGet 
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
        
        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }
    
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);
    
    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    public static extern int GetPixel(IntPtr hdc, int x, int y);
    
    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);
    
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    
    // Method to get the color at a specific position (1200, 500)
    public static Color GetPixelColorAt(int x, int y)
    {
        IntPtr hwnd = GetForegroundWindow(); // Get the handle of the currently focused window (the game)
        IntPtr hdc = GetDC(hwnd); // Get device context for the window
        
        // Retrieve the pixel color at the specified coordinates
        int pixelColor = GetPixel(hdc, x, y);
        
        // Release the device context
        ReleaseDC(hwnd, hdc);
        
        // Extract RGB values from the pixel color
        int red = pixelColor & 0x000000FF;
        int green = (pixelColor & 0x0000FF00) >> 8;
        int blue = (pixelColor & 0x00FF0000) >> 16;
        
        return Color.FromArgb(red, green, blue);
    }
}


public class MouseSimulator
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
        
        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }
    
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);
    public static Point GetCursorPosition()
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);
        
        return lpPoint;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    
    const uint INPUT_MOUSE = 0;
    const uint MOUSEEVENTF_MOVE = 0x0001;
    const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP = 0x0004;
    
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);
    
    public static void SetCursorPosition(int x, int y)
    {
        SetCursorPos(x, y);
    }
    public static void LeftMouseDown()
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].type = INPUT_MOUSE;
        inputs[0].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
    
    public static void LeftMouseUp()
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].type = INPUT_MOUSE;
        inputs[0].mi.dwFlags = MOUSEEVENTF_LEFTUP;
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
    
    public static void SimulateLeftClickAt(int x, int y, int durationMilliseconds)
    {
        Console.WriteLine("---Simulating Left Click---");
        Console.WriteLine("---Holding Left Click for {0}", durationMilliseconds);
        SetCursorPosition(x, y);
        LeftMouseDown();
        Thread.Sleep(durationMilliseconds);
        LeftMouseUp();
    }
}

class Program
{
    private static bool IsPullThrown = false;
    private static bool IsPulling = false;
    private static bool IsSearching = false;
     
    private static WasapiLoopbackCapture capture;
    private static Point StartingPoint = MouseSimulator.GetCursorPosition();
    private static InputSimulator Isimulator = new InputSimulator();
    private static Stopwatch Timer = new Stopwatch();
    private static int LegitPullingSound = 0;
    private static bool IsThreadDone = true; 
    
    static void Main(string[] args)
    {
        capture = new WasapiLoopbackCapture();
        capture.DataAvailable += OnDebugAudio;
        capture.DataAvailable += OnSomething;
        Console.WriteLine("---Started Audio Capturing---");
        capture.StartRecording();
        
        Console.ReadLine();
        capture.StopRecording();
    }
    public static void OnSomething(object sender, WaveInEventArgs args)
    {
        Color clr = ColorGet.GetPixelColorAt(900 , 500); 
        Console.WriteLine(clr.Name);
        float secondsound = 0;
        var buffer = new WaveBuffer(args.Buffer);
        for (int index = 0; index < args.BytesRecorded / 4; index++)
        {
            float sample = buffer.FloatBuffer[index];
            if (sample < 0) sample = -sample;
            if (sample > secondsound) secondsound = sample;
        }
        if (IsPulling = true && secondsound > 0.1 && secondsound < 0.35)
        {
           LegitPullingSound++;
        }
        // Console.WriteLine("Current Volume {1}" , secondsound);
    }     
    public static void OnDebugAudio(object sender, WaveInEventArgs args)
    {
        float sound = 0;
        var buffer = new WaveBuffer(args.Buffer);
        for (int index = 0; index < args.BytesRecorded / 4; index++)
        {
            float sample = buffer.FloatBuffer[index];
            if (sample < 0) sample = -sample;
            if (sample > sound) sound = sample;
        }
         
        // Console.WriteLine("OnDebug Sound {0}" , sound);
        if (IsPullThrown == false && IsPulling == false && IsSearching == false && IsThreadDone == true)
        {
            ThrowPull();
            Timer.Start();
            IsPullThrown = true;
            IsSearching = true;
        }
        if (IsFishFound(sound) && IsPullThrown == true && IsPulling == false && IsThreadDone == true)
        {
            Console.WriteLine("+++Found A Fish+++");
            Timer.Reset();  
            StartPulling();
        }
        TimeSpan elapsedTime = Timer.Elapsed;
        if (elapsedTime.Seconds > 45) 
        {
            Timer.Reset();
            Isimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_Q);
            IsPullThrown = false;
            IsPulling = false;
            IsSearching = false;
            Thread.Sleep(1000);
        }
    }
    
    private static void ThrowPull()
    {
        Random rnd = new Random();
        int randomX = rnd.Next(1100, 1300);
        int randomY = rnd.Next(500, 600);
        Console.WriteLine("Throwing the Pull");
        MouseSimulator.SimulateLeftClickAt(randomX, randomY, 1200);
        Console.WriteLine("Threw the Pull");
    }
    
    private static bool IsFishFound(float sound)
    {
        if (sound > 0.39 && sound < 0.43)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
     
    private static void StartPulling()
    {
        IsThreadDone = false;
        IsSearching = false;
        IsPulling = true;
          
        Thread PullingThread = new Thread(Pull);
        PullingThread.Start();
    }
     
    private static void Pull()
    {
        for (int i = 0; i < 9; i++)
        {
            LegitPullingSound = 0; 
            Thread.Sleep(300);
            Console.WriteLine("Iteration {0}" , i);
            MouseSimulator.SimulateLeftClickAt(StartingPoint.X, StartingPoint.Y + 250, 1400);
            Thread.Sleep(300);
            if ((LegitPullingSound > 9) == false)
            {
                Console.WriteLine("------ Fake Fish -----");
                break;
            }
        }
        Console.WriteLine("--- PRESSING Q ---");
        Isimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_Q);
        Thread.Sleep(3000);
        IsPulling = false;
        IsPullThrown = false;
        IsThreadDone = true;
        Console.WriteLine("---Thread Finished Pulling---");
    }
}