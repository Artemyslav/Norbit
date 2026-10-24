using System;

namespace Norbit.Tasks.OOP
{
    /// <summary>
    /// Класс, описывающий товар
    /// </summary>
    public class Product
    {
        public string Name { get; }
        public string Manufacturer { get; }
        public decimal Price { get; protected set; }
        public DateTime ProductionDate { get; }
        public DateTime ExpirationDate { get; }

        public Product(string name, string manufacturer, decimal price, DateTime productionDate, DateTime expirationDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Наименование не может быть пустым.", nameof(name));

            if (string.IsNullOrWhiteSpace(manufacturer))
                throw new ArgumentException("Производитель не может быть пустым.", nameof(manufacturer));

            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Цена не может быть отрицательной.");

            if (expirationDate <= productionDate)
                throw new ArgumentException("Срок годности должен быть строго позже даты производства.", nameof(expirationDate));

            Name = name;
            Manufacturer = manufacturer;
            Price = price;
            ProductionDate = productionDate;
            ExpirationDate = expirationDate;
        }

        public override string ToString()
        {
            return $"[{Name}] от {Manufacturer}. Цена: {Price:C}. " +
                   $"Произведено: {ProductionDate:dd.MM.yyyy}, Годен до: {ExpirationDate:dd.MM.yyyy}";
        }
    }

    /// <summary>
    /// Класс, описывающий товар со скидкой
    /// </summary>
    public class DiscountedProduct : Product
    {
        private decimal _discountPercentage;

        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            private set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value), "Размер скидки должен быть от 0 до 100%.");
                _discountPercentage = value;
            }
        }

        public decimal PromotionalPrice => Price * (1 - DiscountPercentage / 100m);

        public DiscountedProduct(string name, string manufacturer, decimal price, DateTime productionDate, DateTime expirationDate, decimal discountPercentage)
            : base(name, manufacturer, price, productionDate, expirationDate)
        {
            DiscountPercentage = discountPercentage;
        }

        public override string ToString()
        {
            // Если скидка больше 0, показываем акционную информацию
            if (DiscountPercentage > 0)
            {
                return $"{base.ToString()} !!! АКЦИЯ: скидка {DiscountPercentage}%, Акционная цена: {PromotionalPrice:C}";
            }
            
            return base.ToString();
        }
    }

    /// <summary>
    /// Точка входа
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Примеры\n");

            // 1. Пример обычного товара
            var regularProduct = new Product(
                "Молоко 'Домик в деревне'", "Вимм-Билль-Данн", 89.90m,
                new DateTime(2023, 10, 20), new DateTime(2023, 10, 27)
            );
            Console.WriteLine("1. Обычный товар:");
            Console.WriteLine(regularProduct);
            Console.WriteLine();

            // 2. Пример товара со скидкой
            var zeroDiscountProduct = new DiscountedProduct(
                "Хлеб 'Бородинский'", "Хлебозавод №1", 55.00m,
                DateTime.Today, DateTime.Today.AddDays(3), 20m
            );
            Console.WriteLine("2. Товар со скидкой:");
            Console.WriteLine(zeroDiscountProduct);
            Console.WriteLine();

            // 3. Ручной ввод данных пользователем
            Console.WriteLine("3. Создайте свой товар:");
            
            Console.Write("Введите наименование: ");
            var userInputName = Console.ReadLine();

            Console.Write("Введите производителя: ");
            var userInputManufacturer = Console.ReadLine();

            Console.Write("Введите базовую цену: ");
            decimal userInputPrice;
            while (!decimal.TryParse(Console.ReadLine(), out userInputPrice) || userInputPrice < 0)
            {
                Console.Write("Ошибка! Введите корректную положительную цену: ");
            }

            Console.Write("Введите размер скидки (0-100): ");
            decimal userInputDiscount;
            while (!decimal.TryParse(Console.ReadLine(), out userInputDiscount) || userInputDiscount < 0 || userInputDiscount > 100)
            {
                Console.Write("Ошибка! Введите корректный процент скидки (от 0 до 100): ");
            }

            // Ввод дат с валидацией
            Console.WriteLine("\n--- Ввод дат ---");
            var userProductionDate = ReadValidDate("Введите дату производства (в формате ДД.ММ.ГГГГ, например 20.10.2023): ");
            
            // Передаем дату производства как минимально допустимую для срока годности
            var userExpirationDate = ReadValidDate(
                "Введите срок годности (в формате ДД.ММ.ГГГГ, должен быть строго позже даты производства): ", 
                userProductionDate
            );

            try
            {
                var userProduct = new DiscountedProduct(
                    userInputName, userInputManufacturer, userInputPrice, 
                    userProductionDate, userExpirationDate, userInputDiscount
                );
                
                Console.WriteLine("\n Товар успешно создан:");
                Console.WriteLine(userProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Ошибка при создании товара: {ex.Message}");
            }
        }

        /// <summary>
        /// Метод для безопасного чтения и валидации даты из консоли.
        /// </summary>
        /// <param name="prompt">Текст подсказки для пользователя.</param>
        /// <param name="minDate">Минимально допустимая дата (например, дата производства).</param>
        /// <returns>Корректно введенная дата.</returns>
        private static DateTime ReadValidDate(string prompt, DateTime? minDate = null)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();

                if (DateTime.TryParse(input, out DateTime result))
                {
                    if (minDate.HasValue && result <= minDate.Value)
                    {
                        Console.WriteLine($"Ошибка: Дата должна быть строго позже {minDate.Value:dd.MM.yyyy}. Попробуйте снова.");
                        continue;
                    }
                    return result;
                }
                
                Console.WriteLine("Ошибка: Неверный формат даты. Используйте формат ДД.ММ.ГГГГ (например, 25.10.2023).");
            }
        }
    }
}