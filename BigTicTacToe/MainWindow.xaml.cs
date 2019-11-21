using Microsoft.Win32;
using System;
using System.IO;
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

        //Gameplay vars
        bool gameStarted = false;
        int[,] TTTBoard;
        bool isXTurn = true;

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

                StartButton.IsEnabled = LoadButton.IsEnabled = false;
                SaveButton.IsEnabled = true;
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

            if (prevPos != -1)
            {
                var item = (GameBoard.Children[prevPos] as Border).Tag as Tuple<int, int>;
                int prevCellRow = item.Item1;
                int prevCellCol = item.Item2;
                if (pos.X >= offsetX + prevCellCol * CellWidth && pos.X < offsetX + (prevCellCol + 1) * CellWidth
                    && pos.Y >= offsetY + prevCellRow * CellHeight && pos.Y < offsetY + (prevCellRow + 1) * CellHeight)
                    return;
                GameBoard.Children.RemoveAt(prevPos);
                prevPos = -1;
            }

            if (pos.X < offsetX || pos.X >= GameBoard.ActualWidth - offsetX) return;
            if (pos.Y < offsetY || pos.Y >= GameBoard.ActualHeight - offsetY) return;

            int col = (int)((pos.X - offsetX) / CellWidth);
            int row = (int)((pos.Y - offsetY) / CellHeight);
            choiceBox.Tag = new Tuple<int, int>(row, col);

            if (TTTBoard[row, col] != 0) return;
            prevPos = GameBoard.Children.Count;
            GameBoard.Children.Add(choiceBox);
            Canvas.SetLeft(choiceBox, offsetX + CellWidth * col);
            Canvas.SetTop(choiceBox, offsetY + CellHeight * row);
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

                //Checkwin
                int result = checkWin(row, col);
                if (result != GAME_NOT_OVER)
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
                isXTurn = !isXTurn; 
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
                    if (count == 5 && TTTBoard[row, i] != 0)
                    {
                        barricaded[1] = true;
                        if (i - 5 - 1 >= 0 && TTTBoard[row, i - 5 - 1] == TTTBoard[row, i]) barricaded[0] = true;
                        if (barricaded[0] && barricaded[1]) count = 0;
                        barricaded = Array.ConvertAll(barricaded, x => false);
                    }
                    checkHorizontal = checkHorizontal < 5 ? count > checkHorizontal ? count : checkHorizontal :
                        count == 5 ? count : checkHorizontal;
                    count = 0;
                }
                else
                {
                    count++;
                }
            }

            checkHorizontal = checkHorizontal < 5 ? count > checkHorizontal ? count : checkHorizontal :
                        count == 5 ? count : checkHorizontal;
            if (checkHorizontal == 5)
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
                    if (count == 5 && TTTBoard[i, col] != 0)
                    {
                        barricaded[1] = true;
                        if (i - 5 - 1 >= 0 && TTTBoard[i - 5 - 1, col] == TTTBoard[i, col]) barricaded[0] = true;
                        if (barricaded[0] && barricaded[1]) count = 0;
                        barricaded = Array.ConvertAll(barricaded, x => false);
                    }
                    checkVertical = checkVertical < 5 ? count > checkVertical ? count : checkVertical :
                       count == 5 ? count : checkVertical;
                    count = 0;
                }
                else
                    count++;
            }

            checkVertical = checkVertical < 5 ? count > checkVertical ? count : checkVertical :
                       count == 5 ? count : checkVertical;
            if (checkVertical == 5)
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
                    if (count == 5 && TTTBoard[i, j] != 0)
                    {
                        barricaded[1] = true;
                        if (i - 5 - 1 >= 0 && j - 5 - 1 >= 0 
                            && TTTBoard[i - 5 - 1, j - 5 - 1] == TTTBoard[i, j]) barricaded[0] = true;
                        if (barricaded[0] && barricaded[1]) count = 0;
                        barricaded = Array.ConvertAll(barricaded, x => false);
                    }
                    checkDiagonal = checkDiagonal < 5 ? count > checkDiagonal ? count : checkDiagonal :
                       count == 5 ? count : checkDiagonal;
                    count = 0;
                }
                else
                    count++;
            }

            checkDiagonal = checkDiagonal < 5 ? count > checkDiagonal ? count : checkDiagonal :
                       count == 5 ? count : checkDiagonal;
            if (checkDiagonal == 5)
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
                    if (count == 5 && TTTBoard[i, j] != 0)
                    {
                        barricaded[1] = true;
                        if (i + 5 + 1 <= numOfRows - 1 && j - 5 - 1 >= 0
                            && TTTBoard[i + 5 + 1, j - 5 - 1] == TTTBoard[i, j]) barricaded[0] = true;
                        if (barricaded[0] && barricaded[1]) count = 0;
                        barricaded = Array.ConvertAll(barricaded, x => false);
                    }
                    checkDiagonal = checkDiagonal < 5 ? count > checkDiagonal ? count : checkDiagonal :
                       count == 5 ? count : checkDiagonal;
                    count = 0;
                }
                else
                    count++;
            }

            checkDiagonal = checkDiagonal < 5 ? count > checkDiagonal ? count : checkDiagonal :
                       count == 5 ? count : checkDiagonal;
            if (checkDiagonal == 5 && (!barricaded[0] || !barricaded[1]))
            {
                if (isXTurn) return X_WIN;
                else return O_WIN;
            }

            return GAME_NOT_OVER;
        }
    }
}
