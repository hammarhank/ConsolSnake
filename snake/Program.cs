using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace Snake
{
    /// <summary>
    /// Program-klassen innehåller huvudmetoden och spelets logik.
    /// </summary>
    class Program
    {
        ///  /// <summary>
        /// Bredden på spelplanen.
        /// </summary>
        static readonly int gridW = 90;
        /// <summary>
        /// Höjden på spelplanen.
        /// </summary>
        static readonly int gridH = 25;
        /// <summary>
        /// Spelplanen är strukturerad som en array.
        /// </summary>
        static Cell[,] grid = new Cell[gridH, gridW];
        /// <summary>
        /// Den nuvarande cellen där ormen befinner sig.
        /// </summary>
        static Cell currentCell;
        /// <summary>
        /// Räknare för hur många matbitar som har ätits. Används i metoden 'eatFood' för att öka svårighetsgraden.
        /// </summary>
        static int FoodCount;
        /// <summary>
        /// Riktning för ormens rörelse. 0 = Upp, 1 = Höger, 2 = Ner, 3 = Vänster.
        /// </summary>
        static int direction;
        /// <summary>
        /// Spelets hastighet. Lägre siffra gör spelet snabbare.
        /// </summary>
        static int speed = 100;
        /// <summary>
        /// Kollar om rutnätet är befolkat med objekt eller inte.
        /// </summary>
        static bool Populated = false;
        /// <summary>
        /// Kollar om spelaren har förlorat eller inte.
        /// </summary>
        static bool Lost = false;
        /// <summary>
        /// Ormens längd. 1 längd = 1 ruta på spelplanen.
        /// </summary>
        static int snakeLength;
        /// <summary>
        /// Poängsumman.
        /// </summary>
        static int points = 0;
        /// <summary>
        /// Används för att öka svårighetsgraden på spelet.
        /// </summary>
        static int level = 1;
        /// <summary>
        /// Tiden som skärmen är fryst innan game over. Högre siffra ger längre frystid.
        /// </summary>
        static int freezeTime = 1500;
        /// <summary>
        /// Standardmetoden som kör programmet.
        /// </summary>
        /// <param name="args">Argument för kommandoraden.</param>
        static void Main(string[] args)
        {
            /// <summary>
            /// Alternativen för startmenyn läggs i en array. 'MenuHelper' klassen används sedan för att hantera menyvalet.
            /// </summary>
            string[] options = { "Start", "New", "Load", "Save",
            "Highscore", "Quit" };
            int selectedIndex = MenuHelper.MultipleChoice(true, options);

            Console.Clear();

            if (selectedIndex < 0)
            {
                EditSave(Editor());
                Main(args);
            }

            // FIXME: Om man trycker på ecape i menyn kastas IndexOutOfRangeException
            string command = options[selectedIndex];

            if (command == options[0])
            {
                Start();
            }
            else if (command == options[1])
            {
                Console.WriteLine("New: NYI"); // NYI: Implementera New funktionen
                Thread.Sleep(3000);
                Main(args);
            }
            else if (command == options[2])
            {
                Console.Write("Filename: ");
                string fileName = Console.ReadLine();
                grid = LoadFromFile(fileName);
                SetGameVariables();
                Populated = true;

                Start();
            }
            else if (command == options[3])
            {
                Console.WriteLine("Save: NYI"); // NYI: Implementera Save funktionen
                Thread.Sleep(3000);
                Main(args);
            }
            else if (command == options[4])
            {
                showHighScores();
            }
            else if (command == options[5])
            {
                Console.Clear();
                Console.WriteLine("Good Bye");
                Environment.Exit(0);
            }
        }
        /// <summary>
        /// Startar spelet med 'populateGrid' och 'SetGameVariables' metoderna.
        /// </summary>
        static void Start()
        {
            if (!Populated)
            {
                populateGrid();
                SetGameVariables();
            }

            while (!Lost)
            {
                Restart();
            }
        }

        /// <summary>
        /// Sätter variabelvärden till deras 'standard' när man startar ett nytt spel. Annars behålls variabelvärdena mellan omgångar..
        /// </summary>
        private static void SetGameVariables()
        {
            points = 0;
            FoodCount = 0;
            snakeLength = 5;
            speed = 100;
            level = 1;
            currentCell = grid[(int)Math.Ceiling((double)gridH / 2), (int)Math.Ceiling((double)gridW / 2)];
            updatePos();
            addFood();
            Populated = true;
        }

        /// <summary>
        /// Har ett förvirrande namn. Startar inte om spelet, utan ser till att uppdatera konsoloutputen med 'updateScreen' och ta emot input med 'getInput'.
        /// </summary>
        static void Restart()
        {
            updateScreen();
            getInput();
        }
        /// <summary>
        /// Skriver ut ormens nya position, och på så sätt simulerar rörelse. Skriver också ut vissa variabelvärden som poäng och ormens längd.
        /// </summary>
        static void updateScreen()
        {
            Console.SetCursorPosition(0, 0);
            printGrid();
            Console.WriteLine($"Length: {snakeLength}");
            Console.WriteLine($"Points: {points}");
            Console.WriteLine($"Level: {level}      FoodCount: {FoodCount}");
        }
        /// <summary>
        /// Tar emot spelarens input för att styra ormen.
        /// </summary>
        static void getInput()
        {
            ConsoleKeyInfo input;
            while (!Console.KeyAvailable)
            {
                Move();
                updateScreen();
                Thread.Sleep(speed);
            }
            input = Console.ReadKey();
            doInput(input.Key);
        }
        /// <summary>
        /// Kollar vad som ligger i cellen framför ormens huvud. % = mat, B = bomb, * = vägg, pilar = ormens kropp.
        /// </summary>
        /// <param name="cell">Cellen som ska kontrolleras.</param>
        static void checkCell(Cell cell)
        {
            if (cell.val == "%")
            {
                eatFood();
            }
            else if (cell.val == "B")
            {
                eatBomb();
            }
            else if (cell.visited)
            {
                Thread.Sleep(freezeTime);
                Lose();
            }
        }
        /// <summary>
        /// Hanterar förlust. Startar om genom att köra 'Main' igen.
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
            addHighScore();

            Populated = false;
            Lost = false;
            Main(new string[0]);
        }
        /// <summary>
        /// Sparar highscore till highscore.txt. Om filen inte finns skapas den.
        /// </summary>
        private static void addHighScore()
        {
            if (points > 0)
            {
                Console.Write("Enter your name: ");
                string name = Console.ReadLine();
                string highScoreEntry = $"{name}: {points}";
                File.AppendAllText("highscore.txt", highScoreEntry + Environment.NewLine);
                showHighScores();
            }
            else { showHighScores(); }
        }
        /// <summary>
        /// Visar highscore genom att läsa från highscore.txt.
        /// </summary>
        static void showHighScores()
        {
            Console.Clear();
            Console.WriteLine("High Scores:");
            Console.WriteLine("=============");
            string[] highScores = File.ReadAllLines("highscore.txt");
            Dictionary<int, string> scores = new Dictionary<int, string>();
            foreach (string highScoreEntry in highScores)
            {
                string[] parts = highScoreEntry.Split(':');
                string name = parts[0].Trim();
                int score = int.Parse(parts[1].Trim());
                scores[score] = name;
            }
            var sortedScores = scores.OrderByDescending(x => x.Key);
            foreach (var score in sortedScores)
            {
                Console.WriteLine($"{score.Value}: {score.Key}");
            }
            Console.WriteLine("\nPress Enter to return to Main menu");
            Console.ReadKey();
            Populated = false;
            Lost = false;
            Main(new string[0]);
        }
        /// <summary>
        /// Låter spelaren styra ormen i fyra riktningar med antingen WASD eller piltangenterna.
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
        /// Körs av 'eatFood' när 'foodcount' % 10 = 0. Detta gör så att det läggs ut fler hinder och hastigheten ökar varje gång spelaren tar 10 matbitar.
        /// </summary>
        static void increaseLevel()
        {
            level = level + 1;
            addBomb();
            addWallH();
            addWallV();
            addWallDiagR();
            addWallDiagL();
            speed = speed - 10;
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
                {
                    cell.val = "%";
                    break;
                }
            }
        }
        /// <summary>
        /// Ökar ormens längd och lägger till mer mat på rutnätet.
        /// Kollar även antalet matbitar man tagit. Om det antalet % 10 == 0, körs 'increaseLevel'.
        /// </summary>
        static void eatFood()
        {
            points += 1 * level;
            snakeLength += 1;
            FoodCount = FoodCount + 1;
            //Satte 'maxvärde' på 50, så att level endast kan öka 5 gånger. 
            if (FoodCount % 10 == 0 && FoodCount <= 50)
            {
                increaseLevel();
            }
            //Den första bomben genereras när spelaren får sitt första poäng.
            if (points == 1)
            {
                addBomb();
            }
            addFood();
        }
        /// <summary>
        /// Lägger till en bomb i en slumpmässig ledig cell på rutnätet.
        /// </summary>
        static void addBomb()
        {
            Random r = new Random();
            Cell cell;
            while (true)
            {
                cell = grid[r.Next(grid.GetLength(0)), r.Next(grid.GetLength(1))];
                if (cell.val == " ")
                {
                    cell.val = "B";
                    break;
                }
            }
        }
        /// <summary>
        /// Sänker spelarens poäng med 1 * level och lägger ut en ny bomb på spelplanen.
        /// </summary>
        //FIXME: Om man har 10 poäng och äter en bomb ser poängen ut som 90 istället för 9..
        static void eatBomb()
        {
            points -= 1*level;
            if (points != 0)
            {
                addBomb();
            }
        }
        /// <summary>
        /// Lägger till en horisontell "vägg". Används i 'increaseLevel'.
        /// Väggen genereras som en punkt, och sedan läggs * till åt höger tills den når maxlängd eller en cell som inte är tom.
        /// OBS: 'x' är den vertikala axeln, 'y' är horisontell. Detta gäller för alla 'addWallX' metoder.
        /// </summary>
        static void addWallH()
        {
            Random r = new Random();
            Cell cell;
            //Börjar som 1, för cell = grid[x, y + wallLength] i while loopen.
            int wallLength = 1;
            while (true)
            {
                int x = r.Next(grid.GetLength(0));
                int y = r.Next(grid.GetLength(1));
                cell = grid[x, y];
                if (cell.val == " ")
                {
                    cell.val = "*";
                    //Kan ändra 5 till en högre siffra för att få längre väggar.
                    while (wallLength < 5)
                    {
                        cell = grid[x, y + wallLength];
                        if (cell.val == " ")
                        {
                            cell.val = "*";
                            wallLength = wallLength + 1;
                        }
                        //else satsen ser till att programmet inte kraschar, genom att försöka göra * utanför arrayen.
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// Kopia av 'addWallH', fast med några ändrade värden.
        /// Lägger till en vertikal "vägg".
        /// Väggen genereras som en punkt, och sedan läggs * till neråt tills den når maxlängd eller en cell som inte är tom.
        /// </summary>
        static void addWallV()
        {
            Random r = new Random();
            Cell cell;
            int wallLength = 1;
            while (true)
            {
                int x = r.Next(grid.GetLength(0));
                int y = r.Next(grid.GetLength(1));
                cell = grid[x, y];
                if (cell.val == " ")
                {
                    cell.val = "*";
                    //Satte den som 3 eftersom planen är längre horisontell än vertikal.
                    while (wallLength < 3)
                    {
                        cell = grid[x + wallLength, y];
                        if (cell.val == " ")
                        {
                            cell.val = "*";
                            wallLength = wallLength + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// Lägger till en diagonal "vägg".
        /// Väggen genereras som en punkt, och sedan läggs * till diagonalt upp åt höger tills den når maxlängd eller en cell som inte är tom.
        /// </summary>
        static void addWallDiagR()
        {
            Random r = new Random();
            Cell cell;
            int wallLength = 1;
            while (true)
            {
                int x = r.Next(grid.GetLength(0));
                int y = r.Next(grid.GetLength(1));
                cell = grid[x, y];
                if (cell.val == " ")
                {
                    cell.val = "*";
                    while (wallLength < 4)
                    {
                        //Kör man x - wallLength så tas man 'uppåt' på spelplanen.
                        cell = grid[x - wallLength, y + wallLength];
                        if (cell.val == " ")
                        {
                            cell.val = "*";
                            wallLength = wallLength + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// Är en kopia av addWallDiagR, 'inverterad'.
        /// Lägger till en diagonal "vägg".
        /// Väggen genereras som en punkt, och sedan läggs * till diagonalt neråt åt vänster tills den når maxlängd eller en cell som inte är tom.
        /// </summary>
        static void addWallDiagL()
        {
            Random r = new Random();
            Cell cell;
            int wallLength = 1;
            while (true)
            {
                int x = r.Next(grid.GetLength(0));
                int y = r.Next(grid.GetLength(1));
                cell = grid[x, y];
                if (cell.val == " ")
                {
                    cell.val = "*";
                    while (wallLength < 4)
                    {
                        //y - wallLength gör så att väggen genereras snett neråt åt vänster.
                        cell = grid[x - wallLength, y - wallLength];
                        if (cell.val == " ")
                        {
                            cell.val = "*";
                            wallLength = wallLength + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
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
        /// Flyttar ormen i den aktuella riktningen och hanterar kollisioner med 'visitCell'.
        /// </summary>
        static void Move()
        {
            if (direction == 0)
            {
                //up
                if (grid[currentCell.y - 1, currentCell.x].val == "*")
                {
                    Thread.Sleep(freezeTime);
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
                    Thread.Sleep(freezeTime);
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
                    Thread.Sleep(freezeTime);
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
                    Thread.Sleep(freezeTime);
                    Lose();
                    return;
                }
                visitCell(grid[currentCell.y, currentCell.x + 1]);
            }
        }
        /// <summary>
        /// Märker den givna cellen som 'visited' och uppdaterar ormens position.
        /// </summary>
        /// <param name="cell">Cellen som har åkts in i.</param>
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
        /// Uppdaterar ormens position och riktning. Ormens huvud är @.
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
        /// Skapar spelplanen genom att sätta värden i arrayen 'grid' som bestämmer vart * ska vara..
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
                    {
                        cell.Set("*");
                    } 
                    else
                    {
                        cell.Clear();
                    }

                    grid[col, row] = cell;
                }
            }
        }
        /// <summary>
        /// Skriver ut spelplanen på skärmen.
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
        /// Cell-klassen används för att skapa varje 'ruta' på spelplanen.
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
            public bool visited // TBD: Denna variabel bör heta något annat
            {
                get;
                set;
            }
            public int decay
            {
                get;
                set;
            }

            /// <summary>
            /// Ser till att celler som ormen eller föremål inte längre ligger i blir tomrum igen.
            /// </summary>
            public void decaySnake()
            {
                decay -= 1;
                if (decay == 0)
                {
                    visited = false;
                    val = " ";
                }
            }

            /// <summary>
            /// Tömmer en cell på sitt 'val' värde. Används i 'populateGrid'.
            /// </summary>
            public void Clear()
            {
                val = " ";
            }

            /// <summary>
            /// Ger en cell ett nytt 'val' värde.
            /// </summary>
            public void Set(string newVal)
            {
                val = newVal;
            }
        }

        /// <summary>
        /// En editor för nya arenor för spelet
        /// </summary>
        /// <returns>Returnerar ett Cell object</returns>
        public static Cell[,] Editor()
        {
            Cell[,] editGrid = new Cell[gridH, gridW];

            for (int col = 0; col < gridH; col++)
            {
                for (int row = 0; row < gridW; row++)
                {
                    Cell cell = new Cell();
                    cell.x = row;
                    cell.y = col;
                    cell.visited = false;
                    if (cell.x == 0 || cell.x > gridW - 2 || cell.y == 0 || cell.y > gridH - 2)
                    {
                        cell.Set("*");
                    }

                    else
                    {
                        cell.Clear();
                    }

                    editGrid[col, row] = cell;
                }
            }


            int x = 1;
            int y = 1;

            Console.Clear();
            drawGrid(editGrid, x, y);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    y = Math.Max(0, y - 1);
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    y = Math.Min(gridH - 1, y + 1);
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    x = Math.Max(0, x - 1);
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    x = Math.Min(gridW - 1, x + 1);
                }
                else if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    if (editGrid[y, x].val == "*") editGrid[y, x].val = " ";
                    else editGrid[y, x].val = "*";
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return editGrid;
                }

                Console.Clear();
                drawGrid(editGrid, x, y);
            }
        }

        /// <summary>
        /// Skriver ut editorn
        /// </summary>
        /// <param name="editGrid">Dell objekt</param>
        /// <param name="x">Vilken rad som ska ändras</param>
        /// <param name="y">Vilken kolumn som ska ändras</param>
        static void drawGrid(Cell[,] editGrid, int x, int y)
        {
            string toPrint = "";
            for (int col = 0; col < gridH; col++)
            {
                for (int row = 0; row < gridW; row++)
                {
                    if (col == y && row == x)
                    {
                        toPrint += "+";
                    }
                    else
                    {
                        toPrint += editGrid[col, row].val;
                    }

                }
                toPrint += "\n";
            }
            Console.WriteLine(toPrint);
            Console.WriteLine("Move with arrow keys");
            Console.WriteLine("Spacebar = add wall");
            Console.WriteLine("Enter to save");
        }

        /// <summary>
        /// Spara den ändrade Cell objektet i en text fil
        /// </summary>
        /// <param name="editGrid">Cell objekt</param>
        static void EditSave(Cell[,] editGrid)
        {
            Console.Write("Filename: ");
            string fileName = Console.ReadLine();

            List<Cell[]> rows = new List<Cell[]>();
            for (int i = 0; i < editGrid.GetLength(0); i++)
            {
                Cell[] row = new Cell[editGrid.GetLength(1)];
                for (int j = 0; j < editGrid.GetLength(1); j++)
                {
                    row[j] = editGrid[i, j];
                }
                rows.Add(row);
            }

            string jsonString = JsonSerializer.Serialize(rows);
            File.WriteAllText(fileName + ".json", jsonString);
        }

        /// <summary>
        /// Ladda in ett Cell objekt from en text fil
        /// </summary>
        /// <param name="fileName">Filens namn utan ".jason"</param>
        /// <returns>Returnerar ett cell objekt</returns>
        static Cell[,] LoadFromFile(string fileName)
        {
            string jsonString = File.ReadAllText(fileName + ".json");

            List<Cell[]> rows = JsonSerializer.Deserialize<List<Cell[]>>(jsonString);
            Cell[,] grid = new Cell[rows.Count, rows[0].Length];
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Length; j++)
                {
                    grid[i, j] = rows[i][j];
                }
            }

            return grid;
        }

    }
}