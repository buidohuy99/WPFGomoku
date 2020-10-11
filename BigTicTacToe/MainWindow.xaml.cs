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
        const int CellWidth = 30;
        const int CellHeight = 30;

        //Logic
        GameManager gameManager;

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

            gameManager = new GameManager(numOfRows, numOfCols);

            //Populate Canvas
            for(int i = 0; i < numOfRows; i++)
            {
                for(int j = 0; j < numOfCols; j++)
                {
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
                    GameBoard.Children.RemoveAt(i);
                }
                gameManager.resetBoard();
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
            }
            else
            {
                newText.Text = "O";
                newText.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
            newText.FontWeight = FontWeights.Bold;
            setChoiceBlockText(block, newText);
            block.Opacity = 0.75;
            block.Tag = new Tuple<int, int>(move.row, move.col);
            GameBoard.Children.Add(block);
            Canvas.SetLeft(block, offsetX + CellWidth * move.col);
            Canvas.SetTop(block, offsetY + CellHeight * move.row);
        }

        private void StartGame_Clicked(object sender, RoutedEventArgs e)
        {
            gameManager.restartGame();
            ResetBoard(false, true);
            ShowTurn();
            StartButton.IsEnabled = false;
            LoadButton.IsEnabled = false;
            SaveButton.IsEnabled = true;
            UndoButton.IsEnabled = false;
            RedoButton.IsEnabled = false;
        }

        private void SaveGame_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog screen = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = screen.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                gameManager.saveGame(screen.SelectedPath, (GameManager.SAVE_OUTCOME outcome, Action saveCallback) =>
                {
                    switch (outcome)
                    {
                        case GameManager.SAVE_OUTCOME.FINE:
                            MessageBox.Show("Saved the game!");
                            break;
                        case GameManager.SAVE_OUTCOME.FILE_AVAILABLE:
                            MessageBoxResult confirm = MessageBox.Show("Do you want to override the save file in this location?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                saveCallback?.Invoke();
                            }
                            break;
                        case GameManager.SAVE_OUTCOME.FOLDER_NOTAVAILABLE:
                            MessageBox.Show("Directory not available", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                    }
                });
            }
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

                gameManager.loadGame(filename, (GameManager.LOAD_OUTCOME outcome, int[,] boardState) => {
                    switch (outcome) {
                        case GameManager.LOAD_OUTCOME.FINE:
                            if (boardState != null) {
                                for (int i = 0; i < gameManager.numOfRows; i++)
                                {
                                    for (int j = 0; j < gameManager.numOfCols; j++)
                                    {
                                        if (boardState[i, j] == 0) continue;

                                        TextBlock newText = new TextBlock();
                                        if (boardState[i, j] == 1)
                                        {
                                            newText.Text = "X";
                                            newText.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                                        }
                                        else if (boardState[i, j] == 2)
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
                            }
                            StartButton.IsEnabled = LoadButton.IsEnabled = false;
                            SaveButton.IsEnabled = true;
                            UndoButton.IsEnabled = false;
                            RedoButton.IsEnabled = false;
                            ResetBoard(false, false);
                            ShowTurn();
                            MessageBox.Show("Game is loaded");
                            break;
                        case GameManager.LOAD_OUTCOME.FILE_NOTAVAILABLE:
                            MessageBox.Show("File is not available", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }  
                });
            }
        }

        private void GameBoard_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!gameManager.gameStarted) return;

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
            if (gameManager.getMoveAt(row, col) != 0) return;

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
            if (!gameManager.gameStarted) return;

            //Get position relative to gameboard
            var pos = e.GetPosition(GameBoard);

            if (pos.X < offsetX || pos.X >= GameBoard.ActualWidth - offsetX) return;
            if (pos.Y < offsetY || pos.Y >= GameBoard.ActualHeight - offsetY) return;

            int col = (int)((pos.X - offsetX) / CellWidth);
            int row = (int)((pos.Y - offsetY) / CellHeight);

            if(gameManager.getMoveAt(row, col) == 0)
            {
                //Place block down
                TextBlock newText = new TextBlock();
                if (gameManager.isXTurn)
                {
                    newText.Text = "X";
                    newText.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                }
                else
                {
                    newText.Text = "O";
                    newText.Foreground = new SolidColorBrush(Colors.DarkRed);
                }
                newText.FontWeight = FontWeights.Bold;
                setChoiceBlockText(choiceBox, newText);
                choiceBox.Opacity = 0.75;
                prevPos = -1;
                choiceBox = createChoiceBlock();

                UndoButton.IsEnabled = true;
                // Check, if next turn is overriding the history of moves
                if (gameManager.currentState + 1 < gameManager.getHistoryCount()) { 
                    RedoButton.IsEnabled = false;
                }
                //Move and change to next turn
                gameManager.moveAt(new Move(row, col, gameManager.isXTurn));

                //Checkwin of the move above
                Tuple<GameManager.GAME_STATE, List<Move>> result = gameManager.checkWin(row, col);
                if (result.Item1 != GameManager.GAME_STATE.GAME_NOT_OVER)
                {
                    StartButton.IsEnabled = true;
                    LoadButton.IsEnabled = true;
                    SaveButton.IsEnabled = false;
                    ResetBoard(true, false);
                    if (result.Item2 != null)
                    {
                        colorCells(result.Item2, new SolidColorBrush(Color.FromRgb(169, 255, 99)));
                    }
                    if (result.Item1 == GameManager.GAME_STATE.X_WIN) MessageBox.Show("X wins!");
                    if (result.Item1 == GameManager.GAME_STATE.O_WIN) MessageBox.Show("O wins!");
                    if (result.Item1 == GameManager.GAME_STATE.OUT_OF_CELLS) MessageBox.Show("Game is a tie!");
                    ResetBoard(true, true);
                    UndoButton.IsEnabled = false;
                    RedoButton.IsEnabled = false;
                    GameStatus.Child = gameHome;
                    return;
                }

                //Show visual representation of next Turn
                ShowTurn();
            }
           
        }

        private void UndoMove_Clicked(object sender, RoutedEventArgs e)
        {
            if (gameManager.currentState == -1) {
                return;
            }
            //Remove the current move
            GameBoard.Children.RemoveAt(GameBoard.Children.Count - 1);
            gameManager.undoMove();
            ShowTurn();
            if (gameManager.currentState < 0) UndoButton.IsEnabled = false;
            if (gameManager.currentState < gameManager.getHistoryCount() - 1) RedoButton.IsEnabled = true;          
        }

        private void RedoMove_Clicked(object sender, RoutedEventArgs e)
        {
            if(gameManager.currentState >= gameManager.getHistoryCount() - 1)
            {
                return;
            }
            gameManager.redoMove();
            if (gameManager.currentState >= gameManager.getHistoryCount() - 1) RedoButton.IsEnabled = false;
            if (gameManager.currentState >= 0) UndoButton.IsEnabled = true;
            createTextBlockAt(gameManager.getMoveOfCurrentState());   
            ShowTurn();
        }

        private void ShowTurn()
        {
            string nextTurn;
            if (gameManager.isXTurn)
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
    }
}
