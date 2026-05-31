using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GUI.IR
{
    public sealed class LocalIrOptimizer
    {
        public IrProgram NormalizeConstants(IrProgram source)
        {
            IrProgram result = source.Clone();

            foreach (IrInstruction instruction in result.Instructions)
            {
                if (instruction.Arguments.Count == 0)
                    continue;

                if (instruction.Operation == "const_int")
                    instruction.Arguments[0] = NormalizeInteger(instruction.Arguments[0]);

                if (instruction.Operation == "const_double")
                    instruction.Arguments[0] = NormalizeDouble(instruction.Arguments[0]);
            }

            return result;
        }

        public IrProgram InlineLiteralTemporaries(IrProgram source)
        {
            Dictionary<string, string> literalTemps = new Dictionary<string, string>();

            foreach (IrInstruction instruction in source.Instructions)
            {
                if (instruction.Arguments.Count != 1)
                    continue;

                if (IsLiteralConstant(instruction.Operation))
                    literalTemps[instruction.Result] = instruction.Arguments[0];
            }

            HashSet<string> inlinedTemps = new HashSet<string>();
            List<IrInstruction> optimized = new List<IrInstruction>();

            foreach (IrInstruction instruction in source.Instructions)
            {
                IrInstruction copy = instruction.Clone();

                if (copy.Operation == "listof")
                {
                    for (int i = 0; i < copy.Arguments.Count; i++)
                    {
                        string argument = copy.Arguments[i];

                        if (literalTemps.ContainsKey(argument))
                        {
                            copy.Arguments[i] = literalTemps[argument];
                            inlinedTemps.Add(argument);
                        }
                    }

                    optimized.Add(copy);
                    continue;
                }

                if (inlinedTemps.Contains(copy.Result) && IsLiteralConstant(copy.Operation))
                    continue;

                optimized.Add(copy);
            }

            return new IrProgram(optimized);
        }

        private bool IsLiteralConstant(string operation)
        {
            return operation == "const_int"
                || operation == "const_double"
                || operation == "const_string"
                || operation == "const_char"
                || operation == "const_boolean";
        }

        private string NormalizeInteger(string rawValue)
        {
            int value;

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return value.ToString(CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(rawValue) && rawValue.StartsWith("+"))
                return rawValue.Substring(1);

            return rawValue;
        }

        private string NormalizeDouble(string rawValue)
        {
            double value;

            if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                if (value == 0.0)
                    return "0";

                return value.ToString("G", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrWhiteSpace(rawValue) && rawValue.StartsWith("+"))
                return rawValue.Substring(1);

            return rawValue;
        }
    }
}