using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI.IR
{
    public sealed class IrInstruction
    {
        public string Result { get; set; }
        public string Operation { get; set; }
        public List<string> Arguments { get; private set; }

        public IrInstruction(string result, string operation, params string[] arguments)
        {
            Result = result;
            Operation = operation;
            Arguments = new List<string>(arguments ?? new string[0]);
        }

        public IrInstruction Clone()
        {
            return new IrInstruction(Result, Operation, Arguments.ToArray());
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Result))
                return Operation + " " + string.Join(", ", Arguments);

            if (Arguments.Count == 0)
                return Result + " = " + Operation;

            return Result + " = " + Operation + " " + string.Join(", ", Arguments);
        }
    }
}