using System;
using System.Collections.Generic;

namespace Norbit.Tasks.Collections
{
    /// <summary>
    /// Точка входа в приложение.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            var stack = new SmartStack<int>();
            string[] menuItems = 
            {
                "Push (Добавить элемент)",
                "PushRange (Добавить несколько элементов через запятую)",
                "Pop (Извлечь верхний элемент)",
                "Peek (Посмотреть верхний элемент)",
                "Contains (Проверить наличие элемента)",
                "Показать весь стек (сверху вниз)",
                "Проверить свойства (Count/Capacity)",
                "Выход"
            };

            int selectedIndex = 0;
            bool exit = false;

            Console.CursorVisible = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Умный Стек\n");

                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine($" > {menuItems[i]} ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"   {menuItems[i]}");
                    }
                }

                Console.WriteLine("\nИспользуйте ↑ ↓ для выбора, Enter для подтверждения.");

                var keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + menuItems.Length) % menuItems.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % menuItems.Length;
                        break;
                    case ConsoleKey.Enter:
                        Console.CursorVisible = true;
                        ExecuteAction(selectedIndex, stack);
                        Console.CursorVisible = false;
                        break;
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                }
            }
        }

        private static void ExecuteAction(int index, SmartStack<int> stack)
        {
            Console.Clear();
            Console.WriteLine($"Действие: {GetActionName(index)}\n");

            try
            {
                switch (index)
                {
                    case 0: // Push
                        int item = ReadInt("Введите элемент для добавления: ");
                        stack.Push(item);
                        Console.WriteLine(" Элемент успешно добавлен.");
                        break;

                    case 1: // PushRange
                        Console.Write("Введите элементы через запятую (например: 10, 20, 30): ");
                        string input = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            var items = ParseIntCollection(input);
                            stack.PushRange(items);
                            Console.WriteLine(" Элементы успешно добавлены.");
                        }
                        break;

                    case 2: // Pop
                        int popped = stack.Pop();
                        Console.WriteLine($" Извлечен элемент: {popped}");
                        break;

                    case 3: // Peek
                        int peeked = stack.Peek();
                        Console.WriteLine($" Верхний элемент: {peeked}");
                        break;

                    case 4: // Contains
                        int searchItem = ReadInt("Введите элемент для поиска: ");
                        bool exists = stack.Contains(searchItem);
                        Console.WriteLine(exists ? " Элемент найден в стеке." : " Элемент не найден.");
                        break;

                    case 5: // Показать стек
                        if (stack.Count == 0)
                        {
                            Console.WriteLine("Стек пуст.");
                        }
                        else
                        {
                            Console.WriteLine("Содержимое стека (0 вершина):");
                            for (int i = 0; i < stack.Count; i++)
                            {
                                Console.WriteLine($"  [{i}] {stack[i]}");
                            }
                        }
                        break;

                    case 6: // Свойства
                        Console.WriteLine($" Count (элементов): {stack.Count}");
                        Console.WriteLine($" Capacity (емкость массива): {stack.Capacity}");
                        break;

                    case 7: // Выход
                        Console.WriteLine("До свидания!");
                        Environment.Exit(0);  
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Ошибка: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey(true);
        }

        private static string GetActionName(int index)
        {
            string[] names = 
            {
                "Push (Добавить элемент)",
                "PushRange (Добавить несколько элементов через запятую)",
                "Pop (Извлечь верхний элемент)",
                "Peek (Посмотреть верхний элемент)",
                "Contains (Проверить наличие элемента)",
                "Показать весь стек (сверху вниз)",
                "Проверить свойства (Count/Capacity)",
                "Выход"
            };
            return names[index];
        }


        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result))
                    return result;
                Console.WriteLine(" Ошибка: введите корректное целое число.");
            }
        }

        private static IEnumerable<int> ParseIntCollection(string input)
        {
            string[] parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                if (int.TryParse(part.Trim(), out int value))
                {
                    yield return value;
                }
            }
        }
    }
}