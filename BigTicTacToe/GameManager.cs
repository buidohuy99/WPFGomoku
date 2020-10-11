using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTicTacToe
{
    public class GameManager
    {
        public enum GAME_STATE {
            GAME_NOT_OVER,
            X_WIN,
            O_WIN,
            OUT_OF_CELLS
        }

        public enum SAVE_OUTCOME {
            FINE,
            FILE_AVAILABLE,
            FOLDER_NOTAVAILABLE
        }

        public enum LOAD_OUTCOME
        {
            FINE,
            FILE_NOTAVAILABLE
        }

        //Gameplay vars
        public int numberOfCellsLeft { get; private set; }
        public bool gameStarted { get; private set; }
        int[,] TTTBoard;
        public bool isXTurn { get; private set; }
        List<Move> history;

        public bool startGameIsXTurn { get; private set; }
        public int currentState { get; private set; }
        public int numOfRows { get; private set; }
        public int numOfCols { get; private set; }

        public GameManager(int numOfRows, int numOfCols) {
            this.numOfRows = numOfRows;
            this.numOfCols = numOfCols;
            gameStarted = false;
            numberOfCellsLeft = numOfRows * numOfCols;
            TTTBoard = new int[numOfRows, numOfCols];
            for (int i = 0; i < numOfCols * numOfRows; i++) {
                TTTBoard[i / numOfCols, i % numOfCols] = 0;
            }
            history = new List<Move>();
            currentState = -1;
        }

        public void restartGame() {
            resetBoard();
            var random = new Random();
            int turn = random.Next(0, 2);
            numberOfCellsLeft = numOfCols * numOfRows;
            isXTurn = Convert.ToBoolean(turn);
            startGameIsXTurn = isXTurn;
            gameStarted = true;
        }

        public void resetBoard() {
            for (int i = 0; i < numOfCols * numOfRows; i++)
            {
                TTTBoard[i / numOfCols, i % numOfCols] = 0;
            }
        }

        private void saveGameUnchecked(string filename) {
            var writer = new StreamWriter(filename);
            StringBuilder sb = new StringBuilder();
            // Dong dau tien la luot di hien tai
            sb.AppendLine(isXTurn ? "X" : "O");
            // Dong thu hai la so o con trong
            sb.AppendLine(numberOfCellsLeft.ToString());

            // Theo sau la ma tran bieu dien game
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfCols; j++)
                {
                    sb.Append($"{TTTBoard[i, j]}");
                    if (j != numOfCols - 1)
                    {
                        sb.Append(" ");
                    }
                }
                sb.AppendLine();
            }

            writer.Write(sb.ToString());
            writer.Close();
        }

        public void saveGame(string path, Action<SAVE_OUTCOME, Action> callback) {
            if (!Directory.Exists(path)) {
                callback?.Invoke(SAVE_OUTCOME.FOLDER_NOTAVAILABLE, null);
                return;
            }

            string filename = path + "\\save.tictactoe";
            if (File.Exists(filename)) {
                callback?.Invoke(SAVE_OUTCOME.FILE_AVAILABLE, () => {
                    saveGameUnchecked(filename);
                    return;
                });
                return;
            }

            saveGameUnchecked(filename);
            callback?.Invoke(SAVE_OUTCOME.FINE, null);
        }

        public void loadGame(string filename, Action<LOAD_OUTCOME, int[,]> callback) {
            if (!File.Exists(filename))
            {
                callback?.Invoke(LOAD_OUTCOME.FILE_NOTAVAILABLE, null);
                return;
            }

            var reader = new StreamReader(filename);
            string data = reader.ReadToEnd();
            reader.Close();

            StringReader sr = new StringReader(data);
            var firstLine = sr.ReadLine();
            isXTurn = firstLine.Equals("X");
            startGameIsXTurn = isXTurn;
            numberOfCellsLeft = int.Parse(sr.ReadLine());
            gameStarted = true;

            for (int i = 0; i < numOfRows; i++)
            {
                var tokens = sr.ReadLine().Split(
                    new string[] { " " }, StringSplitOptions.None);

                for (int j = 0; j < numOfCols; j++)
                {
                    TTTBoard[i, j] = int.Parse(tokens[j]);
                }
            }

            sr.Close();
            callback?.Invoke(LOAD_OUTCOME.FINE, TTTBoard.Clone() as int[,]);
        }

        public void moveAt(Move move) {
            TTTBoard[move.row, move.col] = move.isXTurn ? 1 : 2;
            numberOfCellsLeft--;
            currentState++;
            if (currentState < history.Count)
            {
                history.RemoveRange(currentState, history.Count - currentState);
            }
            history.Add(new Move(move.row, move.col, isXTurn));
            isXTurn = !isXTurn;
        }

        private void printBoard() {
            for (int i = 0; i < numOfRows; i++) {
                for (int j = 0; j < numOfCols; j++)
                    Console.Write((TTTBoard[i, j] == 1 ? "X" : TTTBoard[i, j] == 2 ? "O" : "_") + " ");
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public int getMoveAt(int row, int col) {
            return TTTBoard[row, col];
        }

        public int getHistoryCount() {
            return history.Count;
        }

        public void undoMove() {
            if (currentState == -1) return;
            Move current = history[currentState];
            isXTurn = current.isXTurn;
            TTTBoard[current.row, current.col] = 0;
            currentState--;
            numberOfCellsLeft++;
        }

        public void redoMove() {
            if (currentState >= getHistoryCount() - 1) return;
            currentState++;
            numberOfCellsLeft--;
            TTTBoard[history[currentState].row, history[currentState].col] = history[currentState].isXTurn ? 1 : 2;
            isXTurn = !history[currentState].isXTurn;
        }

        public Move getMoveOfCurrentState() {
            return history[currentState];
        }

        public Tuple<GAME_STATE, List<Move>> checkWin(int row, int col)
        {
            List<Move> longestStreak = new List<Move>();
            Stack<Move> countStack = new Stack<Move>();

            //Vars for checking
            bool barricaded = false;

            //Check horizontal
            List<Move> streakCounter = new List<Move>();
            for (int i = 0; i < numOfCols; i++)
            {
                if (TTTBoard[row, i] == 0 || TTTBoard[row, col] != TTTBoard[row, i])
                {
                    //Calculate if there is a streak of 5 or a streak of >= 5
                    if (countStack.Count > streakCounter.Count)
                    {
                        streakCounter = countStack.ToList();
                    }
                    //Reset counter
                    countStack.Clear();
                    //If its an opponent's piece
                    if (TTTBoard[row, i] != 0)
                    {
                        //My pieces after that piece the opponent placed will be barricaded
                        if (!barricaded)
                        {
                            //So I set it to true
                            barricaded = true;
                            //Save information of the longest streak before a barricade appear
                            longestStreak = streakCounter.ToList();
                        }
                        //Remove streak if opponent successfully barricade my streak
                        else
                        {
                            streakCounter.Clear();
                        }
                    }
                }
                else
                {
                    countStack.Push(new Move(row, i, isXTurn));
                }
            }

            //Calculate if there is a streak of 5 or a streak of >= 5
            if (countStack.Count > streakCounter.Count)
            {
                streakCounter = countStack.ToList();
            }
            if (longestStreak.Count < 5)
            {
                longestStreak = streakCounter.ToList();
            }

            if (longestStreak.Count >= 5)
            {
                if (!isXTurn) return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.X_WIN, longestStreak);
                else return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.O_WIN, longestStreak);
            }

            //Reset vars
            longestStreak.Clear();
            countStack.Clear();
            streakCounter.Clear();
            barricaded = false;

            //Check vertical
            for (int i = 0; i < numOfRows; i++)
            {
                if (TTTBoard[i, col] == 0 || TTTBoard[row, col] != TTTBoard[i, col])
                {
                    //Calculate if there is a streak of 5 or a streak of >= 5
                    if (countStack.Count > streakCounter.Count)
                    {
                        streakCounter = countStack.ToList();
                    }
                    //Reset counter
                    countStack.Clear();
                    //If its an opponent's piece
                    if (TTTBoard[i, col] != 0)
                    {
                        //My pieces after that piece the opponent placed will be barricaded
                        if (!barricaded)
                        {
                            //So I set it to true
                            barricaded = true;
                            //Save information of the longest streak before a barricade appear
                            longestStreak = streakCounter.ToList();

                        }
                        //Remove streak if opponent successfully barricade my streak
                        else
                        {
                            streakCounter.Clear();
                        }
                    }
                }
                else
                    countStack.Push(new Move(i, col, isXTurn));
            }

            //Calculate if there is a streak of 5 or a streak of >= 5
            if (countStack.Count > streakCounter.Count)
            {
                streakCounter = countStack.ToList();
            }
            if (longestStreak.Count < 5)
            {
                longestStreak = streakCounter.ToList();
            }

            if (longestStreak.Count >= 5)
            {
                gameStarted = !gameStarted;
                if (!isXTurn) return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.X_WIN, longestStreak);
                else return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.O_WIN, longestStreak);
            }

            //Reset vars
            longestStreak.Clear();
            countStack.Clear();
            streakCounter.Clear();
            barricaded = false;

            //Check main diagonal
            int offsetBefore = 0, offsetAfter = 0;
            offsetBefore = col - row;
            offsetAfter = (numOfCols - 1 - col) - (numOfRows - 1 - row);
            int startCellRow = offsetBefore < 0 ? Math.Abs(offsetBefore) : 0;
            int startCellCol = offsetBefore > 0 ? Math.Abs(offsetBefore) : 0;
            int endCellRow = offsetAfter < 0 ? numOfRows - 1 - Math.Abs(offsetAfter) : numOfRows - 1;
            int endCellCol = offsetAfter > 0 ? numOfCols - 1 - Math.Abs(offsetAfter) : numOfCols - 1;
            for (int i = startCellRow, j = startCellCol; (i <= endCellRow && j <= endCellCol); i++, j++)
            {
                if (TTTBoard[i, j] == 0 || TTTBoard[row, col] != TTTBoard[i, j])
                {
                    //Calculate if there is a streak of 5 or a streak of >= 5
                    if (countStack.Count > streakCounter.Count)
                    {
                        streakCounter = countStack.ToList();
                    }
                    //Reset counter
                    countStack.Clear();
                    //If its an opponent's piece
                    if (TTTBoard[i, j] != 0)
                    {
                        //My pieces after that piece the opponent placed will be barricaded
                        if (!barricaded)
                        {
                            //So I set it to true
                            barricaded = true;
                            //Save information of the longest streak before a barricade appear
                            longestStreak = streakCounter.ToList();
                        }
                        //Remove streak if opponent successfully barricade my streak
                        else
                        {
                            streakCounter.Clear();
                        }
                    }
                }
                else
                    countStack.Push(new Move(i, j, isXTurn));
            }

            //Calculate if there is a streak of 5 or a streak of >= 5
            if (countStack.Count > streakCounter.Count)
            {
                streakCounter = countStack.ToList();
            }
            if (longestStreak.Count < 5)
            {
                longestStreak = streakCounter.ToList();
            }

            if (longestStreak.Count >= 5)
            {
                gameStarted = !gameStarted;
                if (!isXTurn) return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.X_WIN, longestStreak);
                else return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.O_WIN, longestStreak);
            }

            //Reset vars
            longestStreak.Clear();
            countStack.Clear();
            streakCounter.Clear();
            barricaded = false;

            //Check sub diagonal
            offsetBefore = col - (numOfRows - 1 - row);
            offsetAfter = (numOfCols - 1 - col) - row;
            startCellRow = offsetBefore < 0 ? numOfRows - 1 - Math.Abs(offsetBefore) : numOfRows - 1;
            startCellCol = offsetBefore > 0 ? Math.Abs(offsetBefore) : 0;
            endCellRow = offsetAfter < 0 ? Math.Abs(offsetAfter) : 0;
            endCellCol = offsetAfter > 0 ? numOfCols - 1 - Math.Abs(offsetAfter) : numOfCols - 1;
            for (int i = startCellRow, j = startCellCol; (i >= endCellRow && j <= endCellCol); i--, j++)
            {
                if (TTTBoard[i, j] == 0 || TTTBoard[row, col] != TTTBoard[i, j])
                {
                    //Calculate if there is a streak of 5 or a streak of >= 5
                    if (countStack.Count > streakCounter.Count)
                    {
                        streakCounter = countStack.ToList();
                    }
                    //Reset counter
                    countStack.Clear();
                    //If its an opponent's piece
                    if (TTTBoard[i, j] != 0)
                    {
                        //My pieces after that piece the opponent placed will be barricaded
                        if (!barricaded)
                        {
                            //So I set it to true
                            barricaded = true;
                            //Save information of the longest streak before a barricade appear
                            longestStreak = streakCounter.ToList();

                        }
                        //Remove streak if opponent successfully barricade my streak
                        else
                        {
                            streakCounter.Clear();
                        }
                    }
                }
                else
                    countStack.Push(new Move(i, j, isXTurn));
            }

            //Calculate if there is a streak of 5 or a streak of >= 5
            if (countStack.Count > streakCounter.Count)
            {
                streakCounter = countStack.ToList();
            }
            if (longestStreak.Count < 5)
            {
                longestStreak = streakCounter.ToList();
            }

            if (longestStreak.Count >= 5)
            {
                gameStarted = !gameStarted;
                if (!isXTurn) return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.X_WIN, longestStreak);
                else return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.O_WIN, longestStreak);
            }

            if (numberOfCellsLeft == 0)
            {
                gameStarted = !gameStarted;
                return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.OUT_OF_CELLS, null);
            }

            return new Tuple<GAME_STATE, List<Move>>(GAME_STATE.GAME_NOT_OVER, null);
        }

    }
}
