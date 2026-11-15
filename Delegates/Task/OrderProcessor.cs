using System;
using System.Collections.Generic;

namespace Norbit.Tasks.Delegates
{
    /// <summary>
    /// Пользовательский делегат для расчёта итоговой стоимости одной позиции.
    /// </summary>
    public delegate decimal ItemTotalCalculator(OrderItem item, decimal transformedUnitPrice);

    public sealed class OrderItem
    {
        public OrderItem(string name, decimal price, int quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public string Name { get; }
        public decimal Price { get; }
        public int Quantity { get; }
    }

    public sealed class OrderProcessedEventArgs : EventArgs
    {
        public OrderProcessedEventArgs(int orderId, decimal total, int processedItemsCount, DateTimeOffset processedAt)
        {
            OrderId = orderId;
            Total = total;
            ProcessedItemsCount = processedItemsCount;
            ProcessedAt = processedAt;
        }

        public int OrderId { get; }
        public decimal Total { get; }
        public int ProcessedItemsCount { get; }
        public DateTimeOffset ProcessedAt { get; }
    }

    /// <summary>
    /// Процессор заказов, демонстрирующий работу с делегатами и событиями.
    /// </summary>
    public class OrderProcessor
    {
        public event EventHandler<OrderProcessedEventArgs>? OrderProcessed;

        public decimal Process(
            int orderId,
            IEnumerable<OrderItem> items,
            Predicate<OrderItem> shouldProcess,
            Func<decimal, decimal> transformPrice,
            ItemTotalCalculator calculateItemTotal,
            Action<string>? log = null)
        {
            // 1. Валидация входных аргументов
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (shouldProcess == null) throw new ArgumentNullException(nameof(shouldProcess));
            if (transformPrice == null) throw new ArgumentNullException(nameof(transformPrice));
            if (calculateItemTotal == null) throw new ArgumentNullException(nameof(calculateItemTotal));
            if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId), "ID заказа должен быть положительным.");

            log?.Invoke($"[Старт] Начало обработки заказа #{orderId}");

            decimal totalOrderCost = 0m;
            int processedCount = 0;

            // 2. Последовательная обработка позиций
            foreach (var item in items)
            {
                if (item.Price < 0) 
                    throw new ArgumentException($"Отрицательная цена недопустима для товара: {item.Name}");
                if (item.Quantity <= 0) 
                    throw new ArgumentException($"Количество товара должно быть положительным для: {item.Name}");

                // Проверка предиката
                if (!shouldProcess(item))
                {
                    log?.Invoke($"[Пропуск] Товар '{item.Name}' исключен из расчета (не прошел проверку).");
                    continue;
                }

                // Трансформация цены 
                decimal transformedPrice = transformPrice(item.Price);
                log?.Invoke($"[Трансформация] Цена '{item.Name}' изменена: {item.Price} -> {transformedPrice}");

                // Расчет итога по позиции (демонстрация custom delegate)
                decimal itemTotal = calculateItemTotal(item, transformedPrice);
                log?.Invoke($"[Расчёт] Итоговая стоимость позиции '{item.Name}': {itemTotal}");

                totalOrderCost += itemTotal;
                processedCount++;
            }

            log?.Invoke($"[Финал] Обработка заказа #{orderId} завершена. Итоговая сумма: {totalOrderCost}");

            // 3. Формирование события
            var eventArgs = new OrderProcessedEventArgs(orderId, totalOrderCost, processedCount, DateTimeOffset.Now);
            OnOrderProcessed(eventArgs);

            return totalOrderCost;
        }

        /// <summary>
        /// Безопасный вызов события с обработкой ошибок.
        /// </summary>
        protected virtual void OnOrderProcessed(OrderProcessedEventArgs e)
        {
            EventHandler<OrderProcessedEventArgs>? handlers = OrderProcessed;

            if (handlers is not null)
            {
                foreach (EventHandler<OrderProcessedEventArgs> handler in handlers.GetInvocationList())
                {
                    try
                    {
                        handler(this, e);
                    }
                    catch (Exception ex)
                    {
                        // Изоляция ошибки: намеренно оставляем блок пустым.
                    }
                }
            }
        }
    }
}