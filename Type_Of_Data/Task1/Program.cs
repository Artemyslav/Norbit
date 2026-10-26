using System;
using System.Collections.Generic;

namespace Norbit.Tasks.TypeOfData
{
    /// <summary>
    /// Вход в консольное приложение.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Калькулятор сложных процентов\n");

            // Ввод данных от пользователя
            var initialDeposit = Convert.ToDouble(ReadValue("Введите начальный вклад: "));
            var years = int.Parse(ReadValue("Введите количество лет: "));
            var interestRate = int.Parse(ReadValue("Введите годовую процентную ставку: "));

            try
            {
                // Вызов
                var deposits = DepositCalculator.CalculateCompoundInterest(
                    initialDeposit, years, interestRate);

                // Вывод результатов
                Console.WriteLine("\nРезультат расчета:");
                foreach ((int year, double amount) in deposits)
                {
                    Console.WriteLine($"Год {year}: {amount:F2} руб.");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Ошибка: {ex.Message}");
            }
        }

        private static string ReadValue(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
    }
}