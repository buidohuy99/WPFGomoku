using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        //Gameplay vars
        bool gameStarted = false;
        int[,] TTTBoard;
        bool isXTurn = true;

        //UI related
        int numOfRows, numOfCols;
        double offsetX, offsetY;
        TextBlock currentTurn;
        Image gameHome;
        Button[,] TTTCells;

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
            TTTCells = new Button[numOfRows, numOfCols]; 
            
            //Populate Canvas
            for(int i = 0; i < numOfRows; i++)
            {
                for(int j = 0; j < numOfCols; j++)
                {
                    TTTBoard[i, j] = 0;

                    Button temp = new Button();
                    temp.Width = CellWidth;
                    temp.Height = CellHeight;
                    temp.Click += Cell_Clicked;
                    temp.Tag = new Tuple<int, int>(i, j);
                    temp.Style = FindResource("TicTacToeCell") as Style;
                    //Setting border of a cell
                    int[] thickness = {1, 1};
                    if (i == 0) thickness[1] = 0;
                    if (j == 0) thickness[0] = 0;
                    temp.BorderThickness = new Thickness(thickness[0], thickness[1], 0, 0);
                    temp.IsEnabled = false;

                    TTTCells[i, j] = temp;

                    //Add to canvas
                    Canvas.SetLeft(temp, offsetX + j * CellWidth);
                    Canvas.SetTop(temp, offsetY + i * CellHeight);
                    GameBoard.Children.Add(temp);

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

            //Disable Saving
            SaveButton.IsEnabled = false;
        }

        private void ResetBoard(bool makeDisable, bool clearContent)
        {
            foreach (var x in GameBoard.Children)
            {
                Button child = x as Button;
                var buttonTag = child.Tag as Tuple<int, int>;
                int cellRow = buttonTag.Item1;
                int cellCol = buttonTag.Item2;
                if (clearContent) child.Content = null;
                child.IsEnabled = true;
                if (makeDisable) child.IsEnabled = false;
                TTTBoard[cellRow, cellCol] = 0;
            }
        }

        private void StartGame_Clicked(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            int turn = random.Next(0, 2);
            isXTurn = Convert.ToBoolean(turn);
            ResetBoard(false, true);
            ShowTurn();
            StartButton.IsEnabled = false;
            LoadButton.IsEnabled = false;
            SaveButton.IsEnabled = true;
            gameStarted = true;
        }

        private void SaveGame_Clicked(object sender, RoutedEventArgs e)
        {
            const string filename = "save.txt";

            var writer = new StreamWriter(filename);
            // Dong dau tien la luot di hien tai
            writer.WriteLine(isXTurn ? "X" : "O");

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

                var reader = new StreamReader(filename);
                var firstLine = reader.ReadLine();
                isXTurn = firstLine == "X";
                ShowTurn();
                ResetBoard(false, true);

                for (int i = 0; i < numOfRows; i++)
                {
                    var tokens = reader.ReadLine().Split(
                        new string[] { " " }, StringSplitOptions.None);
                    // Model

                    for (int j = 0; j < numOfCols; j++)
                    {
                        TTTBoard[i, j] = int.Parse(tokens[j]);

                        if (TTTBoard[i, j] == 0) continue;

                        TextBlock newText = new TextBlock();
                        if (TTTBoard[i,j] == 1)
                        {
                            newText.Text = "X";
                            newText.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                        }
                        else if (TTTBoard[i,j] == 2)
                        {
                            newText.Text = "O";
                            newText.Foreground = new SolidColorBrush(Colors.DarkRed);
                        }
                        newText.FontWeight = FontWeights.Bold;
                        TTTCells[i, j].Content = newText;
                        TTTCells[i, j].IsEnabled = false;
                    }
                }

                StartButton.IsEnabled = LoadButton.IsEnabled = false;
                SaveButton.IsEnabled = true;
                gameStarted = true;

                reader.Close();
                MessageBox.Show("Game is loaded");
            }
        }

        private void Cell_Clicked(object sender, RoutedEventArgs e)
        {
            if (!gameStarted) return;
            var buttonTag = (sender as Button).Tag as Tuple<int,int>;
            int cellRow = buttonTag.Item1;
            int cellCol = buttonTag.Item2;
            if(TTTBoard[cellRow, cellCol] == 0)
            {
                TextBlock newText = new TextBlock();
                if (isXTurn)
                {
                    newText.Text = "X";
                    newText.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                    TTTBoard[cellRow, cellCol] = 1;
                }
                else
                {
                    newText.Text = "O";
                    newText.Foreground = new SolidColorBrush(Colors.DarkRed);
                    TTTBoard[cellRow, cellCol] = 2;
                }
                newText.FontWeight = FontWeights.Bold;
                (sender as Button).Content = newText;
                (sender as Button).IsEnabled = false;

                //Checkwin
                int result = checkWin(cellRow, cellCol);
                if(result != GAME_NOT_OVER)
                {
                    StartButton.IsEnabled = true;
                    LoadButton.IsEnabled = true;
                    SaveButton.IsEnabled = false;
                    ResetBoard(true, false);
                    if (result == X_WIN) MessageBox.Show("X wins!");
                    if (result == O_WIN) MessageBox.Show("O wins!");
                    ResetBoard(true, true);
                    GameStatus.Child = gameHome;
                    return;
                }

                //Change to next Turn
                isXTurn = !isXTurn; // Model / State
                ShowTurn();
            }    

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

        private int checkWin(int row, int col)
        {
            //Vars for checking
            int count = 0;
            bool[] barricaded = { false, false};

            //Check horizontal
            int checkHorizontal = 0;
            for(int i = 0; i < numOfCols; i++)
            {
                if (TTTBoard[row, i] == 0 || TTTBoard[row, col] != TTTBoard[row, i])
                {
                    if(TTTBoard[row, i] != 0)
                    {
                        if (i < col) barricaded[0] = true;
                        if (i > col) barricaded[1] = true;
                    }
                    checkHorizontal = count > checkHorizontal ? count : checkHorizontal;
                    count = 0;
                }
                else
                    count++;
            }
            
            checkHorizontal = count > checkHorizontal ? count : checkHorizontal;
            if (checkHorizontal == 5 && (!barricaded[0]||!barricaded[1]) )
            {
                if (isXTurn) return X_WIN;
                else return O_WIN;
            }

            //Reset vars
            count = 0;
            barricaded = Array.ConvertAll(barricaded, x => false);

            //Check vertical
            int checkVertical = 0;
            for(int i = 0; i < numOfRows; i++)
            {
                if (TTTBoard[i, col] == 0 || TTTBoard[row, col] != TTTBoard[i, col])
                {
                    if (TTTBoard[i, col] != 0)
                    {
                        if (i < row) barricaded[0] = true;
                        if (i > row) barricaded[1] = true;
                    }
                    checkVertical = count > checkVertical ? count : checkVertical;
                    count = 0;
                }
                else
                    count++;
            }

            checkVertical = count > checkVertical ? count : checkVertical;
            if (checkVertical == 5 && (!barricaded[0] || !barricaded[1]))
            {
                if (isXTurn) return X_WIN;
                else return O_WIN;
            }

            //Reset vars
            count = 0;
            barricaded = Array.ConvertAll(barricaded, x => false);

            //Check main diagonal
            int checkDiagonal = 0;

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
                    if (TTTBoard[i, j] != 0)
                    {
                        if (i < row) barricaded[0] = true;
                        if (i > row) barricaded[1] = true;
                    }
                    checkDiagonal = count > checkDiagonal ? count : checkDiagonal;
                    count = 0;
                }
                else
                    count++;
            }

            checkDiagonal = count > checkDiagonal ? count : checkDiagonal;
            if (checkDiagonal == 5 && (!barricaded[0] || !barricaded[1]))
            {
                if (isXTurn) return X_WIN;
                else return O_WIN;
            }

            //Reset vars
            count = 0;
            barricaded = Array.ConvertAll(barricaded, x => false);

            //Check sub diagonal
            checkDiagonal = 0;

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
                    if (TTTBoard[i, j] != 0)
                    {
                        if (i < row) barricaded[0] = true;
                        if (i > row) barricaded[1] = true;
                    }
                    checkDiagonal = count > checkDiagonal ? count : checkDiagonal;
                    count = 0;
                }
                else
                    count++;
            }

            checkDiagonal = count > checkDiagonal ? count : checkDiagonal;
            if (checkDiagonal == 5 && (!barricaded[0] || !barricaded[1]))
            {
                if (isXTurn) return X_WIN;
                else return O_WIN;
            }

            return GAME_NOT_OVER;
        }
    }
}
