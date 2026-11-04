using System;

namespace Norbit.Tasks.OOP
{
    /// <summary>
    /// Вход
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Создание товара\n");

            try
            {
                // Собираем данные
                string name = ReadString("Наименование товара: ");
                string manufacturer = ReadString("Производитель: ");
                double price = ReadDouble("Цена: ");
                DateTime prodDate = ReadDate("Дата производства (ДД.ММ.ГГГГ): ");
                DateTime expDate = ReadDate("Годен до (ДД.ММ.ГГГГ): ");
                double discount = ReadDouble("Размер скидки (от 0 до 100%): ");

                Product finalProduct;

                if (discount == 0)
                {
                    finalProduct = new Product(name, manufacturer, price, prodDate, expDate);
                }
                else
                {
                    finalProduct = new DiscountedProduct(name, manufacturer, price, prodDate, expDate, discount);
                }

                // Вывод результата
                Console.WriteLine("\n Товар успешно создан:");
                Console.WriteLine(finalProduct);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\n Ошибка валидации данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Произошла ошибка: {ex.Message}");
            }
        }

        private static string ReadString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }

        private static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double result))
                    return result;
                Console.WriteLine(" Ошибка: недопустимый символ.");
            }
        }

        private static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParse(Console.ReadLine(), out DateTime result))
                    return result;
                Console.WriteLine(" Ошибка: введите дату в формате ДД.ММ.ГГГГ.");
            }
        }
    }
}