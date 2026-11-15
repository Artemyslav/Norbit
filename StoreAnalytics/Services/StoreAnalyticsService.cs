using System;
using System.Collections.Generic;
using System.Linq;
using StoreAnalytics.Models;

namespace StoreAnalytics.Services
{
    public static class StoreAnalyticsService
    {
        public static StoreAnalyticsReport BuildReport(
            IEnumerable<Category> categories,
            IEnumerable<Supplier> suppliers,
            IEnumerable<Product> products,
            IEnumerable<Order> orders,
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId,
            string? searchText,
            int pageNumber,
            int pageSize)
        {
            // 1. Валидация аргументов
            if (categories == null) throw new ArgumentNullException(nameof(categories));
            if (suppliers == null) throw new ArgumentNullException(nameof(suppliers));
            if (products == null) throw new ArgumentNullException(nameof(products));
            if (orders == null) throw new ArgumentNullException(nameof(orders));
            if (pageNumber < 1 || pageSize < 1) throw new ArgumentOutOfRangeException("Номер страницы и размер должны быть >= 1");
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice) throw new ArgumentException("minPrice не может быть больше maxPrice");

            // Проверка некорректных товаров (используем All для демонстрации)
            if (!products.All(p => p.Price >= 0 && p.StockQuantity >= 0))
                throw new ArgumentException("Обнаружены товары с отрицательной ценой или остатком");

            // Проверка некорректных заказов
            if (orders.Any(o => o.Items.Any(i => i.Quantity <= 0 || i.UnitPrice < 0)))
                throw new ArgumentException("Обнаружены некорректные позиции в заказах");

            // 2. Общие показатели
            int totalProducts = products.Count();
            int activeProducts = products.Count(p => p.IsActive);
            
            // Демонстрация Any()
            bool hasOutOfStock = products.Any(p => p.StockQuantity == 0);
            int outOfStockProducts = hasOutOfStock ? products.Count(p => p.StockQuantity == 0) : 0;

            decimal totalStockValue = products.Sum(p => p.Price * p.StockQuantity);
            
            // Демонстрация Average с nullable, чтобы избежать исключения на пустой последовательности
            decimal? avgActivePrice = products.Where(p => p.IsActive).Select(p => (decimal?)p.Price).Average();

            // Демонстрация FirstOrDefault
            var expensiveProduct = products.FirstOrDefault(p => p.Price > 100000m); 
            
            // Демонстрация Single (предполагаем, что ID уникален)
            var electronicsCategory = categories.Single(c => c.Id == 1);

            // 3. Каталог товаров (динам-ая фильтрация)
            IEnumerable<Product> filteredProducts = products.Where(p => p.IsActive && p.StockQuantity > 0);

            if (minPrice.HasValue) filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value);
            if (categoryId.HasValue) filteredProducts = filteredProducts.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.ToLower();
                filteredProducts = filteredProducts.Where(p => p.Name.ToLower().Contains(search));
            }

            // Левое соединение с категориями и поставщиками
            var catalogQuery = filteredProducts
                .GroupJoin(categories,
                    p => p.CategoryId,
                    c => c.Id,
                    (p, cGroup) => new { Product = p, Category = cGroup.DefaultIfEmpty(new Category(0, "Неизвестная категория")).First() })
                .GroupJoin(suppliers,
                    x => x.Product.SupplierId,
                    s => (int?)s.Id,
                    (x, sGroup) => new { x.Product, x.Category, Supplier = sGroup.DefaultIfEmpty(new Supplier(0, "Не указан", 0)).First() })
                .Select(x => new ProductCatalogItem(
                    x.Product.Id,
                    x.Product.Name,
                    x.Category.Name,
                    x.Supplier.Name,
                    x.Product.Price,
                    decimal.Round(x.Product.Price * 1.2m, 2, MidpointRounding.AwayFromZero),
                    GetStockStatus(x.Product.StockQuantity),
                    x.Product.IsActive
                ));

            /* 
             * КОММЕНТАРИИ ПО КАТАЛОГУ:
             * 1. OrderBy обязателен перед Skip/Take, потому что без детерминированной сортировки 
             *    база данных (или LINQ to Objects) может возвращать элементы в произвольном порядке, 
             *    что приведет к дублированию или пропуску элементов на разных страницах.
             * 2. ThenBy(item => item.Id) критически важен как "tie-breaker". Если у двух товаров 
             *    одинаковая цена и имя, сортировка по ID гарантирует стабильный и предсказуемый порядок.
             * 3. Вызов ToList() ДО фильтрации и сортировки материализовал бы всю коллекцию в память, 
             *    после чего Skip/Take работали бы уже с загруженными данными. Это менее эффективно, 
             *    так как мы теряем возможность провайдера (например, SQL) выполнить фильтрацию и пагинацию на стороне сервера.
             */

            var sortedCatalog = catalogQuery
                .OrderBy(item => item.Price)
                .ThenBy(item => item.Name)
                .ThenBy(item => item.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // Материализация только финальной страницы

            // 4. Отчёт по категориям (Query Syntax для демонстрации)
            /* 
             * ДЕМОНСТРАЦИЯ AsEnumerable vs AsQueryable:
             * Если бы "categories" был IQueryable, вызов AsEnumerable() здесь 
             * принудительно выполнил бы запрос к БД и перенес бы дальнейшую группировку в память приложения.
             * Вызов AsQueryable() на List<T> оборачивает список, но выполнение 
             * всё равно остаётся в памяти (LINQ to Objects). Мы используем это здесь для явного указания намерения.
             */
            var categoryReportQuery = 
                from c in categories.AsEnumerable() // Явное указание на клиентскую обработку
                join p in products on c.Id equals p.CategoryId into productGroup
                let activeCount = productGroup.Count(p => p.IsActive)
                let inStockCount = productGroup.Count(p => p.StockQuantity > 0)
                select new CategoryStockReport(
                    c.Name,
                    productGroup.Count(),
                    activeCount,
                    inStockCount,
                    productGroup.Sum(p => p.StockQuantity),
                    productGroup.Sum(p => p.Price * p.StockQuantity),
                    productGroup.Select(p => (decimal?)p.Price).Average(),
                    productGroup.Select(p => (decimal?)p.Price).Min(),
                    productGroup.Select(p => (decimal?)p.Price).Max()
                );

            var categoryReports = categoryReportQuery.OrderBy(r => r.CategoryName).ToList();

            // Доп группировка по ценовому диапазону (вывод в консоль в Program.cs)
            var priceRangeGroups = products.GroupBy(p => new 
            { 
                p.CategoryId, 
                Range = decimal.Floor(p.Price / 500m) * 500m 
            });

            // 5. Отчёт по продажам
            /*
             * КОММЕНТАРИИ ПО ПРОДАЖАМ:
             * 1. SelectMany используется вместо Select, потому что Select вернул бы IEnumerable<IEnumerable<OrderItem>> 
             *    (вложенную структуру), а SelectMany "выпрямляет" (flattens) её в одну плоскую последовательность строк.
             * 2. Distinct() необходим для OrdersCount, потому что один и тот же OrderId может встречаться 
             *    несколько раз в плоской таблице (если в одном заказе несколько разных товаров), и нам нужно 
             *    посчитать именно количество уникальных заказов, а не общее количество строк.
             * 3. Товар, который не встречается в заказах, не попадёт в этот рейтинг при использовании 
             *    обычного Join (Inner Join), так как Join требует совпадения ключей в обеих коллекциях. 
             *    Для включения таких товаров потребовался бы GroupJoin (Left Join).
             */
            
            var flatOrderItems = orders.SelectMany(o => o.Items.Select(i => new 
            { 
                OrderId = o.Id, 
                ProductId = i.ProductId, 
                Quantity = i.Quantity, 
                Revenue = i.Quantity * i.UnitPrice 
            }));

            var salesReportQuery = 
                from item in flatOrderItems
                join p in products on item.ProductId equals p.Id
                group item by new { p.Id, p.Name } into g
                orderby g.Sum(x => x.Revenue) descending,
                        g.Sum(x => x.Quantity) descending,
                        g.Key.Name ascending,
                        g.Key.Id ascending
                select new ProductSalesReport(
                    g.Key.Id,
                    g.Key.Name,
                    g.Sum(x => x.Quantity),
                    g.Select(x => x.OrderId).Distinct().Count(),
                    g.Sum(x => x.Revenue)
                );

            var topSellingProducts = salesReportQuery.Take(5).ToList();

            return new StoreAnalyticsReport(
                totalProducts,
                activeProducts,
                outOfStockProducts,
                totalStockValue,
                avgActivePrice,
                sortedCatalog,
                categoryReports,
                topSellingProducts
            );
        }

        private static string GetStockStatus(int quantity)
        {
            if (quantity == 0) return "Нет в наличии";
            if (quantity >= 1 && quantity <= 5) return "Мало осталось";
            return "В наличии";
        }
    }
}