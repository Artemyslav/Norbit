using System;
using System.Collections.Generic;

namespace Norbit.Tasks.Delegates
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Обработка заказов интернет-магазина\n");

            var processor = new OrderProcessor();

            processor.OrderProcessed += (sender, e) => 
            {
                Console.WriteLine($"[СОБЫТИЕ] Заказ #{e.OrderId} обработан. Итог: {e.Total}");
            };

            var items = CollectOrderFromUser();
            if (items.Count == 0) return;

            // Делегаты
            decimal taxRate = 1.10m; // Налог 10%
            Func<decimal, decimal> transformPrice = price => price * taxRate; // Замыкание
            ItemTotalCalculator calculateItemTotal = static (item, price) => price * item.Quantity; // Static-лямбда
            Predicate<OrderItem> shouldProcess = item => !item.Name.Contains("Брак", StringComparison.OrdinalIgnoreCase);
            Action<string> logger = message => Console.WriteLine($"[LOG] {message}");

            try
            {
                decimal finalTotal = processor.Process(1001, items, shouldProcess, transformPrice, calculateItemTotal, logger);
                Console.WriteLine($"\n Заказ успешно обработан. Итоговая сумма: {finalTotal:F2} руб.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Ошибка: {ex.Message}");
            }
        }

        private static List<OrderItem> CollectOrderFromUser()
        {
            var items = new List<OrderItem>();
            Console.WriteLine("Введите товары (введите 'стоп' для завершения):");

            int itemNumber = 1;
            while (true)
            {
                Console.Write($"\nТовар #{itemNumber}- Название: ");
                string name = Console.ReadLine()?.Trim();
                if (string.Equals(name, "стоп", StringComparison.OrdinalIgnoreCase)) break;
                if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine(" Название не может быть пустым."); continue; }

                decimal price = ReadDecimal($"Товар #{itemNumber}- Цена: ");
                int quantity = ReadInt($"Товар #{itemNumber}- Количество: ");

                items.Add(new OrderItem(name, price, quantity));
                itemNumber++;
            }
            return items;
        }

        private static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out decimal result) && result >= 0) return result;
                Console.WriteLine(" Ошибка: введите корректное неотрицательное число.");
            }
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result) && result > 0) return result;
                Console.WriteLine(" Ошибка: введите корректное положительное целое число.");
            }
        }
    }
}