using System;
using System.Collections.Generic;

namespace Norbit.Tasks.TypeOfData
{
    /// <summary>
    /// Калькулятор вклада.
    /// </summary>
    public class DepositCalculator
    {
        /// <summary>
        /// Вычисляет сложный процент.
        /// </summary>
        /// <param name="initialDeposit">Стартовый размер вклада.</param>
        /// <param name="years">Кол-во лет.</param>
        /// <param name="interestRate">Ставка.</param>
        /// <returns>Список кортежей (год, сумма).</returns>
        public static List<(int year, double deposit)> CalculateCompoundInterest(
            double initialDeposit, int years, double interestRate)
        {
            // Валидация
            CheckIsPositive(initialDeposit, "Стартовый размер вклада ожидается > 0.");
            CheckIsPositive(years, "Кол-во лет ожидается > 0.");
            CheckIsPositive(interestRate, "Ставка ожидается > 0.");

            double amount = initialDeposit;
            var result = new List<(int year, double deposit)>();

            for (int i = 1; i <= years; i++)
            {
                amount += amount / 100 * interestRate;
                result.Add((i, amount));
            }

            return result;
        }

        /// <summary>
        /// Проверяет, что значение положительное.
        /// </summary>
        private static void CheckIsPositive(double value, string errorMessage)
        {
            if (value <= 0)
            {
                throw new ArgumentException(errorMessage);
            }
        }
    }
}