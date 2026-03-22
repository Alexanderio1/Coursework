using System;
using System.Runtime.InteropServices;

namespace GUI
{
    internal static class NativeParserInterop
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void ErrorCallback(
            int startLine,
            int startColumn,
            int endLine,
            int endColumn,
            IntPtr message,
            IntPtr lexeme
        );

        [DllImport("NativeParser.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int ParseSourceCode(string sourceCode, ErrorCallback errorCallback);
    }

    internal sealed class ParserErrorInfo
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public string Message { get; set; }
        public string Lexeme { get; set; }
    }
}