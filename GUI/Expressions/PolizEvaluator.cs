using System;
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
                int number;

                if (!int.TryParse(item, out number) && !IsOperator(item))
                {
                    result.CanEvaluate = false;
                    result.Errors.Add(
                        "Вычисление невозможно: выражение содержит идентификатор '" + item + "'");

                    return result;
                }
            }

            var stack = new Stack<int>();
            int stepNumber = 1;

            foreach (var item in items)
            {
                int number;

                if (int.TryParse(item, out number))
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

                    int right = stack.Pop();
                    int left = stack.Pop();

                    int operationResult;

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
            int left,
            int right,
            string operation,
            out int value,
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

        private string FormatStack(Stack<int> stack)
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