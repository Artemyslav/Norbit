using System;
using System.Linq;
using StoreAnalytics.Data;
using StoreAnalytics.Services;

namespace StoreAnalytics
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Аналитика интернет-магазина\n");

            var categories = TestDataFactory.GetCategories();
            var suppliers = TestDataFactory.GetSuppliers();
            var products = TestDataFactory.GetProducts();
            var orders = TestDataFactory.GetOrders();

            try
            {
                // Формируем отчёт с фильтрацией
                var report = StoreAnalyticsService.BuildReport(
                    categories,
                    suppliers,
                    products,
                    orders,
                    minPrice: null,
                    maxPrice: null,
                    categoryId: null,
                    searchText: null,
                    pageNumber: 1,
                    pageSize: 5
                );

                Console.WriteLine("Общие показатели:");
                Console.WriteLine($"Всего товаров: {report.TotalProducts}");
                Console.WriteLine($"Активных товаров: {report.ActiveProducts}");
                Console.WriteLine($"Нет в наличии: {report.OutOfStockProducts}");
                Console.WriteLine($"Общая стоимость запасов: {report.TotalStockValue:C}");
                Console.WriteLine($"Средняя цена активного товара: {report.AverageActiveProductPrice:C}\n");

                Console.WriteLine(" Каталог (Страница 1, 5 шт):");
                foreach (var item in report.CatalogPage)
                {
                    Console.WriteLine($"- [{item.Id}] {item.Name} | {item.CategoryName} | Поставщик: {item.SupplierName}");
                    Console.WriteLine($"  Цена: {item.Price:C} (с НДС: {item.PriceWithTax:C}) | Статус: {item.StockStatus}");
                }
                Console.WriteLine();

                Console.WriteLine(" ОТЧЁТ ПО КАТЕГОРИЯМ:");
                foreach (var cat in report.Categories)
                {
                    Console.WriteLine($"- {cat.CategoryName}: Товаров: {cat.ProductCount}, Активных: {cat.ActiveProductCount}, В наличии: {cat.InStockProductCount}");
                    Console.WriteLine($"  Сумма запасов: {cat.TotalStockValue:C}, Ср. цена: {cat.AveragePrice?.ToString("C") ?? "N/A"}");
                }
                Console.WriteLine();

                Console.WriteLine(" ТОП-5 ПРОДАЖ:");
                foreach (var sale in report.TopSellingProducts)
                {
                    Console.WriteLine($"- {sale.ProductName} (ID: {sale.ProductId}): Продано {sale.TotalQuantity} шт. в {sale.OrdersCount} заказах. Выручка: {sale.Revenue:C}");
                }

                // Демонстрация отложенного выполнения vs материализации
                Console.WriteLine("\n ДЕМОНСТРАЦИЯ ОТЛОЖЕННОГО ВЫПОЛНЕНИЯ:");
                var deferredQuery = products.Where(p => p.Price > 10000m);
                Console.WriteLine("Запрос создан, но ещё не выполнен (тип: " + deferredQuery.GetType().Name + ")");
                
                var materializedList = deferredQuery.ToList();
                Console.WriteLine("Вызван .ToList(). Запрос выполнен, получено " + materializedList.Count + " элементов.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Ошибка: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}