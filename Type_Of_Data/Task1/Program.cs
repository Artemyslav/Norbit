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
            Console.Write("Введите начальный вклад: ");
            var initialDeposit = Convert.ToDouble(Console.ReadLine());

            Console.Write("Введите количество лет: ");
            var years = int.Parse(Console.ReadLine());

            Console.Write("Введите годовую процентную ставку: ");
            var interestRate = int.Parse(Console.ReadLine());

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
    }
}