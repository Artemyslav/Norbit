using System;
using System.Collections.Generic;

namespace Norbit.Tasks.TypeOfData
{
    /// <summary>
    /// Генератор ромба с пустым центром.
    /// </summary>
    public class DiamondDrawer
    {
        /// <summary>
        /// Генерирует ромб из символов X с пустым центром.
        /// </summary>
        /// <param name="n">Длина диагонали ромба (положительное нечётное целое число).</param>
        /// <returns>Список строк, представляющих ромб.</returns>
        public static List<string> GenerateDiamond(int n)
        {
            // Валидация
            CheckIsPositive(n, "Длина диагонали должна быть положительным числом.");
            CheckIsOdd(n, "Длина диагонали должна быть нечётным числом.");

            var result = new List<string>();
            var center = n / 2;

            for (var i = 0; i < n; i++)
            {
                var line = "";
                for (var j = 0; j < n; j++)
                {
                    var distanceFromCenter = Math.Abs(i - center) + Math.Abs(j - center);

                    if (distanceFromCenter == center)
                    {
                        // Если это центр ромба, рисуем пробел
                        if (i == center && j == center)
                        {
                            line += ' ';
                        }
                        else
                        {
                            line += 'X';
                        }
                    }
                    else
                    {
                        line += ' ';
                    }
                }
                result.Add(line);
            }

            return result;
        }

        /// <summary>
        /// Проверяет, что значение положительное.
        /// </summary>
        private static void CheckIsPositive(int value, string errorMessage)
        {
            if (value <= 0)
            {
                throw new ArgumentException(errorMessage);
            }
        }

        /// <summary>
        /// Проверяет, что значение нечётное.
        /// </summary>
        private static void CheckIsOdd(int value, string errorMessage)
        {
            if (value % 2 == 0)
            {
                throw new ArgumentException(errorMessage);
            }
        }
    }
}