using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GUI.IR
{
    public sealed class LocalIrOptimizer
    {
        public IrProgram NormalizeConstants(IrProgram source)
        {
            if (source == null)
                return new IrProgram();

            IrProgram result = source.Clone();

            foreach (IrInstruction instruction in result.Instructions)
            {
                if (instruction == null || instruction.Arguments.Count == 0)
                    continue;

                if (instruction.Operation == "const_int")
                {
                    instruction.Arguments[0] = NormalizeInteger(instruction.Arguments[0]);
                }
                else if (instruction.Operation == "const_double")
                {
                    instruction.Arguments[0] = NormalizeDouble(instruction.Arguments[0]);
                }
            }

            return result;
        }

        public IrProgram InlineLiteralTemporaries(IrProgram source)
        {
            if (source == null)
                return new IrProgram();

            Dictionary<string, string> literalTemps = BuildLiteralTemporaryDictionary(source);
            Dictionary<string, int> usageCounts = CountArgumentUsages(source);

            HashSet<string> tempsToInline = new HashSet<string>();

            foreach (IrInstruction instruction in source.Instructions)
            {
                if (instruction == null)
                    continue;

                if (instruction.Operation != "listof")
                    continue;

                foreach (string argument in instruction.Arguments)
                {
                    if (!literalTemps.ContainsKey(argument))
                        continue;

                    int usageCount = usageCounts.ContainsKey(argument)
                        ? usageCounts[argument]
                        : 0;

                    if (usageCount == 1)
                        tempsToInline.Add(argument);
                }
            }

            List<IrInstruction> optimized = new List<IrInstruction>();

            foreach (IrInstruction instruction in source.Instructions)
            {
                if (instruction == null)
                    continue;

                if (tempsToInline.Contains(instruction.Result) && IsLiteralConstant(instruction.Operation))
                    continue;

                IrInstruction copy = instruction.Clone();

                if (copy.Operation == "listof")
                {
                    for (int i = 0; i < copy.Arguments.Count; i++)
                    {
                        string argument = copy.Arguments[i];

                        if (tempsToInline.Contains(argument))
                            copy.Arguments[i] = literalTemps[argument];
                    }
                }

                optimized.Add(copy);
            }

            return new IrProgram(optimized);
        }

        public Dictionary<string, string> BuildLiteralTemporaryDictionary(IrProgram source)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (source == null)
                return result;

            foreach (IrInstruction instruction in source.Instructions)
            {
                if (instruction == null)
                    continue;

                if (!IsTemporaryName(instruction.Result))
                    continue;

                if (instruction.Arguments.Count != 1)
                    continue;

                if (!IsLiteralConstant(instruction.Operation))
                    continue;

                result[instruction.Result] = instruction.Arguments[0];
            }

            return result;
        }

        private Dictionary<string, int> CountArgumentUsages(IrProgram source)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            if (source == null)
                return result;

            foreach (IrInstruction instruction in source.Instructions)
            {
                if (instruction == null)
                    continue;

                foreach (string argument in instruction.Arguments)
                {
                    if (!result.ContainsKey(argument))
                        result[argument] = 0;

                    result[argument]++;
                }
            }

            return result;
        }

        private bool IsLiteralConstant(string operation)
        {
            return operation == "const_int"
                || operation == "const_double"
                || operation == "const_string"
                || operation == "const_char"
                || operation == "const_boolean";
        }

        private bool IsTemporaryName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!name.StartsWith("t"))
                return false;

            if (name.Length == 1)
                return false;

            return name.Skip(1).All(char.IsDigit);
        }

        private string NormalizeInteger(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return rawValue;

            int value;

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return value.ToString(CultureInfo.InvariantCulture);

            if (rawValue.StartsWith("+"))
                return rawValue.Substring(1);

            return rawValue;
        }

        private string NormalizeDouble(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return rawValue;

            string preparedValue = rawValue.Replace(',', '.');

            double value;

            if (double.TryParse(preparedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                if (value == 0.0)
                    return "0";

                return value.ToString("G15", CultureInfo.InvariantCulture);
            }

            if (preparedValue.StartsWith("+"))
                return preparedValue.Substring(1);

            return preparedValue;
        }
    }
}