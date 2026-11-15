using System;
using System.Collections.Generic;
using StoreAnalytics.Models;

namespace StoreAnalytics.Data
{
    public static class TestDataFactory
    {
        public static IReadOnlyList<Category> GetCategories() => new List<Category>
        {
            new(1, "Электроника"),
            new(2, "Книги"),
            new(3, "Одежда"),
            new(4, "Пустая категория") // Категория без товаров
        };

        public static IReadOnlyList<Supplier> GetSuppliers() => new List<Supplier>
        {
            new(1, "TechCorp", 4.8m),
            new(2, "BookWorld", 4.5m),
            new(3, "FashionInc", 3.9m),
            new(4, "NoProductsSupplier", 5.0m), // Поставщик без товаров
            new(5, "GlobalParts", 4.2m)
        };

        public static IReadOnlyList<Product> GetProducts() => new List<Product>
        {
            // Электроника
            new(1, "Ноутбук Pro", 1, 1, 100000m, 10, true, new DateTime(2026, 1, 15)),
            new(2, "Смартфон X", 1, 1, 50000m, 0, true, new DateTime(2026, 1, 10)), // Остаток 0
            new(3, "Наушники", 1, null, 5000m, 50, true, new DateTime(2026, 1, 5)), // Без поставщика
            new(4, "Планшет", 1, 5, 30000m, 5, true, new DateTime(2026, 1, 20)),
            new(5, "Зарядка", 1, 1, 1500m, 100, false, new DateTime(2026, 1, 1)), // Неактивный
            
            // Книги
            new(6, "C# для профессионалов", 2, 2, 3500m, 20, true, new DateTime(2026, 2, 1)),
            new(7, "LINQ в действии", 2, 2, 3500m, 15, true, new DateTime(2026, 2, 5)), // Та же цена, что и у 6 (для проверки ThenBy)
            new(8, "Алгоритмы", 2, 2, 4000m, 0, true, new DateTime(2026, 2, 10)),
            
            // Одежда
            new(9, "Футболка", 3, 3, 1200m, 200, true, new DateTime(2026, 3, 1)),
            new(10, "Джинсы", 3, 3, 3500m, 40, true, new DateTime(2026, 3, 5)),
            new(11, "Кроссовки", 3, 3, 8000m, 30, true, new DateTime(2026, 3, 10)),
            new(12, "Куртка", 3, 3, 12000m, 10, true, new DateTime(2026, 3, 15)),
            new(13, "Шапка", 3, null, 800m, 5, true, new DateTime(2026, 3, 20)),
            new(14, "Перчатки", 3, 5, 600m, 0, true, new DateTime(2026, 3, 25)),
            new(15, "Шарф", 3, 5, 900m, 15, true, new DateTime(2026, 3, 30)),
            new(16, "Носки", 3, 3, 200m, 500, true, new DateTime(2026, 4, 1)),
            new(17, "Ремень", 3, 3, 1500m, 20, true, new DateTime(2026, 4, 5)),
            new(18, "Сумка", 3, 3, 4500m, 8, true, new DateTime(2026, 4, 10)),
            new(19, "Очки", 3, 1, 2500m, 12, true, new DateTime(2026, 4, 15)),
            new(20, "Часы", 1, 1, 15000m, 3, true, new DateTime(2026, 4, 20)) // Будет в нескольких заказах
        };

        public static IReadOnlyList<Order> GetOrders() => new List<Order>
        {
            new(1, 101, new DateTime(2026, 5, 1), new List<OrderItem> {
                new(1, 1, 100000m),
                new(6, 2, 3500m)
            }),
            new(2, 102, new DateTime(2026, 5, 2), new List<OrderItem> {
                new(20, 1, 15000m) // Часы куплены
            }),
            new(3, 103, new DateTime(2026, 5, 3), new List<OrderItem> {
                new(20, 2, 15000m), // Часы куплены снова (несколько заказов)
                new(9, 3, 1200m)
            }),
            new(4, 104, new DateTime(2026, 5, 4), new List<OrderItem> {
                new(11, 1, 8000m)
            }),
            new(5, 105, new DateTime(2026, 5, 5), new List<OrderItem> {
                new(6, 1, 3500m),
                new(7, 1, 3500m)
            }),
            new(6, 106, new DateTime(2026, 5, 6), new List<OrderItem> {
                new(10, 2, 3500m)
            }),
            new(7, 107, new DateTime(2026, 5, 7), new List<OrderItem> {
                new(16, 10, 200m)
            }),
            new(8, 108, new DateTime(2026, 5, 8), new List<OrderItem> {
                new(18, 1, 4500m)
            })
            // Товар 3 (Наушники) и 4 (Планшет) не куплены ни разу
        };
    }
}