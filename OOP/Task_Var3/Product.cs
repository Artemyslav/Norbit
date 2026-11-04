using System;

namespace Norbit.Tasks.OOP
{
    /// <summary>
    /// Описание товара
    /// </summary>
    public class Product
    {
        public string Name { get; }
        public string Manufacturer { get; }
        public double Price { get; protected set; }
        public DateTime ProductionDate { get; }
        public DateTime ExpirationDate { get; }

        public Product(string name, string manufacturer, double price, DateTime productionDate, DateTime expirationDate)
        {
            CheckNotNullOrWhiteSpace(name, nameof(name));
            CheckNotNullOrWhiteSpace(manufacturer, nameof(manufacturer));
            CheckIsPositive(price, nameof(price), "Цена должна быть больше нуля.");
            CheckExpiration(productionDate, expirationDate);

            Name = name;
            Manufacturer = manufacturer;
            Price = price;
            ProductionDate = productionDate;
            ExpirationDate = expirationDate;
        }

        private static void CheckNotNullOrWhiteSpace(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Значение не может быть пустым.", paramName);
        }

        private static void CheckIsPositive(double value, string paramName, string message)
        {
            if (value <= 0)
                throw new ArgumentException(message, paramName);
        }

        private static void CheckExpiration(DateTime production, DateTime expiration)
        {
            if (expiration <= production)
                throw new ArgumentException("Срок годности должен быть строго позже даты производства.");
        }

        public override string ToString()
        {
            return $"[{Name}] от {Manufacturer}. Цена: {Price:F2} руб. " +
                   $"Произведено: {ProductionDate:dd.MM.yyyy}, Годен до: {ExpirationDate:dd.MM.yyyy}";
        }
    }

    /// <summary>
    /// Описание товара со скидкой.
    /// </summary>
    public class DiscountedProduct : Product
    {
        private double _discountPercentage;

        public double DiscountPercentage
        {
            get => _discountPercentage;
            private set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentException("Размер скидки должен быть от 0 до 100%.", nameof(value));
                _discountPercentage = value;
            }
        }

        public double PromotionalPrice => Price * (1 - DiscountPercentage / 100.0);

        public DiscountedProduct(string name, string manufacturer, double price, DateTime productionDate, DateTime expirationDate, double discountPercentage)
            : base(name, manufacturer, price, productionDate, expirationDate)
        {
            DiscountPercentage = discountPercentage;
        }

        public override string ToString()
        {
            if (DiscountPercentage > 0)
            {
                return $"{base.ToString()} | АКЦИЯ: скидка {DiscountPercentage:F1}%, Акционная цена: {PromotionalPrice:F2} руб.";
            }
            return base.ToString();
        }
    }
}