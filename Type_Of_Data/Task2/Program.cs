using System;
using System.Collections.Generic;

namespace Norbit.Tasks.TypeOfData
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Генератор ромба с пустым центром\n");

            Console.Write("Введите длину диагонали ромба (положительное нечётное число): ");
            string input = Console.ReadLine();
            
            try
            {
                int n = int.Parse(input);
                var diamond = DiamondDrawer.GenerateDiamond(n);

                Console.WriteLine("\nРезультат:");
                foreach (var line in diamond)
                {
                    Console.WriteLine(line);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("\n Ошибка: недопустимый символ.");
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