using System.Collections.Generic;
using System.Linq;

namespace GUI.Expressions
{
    public class PolizEvaluator
    {
        public PolizEvaluationResult Evaluate(List<string> items)
        {
            var result = new PolizEvaluationResult();

            if (items == null || items.Count == 0)
            {
                result.CanEvaluate = false;
                result.Errors.Add("ПОЛИЗ пустая, вычисление невозможно");
                return result;
            }

            foreach (var item in items)
            {
                if (IsOperator(item))
                {
                    continue;
                }

                if (!IsIntegerLiteral(item))
                {
                    result.CanEvaluate = false;
                    result.Errors.Add(
                        "Вычисление невозможно: выражение содержит идентификатор '" + item + "'");
                    return result;
                }

                long parsedNumber;

                if (!long.TryParse(item, out parsedNumber))
                {
                    result.CanEvaluate = false;
                    result.Errors.Add(
                        "Вычисление невозможно: число '" + item + "' выходит за допустимый диапазон");
                    return result;
                }
            }

            var stack = new Stack<long>();
            int stepNumber = 1;

            foreach (var item in items)
            {
                long number;

                if (long.TryParse(item, out number))
                {
                    stack.Push(number);

                    result.Steps.Add(new PolizEvaluationStep(
                        stepNumber++,
                        item,
                        "Поместить число в стек",
                        FormatStack(stack)));

                    continue;
                }

                if (IsOperator(item))
                {
                    if (stack.Count < 2)
                    {
                        result.Errors.Add(
                            "Недостаточно операндов для операции '" + item + "'");
                        return result;
                    }

                    long right = stack.Pop();
                    long left = stack.Pop();
                    long operationResult;

                    if (!TryCalculate(left, right, item, out operationResult, result))
                    {
                        return result;
                    }

                    stack.Push(operationResult);

                    result.Steps.Add(new PolizEvaluationStep(
                        stepNumber++,
                        item,
                        left + " " + item + " " + right + " = " + operationResult,
                        FormatStack(stack)));
                }
            }

            if (stack.Count != 1)
            {
                result.Errors.Add("После вычисления в стеке осталось некорректное количество значений");
                return result;
            }

            result.Value = stack.Pop();
            result.Success = true;
            return result;
        }

        private bool TryCalculate(
            long left,
            long right,
            string operation,
            out long value,
            PolizEvaluationResult result)
        {
            value = 0;

            switch (operation)
            {
                case "+":
                    value = left + right;
                    return true;

                case "-":
                    value = left - right;
                    return true;

                case "*":
                    value = left * right;
                    return true;

                case "/":
                    if (right == 0)
                    {
                        result.Errors.Add("Деление на ноль");
                        return false;
                    }

                    value = left / right;
                    return true;

                case "%":
                    if (right == 0)
                    {
                        result.Errors.Add("Остаток от деления на ноль");
                        return false;
                    }

                    value = left % right;
                    return true;

                default:
                    result.Errors.Add("Неизвестная операция '" + operation + "'");
                    return false;
            }
        }

        private bool IsOperator(string item)
        {
            return item == "+" ||
                   item == "-" ||
                   item == "*" ||
                   item == "/" ||
                   item == "%";
        }

        private bool IsIntegerLiteral(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return false;
            }

            for (int i = 0; i < item.Length; i++)
            {
                if (!char.IsDigit(item[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private string FormatStack(Stack<long> stack)
        {
            if (stack.Count == 0)
            {
                return "[]";
            }

            var values = stack.ToArray().Reverse();
            return "[" + string.Join(", ", values) + "]";
        }
    }
}