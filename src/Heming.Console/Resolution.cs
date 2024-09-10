using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Heming.Console
{
    internal class Resolution
    {
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            HORZRES = 8,
            VERTRES = 10,
            DESKTOPVERTRES = 117,
            DESKTOPHORZRES = 118,
        }

        static int screenHeight, screenWidth, deviceHeight, deviceWidth;

        static IntPtr desktop;

        public static void Initilize()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            desktop = g.GetHdc();
            screenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            screenWidth = GetDeviceCaps(desktop, (int)DeviceCap.HORZRES);
            deviceHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
            deviceWidth = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPHORZRES);
        }

        public static int ScreenHeight
        {
            get
            {
                if (screenHeight == 0)
                    screenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
                return screenHeight;
            }
        }

        public static int ScreenWidth
        {
            get
            {
                if (screenWidth == 0)
                    screenWidth = GetDeviceCaps(desktop, (int)DeviceCap.HORZRES);
                return screenWidth;
            }
        }

        public static int DeviceHeight
        {
            get
            {
                if (deviceHeight == 0)
                    deviceHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                return deviceHeight;
            }
        }

        public static int DeviceWidth
        {
            get
            {
                if (deviceWidth == 0)
                    deviceWidth = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPHORZRES);
                return deviceWidth;
            }
        }
    }
}
