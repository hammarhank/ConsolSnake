using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snake
{
    public class MenuHelper
    {
        public static int MultipleChoice(bool canBeCanceled, string[] options)
        {
            const int startX = 10;
            const int startY = 20;
            const int optionsPerLine = 3;
            const int spacingPerLine = 14;

            int currentSelection = 0;

            ConsoleKey key;

            Console.CursorVisible = false;

            do
            {
                Console.Clear();

                DisplayWelcomeMessage();

                for (int i = 0; i < options.Length; i++)
                {
                    Console.SetCursorPosition(startX + (i % optionsPerLine) * spacingPerLine, startY + i / optionsPerLine);

                    if (i == currentSelection)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write($" {options[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        {
                            if (currentSelection % optionsPerLine > 0)
                                currentSelection--;
                            break;
                        }
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        {
                            if (currentSelection % optionsPerLine < optionsPerLine - 1)
                                currentSelection++;
                            break;
                        }
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        {
                            if (currentSelection >= optionsPerLine)
                                currentSelection -= optionsPerLine;
                            break;
                        }
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        {
                            if (currentSelection + optionsPerLine < options.Length)
                                currentSelection += optionsPerLine;
                            break;
                        }
                    case ConsoleKey.Escape:
                        {
                            if (canBeCanceled)
                                return -1;
                            break;
                        }
                }
            } while (key != ConsoleKey.Enter);

            Console.CursorVisible = true;

            return currentSelection;
        }

        static void DisplayWelcomeMessage()
        {
            var list = new List<string>()
            {
            @"$$\      $$\                $$$$$$\   $$$$$$\         $$\   ",
            @"$$$\    $$$ |              $$  __$$\ $$ ___$$\      $$$$ |  ",
            @"$$$$\  $$$$ |$$\ $$\   $$\ \__/  $$ |\_/   $$ |     \_$$ |  ",
            @"$$\$$\$$ $$ |\__|$$ |  $$ | $$$$$$  |  $$$$$ /$$$$$$\ $$ |  ",
            @"$$ \$$$  $$ |$$\ $$ |  $$ |$$  ____/   \___$$\\______|$$ |  ",
            @"$$ |\$  /$$ |$$ |$$ |  $$ |$$ |      $$\   $$ |       $$ |  ",
            @"$$ | \_/ $$ |$$ |\$$$$$$  |$$$$$$$$\ \$$$$$$  |     $$$$$$\ ",
            @"\__|     \__|$$ | \______/ \________| \______/      \______|",
            @"       $$\   $$ |                                           ",
            @"       \$$$$$$  |                                           ",
            @"        \______/                                            ",
            @"      $$$$$$\                      $$\                      ",
            @"     $$  __$$\                     $$ |                     ",
            @"     $$ /  \__|$$$$$$$\   $$$$$$\  $$ |  $$\  $$$$$$\       ",
            @"     \$$$$$$\  $$  __$$\  \____$$\ $$ | $$  |$$  __$$\      ",
            @"      \____$$\ $$ |  $$ | $$$$$$$ |$$$$$$  / $$$$$$$$ |     ",
            @"     $$\   $$ |$$ |  $$ |$$  __$$ |$$  _$$<  $$   ____|     ",
            @"     \$$$$$$  |$$ |  $$ |\$$$$$$$ |$$ | \$$\ \$$$$$$$\      ",
            @"      \______/ \__|  \__| \_______|\__|  \__| \_______|     ",
            };

            list.ForEach(Console.WriteLine);
        }
    }
}
