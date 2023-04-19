using System.Text;

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
        //TBD: food är oanvänd
        static Cell food;
        /// <summary>
        /// Räknare för hur mycket mat som har ätits.
        /// </summary>
        static int FoodCount;
        /// <summary>
        /// Räknare för hur många bomber ätits. Används för att räkna ut antalet poäng man har.
        /// </summary>
        static int BombCount;
        /// <summary>
        /// Riktning för ormens rörelse. 0 = Upp, 1 = Höger, 2 = Ner, 3 = Vänster.
        /// </summary>
        static int direction; //0=Up 1=Right 2=Down 3=Left
        /// <summary>
        /// Spelets hastighet
        /// </summary>
        static int speed = 100; // TBD: Bör vara en double så man kan fininställa hastighet
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
        /// Game poäng
        /// </summary>
        static int points = 0;
        /// <summary>
        /// Svårighetsgraden på spelet.
        /// </summary>
        static int level = 1;
        /// <summary>
        /// Tid att frysa skärmen innan game over
        /// </summary>
        static int freezeTime = 1500;

        static string Cow = "🐂";
        static string Dead = "💀";
        static string Hand = "👋";

        /// <summary>
        /// Huvudmetoden för Program-klassen.
        /// </summary>
        /// <param name="args">Argument för kommandoraden.</param>

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string[] options = { "Start", "New", "Load", "Save",
            "Highscore", "Quit" };
            int selectedIndex = MenuHelper.MultipleChoice(true, options);

            Console.Clear();

            if (selectedIndex < 0)
            {
                Environment.Exit(0);
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
                Console.WriteLine("Load: NYI"); // NYI: Implementera Load funktionen
                Thread.Sleep(3000);
                Main(args);
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
        /// Startar spelet
        /// </summary>
        static void Start()
        {
            if (!Populated)
            {
                points = 0;
                FoodCount = 0;
                BombCount = 0;
                snakeLength = 5;
                speed = 100;
                level = 1;
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
            updateScreen();
            getInput();
        }
        /// <summary>
        /// Uppdaterar skärmen med aktuell rutnät och orm.
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
            //Console.Write("Where to move? [WASD] ");
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
        /// Kontrollerar om den givna cellen innehåller mat eller om ormen kolliderar med sig själv.
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
            addHighScore();

            Populated = false;
            Lost = false;
            Main(new string[0]);
        }
        /// <summary>
        /// Sparar highscore till highscore.txt
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
        /// Visar highscore
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
        //TODO: Gör så att när foodcount % 10 = 0, level = level + 1
        //Använder foodcount, som blir +1 för varje matbit man tar nere i 'eatFood'.
        //Lägger till en till bomb och en av varje väggtyp. Ökar hastigheten med -10. (100 är bashastighet).
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
        /// Kollar även antalet matbitar man tagit. Om det antalet % 10 == 0, ökar svårighetsgraden.
        /// </summary>
        static void eatFood()
        {
            //Ska man öka level före eller efter att man tar biten som ökar svårighetsgraden?
            points += 1 * level;
            snakeLength += 1;
            //FoodCount används för att öka level med 1 varje gång man tar 10 matbitar.
            FoodCount = FoodCount + 1;
            //Satte 'maxvärde' på 50, så att level endast kan öka 5 gånger.
            if (FoodCount % 10 == 0 && FoodCount <= 50)
            {
                increaseLevel();
            }
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
        /// Sänka spelarens poäng, men inte längden är samma som innan.
        /// </summary>
        //FIXME: Om man har 10 poäng och äter en bomb ser poängen ut som 90.
        static void eatBomb()
        {
            //Ökade antalet poäng man förlorar nu när levels finns. Annars förlorade man alltid 1 poäng.
            points -= 1*level;
            if (points != 0)
            {
                addBomb();
            }
        }
        /// <summary>
        /// Lägger till en horisontell "vägg". Används varje gång man äter mat.
        /// Väggen genereras som en punkt, och sedan läggs * till åt höger tills den når maxlängd eller en cell som inte är tom.
        /// </summary>
        //TODO: Gör så att addWallH körs när nivån ökar. Den körs när man äter mat än så länge.
        static void addWallH()
        {
            Random r = new Random();
            Cell cell;
            //Börjar som 1, för cell = grid[x, y + wallLength] i while loopen.
            int wallLength = 1;
            while (true)
            {
                //OBS: 'x' är den vertikala axeln, y är horisontell.
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
        /// Lägger till en vertikal "vägg". Används varje gång man äter mat.
        /// Väggen genereras som en punkt, och sedan läggs * till neråt tills den når maxlängd eller en cell som inte är tom.
        /// </summary>
        //TODO: Gör så att addWallV körs när nivån ökar. Den körs när man äter mat än så länge.
        //Är en kopia av addWallH, kolla den för kommentarer.
        static void addWallV()
        {
            Random r = new Random();
            Cell cell;
            int wallLength = 1;
            while (true)
            {
                //OBS: 'x' är den vertikala axeln, y är horisontell.
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
        /// Lägger till en diagonal "vägg". Används varje gång man äter mat.
        /// Väggen genereras som en punkt, och sedan läggs * till diagonalt upp åt höger tills den når maxlängd eller en cell som inte är tom.
        /// </summary>
        //TODO: Gör så att addWallDiagR körs när nivån ökar. Den körs när man äter mat än så länge.
        static void addWallDiagR()
        {
            Random r = new Random();
            Cell cell;
            int wallLength = 1;
            while (true)
            {
                //OBS: 'x' är den vertikala axeln, y är horisontell.
                int x = r.Next(grid.GetLength(0));
                int y = r.Next(grid.GetLength(1));
                cell = grid[x, y];
                if (cell.val == " ")
                {
                    cell.val = "*";
                    while (wallLength < 4)
                    {
                        //Kör man x - wallLength så tas man 'uppåt' på spelplanen.
                        //Kom ihåg att x är vertikal och y horisontell!
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
        /// Lägger till en diagonal "vägg". Används varje gång man äter mat.
        /// Väggen genereras som en punkt, och sedan läggs * till diagonalt neråt åt vänster tills den når maxlängd eller en cell som inte är tom.
        /// </summary>
        //TODO: Gör så att addWallDiagL körs när nivån ökar. Den körs när man äter mat än så länge.
        static void addWallDiagL()
        {
            Random r = new Random();
            Cell cell;
            int wallLength = 1;
            while (true)
            {
                //OBS: 'x' är den vertikala axeln, y är horisontell.
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
        /// Flyttar ormen i den aktuella riktningen och hanterar kollisioner.
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
                    {
                        cell.Set("*");
                    } 
                    else if (cell.y == 10 && cell.x > 25 && cell.x < 35)
                    {
                        cell.Set("^");
                    } 
                    else if (cell.x == 8 && cell.y > 4 && cell.y < 16)
                    {
                        cell.Set("~");
                    } 
                    else if (cell.y == 18 && cell.x > 22 && cell.x < 35)
                    {
                        cell.Set("~");
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