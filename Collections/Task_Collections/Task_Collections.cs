using System;
using System.Collections;
using System.Collections.Generic;

namespace Norbit.Tasks.Collections
{
    /// <summary>
    /// Реализация структуры данных "Умный стек" на базе обычного массива.
    /// </summary>
    /// <typeparam name="T">Тип элементов в стеке.</typeparam>
    public class SmartStack<T> : IEnumerable<T>
    {
        private T[] _array;
        private int _count;

        /// <summary>
        /// Конструктор без параметров. Создаёт массив ёмкостью 4 элемента.
        /// </summary>
        public SmartStack() : this(4)
        {
        }

        /// <summary>
        /// Конструктор с указанием начальной ёмкости.
        /// </summary>
        /// <param name="capacity">Начальная ёмкость массива.</param>
        public SmartStack(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Ёмкость не может быть отрицательной.");
            }
            
            _array = new T[capacity];
            _count = 0;
        }

        /// <summary>
        /// Конструктор, инициализирующий стек элементами из коллекции.
        /// Последний элемент коллекции станет вершиной стека.
        /// </summary>
        /// <param name="collection">Коллекция элементов.</param>
        public SmartStack(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            // Оптимизация: если коллекция знает свой размер, создаём массив сразу нужного размера
            if (collection is ICollection<T> c)
            {
                _count = c.Count;
                _array = new T[_count];
                c.CopyTo(_array, 0);
            }
            else
            {
                // Fallback для чистого IEnumerable (например, результат LINQ-запроса)
                // Динамически расширяем массив, а в конце обрезаем до точного размера
                _array = new T[4];
                _count = 0;
                
                foreach (var item in collection)
                {
                    if (_count == _array.Length)
                    {
                        Resize(_array.Length * 2);
                    }
                    _array[_count++] = item;
                }

                // Обрезаем массив до фактического количества элементов, чтобы выполнить требование "нужного размера"
                if (_count < _array.Length)
                {
                    Resize(_count);
                }
            }
        }

        /// <summary>
        /// Добавляет элемент на вершину стека.
        /// </summary>
        public void Push(T item)
        {
            if (_count == _array.Length)
            {
                Resize(_array.Length * 2);
            }
            _array[_count++] = item;
        }

        /// <summary>
        /// Добавляет элементы из коллекции на вершину стека.
        /// Последний элемент коллекции станет новой вершиной стека.
        /// </summary>
        public void PushRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection is ICollection<T> c)
            {
                int neededCapacity = _count + c.Count;
                if (neededCapacity > _array.Length)
                {
                    int newCapacity = _array.Length;
                    while (newCapacity < neededCapacity)
                    {
                        newCapacity *= 2;
                    }
                    Resize(newCapacity);
                }
                c.CopyTo(_array, _count);
                _count += c.Count;
            }
            else
            {
                // Fallback для чистого IEnumerable
                foreach (var item in collection)
                {
                    Push(item); // Используем существующий метод Push для гарантии корректного расширения
                }
            }
        }

        /// <summary>
        /// Удаляет и возвращает элемент с вершины стека.
        /// </summary>
        public T Pop()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Стек пуст.");
            }

            _count--;
            T item = _array[_count];
            
            // Очищаем ссылку, чтобы сборщик мусора мог удалить объект (предотвращение утечек памяти)
            _array[_count] = default(T); 
            
            return item;
        }

        /// <summary>
        /// Возвращает элемент с вершины стека без его удаления.
        /// </summary>
        public T Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Стек пуст.");
            }
            return _array[_count - 1];
        }

        /// <summary>
        /// Проверяет наличие элемента в стеке.
        /// </summary>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(_array[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Получает количество элементов в стеке.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Получает текущую ёмкость внутреннего массивa.
        /// </summary>
        public int Capacity => _array.Length;

        /// <summary>
        /// Индексатор для доступа к элементу по глубине.
        /// 0 - вершина стека, Count - 1 - основание стека.
        /// </summary>
        public T this[int depth]
        {
            get
            {
                if (depth < 0 || depth >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(depth), "Глубина выходит за границы стека.");
                }
                return _array[_count - 1 - depth];
            }
            set
            {
                if (depth < 0 || depth >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(depth), "Глубина выходит за границы стека.");
                }
                _array[_count - 1 - depth] = value;
            }
        }

        /// <summary>
        /// Возвращает перечислитель, выполняющий итерацию по стеку от вершины к основанию.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _count - 1; i >= 0; i--)
            {
                yield return _array[i];
            }
        }

        /// <summary>
        /// Неявная реализация интерфейса IEnumerable.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Внутренний метод для изменения размера массива.
        /// </summary>
        private void Resize(int newCapacity)
        {
            if (newCapacity < _count)
            {
                newCapacity = _count; // Защита от некорректных значений
            }

            T[] newArray = new T[newCapacity];
            Array.Copy(_array, newArray, _count);
            _array = newArray;
        }
    }

    /// <summary>
    /// Точка входа в приложение для демонстрации работы SmartStack.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Демонстрация SmartStack<T> ===\n");

            // 1. Тест конструктора без параметров и Push
            var stack1 = new SmartStack<int>();
            Console.WriteLine($"Новый стек: Count = {stack1.Count}, Capacity = {stack1.Capacity}"); // 0, 4
            
            stack1.Push(10);
            stack1.Push(20);
            stack1.Push(30);
            stack1.Push(40);
            stack1.Push(50); // Здесь произойдёт удвоение Capacity до 8
            Console.WriteLine($"После 5 Push: Count = {stack1.Count}, Capacity = {stack1.Capacity}");

            // 2. Тест Pop и Peek
            Console.WriteLine($"\nPeek (вершина): {stack1.Peek()}"); // 50
            Console.WriteLine($"Pop (удаление): {stack1.Pop()}");    // 50
            Console.WriteLine($"После Pop: Count = {stack1.Count}, Capacity = {stack1.Capacity}"); // Capacity не уменьшился!

            // 3. Тест конструктора с IEnumerable и PushRange
            var list = new List<string> { "Apple", "Banana", "Cherry" };
            var stack2 = new SmartStack<string>(list);
            
            Console.WriteLine("\nСтек из списка (Apple, Banana, Cherry):");
            Console.WriteLine($"Вершина стека (должна быть Cherry): {stack2.Peek()}");
            
            stack2.PushRange(new[] { "Date", "Elderberry" });
            Console.WriteLine($"После PushRange массива: Вершина = {stack2.Peek()}"); // Elderberry

            // 4. Тест Contains
            Console.WriteLine($"\nСодержит 'Banana'? {stack2.Contains("Banana")}"); // True
            Console.WriteLine($"Содержит 'Zucchini'? {stack2.Contains("Zucchini")}"); // False

            // 5. Тест индексатора (глубина)
            Console.WriteLine("\nОбход по индексу (глубине):");
            for (int i = 0; i < stack2.Count; i++)
            {
                Console.WriteLine($"Глубина {i}: {stack2[i]}");
            }

            // 6. Тест IEnumerable (foreach от вершины к основанию)
            Console.WriteLine("\nОбход через foreach (от вершины к основанию):");
            foreach (var item in stack2)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();

            // 7. Тест исключений
            Console.WriteLine("\nПроверка исключений:");
            var emptyStack = new SmartStack<double>();
            try
            {
                emptyStack.Pop();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"✅ Ожидаемая ошибка при Pop: {ex.Message}");
            }

            try
            {
                var _ = stack1[100];
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"✅ Ожидаемая ошибка индексатора: {ex.Message}");
            }
        }
    }
}