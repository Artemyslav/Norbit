using System;
using System.Text;

namespace Norbit.Tasks.TypeOfData
{
    // <summary>
    // Класс для выполнения расчетов по финансовым задачам.
    // </summary>
    public class CompoundInterestCalculator
    {
        // <summary>
        // Формирует строку с расчетом сложных процентов по годам.
        // </summary>
        // <param name="initialDeposit">Начальный вклад (положительное число).</param>
        // <param name="years">Количество лет (положительное целое число).</param>
        // <param name="interestRate">Годовая процентная ставка (положительное число).</param>
        // <returns>Строка с поэтапным расчетом накоплений.</returns>
        public static string CalculateCompoundInterest(decimal initialDeposit, int years, decimal interestRate)
        {
            var stringBuilder = new StringBuilder();
            var currentAmount = initialDeposit;

            for (var year = 1; year <= years; year++)
            {
                currentAmount += currentAmount * (interestRate / 100m);
                stringBuilder.AppendLine($"Год {year}: {currentAmount:F2} руб.");
            }

            return stringBuilder.ToString().TrimEnd();
        }
    }

    // <summary>
    // Точка входа и тест
    // </summary>
    public class Program
    {
        public static void Main()
        {
            // Тестовые данные из условия
            var initialDeposit = 1000m;
            var years = 3;
            var interestRate = 10m;

            var result = CompoundInterestCalculator.CalculateCompoundInterest(initialDeposit, years, interestRate);
            
            Console.WriteLine(result);
        }
    }
}