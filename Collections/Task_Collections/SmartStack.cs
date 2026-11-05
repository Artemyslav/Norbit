using System;
using System.Collections;
using System.Collections.Generic;

namespace Norbit.Tasks.Collections
{
    /// <summary>
    /// Реализация структуры данных "Умный стек" на базе обычного массива
    /// </summary>
    public class SmartStack<T> : IEnumerable<T>
    {
        private T[] arr;
        private int K;

        /// <summary>
        /// Создает стек с начальной емкостью 4 элемента.
        /// </summary>
        public SmartStack()
        {
            arr = new T[4];
            K = 0;
        }

        /// <summary>
        /// Создает стек с указанной емкостью.
        /// </summary>
        public SmartStack(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Емкость не может быть отрицательной.");
            
            arr = new T[capacity];
            K = 0;
        }

        /// <summary>
        /// Создает стек на основе коллекции
        /// </summary>
        public SmartStack(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (collection is ICollection<T> collectionWithCount)
            {
                arr = new T[collectionWithCount.Count];
                K = 0;
                foreach (var item in collection)
                {
                    arr[K++] = item;
                }
            }
            else
            {
                int count = 0;
                foreach (var _ in collection) count++;

                arr = new T[count];
                K = 0;
                foreach (var item in collection)
                {
                    arr[K++] = item;
                }
            }
        }

        /// <summary>
        /// Количество элементов в стеке.
        /// </summary>
        public int Count => K;

        /// <summary>
        /// Текущая емкость внутреннего массива.
        /// </summary>
        public int Capacity => arr.Length;

        /// <summary>
        /// Добавляет элемент на вершину стека. При нехватке места емкость удваивается.
        /// </summary>
        public void Push(T item)
        {
            if (K == arr.Length)
            {
                int newCapacity = arr.Length == 0 ? 4 : arr.Length * 2;
                Array.Resize(ref arr, newCapacity);
            }
            arr[K++] = item;
        }

        /// <summary>
        /// Добавляет элементы из коллекции. Последний элемент коллекции станет вершиной стека.
        /// </summary>
        public void PushRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            int collectionCount = 0;
            foreach (var _ in collection) collectionCount++;
            
            if (collectionCount == 0) return;

            while (K + collectionCount > arr.Length)
            {
                int newCapacity = arr.Length == 0 ? 4 : arr.Length * 2;
                Array.Resize(ref arr, newCapacity);
            }

            foreach (var item in collection)
            {
                arr[K++] = item;
            }
        }

        /// <summary>
        /// Удаляет и возвращает элемент с вершины стека.
        /// </summary>
        public T Pop()
        {
            if (K == 0)
                throw new InvalidOperationException("Стек пуст. Невозможно извлечь элемент.");
            
            K--;
            T item = arr[K];
            arr[K] = default; 
            return item;
        }

        /// <summary>
        /// Возвращает элемент с вершины стека без его удаления.
        /// </summary>
        public T Peek()
        {
            if (K == 0)
                throw new InvalidOperationException("Стек пуст. Невозможно просмотреть вершину.");
            
            return arr[K - 1];
        }

        /// <summary>
        /// Проверяет наличие элемента в стеке.
        /// </summary>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < K; i++)
            {
                if (comparer.Equals(arr[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Индексатор для доступа к элементу по глубине (0 вершина стека).
        /// </summary>
        public T this[int depth]
        {
            get
            {
                if (depth < 0 || depth >= K)
                    throw new ArgumentOutOfRangeException(nameof(depth), "Глубина вне диапазона элементов стека.");
                
                return arr[K - 1 - depth];
            }
            set
            {
                if (depth < 0 || depth >= K)
                    throw new ArgumentOutOfRangeException(nameof(depth), "Глубина вне диапазона элементов стека.");
                
                arr[K - 1 - depth] = value;
            }
        }

        /// <summary>
        /// Возвращает перечислитель для обхода стека от вершины к основанию.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = K - 1; i >= 0; i--)
            {
                yield return arr[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}