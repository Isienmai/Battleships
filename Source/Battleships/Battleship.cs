using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Battleships
{
    class Battleship
    {
        #region Variables
        //p1 and p2 store the player information
        Player p1;
        Player p2;
        //turnplayer and standbyplayer are used to determine which player's turn it is
        Player turnPlayer;
        Player standbyPlayer;
        //menu is used to determine if the menu should be shown or not at any given time
        bool menu;
        //GameInProgress is used to determine if a game is currently underway
        bool gameInProgress;
        //logFileName stores the name of the gamelog for the current game.
        string logFileName;
        //turn log stores the string that represents the current turn, the string which gets written to the log at the end of the turn.
        string turnLog;
        //Contains the ID of the save file that was last loaded from
        int saveLoadedFrom;
        #endregion

        //initialise the game taking two instances of player
        public Battleship(Player player1, Player player2)
        {
            turnPlayer = new Player("");
            standbyPlayer = new Player("");
            p1 = player1;
            p2 = player2;
            menu = true;
            gameInProgress = false;
            saveLoadedFrom = -1;
        }

        //The following methods are all related to reading/writing to or from various files
        #region Reading and writing to and from files

        #region Game saves
        //The following method will save the current state of a game to a text file
        private bool SaveGame()
        {
            StreamWriter writer;
            int gameSaveIndex;
            if (saveLoadedFrom == -1)
            {
                for (gameSaveIndex = 0; gameSaveIndex < 99; gameSaveIndex++)
                {
                    if (!File.Exists("Saves\\Save" + p1.Name + p2.Name + gameSaveIndex + ".txt"))
                    {
                        writer = new StreamWriter("Saves\\Save" + p1.Name + p2.Name + gameSaveIndex + ".txt");
                        writer.WriteLine(logFileName);
                        turnPlayer.SaveGameData(writer);
                        standbyPlayer.SaveGameData(writer);
                        writer.Close();
                        Console.WriteLine("\nYour game has been saved in file: Save" + p1.Name + p2.Name + gameSaveIndex + "\n");
                        writer = new StreamWriter("Logs\\" + logFileName + ".txt", true);
                        writer.WriteLine("Game Saved");
                        writer.Close();
                        return true;
                    }
                }
            }
            else
            {
                gameSaveIndex = saveLoadedFrom;
                writer = new StreamWriter("Saves\\Save" + p1.Name + p2.Name + gameSaveIndex + ".txt");
                writer.WriteLine(logFileName);
                turnPlayer.SaveGameData(writer);
                standbyPlayer.SaveGameData(writer);
                writer.Close();
                Console.WriteLine("\nYour game has been saved in file: Save" + p1.Name + p2.Name + gameSaveIndex + "\n");
                writer = new StreamWriter("Logs\\" + logFileName + ".txt", true);
                writer.WriteLine("Game Saved");
                writer.Close();
                return true;
            }
            DisplayError("You have used up all 100 save slots for this player combination. You cannot save any more games between these two players until you delete some saves.");
            return false;
        }

        //The following method asks the user for a game save and then loads that save.
        private void LoadGame()
        {
            DisplaySaves();
            string toLoad = GetGameSaveToLoad();
            if (toLoad != "Error")
            {
                StreamReader reader = new StreamReader(toLoad);
                logFileName = reader.ReadLine();
                turnPlayer.LoadGameData(reader);
                standbyPlayer.LoadGameData(reader);
                UpdateLogOnGameLoad();
                if (p1.Name == turnPlayer.Name)
                {
                    p1 = turnPlayer;
                    p2 = standbyPlayer;
                }
                if (p2.Name == turnPlayer.Name)
                {
                    p2 = turnPlayer;
                    p1 = standbyPlayer;
                }
                reader.Close();
                gameInProgress = true;
                menu = false;
            }
        }

        //Get the save file ID from the user, make sure it is valid, and return it as a string
        private string GetGameSaveToLoad()
        {
            Console.WriteLine("Please type in the number of the game save you wish to load(you cannot load saves of games between other players).");
            string input = Console.ReadLine();
            if (!ValidGameSaveToLoad(input))
            {
                return "Error";
            }

            saveLoadedFrom = Convert.ToInt32(input);
    
            return "Saves\\Save" + p1.Name + p2.Name + input + ".txt";
        }

        //Validate a given file ID (return true if the specified file exists)
        private bool ValidGameSaveToLoad(string input)
        {
            if (File.Exists("Saves\\Save" + p1.Name + p2.Name + input + ".txt"))
            {
                return true;
            }
            else
            {
                DisplayError("The current players have not got a saved game #" + input);
                return false;
            }
        }
        #endregion

        #region GameBoards
        //The following method reads the gameboards for each player from a single file.
        private void ReadGameBoardsFromFile(string textFileName)
        {
            if (textFileName != "Error")
            {
                StreamReader reader = new StreamReader(textFileName);
                string file = reader.ReadToEnd();
                reader.Close();
                file = file.Replace("\r\n", "");
                char[,] toReturn = new char[10, 20];

                for (int y = 0; y < 20; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        toReturn[x, y] = file[x + 10 * y];
                    }
                }
                char[,] p1GameBoard = CropCharArray(toReturn, new int[] { 0, 0 }, new int[] { 9, 9 });
                char[,] p2GameBoard = CropCharArray(toReturn, new int[] { 0, 10 }, new int[] { 9, 19 });
                p1.GameBoard.FillGameBoard(p1GameBoard);
                p2.GameBoard.FillGameBoard(p2GameBoard);
                gameInProgress = true;
                menu = false;
                turnPlayer = p1;
                standbyPlayer = p2;
                CreateGameLog();
            }
        }

        //The following method will write both player's gameboards to the same file.
        private bool WriteGameBoardsToFile()
        {
            int gameBoardIndex;
            for (gameBoardIndex = 0; gameBoardIndex < 99; gameBoardIndex++)
            {
                if (!File.Exists("GameBoards\\GameBoard" + gameBoardIndex + ".txt"))
                {
                    StreamWriter writer = new StreamWriter("GameBoards\\GameBoard" + gameBoardIndex + ".txt");
                    for (int y = 0; y < 20; y++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            if (y < 10)
                            {
                                writer.Write(p1.GameBoard.ShipsAndSea[x, y]);
                            }
                            else
                            {
                                writer.Write(p2.GameBoard.ShipsAndSea[x, y - 10]);
                            }
                        }
                        writer.WriteLine();
                    }
                    writer.Close();
                    return true;
                }
            }
            DisplayError("You have used up all 100 GameBoard save slots. Please delete one or several and then try again.");
            return false;
        }

        //Takes a char array a returns a chunk from it. Used primarily by ReadGameBoardsFromFile to read 
        private char[,] CropCharArray(char[,] arrayToCrop, int[] startIndex, int[] endIndex)
        {
            char[,] toReturn = new char[endIndex[0] - startIndex[0] + 1, endIndex[1] - startIndex[1] + 1];
            for (int y = startIndex[1]; y <= endIndex[1]; y++)
            {
                for (int x = startIndex[0]; x <= endIndex[0]; x++)
                {
                    toReturn[x - startIndex[0], y - startIndex[1]] = arrayToCrop[x, y];
                }
            }
            return toReturn;
        }

        //Get the ID of the gameboards file to be loaded
        private string GetGameBoardToLoad()
        {
            DisplayGameBoardSaves();
            Console.WriteLine("Please type in the number of the game save you wish to load(you cannot load saves of games between other players).");
            string input = Console.ReadLine();

            if (!ValidGameBoardSaveToLoad(input))
            {
                return "Error";
            }

            return "GameBoards\\GameBoard" + input + ".txt";
        }

        //Validate a given file ID (return true if the specified file exists)
        private bool ValidGameBoardSaveToLoad(string input)
        {
            if (File.Exists("GameBoards\\GameBoard" + input + ".txt"))
            {
                return true;
            }
            else
            {
                DisplayError("There is no gameboards save #" + input);
                return false;
            }
        }
        #endregion

        #region Logs
        //Creates a game log for the current game
        private void CreateGameLog()
        {
            CreateGameLogName();
            StreamWriter writer = new StreamWriter("Logs\\" + logFileName + ".txt");
            writer.WriteLine("PlayerName, GridToAttack, Result");
            writer.WriteLine("................................");
            writer.Close();
        }

        //Generates a new, unique(sort of) name for this match's game log
        private void CreateGameLogName()
        {
            string gameLogName;
            gameLogName = Convert.ToString(DateTime.Now);
            gameLogName = gameLogName.Replace("/", ",");
            gameLogName = gameLogName.Replace(":", ",");
            gameLogName = gameLogName + " " + p1.Name + " " + p2.Name;
            logFileName = gameLogName;
        }

        //Open the game log and delete all data that was written to it after the last save
        private void UpdateLogOnGameLoad()
        {
            StreamReader reader = new StreamReader("Logs\\" + logFileName + ".txt");
            string fileDump = reader.ReadToEnd();
            reader.Close();

            fileDump = fileDump.Replace("Game Saved", "&:");
            int fileCharIndex = fileDump.Length;

            for (int i = fileDump.Length - 1; fileDump[i] != '&'; i--)
            {
                fileCharIndex = i;
            }

            fileDump = fileDump.Remove(fileCharIndex);
            
            StreamWriter writer = new StreamWriter("Logs\\" + logFileName + ".txt");
            writer.WriteLine(fileDump.Replace("&", "Game Saved"));
            writer.WriteLine("Game Loaded");
            writer.Close();
        }
        #endregion       

        #endregion
        
        //The following methods are used to draw various things to the screen 
        #region DisplayMethods
        //Displays the gameboard of both players
        private void DisplayGrids()
        {
            Console.Clear();
            Console.WriteLine("\n" + p1.Name + "'s board:");
            p1.GameBoard.DrawGrid();
            Console.WriteLine();
            Console.WriteLine(p2.Name + "'s board:");
            p2.GameBoard.DrawGrid();
        }

        //Displays "hit" in a fabulous fashion
        private void WriteHit()
        {
            Console.Write("--------------------------------------------------------------------------------");
            Console.Write("----------------------------------- │ │ ° ─┬─ ----------------------------------");
            Console.Write("----------------------------------- ├─┤ │  │  ----------------------------------");
            Console.Write("----------------------------------- │ │ │  │  ----------------------------------");
            Console.Write("--------------------------------------------------------------------------------");
        }

        //Displays "miss" in a fabulous fashion
        private void WriteMiss()
        {
            Console.Write("--------------------------------------------------------------------------------");
            Console.Write("---------------------------   /\\ /\\   °  ┌──  ┌──  -----------------------------");
            Console.Write("---------------------------  /  V  \\  │  └──┐ └──┐ -----------------------------");
            Console.Write("--------------------------- /       \\ │  ───┘ ───┘ -----------------------------");
            Console.Write("--------------------------------------------------------------------------------");
        }

        //Display a message to congratulate a player on their 100th win.
        private void WriteCongrats()
        {
            Console.Write("--------------------------------------------------------------------------------");
            Console.Write("-------------------Congratulations on your 100th Victory!!----------------------");
            Console.Write("--------------------------------------------------------------------------------");
        }

        //Displays the winner, and both player's win/loss statistics
        private void DisplayVictor()
        {
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            Console.WriteLine("Congratulations! {0} has won this game! Better luck next time {1}.", turnPlayer.Name, standbyPlayer.Name);            
            Console.WriteLine("{0}: {1} wins, {2} losses", p1.Name, p1.TotalWins, p1.TotalLosses);
            Console.WriteLine("{0}: {1} wins, {2} losses", p2.Name, p2.TotalWins, p2.TotalLosses);
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            System.Threading.Thread.Sleep(1000);
            if (turnPlayer.TotalWins == 100)
            {
                WriteCongrats();
            }
        }
        
        //used to write a single line of text in red, to be used only for displaying errors
        private void DisplayError(string toDisplay)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(toDisplay);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        //Displays the menu options to the player(s)
        private void DisplayMenu()
        {
            Console.WriteLine("\n\nPlayer1:{0}           Player2:{1}", p1.Name, p2.Name);

            Console.WriteLine("---MENU---");

            Console.WriteLine("1: New Game");

            if (!gameInProgress)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("2: Save Game");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.WriteLine("2: Save Game");
            }

            Console.WriteLine("3: Load Game");

            Console.WriteLine("4: Change Players");

            if (!gameInProgress)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("5: Continue Game");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.WriteLine("5: Continue Game");
            }

            Console.WriteLine("6: See Player Stats");

            Console.WriteLine("7: Create a gameboard for each player and save them as a preset.");

            Console.WriteLine("8: Exit Game\n\n");
        }

        //Writes the wins and losses of both players to the console.
        private void DisplayStats()
        {
            Console.Write("--------------------------------------------------------------------------------");
            Console.Write("Player 1 Stats                 \\|/       Player 2 Stats                         ");
            Console.Write("Name:" + p1.Name);
            Console.SetCursorPosition(31,Console.CursorTop);
            Console.WriteLine("/|\\       Name:" + p2.Name);

            Console.Write("Wins:" + p1.TotalWins);
            Console.SetCursorPosition(31,Console.CursorTop);
            Console.WriteLine("\\|/       Wins:" + p2.TotalWins);

            Console.Write("Losses:" + p1.TotalLosses);
            Console.SetCursorPosition(31,Console.CursorTop);
            Console.WriteLine("/|\\       Losses:" + p2.TotalLosses);

            Console.Write("Win/Loss Ratio:" + p1.WinLossRatio);
            Console.SetCursorPosition(31,Console.CursorTop);
            Console.WriteLine("\\|/       Win/Loss Ratio:" + p2.WinLossRatio);
            Console.Write("--------------------------------------------------------------------------------");
        }

        //Display a list of all saved games between the current two players
        private void DisplaySaves()
        {
            Console.WriteLine("The following numbers are available for loading. Please pick one from this list.");
            DisplayFilesInGivenFolder("Saves\\Save" + p1.Name + p2.Name);
        }

        //Display a list of all pre-made gameboards
        private void DisplayGameBoardSaves()
        {
            Console.WriteLine("The following numbers are available for loading. Please pick one from this list.");
            DisplayFilesInGivenFolder("GameBoards\\GameBoard");
        }

        //Display all files that are numbered and saved in a given location with a given filename prefix
        private void DisplayFilesInGivenFolder(string folderAndFileNameUpToDigit)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            for (int i = 0; i < 100; i++)
            {
                if (File.Exists(folderAndFileNameUpToDigit + i + ".txt"))
                {
                    Console.WriteLine("  :" + i);
                }
            }
            Console.WriteLine("--------------------------------------------------------------------------------");
        }

        //Display a small thing that spins...as long as this method is called at least four times, each time with a frame 1 higher than the last frame
        private void DisplayFrameOfLoading(int frame)
        {
            while (frame > 4)
            {
                frame -= 4;
            }
            switch (frame)
            {
                case 0:
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("|");
                    break;
                case 1:
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("/");
                    break;
                case 2:
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("-");
                    break;
                case 3:
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("\\");
                    break;
            }
        }

        //Display You sunk my battleship with kirby dancing below it.
        private void DisplayYouSunkMyBattleship()
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("                         You Sunk My Battleship                                 ");
            Console.WriteLine("--------------------------------------------------------------------------------");
            DancingKirby();
        }

        //It displays Kirby, dancing
        private void DancingKirby()
        {
            for (int i = 0; i < 2; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("<(^.^ <)  ");
                System.Threading.Thread.Sleep(200);

                Console.SetCursorPosition(0,Console.CursorTop);
                Console.Write(" (^'_'^)  ");
                System.Threading.Thread.Sleep(200);

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(" (> ^.^)> ");
                System.Threading.Thread.Sleep(200);

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(" (^'_'^)  ");
                System.Threading.Thread.Sleep(200);                
            }
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine("                  ");
        }        
        #endregion

        //The following methods run the basic logic of the game
        #region Gamelogic

        //This method loops while the game is running. When this method returns false the game exits, and until it returns false it is looping.
        private bool GameLoop()
        {
            int menuChoice;            
            if (menu)
            {
                DisplayMenu();
                menuChoice = GetMenuChoice();
                return ExecuteMenuChoice(menuChoice);
            }
            while (gameInProgress && !menu)
            {
                PlayMove();
            }
            return true;
        }
              
        //swaps turn player and standby player
        private void UpdateTurnPlayer()
        {
            if (turnPlayer == p1)
            {
                turnPlayer = p2;
                standbyPlayer = p1;
            }
            else
            {
                turnPlayer = p1;
                standbyPlayer = p2;
            }
        }
                
        #region Menu Methods
        //Returns false if the menu choice is invalid or if the user selected the wrong choice by accident.
        private bool ValidMenuChoice(string menuChoice)
        {
            switch (menuChoice)
            {
                case "1":
                    if (gameInProgress)
                    {
                        return GetUserInput("You are currently playing a game. Are you sure you wish to close it? (Y/N)");
                    }
                    else
                    {
                        return true;
                    }
                case "2":
                    if (gameInProgress)
                    {
                        return true;
                    }
                    else
                    {
                        DisplayError("You cannot save a game if you are not playing a game.");
                        return false;
                    }
                case "3":
                    if (gameInProgress)
                    {
                        return GetUserInput("You are currently playing a game. Are you sure you wish to close it? (Y/N)");
                    }
                    else
                    {
                        return true;
                    }
                case "4":
                    if (gameInProgress)
                    {
                        return GetUserInput("You are currently playing a game. If you change players you will need to start a new game, or load a saved game.\nDo you still wish to change players? (Y/N)");
                    }
                    else
                    {
                        return true;
                    }
                case "5":
                    if (gameInProgress)
                    {
                        return true;
                    }
                    else
                    {
                        DisplayError("You cannot continue a game if you are not playing a game.");
                        return false;
                    }
                case "6":
                    return true;
                case "7":
                    if (gameInProgress)
                    {
                        return GetUserInput("This will erase your current game progress. Are you sure? (Y/N)");
                    }
                    return true;
                case "8":
                    return GetUserInput("Are you sure you wish to exit? (Y/N)");
                default:
                    return false;
            }
        }

        //asks the user for a menu choice and returns it.
        private int GetMenuChoice()
        {
            string input;
            Console.WriteLine("Please enter the number of the menu item you wish to select.");
            input = Console.ReadLine();
            if (!ValidMenuChoice(input))
            {
                return 0;
            }
            return Convert.ToInt32(input);
        }

        //Takes an int representing the user's menu choice and calls the appropriate method.
        //If the menu choice may affect a game in progress this method will ask the user to confirm that they wish to do this.
        private bool ExecuteMenuChoice(int menuChoice)
        {
            switch (menuChoice)
            {
                case 0:
                    return true;
                case 1:
                    NewGame();
                    return true;
                case 2:
                    SaveGame();
                    return true;
                case 3:
                    LoadGame();
                    return true;
                case 4:
                    ChangePlayers();
                    return true;
                case 5:
                    menu = false;
                    return true;
                case 6:
                    DisplayStats();
                    return true;
                case 7:                    
                    CreateGameBoardSave();
                    return true;
                case 8:
                    return false;
                default:
                    DisplayError("It appears a non-existant menu option passed the validation when it shouldn't have.\nPress enter to exit the program (no you have no choice in this matter).");
                    Console.ReadLine();
                    return false;
            }
        }

        //Asks the user if they wish to change each player and recieves the name of the replacement players from the user, and then initialises the new players.
        private void ChangePlayers()
        {
            if (GetUserInput("Do you wish to change Player1 from " + p1.Name + " to someone else? (Y/N)"))
            {
                p1.ChangePlayer();
            }
            if (GetUserInput("Do you wish to change Player2 from " + p2.Name + " to someone else? (Y/N)"))
            {
                p2.ChangePlayer();
            }
            gameInProgress = false;
        }

        //Asks the user how they wish to set up the board, sets up the board accordingly and then begins a match.
        private void NewGame()
        {
            if (GetUserInput("Do you want to place the ships yourselves? (Y/N)"))
            {
                saveLoadedFrom = -1;
                p1.InitialiseGameBoard();
                p2.InitialiseGameBoard();
                gameInProgress = true;
                menu = false;
                turnPlayer = p1;
                standbyPlayer = p2;
                CreateGameLog();
            }
            else
            {
                saveLoadedFrom = -1;
                ReadGameBoardsFromFile(GetGameBoardToLoad());
            }            
        }

        //Gets the user to put ships on the gameboard for both players and then saves the input placement into a text file that can be loaded later.
        private void CreateGameBoardSave()
        {
            p1.InitialiseGameBoard();
            p2.InitialiseGameBoard();
            WriteGameBoardsToFile();
        }
        #endregion        

        #region Bombing methods
        //This method gets a target from the user, bombs said target, displays wether the bomb hit or missed a ship, and then checks to see if the game is won.
        private void PlayMove()
        {
            DisplayGrids();
            string target = GetTarget();
            if (target != "M")
            {
                bool hit = BombTarget(target);
                Console.WriteLine();
                
                for (int i = 0; i < 10; i++)
                {
                    DisplayFrameOfLoading(i);
                    System.Threading.Thread.Sleep(100);
                } 
                if (!GameOver())
                {
                    UpdateTurnPlayer();
                }                 
            }
            else
            {
                menu = true;
                Console.Clear();
            }
        }

        //takes a target, sets it's bombed value to true in the standby player's gameboard and writes hit or miss depending on the outcome of the bombing.
        private bool BombTarget(string target)
        {            
            int[] coordsOfTarget = new int[2];
            coordsOfTarget[0] = target[0] - 65;
            coordsOfTarget[1] = target[1] - 49;

            StreamWriter writer = new StreamWriter("Logs\\" + logFileName + ".txt", true);
            target = target.Replace(":", "10");
            turnLog = turnPlayer.Name + "," + target + ",";

            standbyPlayer.GameBoard.Bomb(coordsOfTarget[0], coordsOfTarget[1]);

            if (standbyPlayer.GameBoard.ShipsAndSea[coordsOfTarget[0], coordsOfTarget[1]] != 'x')
            {
                turnLog = turnLog + "Hit";
                turnPlayer.TargetCalculator.LocatedUnsunkShip = true;
                writer.WriteLine(turnLog);
                DisplayGrids();
                if (SunkShip(coordsOfTarget))
                {
                    turnPlayer.TargetCalculator.LocatedUnsunkShip = false;
                    DisplayYouSunkMyBattleship();
                    writer.WriteLine(turnPlayer.Name + " sunk " + standbyPlayer.Name + "'s battleship.");
                    if (turnPlayer.Name == "CPU")
                    {
                        turnPlayer.TargetCalculator.GettingShipDirection = true;
                        switch (standbyPlayer.GameBoard.ShipsAndSea[coordsOfTarget[0], coordsOfTarget[1]])
                        {
                            case 'a':                                
                                turnPlayer.TargetCalculator.ShipsDown[0] = true;
                                break;
                            case 'b':
                                turnPlayer.TargetCalculator.ShipsDown[1] = true;
                                break;
                            case 's':
                                turnPlayer.TargetCalculator.ShipsDown[2] = true;
                                break;
                            case 'd':
                                turnPlayer.TargetCalculator.ShipsDown[3] = true;
                                break;
                            case 'p':
                                turnPlayer.TargetCalculator.ShipsDown[4] = true;
                                break;
                        }
                    }
                }
                else
                {
                    WriteHit();
                }
                
                writer.Close();                
                return true;
            }
            else
            {
                turnLog = turnLog + "Miss";
                writer.WriteLine(turnLog);
                writer.Close();
                DisplayGrids();
                WriteMiss();
                return false;
            }
        }

        //asks the player for a target coordinate until a valid coordinate has been entered, then returns this coordinate
        private string GetTarget()
        {
            string target;
            do
            {
                target = RecieveTarget();
            }
            while (target != "M" && !TargetIsValid(target));

            return target;
        }

        //returns true if an input coordinate is valid, else throws up an error message and returns false
        private bool TargetIsValid(string target)
        {
            if (target.Length < 2)
            {
                DisplayError("Please enter a full set of coordinates.");
                return false;
            }

            int[] coordsOfTarget = new int[2];
            coordsOfTarget[0] = target[0] - 65;
            coordsOfTarget[1] = target[1] - 49;

            if (coordsOfTarget[0] < 0 || coordsOfTarget[0] > 9 || coordsOfTarget[1] < 0 || coordsOfTarget[1] > 9)
            {

                DisplayError("Your target must be on the grid.");
                return false;
            }

            if (standbyPlayer.GameBoard.Bombed[coordsOfTarget[0], coordsOfTarget[1]])
            {

                DisplayError("You cannot target somewhere you have already hit.");
                return false;
            }

            return true;
        }

        //Asks user for a target, modifies the string slightly and then returns it
        private string RecieveTarget()
        {
            if (turnPlayer.Name == "CPU")
            {
                return turnPlayer.TargetCalculator.GetAITarget(standbyPlayer.GameBoard);
            }
            else
            {
                string inputCoord;
                Console.WriteLine(turnPlayer.Name + ", select an unbombed square from " + standbyPlayer.Name + "'s grid(enter 'm' to open the menu):");
                inputCoord = Console.ReadLine();
                if (inputCoord.Contains("10"))
                {
                    inputCoord = inputCoord.Replace("10", ":");
                }

                inputCoord = inputCoord.ToUpper();
                if (inputCoord.Length > 2)
                {
                    inputCoord = inputCoord.Remove(2);
                }

                return inputCoord;
            }
        }

        //Determines if an entire ship has been sunk at the given location
        private bool SunkShip(int[] target)
        {
            char direction = GetDirection(target);
            while (CheckDirection(direction, target))
            {
                //target = MoveTarget(direction, target);
            }
            direction = FlipDirection(direction);
            if (!standbyPlayer.GameBoard.Bombed[target[0], target[1]])
            {
                return false;
            }
            while (CheckDirection(direction, target))
            {                
                //target = MoveTarget(direction, target);
                if (!standbyPlayer.GameBoard.Bombed[target[0], target[1]])
                {
                    return false;
                }
            }
            return true;
        }

        //Edit the target coordinate based on a given cardinal direction
        private int[] MoveTarget(char direction, int[] target)
        {
            switch (direction)
            {
                case 'N':
                    target[1]--;
                    return target;
                case 'E':
                    target[0]++;
                    return target;
                case 'S':
                    target[1]++;
                    return target;
                case 'W':
                    target[0]--;
                    return target;
                default:
                    return target;
            }
        }

        //Reverse a given cardinal direction
        private char FlipDirection(char direction)
        {
            switch (direction)
            {
                case 'N':
                    return 'S';
                case 'E':
                    return 'W';
                case 'S':
                    return 'N';
                case 'W':
                    return 'E';
                default :
                    return '*';
            }
        }

        //Determine which direction the currently targeted ship is facing and return it as a char
        private char GetDirection(int[] target)
        {
            if (CheckNorth(target))
            {
                return 'N';
            }
            if (CheckEast(target))
            {
                return 'E';
            }
            if (CheckSouth(target))
            {
                return 'S';
            }
            if (CheckWest(target))
            {
                return 'W';
            }
            else
            {
                return '*';
            }
        }

        //Return true if a given targeted ship is facing a given direction, and then move the target one square in that direction
        private bool CheckDirection(char direction, int[] target)
        {
            switch (direction)
            {
                case 'N':
                    if (CheckNorth(target))
                    {
                        target[1] -= 1;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 'E':
                    if (CheckEast(target))
                    {
                        target[0] += 1;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 'S':
                    if (CheckSouth(target))
                    {
                        target[1] += 1;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 'W':
                    if (CheckWest(target))
                    {
                        target[0] -= 1;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default :
                    return false;
            }
        }

        //the following four methods each check a specific cardinal direction to see if it is the direction the targeted ship is facing
        #region cardinal checkers
        private bool CheckNorth(int[] target)
        {
            if (target[1] != 0)
            {
                if (standbyPlayer.GameBoard.ShipsAndSea[target[0], target[1]] == standbyPlayer.GameBoard.ShipsAndSea[target[0], target[1] - 1])
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckEast(int[] target)
        {
            if (target[0] != 9)
            {
                if (standbyPlayer.GameBoard.ShipsAndSea[target[0], target[1]] == standbyPlayer.GameBoard.ShipsAndSea[target[0] + 1, target[1]])
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckSouth(int[] target)
        {
            if (target[1] != 9)
            {
                if (standbyPlayer.GameBoard.ShipsAndSea[target[0], target[1]] == standbyPlayer.GameBoard.ShipsAndSea[target[0], target[1] + 1])
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckWest(int[] target)
        {
            if (target[0] != 0)
            {
                if (standbyPlayer.GameBoard.ShipsAndSea[target[0], target[1]] == standbyPlayer.GameBoard.ShipsAndSea[target[0] - 1, target[1]])
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion

        //Asks the user a given yes or no question and returns true if the user inputs yes, or false if the user inputs no
        private bool GetUserInput(string questionToAsk)
        {
            Console.WriteLine(questionToAsk);
            string replay = Console.ReadLine().ToUpper();
            if (replay == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }       

        //Adds one win to the turn player's total wins, one loss to the standby player's total losses, and ends the game, sending the user back to the menu.
        private void TurnPlayerWin()
        {
            gameInProgress = false;
            turnPlayer.AddWin();
            standbyPlayer.AddLoss();
            menu = true;
            DisplayVictor();

            StreamWriter writer = new StreamWriter("Logs\\" + logFileName + ".txt");
            writer.WriteLine("-------------------------------------");
            writer.WriteLine(turnPlayer.Name + " Has Won.");
            writer.Close();
        }

        //returns false if both player still have ships unbombed. Else, declares the turn player to be the winner and returns true.
        private bool GameOver()
        {
            bool result = true;
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (p1.GameBoard.ShipsAndSea[x, y] != 'x' && !p1.GameBoard.Bombed[x, y])
                    {
                        result = false;
                    }
                }
            }

            if (result == false)
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        if (p2.GameBoard.ShipsAndSea[x, y] != 'x' && !p2.GameBoard.Bombed[x, y])
                        {
                            return false;
                        }
                    }
                }
            }

            TurnPlayerWin();
            return true;
        }      
        #endregion

        //This method keeps the gameloop looping until the game loop returns false, at which point the method exits and the game ends.
        public void Start()
        {
            while (GameLoop())
            {
            }
            if (gameInProgress)
            {
                StreamWriter writer = new StreamWriter("Logs\\" + logFileName + ".txt", true);
                writer.WriteLine("-------------------------------------");
                writer.WriteLine("Game Exited");
                writer.Close();
            }             
        }
    }
}
