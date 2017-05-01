using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Battleships
{
    class Player
    {
        #region variables
        string name;
        int totalWins;
        int totalLosses;
        double winLossRatio;
        //Gameboards have two dimensions to store the grid, and a third to store both ship placement, and whether or not a given square has been hit.
        GameBoard gameBoard;
        //The AI grid initialiser
        AIShipInitialiser randomShipPlacer;
        //The AI targeter
        AITargeter targetCalculator = new AITargeter();
        #endregion

        //The player class requires a name at instantiation.
        public Player(string playerName)
        {
            name = playerName;
            if (playerName != "")
            {
                SyncPlayerWithFile();
            }
            gameBoard = new GameBoard();
            randomShipPlacer = new AIShipInitialiser(gameBoard);
        }

        //Reload the current instance of player with data of a new player.
        public void ChangePlayer()
        {
            UpdateFile();
            Console.WriteLine("What is the name of the new player?");
            name = Console.ReadLine().ToUpper();
            if (name == "")
            {
                name = "CPU";
            }
            totalWins = 0;
            totalLosses = 0;
            winLossRatio = 0;
            SyncPlayerWithFile();
        }        
        
        #region Properties
        //The following properties allow access (but not modification) to the name, totalWins, totalLosses and winLossRatio, and both access and modification to the gameboard and the targetCalculator
        public string Name
        {
            get
            {
                return name;
            }
        }

        public int TotalWins
        {
            get
            {
                return totalWins;
            }
        }

        public int TotalLosses
        {
            get
            {
                return totalLosses;
            }
        }

        public double WinLossRatio
        {
            get
            {
                return winLossRatio;
            }
        }

        public GameBoard GameBoard
        {
            get 
            {
                return gameBoard;
            }

            set
            {
                gameBoard = value;
            }
        }

        public AITargeter TargetCalculator
        {
            get
            {
                return targetCalculator;
            }
            set
            {
                targetCalculator = value;
            }
        }
        #endregion

        #region file using methods
        //The following method saves the player's midgame data to a selected file
        public void SaveGameData(StreamWriter fileToUse)
        {
            fileToUse.WriteLine(name);
            fileToUse.WriteLine(totalWins);
            fileToUse.WriteLine(totalLosses);
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    fileToUse.Write(gameBoard.ShipsAndSea[x, y]);
                }
                fileToUse.WriteLine();
                for (int x = 0; x < 10; x++)
                {
                    fileToUse.Write(gameBoard.Bombed[x, y]);
                }
                fileToUse.WriteLine();
            }
        }

        //Read the player data and gameboard from a given save file.
        public void LoadGameData(StreamReader fileToUse)
        {
            name = fileToUse.ReadLine();
            totalWins = Convert.ToInt32(fileToUse.ReadLine());
            totalLosses = Convert.ToInt32(fileToUse.ReadLine());
            GetWinLossRatio();
            gameBoard.GetBoardFromGameFile(fileToUse);
        }

        //The following method will read information from the player file with the same name as this player, to populate this player's win and loss records.
        //If this player doesn't have a player file then one is created and populated with the default values of playername, 0 wins, 0 losses, 0 win/loss ratio
        private void SyncPlayerWithFile()
        {
            if (File.Exists("Players\\" + name.ToUpper() + ".txt"))
            {
                StreamReader reader = new StreamReader("Players\\" + name.ToUpper() + ".txt");
                name = reader.ReadLine();
                totalWins = Convert.ToInt32(reader.ReadLine());
                totalLosses = Convert.ToInt32(reader.ReadLine());
                winLossRatio = Convert.ToDouble(reader.ReadLine());
                reader.Close();
            }
            else
            {
                FileStream fs = File.Create("Players\\" + name.ToUpper() + ".txt");
                fs.Close();
                StreamWriter writer = new StreamWriter("Players\\" + name.ToUpper() + ".txt");
                writer.WriteLine(name);
                writer.WriteLine(totalWins);
                writer.WriteLine(totalLosses);
                writer.WriteLine(winLossRatio);
                writer.Close();
            }
        }

        //The following method updates this player's file, replacing it's data with the current ingame data.
        private void UpdateFile()
        {
            StreamWriter writer = new StreamWriter("Players\\" + name.ToUpper() + ".txt");
            writer.WriteLine(name);
            writer.WriteLine(totalWins);
            writer.WriteLine(totalLosses);
            writer.WriteLine(winLossRatio);
            writer.Close();
        }        
        #endregion

        #region gamewon methods
        //The following two methods add either a win or a loss to this players record, updates the win/loss ration and then updates this player's file.
        public void AddWin()
        {
            totalWins++;
            GetWinLossRatio();
            UpdateFile();
        }
        public void AddLoss()
        {
            totalLosses++;
            GetWinLossRatio();
            UpdateFile();
        }

        //This method calculates the win/loss ratio
        private void GetWinLossRatio()
        {
            if (totalLosses == 0)
            {
                winLossRatio = totalWins;
            }
            else
            {
                winLossRatio = totalWins / totalLosses;
            }
        }
        #endregion    
        
        //initialises this players gameboard
        public void InitialiseGameBoard()
        {
            if (name == "CPU")
            {
                gameBoard.FillGridWithDefaultValues();
                randomShipPlacer.InitialiseGameBoard("CPU");
            }
            else
            {
                gameBoard.InitialiseGameBoard(name);
            }
        }
    }
}
