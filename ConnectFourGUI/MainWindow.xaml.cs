using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Subliminal;
using System.IO;
using Microsoft.Win32; // SaveFileDialog

namespace ConnectFourGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Standard: 7 columns, 6 rows
        private const int NUM_COLUMNS = 7;
        private const int NUM_ROWS = 6;
        private BoardState GameBoard;
        private int LookAhead = 5;

        private SolidColorBrush Color_Background;
        private SolidColorBrush Color_Player1;
        private SolidColorBrush Color_Player2;
        private SolidColorBrush Color_Empty;

        public MainWindow()
        {
            InitializeComponent();

            // Sets brush colors
            Color_Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD1D136")); // Yellow
            Color_Empty = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF")); // White
            Color_Player1 = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0000")); // Red
            Color_Player2 = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0000FF")); // Blue
            
            GameBoard = new BoardState(NUM_ROWS, NUM_COLUMNS);
            GameBoard.PlayerMoved += GameBoard_PlayerMoved;

            Position p1 = new Position(0, 0);
            Position p2 = new Position();
            Position p3 = new Position(2, 5);
            Position p4 = new Position(7, 6);
            p3.Row = 7;
            p3.Column = 6;

            Label_Difficulty.Content = LookAhead.ToString();
            Slider_Difficulty.Value = LookAhead;
        }

        private void GameBoard_PlayerMoved(Position pos, bool player)
        {
            // Updates game board
            Ellipse disc = GetDisc(pos);
            if (disc == null) return;
            
            // Sets color and removes events
            SetColor(disc, (player ? 2 : 1));
            RemoveEvents(disc);
            
            if (GameBoard.Player1Solutions > 0)
            {
                // Player 1 wins!
                MessageBox.Show("Player 1 wins!");

                foreach (var child in Grid_GameBoard.Children)
                {
                    // Removes events from all discs
                    Ellipse remove = child as Ellipse;
                    RemoveEvents(remove);
                }

                return;
            }
            else if (GameBoard.Player2Soltuions > 0)
            {
                // Player 2 wins!
                MessageBox.Show("Computer wins!");

                foreach (var child in Grid_GameBoard.Children)
                {
                    // Removes events from all discs
                    Ellipse remove = child as Ellipse;
                    RemoveEvents(remove);
                }

                return;
            }

            if (!player) // Computer's turn
                GameBoard.CommitMove(ComRoutines.FindBestMove(GameBoard, LookAhead, GameBoard.PlayerTurn), GameBoard.PlayerTurn);
            //else GameBoard.CommitMove(ComRoutines.FindBestMove(GameBoard, LookAhead, GameBoard.PlayerTurn), GameBoard.PlayerTurn);
        }

        private Ellipse GetDisc(Position pos)
        {
            if (pos == null) return null;

            // Calculates child index
            int index = GameBoard.ColumnSize * pos.Row + pos.Column;
            if (index >= Grid_GameBoard.Children.Count) return null;

            return Grid_GameBoard.Children[index] as Ellipse;
        }

        private void Grid_GameBoard_Loaded(object sender, RoutedEventArgs e)
        {
            // Resets game board
            GameBoard.Reset();

            // Resets columns + children
            Grid_GameBoard.RowDefinitions.Clear();
            Grid_GameBoard.ColumnDefinitions.Clear();
            Grid_GameBoard.Children.Clear();

            // Adds discs to game board
            for (int row = 0; row < NUM_ROWS; row++)
            {
                // Sets row defintion
                Grid_GameBoard.RowDefinitions.Add(new RowDefinition());

                for (int col = 0; col < NUM_COLUMNS; col++)
                {
                    if (row == 0)
                        Grid_GameBoard.ColumnDefinitions.Add(new ColumnDefinition()); // Sets column defintion

                    // Creates disc object
                    Ellipse disc = Grid_GameBoard.Resources["Disc"] as Ellipse;
                    disc.Fill = Color_Empty; // Sets color to empty
                    disc.Tag = new Position(row, col);

                    // Defines row + col
                    Grid.SetRow(disc, row);
                    Grid.SetColumn(disc, col);
                    
                    // Adds events
                    disc.MouseEnter += Disc_MouseEnter;
                    disc.MouseLeave += Disc_MouseLeave;
                    disc.MouseLeftButtonUp += Disc_MouseLeftButtonUp;

                    // Adds disc object to game board
                    Grid_GameBoard.Children.Add(disc);
                }
            }

            // Sets context menu to game board
            Grid_GameBoard.ContextMenu = Grid_GameBoard.Resources["ContextMenu_Options"] as ContextMenu;

            // Debug!
            //DebugStates.ImpossibleWin_Before(GameBoard);
        }

        private void Disc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Ellipse)) return;
            
            Ellipse disc = sender as Ellipse;
            Position pos = disc.Tag as Position;

            GameBoard.CommitMove(pos.Column, false);
        }

        private void SetColor(Ellipse disc, int color)
        {
            if (disc == null) return;

            switch(color)
            {
                default:
                    disc.Fill = Color_Empty;
                    break;
                case 1:
                    disc.Fill = Color_Player1;
                    break;
                case 2:
                    disc.Fill = Color_Player2;
                    break;
            }
        }

        private void RemoveEvents(Ellipse disc)
        {
            if (disc == null) return;

            // Removes events
            disc.MouseEnter -= Disc_MouseEnter;
            disc.MouseLeave -= Disc_MouseLeave;
            disc.MouseLeftButtonUp -= Disc_MouseLeftButtonUp;
        }

        private void Disc_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!(sender is Ellipse)) return;

            // Sets to empty color
            SetColor(sender as Ellipse, 0);
        }

        private void Disc_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!(sender is Ellipse)) return;

            // Sets to player color
            SetColor(sender as Ellipse, 1);
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            Grid_GameBoard_Loaded(sender, e);
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Slider_Difficulty_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Updates look ahead value
            LookAhead = (int)Math.Round(Slider_Difficulty.Value);

            Label_Difficulty.Content = LookAhead.ToString();
            Slider_Difficulty.Value = LookAhead;
        }

        private void SaveToBmp(FrameworkElement visual, string fileName)
        {
            var encoder = new BmpBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        private void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        private void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        SaveFileDialog sfd = new SaveFileDialog();
        private void MenuItem_ExportImage_Click(object sender, RoutedEventArgs e)
        {
            sfd.Title = "Export Image";
            sfd.Filter = "PNG|*.png";
            sfd.FileName = "image.png";

            if (sfd.ShowDialog() == false) return;

            try
            {
                SaveToPng(Grid_GameBoard, sfd.FileName);
                MessageBox.Show("Image exported successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error exporting image.");
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.R:
                    // Resets board
                    Grid_GameBoard_Loaded(sender, e);
                    break;
                case Key.X:
                case Key.Q:
                case Key.Escape:
                    // Quits application
                    this.Close();
                    break;
            }
        }
    }
}
