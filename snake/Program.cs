using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Snake
{
    /// <summary>
    /// Program-klassen innehåller huvudmetoden och spelets logik.
    /// </summary>
    class Program
    {

        ///  /// <summary>
        /// Bredden på rutnätet.
        /// </summary>
        static readonly int gridW = 90;
        /// <summary>
        /// Höjden på rutnätet.
        /// </summary>
        static readonly int gridH = 25;
        /// <summary>
        /// Rutnätet där spelet utspelas.
        /// </summary>
        static Cell[,] grid = new Cell[gridH, gridW];
        /// <summary>
        /// Den nuvarande cellen där ormen befinner sig.
        /// </summary>
        static Cell currentCell;
        /// <summary>
        /// Den cell som innehåller mat för ormen.
        /// </summary>
        static Cell food;
        /// <summary>
        /// Räknare för hur mycket mat som har ätits.
        /// </summary>
        static int FoodCount;
        /// <summary>
        /// Riktning för ormens rörelse. 0 = Upp, 1 = Höger, 2 = Ner, 3 = Vänster.
        /// </summary>
        static int direction; //0=Up 1=Right 2=Down 3=Left
        static readonly int speed = 1;
        /// <summary>
        /// Om rutnätet är befolkat med objekt eller inte.
        /// </summary>
        static bool Populated = false;
        /// <summary>
        /// Om spelaren har förlorat eller inte.
        /// </summary>
        static bool Lost = false;
        /// <summary>
        /// Ormens längd.
        /// </summary>
        static int snakeLength;

        /// <summary>
        /// Huvudmetoden för Program-klassen.
        /// </summary>
        /// <param name="args">Argument för kommandoraden.</param>

        static void Main(string[] args)
        {
            string[] options = { "Start", "New", "Load", "Save",
            "Highscore", "Foo", "Bar", "FooBar", "etc." };
            int selectedIndex = MenuHelper.MultipleChoice(true, options);

            Console.Clear();

            // FIXME: Om man trycker på ecape i menyn kastas IndexOutOfRangeException
            string command = options[selectedIndex];

            if (command == options[0])
            {
                Start();
            }
        }
        /// <summary>
        /// Startar spelet
        /// </summary>
        static void Start()
        {
            if (!Populated)
            {
                FoodCount = 0;
                snakeLength = 5;
                populateGrid();
                currentCell = grid[(int)Math.Ceiling((double)gridH / 2), (int)Math.Ceiling((double)gridW / 2)];
                updatePos();
                addFood();
                Populated = true;
            }

            while (!Lost)
            {
                Restart();
            }
        }

        static void Restart()
        {
            Console.SetCursorPosition(0, 0);
            printGrid();
            Console.WriteLine("Length: {0}", snakeLength);
            getInput();
        }
        /// <summary>
        /// Uppdaterar skärmen med aktuell rutnät och orm.
        /// </summary>
        static void updateScreen()
        {
            Console.SetCursorPosition(0, 0);
            printGrid();
            Console.WriteLine("Length: {0}", snakeLength);
        }
        /// <summary>
        /// Tar emot spelarens input för att styra ormen.
        /// </summary>
        static void getInput()
        {

            //Console.Write("Where to move? [WASD] ");
            ConsoleKeyInfo input;
            while (!Console.KeyAvailable)
            {
                Move();
                updateScreen();
            }
            input = Console.ReadKey();
            doInput(input.Key);
        }
        /// <summary>
        /// Kontrollerar om den givna cellen innehåller mat eller om ormen kolliderar med sig själv.
        /// </summary>
        /// <param name="cell">Cellen som ska kontrolleras.</param>
        static void checkCell(Cell cell)
        {
            if (cell.val == "%")
            {
                eatFood();
            }
            if (cell.visited)
            {
                Lose();
            }
        }
        /// <summary>
        /// Hanterar förlust av spelet och återstartar det.
        /// </summary>
        static void Lose()
        {
            Console.Clear();
            var list = new List<string>()
            {
            @" $$$$$$\                                                                                  ",
            @"$$  __$$\                                                                                 ",
            @"$$ /  \__| $$$$$$\  $$$$$$\$$$$\   $$$$$$\         $$$$$$\ $$\    $$\  $$$$$$\   $$$$$$\  ",
            @"$$ |$$$$\  \____$$\ $$  _$$  _$$\ $$  __$$\       $$  __$$\\$$\  $$  |$$  __$$\ $$  __$$\ ",
            @"$$ |\_$$ | $$$$$$$ |$$ / $$ / $$ |$$$$$$$$ |      $$ /  $$ |\$$\$$  / $$$$$$$$ |$$ |  \__|",
            @"$$ |  $$ |$$  __$$ |$$ | $$ | $$ |$$   ____|      $$ |  $$ | \$$$  /  $$   ____|$$ |      ",
            @"\$$$$$$  |\$$$$$$$ |$$ | $$ | $$ |\$$$$$$$\       \$$$$$$  |  \$  /   \$$$$$$$\ $$ |      ",
            @" \______/  \_______|\__| \__| \__| \_______|       \______/    \_/     \_______|\__|      ",
            };

            Console.CursorVisible = false;
            for (int i = 0; i < list.Count; i++)
            {
                Console.SetCursorPosition((Console.WindowWidth - list[i].Length) / 2, ((Console.WindowHeight - (list.Count)) / 2) + i);
                Console.WriteLine(list[i]);
            }

            Thread.Sleep(3000);
            Console.CursorVisible = true;
            Console.Clear();

            Populated = false;
            Lost = false;
            Main(new string[0]);
        }
        /// <summary>
        /// Utför en åtgärd baserat på spelarens input.
        /// </summary>
        /// <param name="inp">Spelarens input.</param>
        static void doInput(ConsoleKey inp)
        {
            switch (inp)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    goUp();
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    goDown();
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    goRight();
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    goLeft();
                    break;
            }
        }
        /// <summary>
        /// Lägger till mat i en slumpmässig ledig cell på rutnätet.
        /// </summary>
        static void addFood()
        {
            Random r = new Random();
            Cell cell;
            while (true)
            {
                cell = grid[r.Next(grid.GetLength(0)), r.Next(grid.GetLength(1))];
                if (cell.val == " ")
                    cell.val = "%";
                break;
            }
        }
        /// <summary>
        /// Ökar ormens längd och lägger till mer mat på rutnätet.
        /// </summary>
        static void eatFood()
        {
            snakeLength += 1;
            addFood();
        }
        /// <summary>
        /// Ändrar ormens riktning till upp.
        /// </summary>
        static void goUp()
        {
            if (direction == 2)
                return;
            direction = 0;
        }
        /// <summary>
        /// Ändrar ormens riktning till höger.
        /// </summary>
        static void goRight()
        {
            if (direction == 3)
                return;
            direction = 1;
        }
        /// <summary>
        /// Ändrar ormens riktning till ner.
        /// </summary>
        static void goDown()
        {
            if (direction == 0)
                return;
            direction = 2;
        }
        /// <summary>
        /// Ändrar ormens riktning till vänster.
        /// </summary>
        static void goLeft()
        {
            if (direction == 1)
                return;
            direction = 3;
        }
        /// <summary>
        /// Flyttar ormen i den aktuella riktningen och hanterar kollisioner.
        /// </summary>
        static void Move()
        {
            if (direction == 0)
            {
                //up
                if (grid[currentCell.y - 1, currentCell.x].val == "*")
                {
                    Lose();
                    return;
                }
                visitCell(grid[currentCell.y - 1, currentCell.x]);
            }
            else if (direction == 1)
            {
                //right
                if (grid[currentCell.y, currentCell.x - 1].val == "*")
                {
                    Lose();
                    return;
                }
                visitCell(grid[currentCell.y, currentCell.x - 1]);
            }
            else if (direction == 2)
            {
                //down
                if (grid[currentCell.y + 1, currentCell.x].val == "*")
                {
                    Lose();
                    return;
                }
                visitCell(grid[currentCell.y + 1, currentCell.x]);
            }
            else if (direction == 3)
            {
                //left
                if (grid[currentCell.y, currentCell.x + 1].val == "*")
                {
                    Lose();
                    return;
                }
                visitCell(grid[currentCell.y, currentCell.x + 1]);
            }
            Thread.Sleep(speed * 100);
        }
        /// <summary>
        /// Märker den givna cellen som besökt och uppdaterar ormens position.
        /// </summary>
        /// <param name="cell">Cellen som ska besökas.</param>
        static void visitCell(Cell cell)
        {
            currentCell.val = "#";
            currentCell.visited = true;
            currentCell.decay = snakeLength;
            checkCell(cell);
            currentCell = cell;
            updatePos();

            //checkCell(currentCell);
        }
        /// <summary>
        /// Uppdaterar ormens position och riktning.
        /// </summary>
        static void updatePos()
        {

            currentCell.Set("@");
            if (direction == 0)
            {
                currentCell.val = "^";
            }
            else if (direction == 1)
            {
                currentCell.val = "<";
            }
            else if (direction == 2)
            {
                currentCell.val = "v";
            }
            else if (direction == 3)
            {
                currentCell.val = ">";
            }

            currentCell.visited = false;
            return;
        }
        /// <summary>
        /// Fyller rutnätet med celler och sätter upp spelplanen.
        /// </summary>
        static void populateGrid()
        {
            Random random = new Random();
            for (int col = 0; col < gridH; col++)
            {
                for (int row = 0; row < gridW; row++)
                {
                    Cell cell = new Cell();
                    cell.x = row;
                    cell.y = col;
                    cell.visited = false;
                    if (cell.x == 0 || cell.x > gridW - 2 || cell.y == 0 || cell.y > gridH - 2)
                        cell.Set("*");
                    else
                        cell.Clear();
                    grid[col, row] = cell;
                }
            }
        }
        /// <summary>
        /// Skriver ut rutnätet på skärmen.
        /// </summary>

        static void printGrid()
        {
            string toPrint = "";
            for (int col = 0; col < gridH; col++)
            {
                for (int row = 0; row < gridW; row++)
                {
                    grid[col, row].decaySnake();
                    toPrint += grid[col, row].val;

                }
                toPrint += "\n";
            }
            Console.WriteLine(toPrint);
        }
        /// <summary>
        /// Cell-klassen representerar en cell på rutnätet där spelet utspelas.
        /// </summary>
        public class Cell
        {
            public string val
            {
                get;
                set;
            }
            public int x
            {
                get;
                set;
            }
            public int y
            {
                get;
                set;
            }
            public bool visited
            {
                get;
                set;
            }
            public int decay
            {
                get;
                set;
            }

            public void decaySnake()
            {
                decay -= 1;
                if (decay == 0)
                {
                    visited = false;
                    val = " ";
                }
            }

            public void Clear()
            {
                val = " ";
            }

            public void Set(string newVal)
            {
                val = newVal;
            }
        }
    }
}