using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STVDirector
{
    public static class ClientMod
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int WM_COPYDATA = 0x4A;

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private static IntPtr IntPtrAlloc<T>(T param)
        {
            IntPtr retValue = Marshal.AllocHGlobal(Marshal.SizeOf(param));
            Marshal.StructureToPtr(param, retValue, false);
            return (retValue);
        }

        public static int SendCommandToWindow(string szCommand)
        {
            IntPtr hWnd = FindWindow("ClientModListener", null);

            if (hWnd != null)
            {
                COPYDATASTRUCT CopyData;
                CopyData.cbData = szCommand.Length + 1;
                CopyData.dwData = IntPtr.Zero;
                CopyData.lpData = Marshal.StringToHGlobalAnsi(szCommand);
                IntPtr copyDataBuff = IntPtrAlloc(CopyData);
                return SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, copyDataBuff);
            }

            return 0;
        }
    }
}
