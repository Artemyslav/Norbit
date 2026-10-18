using System;
using System.Text;

namespace Norbit.Tasks.TypeOfData
{
    // <summary>
    // Класс рисует ромб
    // </summary>
    public class DiamondDrawer
    {
        // <summary>
        // Выводит на экран ромб из символов X (в центре ничего)
        // </summary>
        // <param name="n">Длина диагонали ромба (положительное нечётное целое число).</param>
        public static void DrawDiamond(int n)
        {
            // Проверка входных данных
            if (n <= 0 || n % 2 == 0)
            {
                Console.WriteLine("Ошибка: N должно быть положительным нечётным числом.");
                return;
            }

            var center = n / 2;
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    // Проверяем, принадлежит ли точка контуру ромба
                    var distanceFromCenter = Math.Abs(i - center) + Math.Abs(j - center);
                    
                    if (distanceFromCenter == center)
                    {
                        // Если это центр ромба, рисуем пробел
                        if (i == center && j == center)
                        {
                            stringBuilder.Append(' ');
                        }
                        else
                        {
                            stringBuilder.Append('X');
                        }
                    }
                    else
                    {
                        stringBuilder.Append(' ');
                    }
                }
                
                stringBuilder.AppendLine();
            }

            Console.Write(stringBuilder.ToString());
        }
    }

    // <summary>
    // Точка входа и тесты
    // </summary>
    public class Program
    {
        public static void Main()
        {
            // Тестовые данные из условия
            var n = 5;
            
            Console.WriteLine($"Ромб для N={n}:");
            DiamondDrawer.DrawDiamond(n);
            
            Console.WriteLine();
            
            // Доп тесты
            Console.WriteLine($"Ромб для N=3:");
            DiamondDrawer.DrawDiamond(3);
            
            Console.WriteLine();
            
            Console.WriteLine($"Ромб для N=7:");
            DiamondDrawer.DrawDiamond(7);

            Console.WriteLine();

            // Для ручной проверки вводом
            var k = int.Parse(Console.ReadLine());
            
            Console.WriteLine($"Ромб для N={k}:");
            DiamondDrawer.DrawDiamond(k);

        }
    }
}