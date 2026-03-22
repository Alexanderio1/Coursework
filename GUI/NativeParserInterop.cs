using System;
using System.Runtime.InteropServices;

namespace GUI
{
    internal static class NativeParserInterop
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void ErrorCallback(int line, IntPtr message);

        [DllImport("NativeParser.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int ParseSourceCode(string sourceCode, ErrorCallback errorCallback);
    }
}