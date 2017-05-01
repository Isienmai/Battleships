using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Battleships
{
    class GameBoard
    {
        //Region: contains variables
        #region Variables
        //The following variables are the two grids which make up the gameboard and the name of the player who's board this is.
        char[,] shipsAndSea = new char[10, 10];
        bool[,] bombed = new bool[10, 10];
        string name;
        #endregion

        //Initialise the gameboard by giving it a player and filling the grid with default values
        public GameBoard()
        {
            FillGridWithDefaultValues();
        }

        //Region: contains methods used to make the entire grid either visible or hidden
        #region AlterVisibility
        //The following two methods fill the "bombed" grid, either with true or false, in order to hide or display the entire "shipsAndSea" grid.
        //Note: use wipes bombing progress of this gameboard. BE WARNED!!
        private void DisplayShips()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    bombed[x, y] = true;
                }
            }
        }

        public void HideShips()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    bombed[x, y] = false;
                }
            }
        }
        #endregion

        //Region: contains methods used to draw the grid to the screen
        #region DisplayGrid
        //The following method displays the gameboard to the screen.
        //The four methods after DrawGrid are used in DrawGrid, each one drawing a different part of the game board.
        public void DrawGrid()
        {
            DrawFirstLine();
            for (int i = 0; i < 9; i++)
            {
                DrawRow(i);
                DrawLine();
            }
            DrawRow(9);
            DrawLastLine();
        }

        private void DrawRow(int y)
        {
            char dataRepresentation;
            if (y < 9)
            {
                Console.Write(" " + (y + 1));
            }
            else
            {
                Console.Write("10");
            }

            for (int x = 0; x < 10; x++)
            {
                Console.Write("|");
                Console.BackgroundColor = ConsoleColor.Blue;
                if (bombed[x,y] == true)
                {
                    dataRepresentation = shipsAndSea[x, y];
                }
                else
                {
                    dataRepresentation = ' ';
                }

                switch (dataRepresentation)
                {
                    case 'x':
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("><");
                        break;
                    case 'a':
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("()");
                        break;
                    case 'b':
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("()");
                        break;
                    case 's':
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("()");
                        break;
                    case 'd':
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("()");
                        break;
                    case 'p':
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("()");
                        break;
                    case ' ':
                        Console.Write("  ");
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            Console.Write("|\n");
        }

        private void DrawLine()
        {
            Console.WriteLine("──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼──┤");
        }

        private void DrawFirstLine()
        {
            Console.WriteLine("  | A| B| C| D| E| F| G| H| I| J|");
            Console.WriteLine("──┼──┼──┼──┼──┼──┼──┼──┼──┼──┼──┤");
        }

        private void DrawLastLine()
        {
            Console.WriteLine("──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘");
        }
        #endregion
        
        //Used to display a given string in red, rather than white.
        private void DisplayError(string toDisplay)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(toDisplay);
            Console.ForegroundColor = ConsoleColor.Gray;
        }        

        //Set a given coordinates "bombed" value to true.
        public void Bomb(int x, int y)
        {
            bombed[x, y] = true;
        }

        //Region: contains all methods used to fill the game board
        #region PopulateGameBoard
        //Region: contains methods used to populate the grid from a file and to save the grid to a file
        #region Fill Grid From File
        //The following method will load a gameboard from a file into this instance
        private void ReadBoardFromFile(int fileToRead)
        {
            if (File.Exists("GameBoards\\GameBoard" + fileToRead + ".txt"))
            {
                StreamReader reader = new StreamReader("GameBoards\\GameBoard" + fileToRead + ".txt");
                string file = reader.ReadToEnd();
                reader.Close();
                file = file.Replace("\r\n", "");
                char[,] toReturn = new char[10, 10];

                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        toReturn[x, y] = file[x + 10 * y];
                    }
                }

                shipsAndSea = toReturn;
            }
            else
            {
                DisplayError("There is no GameBoard saved of that number.");
            }
        }

        //Save this gameboard to the first unused save slot out of 100. It is saved as a text file.
        private bool SaveGameBoard()
        {
            int gameBoardIndex;
            for (gameBoardIndex = 0; gameBoardIndex < 100; gameBoardIndex++)
            {
                if (!File.Exists("GameBoards\\GameBoard" + gameBoardIndex + ".txt"))
                {
                    FileStream fs = File.Create("GameBoards\\GameBoardGameBoard" + gameBoardIndex + ".txt");
                    fs.Close();
                    gameBoardIndex = 101;
                }
            }
            if (gameBoardIndex == 101)
            {
                StreamWriter writer = new StreamWriter("GameBoards\\GameBoardGameBoard" + gameBoardIndex + ".txt");
                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        writer.Write(shipsAndSea[x, y]);
                    }
                    writer.WriteLine();
                }
                writer.Close();
                return true;
            }
            else
            {
                DisplayError("You have used up all 100 gameboard save slots. Please delete one or several and then try again.");
                return false;
            }
        }

        //Takes a stream reader and reads in a gameboard from the given save file.
        public void GetBoardFromGameFile(StreamReader fileToUse)
        {
            for (int i = 0; i < 10; i++)
            {
                LineToShipArray(fileToUse.ReadLine(), i);
                LineToBoolArray(fileToUse.ReadLine(), i);
            }
        }

        //Takes a string and converts it into a row of the ships/sea grid
        private void LineToShipArray(string line, int lineNumber)
        {
            for (int i = 0; i < 10; i++)
            {
                shipsAndSea[i, lineNumber] = line[i];
            }
        }

        //takes a string and converts it into a row of the bombed grid
        private void LineToBoolArray(string line, int lineNumber)
        {
            line = line.Replace("True", "1");
            line = line.Replace("False", "0");
            for (int i = 0; i < 10; i++)
            {
                if (line[i] == '1')
                {
                    bombed[i, lineNumber] = true;
                }
                else
                    if (line[i] == '0')
                    {
                        bombed[i, lineNumber] = false;
                    }
            }
        }

        //Fills the grid with a given array of chars
        public void FillGameBoard(char[,] boardFromFile)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    shipsAndSea[x, y] = boardFromFile[x, y];
                    bombed[x, y] = false;
                }
            }
        }

        #endregion

        //Region: contains methods used to fill the grid using player given cordinates
        #region Fill Grid From Player Input
        //Gets a user to populate the grid with ships.
        public void InitialiseGameBoard(string nameOfBoardUser)
        {
            FillGridWithDefaultValues();
            name = nameOfBoardUser;
            DrawGrid();
            Console.WriteLine(name + " will now place their ships on the grid.");
            AddShips();
            HideShips();
            Console.Clear();
        }

        //This method will ask the user to input the coordinates and vectors for each of five ships.
        private void AddShips()
        {
            int index = 0;
            int size = 0;
            char ship = 'x';
            int[] vectorInt = new int[3];
            while (index < 5)
            {
                switch (index)
                {
                    case 0:
                        size = 5;
                        ship = 'a';
                        break;

                    case 1:
                        size = 4;
                        ship = 'b';
                        break;

                    case 2:
                        size = 3;
                        ship = 's';
                        break;

                    case 3:
                        size = 3;
                        ship = 'd';
                        break;

                    case 4:
                        size = 2;
                        ship = 'p';
                        break;
                }
                vectorInt = GetShipVector(size);

                if (ValidateVectorInt(vectorInt, size))
                {
                    AddShip(vectorInt, size, ship);
                    index++;
                }

                DrawGrid();
            }
        }

        //If the input char array is correctly formatted to place a ship return true
        private bool ValidateVectorChar(char[] toValidate)
        {
            if (toValidate[0] < 'A' || toValidate[0] > 'J')
            {
                return false;
            }
            if (toValidate[1] < '1' || toValidate[1] > ':')
            {
                return false;
            }
            if (toValidate[2] != 'N' && toValidate[2] != 'E' && toValidate[2] != 'S' && toValidate[2] != 'W')
            {
                return false;
            }

            return true;
        }

        //Return's true if the input coordinate is on the board and doesn't result in ships being placed on top of one another.
        private bool ValidateVectorInt(int[] toValidate, int size)
        {
            switch (toValidate[2])
            {
                case 78:
                    if (!CheckUp(toValidate, size))
                    {
                        return false;
                    }
                    break;
                case 69:
                    if (!CheckRight(toValidate, size))
                    {
                        return false;
                    }
                    break;
                case 83:
                    if (!CheckDown(toValidate, size))
                    {
                        return false;
                    }
                    break;
                case 87:
                    if (!CheckLeft(toValidate, size))
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        //Region: Contains methods used to make sure ships are not placed on top of one another
        #region ValidateVectorInt Direction Specific Methods
        //Each of the four following methods checks to make sure a ship being placed at a given coord, facing a given direction, does not end up placed on top of any other ships.
        //There is one method for each cardinal direction.
        private bool CheckUp(int[] toValidate, int size)
        {
            if (toValidate[1] - size < -1)
            {
                DisplayError("Ships must be on the grid.");
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (shipsAndSea[toValidate[0], toValidate[1] - i] != 'x')
                {
                    DisplayError("Ships cannot be placed on top of one another.");
                    return false;
                }
            }
            return true;
        }

        private bool CheckRight(int[] toValidate, int size)
        {
            if (toValidate[0] + size > 10)
            {
                DisplayError("Ships must be on the grid.");
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (shipsAndSea[toValidate[0] + i, toValidate[1]] != 'x')
                {
                    DisplayError("Ships cannot be placed on top of one another.");
                    return false;
                }
            }
            return true;
        }

        private bool CheckDown(int[] toValidate, int size)
        {
            if (toValidate[1] + size > 10)
            {
                DisplayError("Ships must be on the grid.");
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (shipsAndSea[toValidate[0], toValidate[1] + i] != 'x')
                {
                    DisplayError("Ships cannot be placed on top of one another.");
                    return false;
                }
            }
            return true;
        }

        private bool CheckLeft(int[] toValidate, int size)
        {
            if (toValidate[0] - size < -1)
            {
                DisplayError("Ships must be on the grid.");
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (shipsAndSea[toValidate[0] - i, toValidate[1]] != 'x')
                {
                    DisplayError("Ships cannot be placed on top of one another.");
                    return false;
                }
            }
            return true;
        }
        #endregion

        //This method takes a ship size and a vector and then adds a ship to a given gameboard that is the given size and is placed where the vector dictates.
        public void AddShip(int[] vector, int size, char ship)
        {
            for (int i = 0; i < size; i++)
            {
                switch (vector[2])
                {
                    case 78:
                        shipsAndSea[vector[0], vector[1] - i] = ship;
                        break;
                    case 69:
                        shipsAndSea[vector[0] + i, vector[1]] = ship;
                        break;
                    case 83:
                        shipsAndSea[vector[0], vector[1] + i] = ship;
                        break;
                    case 87:
                        shipsAndSea[vector[0] - i, vector[1]] = ship;
                        break;
                }
            }
        }

        //This method translates the string vector that the player input into a mathematical vector.
        public int[] CharVectToIntVect(char[] charVector)
        {
            int[] intVector = new int[3];

            intVector[0] = charVector[0] - 65;
            intVector[1] = charVector[1] - 49;
            intVector[2] = charVector[2];

            return intVector;
        }

        //This method asks the user for a vector. If the vector is valid it get's translated to an int and returned, else the user is asked to input it again.
        private int[] GetShipVector(int size)
        {
            int[] toReturn = new int[3];
            char[] vector = new char[3];
            string temporary;
            do
            {
                temporary = " ";
                while (temporary.Length < 3)
                {
                    Console.WriteLine("Please enter the coordinate and cardinal direction of your {0} long ship.", size);
                    temporary = Console.ReadLine();
                    temporary = temporary.ToUpper();

                    if (temporary.Contains("10"))
                    {
                        temporary = temporary.Replace("10", ":");
                    }

                    if (temporary.Length < 3)
                    {
                        DisplayError("Error: Incorrect format. Try again.");
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    vector[i] = temporary[i];
                }

                if (ValidateVectorChar(vector))
                {
                    toReturn = CharVectToIntVect(vector);
                }
                else
                {
                    DisplayError("The input location and direction was invalid. Make sure it is formatted as\nletter,number,letter (without the commas).");
                    DisplayError("The first letter and number represent the selected coordinate whilst the final\nletter must be one of N,E,S,W and represents the cardinal direction the boat\nwill face.");
                }
            }
            while (!ValidateVectorChar(vector));
            return toReturn;
        }
        #endregion

        //fills the grid with default values
        public void FillGridWithDefaultValues()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    shipsAndSea[x, y] = 'x';
                    bombed[x, y] = true;
                }
            }
        }                
        #endregion

        #region Properties
        //The following two properties allow access, but not modification, to the two grids.
        public char[,] ShipsAndSea
        {
            get
            {
                return shipsAndSea;
            }
        }

        public bool[,] Bombed
        {
            get
            {
                return bombed;
            }
        }
        #endregion
    }    
}
