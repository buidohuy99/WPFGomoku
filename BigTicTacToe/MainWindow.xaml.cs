using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BigTicTacToe
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Consts
            //UI
        const int CellWidth = 30;
        const int CellHeight = 30;
            //Gameplay
        const int X_WIN = 1;
        const int O_WIN = 2;
        const int GAME_NOT_OVER = 0;
        const int OUT_OF_CELLS = 3;

        //Gameplay vars
        int numberOfCellsLeft;
        bool gameStarted = false;
        int[,] TTTBoard;
        bool isXTurn = true;
        List<Move> history = new List<Move>();

        bool startGameIsXTurn;
        int[,] initialState;
        int currentState = -1;

        //UI related
        int numOfRows, numOfCols;
        double offsetX, offsetY;
        int prevPos = -1;
        TextBlock currentTurn;
        Image gameHome;
        Border choiceBox;

        public MainWindow()
        {
            InitializeComponent();    
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            numOfCols = (int)(GameBoard.ActualWidth / CellWidth);
            numOfRows = (int)(GameBoard.ActualHeight / CellHeight);
            offsetX = (GameBoard.ActualWidth - numOfCols * CellWidth) / 2;
            offsetY = (GameBoard.ActualHeight - numOfRows * CellHeight) / 2;

            TTTBoard = new int[numOfRows, numOfCols];
            numberOfCellsLeft = numOfCols * numOfRows;
            
            //Populate Canvas
            for(int i = 0; i < numOfRows; i++)
            {
                for(int j = 0; j < numOfCols; j++)
                {
                    TTTBoard[i, j] = 0;

                    if (i != 0 && j != 0) continue;
                    if (i == numOfRows - 1 || j == numOfCols - 1) continue;
                    
                    if (i == 0)
                    {
                        Line line = new Line();
                        line.Stroke = new SolidColorBrush(Color.FromRgb(250, 244, 142));
                        line.StrokeThickness = 1;
                        GameBoard.Children.Add(line);
                        line.X1 = offsetX + j * CellWidth + CellWidth;
                        line.X2 = line.X1;
                        line.Y1 = offsetY;
                        line.Y2 = line.Y1 + numOfRows * CellHeight;
                    }

                    if(j == 0)
                    {
                        Line line = new Line();
                        line.Stroke = new SolidColorBrush(Color.FromRgb(250, 244, 142));
                        line.StrokeThickness = 1;
                        GameBoard.Children.Add(line);
                        line.X1 = offsetX;
                        line.X2 = line.X1 + numOfCols * CellWidth;
                        line.Y1 = offsetY + i * CellHeight + CellHeight;
                        line.Y2 = line.Y1;
                    }

                }
            }

            //Set Game Home Image
            gameHome = new Image
            {
                Source = new BitmapImage(
                new Uri("Images/GameHome.png", UriKind.Relative)),
                Width = 180,
                Height = 180,
            };
            GameStatus.Child = gameHome;

            //Set current turn text properties
            currentTurn = new TextBlock();
            currentTurn.FontWeight = FontWeights.ExtraBold;
            currentTurn.VerticalAlignment = VerticalAlignment.Center;
            currentTurn.HorizontalAlignment = HorizontalAlignment.Center;
            currentTurn.FontSize = 130;
            currentTurn.Padding = new Thickness(0, 0, 0, 15);

            //Create choice box
            choiceBox = createChoiceBlock();

            //Disable Saving
            SaveButton.IsEnabled = false;
            UndoButton.IsEnabled = false;
            RedoButton.IsEnabled = false;

            //Disable Game Board
            GameBoard.IsEnabled = false;

        }

        private Border createChoiceBlock()
        {
            Border returnBlock = new Border();
            returnBlock.Background = new SolidColorBrush(Color.FromRgb(214, 168, 51));
            returnBlock.Height = CellHeight;
            returnBlock.Width = CellWidth;
            return returnBlock;
        }

        private void setChoiceBlockText(Border choiceBlock, TextBlock text)
        {
            if (choiceBlock == null || text == null) return;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            choiceBlock.Child = text;
        }

        private void ResetBoard(bool makeDisable, bool clearContent)
        {
            if (clearContent)
            {
                int count = GameBoard.Children.Count;
                int numOfLines = numOfRows + numOfCols - 2;
                for (int i = count - 1; i >= numOfLines; i--)
                {
                    var item = (GameBoard.Children[i] as Border).Tag as Tuple<int,int>;
                    TTTBoard[item.Item1, item.Item2] = 0;
                    GameBoard.Children.RemoveAt(i);
                }
            }
            GameBoard.IsEnabled = true;
            if (makeDisable) GameBoard.IsEnabled = false;
        }

        private void createTextBlockAt(Move move) {
            Border block = createChoiceBlock();
            //Place block down
            TextBlock newText = new TextBlock();
            if (move.isXTurn)
            {
                newText.Text = "X";
                newText.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                TTTBoard[move.row, move.col] = 1;
            }
            else
            {
                newText.Text = "O";
                newText.Foreground = new SolidColorBrush(Colors.DarkRed);
                TTTBoard[move.row, move.col] = 2;
            }
            newText.FontWeight = FontWeights.Bold;
            setChoiceBlockText(block, newText);
            block.Opacity = 0.75;
            block.Tag = new Tuple<int, int>(history[currentState].row, history[currentState].col);
            GameBoard.Children.Add(block);
            Canvas.SetLeft(block, offsetX + CellWidth * history[currentState].col);
            Canvas.SetTop(block, offsetY + CellHeight * history[currentState].row);
        }

        private void StartGame_Clicked(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            int turn = random.Next(0, 2);
            numberOfCellsLeft = numOfCols * numOfRows;
            isXTurn = Convert.ToBoolean(turn);
            startGameIsXTurn = isXTurn;
            ResetBoard(false, true);
            ShowTurn();
            StartButton.IsEnabled = false;
            LoadButton.IsEnabled = false;
            SaveButton.IsEnabled = true;
            UndoButton.IsEnabled = false;
            RedoButton.IsEnabled = false;

            gameStarted = true;
        }

        private void SaveGame_Clicked(object sender, RoutedEventArgs e)
        {
            const string filename = "save.tictactoe";

            var writer = new StreamWriter(filename);
            // Dong dau tien la luot di hien tai
            writer.WriteLine(isXTurn ? "X" : "O");
            // Dong thu hai la so o con trong
            writer.WriteLine(numberOfCellsLeft);

            // Theo sau la ma tran bieu dien game
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfCols; j++)
                {
                    writer.Write($"{TTTBoard[i, j]}");
                    if (j != numOfCols - 1)
                    {
                        writer.Write(" ");
                    }
                }
                writer.WriteLine("");
            }

            writer.Close();
            MessageBox.Show("Game is saved");
        }

        private void LoadGame_Clicked(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var filename = screen.FileName;
                if (!System.IO.Path.GetExtension(filename).ToLower().Equals(".tictactoe")) {
                    MessageBox.Show("Wrong file extension");
                    return;
                }

                var reader = new StreamReader(filename);
                var firstLine = reader.ReadLine();
                isXTurn = firstLine == "X";
                startGameIsXTurn = isXTurn;
                numberOfCellsLeft = int.Parse(reader.ReadLine());
                ShowTurn();
                ResetBoard(false, true);

                for (int i = 0; i < numOfRows; i++)
                {
                    var tokens = reader.ReadLine().Split(
                        new string[] { " " }, StringSplitOptions.None);

                    for (int j = 0; j < numOfCols; j++)
                    {
                        TTTBoard[i, j] = int.Parse(tokens[j]);

                        if (TTTBoard[i, j] == 0) continue;

                        TextBlock newText = new TextBlock();
                        if (TTTBoard[i, j] == 1)
                        {
                            newText.Text = "X";
                            newText.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                        }
                        else if (TTTBoard[i, j] == 2)
                        {
                            newText.Text = "O";
                            newText.Foreground = new SolidColorBrush(Colors.DarkRed);
                        }
                        newText.FontWeight = FontWeights.Bold;

                        setChoiceBlockText(choiceBox, newText);
                        choiceBox.Opacity = 0.75;

                        choiceBox.Tag = new Tuple<int, int>(i, j);
                        GameBoard.Children.Add(choiceBox);
                        Canvas.SetLeft(choiceBox, offsetX + CellWidth * j);
                        Canvas.SetTop(choiceBox, offsetY + CellHeight * i);

                        choiceBox = createChoiceBlock();
                    }
                }
                initialState = TTTBoard.Clone() as int[,];

                StartButton.IsEnabled = LoadButton.IsEnabled = false;
                SaveButton.IsEnabled = true;
                UndoButton.IsEnabled = false;
                RedoButton.IsEnabled = false;
                gameStarted = true;

                reader.Close();
                MessageBox.Show("Game is loaded");
            }
        }

        private void GameBoard_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!gameStarted) return;

            //Get position relative to gameboard
            var pos = e.GetPosition(GameBoard);
           
            if (pos.X < offsetX || pos.X >= GameBoard.ActualWidth - offsetX || pos.Y < offsetY || pos.Y >= GameBoard.ActualHeight - offsetY)
            {             
                if (prevPos != -1) {
                    GameBoard.Children.RemoveAt(prevPos);
                    prevPos = -1;
                }
                return;
            }

            if (prevPos != -1)
            {
                var item = (GameBoard.Children[prevPos] as Border).Tag as Tuple<int, int>;
                int prevCellRow = item.Item1;
                int prevCellCol = item.Item2;
                
                if (pos.X >= offsetX + prevCellCol * CellWidth && pos.X < offsetX + (prevCellCol + 1) * CellWidth
                    && pos.Y >= offsetY + prevCellRow * CellHeight && pos.Y < offsetY + (prevCellRow + 1) * CellHeight) return;

                GameBoard.Children.RemoveAt(prevPos);
                prevPos = -1;
            }

            int col = (int)((pos.X - offsetX) / CellWidth);
            int row = (int)((pos.Y - offsetY) / CellHeight);
            if (TTTBoard[row, col] != 0) return;

            choiceBox.Tag = new Tuple<int, int>(row, col);
            prevPos = GameBoard.Children.Count;
            GameBoard.Children.Add(choiceBox);
            Canvas.SetLeft(choiceBox, offsetX + CellWidth * col);
            Canvas.SetTop(choiceBox, offsetY + CellHeight * row);
        }

        private void colorCells(List<Move> cells, SolidColorBrush color) {
            foreach (Move iteration in cells) {
                Border border = GameBoard.Children.OfType<Border>().Where(x => (x.Tag as Tuple<int, int>).Item1 == iteration.row && (x.Tag as Tuple<int, int>).Item2 == iteration.col).FirstOrDefault();
                border.Background = color;
            }
        }

        private void GameBoard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!gameStarted) return;

            //Get position relative to gameboard
            var pos = e.GetPosition(GameBoard);

            if (pos.X < offsetX || pos.X >= GameBoard.ActualWidth - offsetX) return;
            if (pos.Y < offsetY || pos.Y >= GameBoard.ActualHeight - offsetY) return;

            int col = (int)((pos.X - offsetX) / CellWidth);
            int row = (int)((pos.Y - offsetY) / CellHeight);

            if(TTTBoard[row, col] == 0)
            {
                //Place block down
                TextBlock newText = new TextBlock();
                if (isXTurn)
                {
                    newText.Text = "X";
                    newText.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                    TTTBoard[row, col] = 1;
                }
                else
                {
                    newText.Text = "O";
                    newText.Foreground = new SolidColorBrush(Colors.DarkRed);
                    TTTBoard[row, col] = 2;
                }
                newText.FontWeight = FontWeights.Bold;
                setChoiceBlockText(choiceBox, newText);
                choiceBox.Opacity = 0.75;
                prevPos = -1;
                choiceBox = createChoiceBlock();
                numberOfCellsLeft--;

                currentState++;
                UndoButton.IsEnabled = true;
                if (currentState < history.Count) { 
                    history.RemoveRange(currentState, history.Count - currentState);
                    RedoButton.IsEnabled = false;
                }
                history.Add(new Move(row,col,isXTurn));

                //Checkwin
                Tuple<int, List<Move>> result = checkWin(row, col);
                if (result.Item1 != GAME_NOT_OVER)
                {
                    StartButton.IsEnabled = true;
                    LoadButton.IsEnabled = true;
                    SaveButton.IsEnabled = false;
                    ResetBoard(true, false);
                    if (result.Item2 != null)
                    {
                        colorCells(result.Item2, new SolidColorBrush(Color.FromRgb(169, 255, 99)));
                    }
                    if (result.Item1 == X_WIN) MessageBox.Show("X wins!");
                    if (result.Item1 == O_WIN) MessageBox.Show("O wins!");
                    if (result.Item1 == OUT_OF_CELLS) MessageBox.Show("Game is a tie!");
                    ResetBoard(true, true);
                    UndoButton.IsEnabled = false;
                    RedoButton.IsEnabled = false;
                    GameStatus.Child = gameHome;
                    return;
                }

                //Change to next Turn
                isXTurn = !isXTurn; 
                ShowTurn();
            }
           
        }

        private void UndoMove_Clicked(object sender, RoutedEventArgs e)
        {
            if (currentState == -1) {
                return;
            }
            //Get move and update the block in
            Move current = history[currentState];
            GameBoard.Children.RemoveAt(GameBoard.Children.Count - 1);
            isXTurn = current.isXTurn;
            ShowTurn();
            TTTBoard[history[currentState].row, history[currentState].col] = 0;
            currentState--;
            numberOfCellsLeft++;
            if (currentState < 0) UndoButton.IsEnabled = false;
            if (currentState < history.Count - 1) RedoButton.IsEnabled = true;    
        }

        private void RedoMove_Clicked(object sender, RoutedEventArgs e)
        {
            if(currentState >= history.Count - 1)
            {
                return;
            }
            currentState++;
            numberOfCellsLeft--;
            if (currentState >= history.Count - 1) RedoButton.IsEnabled = false;
            if (currentState >= 0) UndoButton.IsEnabled = true;
            isXTurn = !history[currentState].isXTurn;
            TTTBoard[history[currentState].row, history[currentState].col] = isXTurn ? 1 : 2;
            createTextBlockAt(history[currentState]);   
            ShowTurn();
        }

        private void ShowTurn()
        {
            string nextTurn;
            if (isXTurn)
            {
                currentTurn.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                nextTurn = "X";
            }
            else
            {
                currentTurn.Foreground = new SolidColorBrush(Colors.DarkRed);
                nextTurn = "O";
            }
            currentTurn.Text = nextTurn;
            GameStatus.Child = currentTurn; 
        }

        private Tuple<int, List<Move>> checkWin(int row, int col)
        {
            List<Move> longestStreak = new List<Move>();
            Stack<Move> countStack = new Stack<Move>();
            
            //Vars for checking
            bool barricaded = false;

            //Check horizontal
            List<Move> streakCounter = new List<Move>();
            for(int i = 0; i < numOfCols; i++)
            {
                if (TTTBoard[row, i] == 0 || TTTBoard[row, col] != TTTBoard[row, i])
                {
                    //Calculate if there is a streak of 5 or a streak of >= 5
                    if (countStack.Count > streakCounter.Count) {
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
                if (isXTurn) return new Tuple<int, List<Move>>(X_WIN, longestStreak);
                else return new Tuple<int, List<Move>>(O_WIN, longestStreak);
            }

            //Reset vars
            longestStreak.Clear();
            countStack.Clear();
            streakCounter.Clear();
            barricaded = false;

            //Check vertical
            for(int i = 0; i < numOfRows; i++)
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
                if (isXTurn) return new Tuple<int, List<Move>>(X_WIN, longestStreak);
                else return new Tuple<int, List<Move>>(O_WIN, longestStreak);
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
            for(int i = startCellRow, j = startCellCol; (i <= endCellRow && j <= endCellCol); i++, j++)
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
                if (isXTurn) return new Tuple<int, List<Move>>(X_WIN, longestStreak);
                else return new Tuple<int, List<Move>>(O_WIN, longestStreak);
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
            if (longestStreak.Count < 5) {
                longestStreak = streakCounter.ToList();
            }

            if (longestStreak.Count >= 5)
            {
                if (isXTurn) return new Tuple<int, List<Move>>(X_WIN, longestStreak);
                else return new Tuple<int, List<Move>>(O_WIN, longestStreak);
            }

            if (numberOfCellsLeft == 0) {
                return new Tuple<int, List<Move>>(OUT_OF_CELLS, null);
            }

            return new Tuple<int, List<Move>>(GAME_NOT_OVER, null);
        }
    }
}
