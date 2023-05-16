using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative))
        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative))
        };

        private readonly Image[,] imageControls;
        private TetrisSettings settings = new TetrisSettings();
        private TetrisSettings tempSettings = new TetrisSettings();
        private XmlSerializer serializer = new XmlSerializer(typeof(TetrisSettings));
        private int maxDelay;
        private int minDelay;
        private int delayDecrease;
        private bool gamePaused = false;
        private int activeSetting = 0;

        private GameState gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
        }

        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    Image imageControl = new Image
                    {
                        Width = cellSize,
                        Height = cellSize
                    };

                    Canvas.SetTop(imageControl, (r - 2) * cellSize);
                    Canvas.SetLeft(imageControl, c * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }

        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;
                    imageControls[r, c].Source = tileImages[id];
                }
            }
        }

        private void DrawBlock(Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1;
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }

        private void DrawGhostBlock(Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock);
            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            ScoreText.Text = $"Score: {gameState.Score}";
        }

        private async Task GameLoop()
        {
            Draw(gameState);

            while (!gameState.GameOver)
            {
                int delay = Math.Max(minDelay, maxDelay - ((int)gameState.Score / 100 * delayDecrease));
                await Task.Delay(delay);
                if (!gamePaused)
                {
                    gameState.MoveBlockDown();
                    Draw(gameState);
                }
            }

            GameOverMenu.Visibility = Visibility.Visible;
            SettingsButton.Visibility = Visibility.Visible;
            FinalScoreText.Text = $"Score: {gameState.Score}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver || gamePaused)
            {
                if (e.Key != Key.P && e.Key != Key.Escape)
                    return;
            }

            if (e.Key == settings.MoveDownKey)
                gameState.MoveBlockDown();
            else if (e.Key == settings.MoveLeftKey)
                gameState.MoveBlockLeft();
            else if (e.Key == settings.MoveRightKey)
                gameState.MoveBlockRight();
            else if (e.Key == settings.DropBlockKey)
                gameState.DropBlock();
            else if (e.Key == settings.RotateCWKey)
                gameState.RotateBlockCW();
            else if (e.Key == settings.RotateCCWKey)
                gameState.RotateBlockCCW();
            else if (e.Key == Key.P)
                PauseGame();
            else if (e.Key == Key.Escape)
            {
                activeSetting = 0;
                KeyDown -= KeyScanWindow_KeyDown;
                SettingsMenu.Visibility = Visibility.Hidden;
                DifficultyMenu.Visibility = Visibility.Hidden;
                SettingsMenu.Visibility = Visibility.Hidden;
                SettingsButton.Visibility = Visibility.Visible;
                PlayButton.IsEnabled = true;
            }
            else return;

            Draw(gameState);
        }

        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DisableButtonNavigation();

            try
            {
                using (TextReader reader = new StreamReader("TetrisSettings.xml"))
                {
                    settings = (TetrisSettings)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                settings = new TetrisSettings(Key.Down, Key.Left, Key.Right, Key.Space, Key.Up, Key.C);

                using (TextWriter writer = new StreamWriter("TetrisSettings.xml"))
                {
                    serializer.Serialize(writer, settings);
                }
            }
        }

        private void DisableButtonNavigation()
        {
            Button[] allButtons = { MoveDownButton, MoveLeftButton, MoveRightButton, DropBlockButton, RotateCWButton, RotateCCWButton,
                                    StartGameButton, SettingsButton, SaveAndQuitButton, ResetSettingsButton };

            foreach (var button in allButtons)
            {
                KeyboardNavigation.SetDirectionalNavigation(button, KeyboardNavigationMode.None);
                KeyboardNavigation.SetIsTabStop(button, false);
            }
        }

        private async void StartGame()
        {
            gameState = new GameState();
            DifficultyMenu.Visibility = Visibility.Hidden;
            StartMenu.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;
            SettingsButton.Visibility = Visibility.Hidden;
            gamePaused = false;
            PauseMenu.Visibility = Visibility.Hidden;
            PlayButton.Visibility = Visibility.Hidden;
            await GameLoop();
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            DifficultyMenu.Visibility = Visibility.Visible;
            SettingsButton.Visibility = Visibility.Hidden;
        }

        private void PauseGame()
        {
            if (gamePaused)
            {
                PlayButton.Focus();
                gamePaused = false;
                PlayButton.Visibility = Visibility.Hidden;
                PauseMenu.Visibility = Visibility.Hidden;
                SettingsButton.Visibility = Visibility.Hidden;
            }
            else
            {
                gamePaused = true;
                PlayButton.Visibility = Visibility.Visible;
                PauseMenu.Visibility = Visibility.Visible;
                SettingsButton.Visibility = Visibility.Visible;
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            DifficultyMenu.Visibility = Visibility.Visible;
            SettingsButton.Visibility = Visibility.Hidden;
        }

        private void EasyDifficulty_Click(object sender, RoutedEventArgs e)
        {
            maxDelay = 1500;
            minDelay = 300;
            delayDecrease = 12;
            StartGame();
        }

        private void MediumDifficulty_Click(object sender, RoutedEventArgs e)
        {
            maxDelay = 1300;
            minDelay = 200;
            delayDecrease = 15;
            StartGame();
        }

        private void HardDifficulty_Click(object sender, RoutedEventArgs e)
        {
            maxDelay = 1000;
            minDelay = 100;
            delayDecrease = 17;
            StartGame();
        }

        private void EndGame_Click(object sender, RoutedEventArgs e)
        {
            StartMenu.Visibility = Visibility.Visible;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsMenu.Visibility == Visibility.Visible)
            {
                KeyDown -= KeyScanWindow_KeyDown;
                SetSettings(settings);
                SettingsMenu.Visibility = Visibility.Hidden;
                PlayButton.IsEnabled = true;
            }
            else
            {
                SetSettings(settings);
                tempSettings = settings;
                PlayButton.IsEnabled = false;
                SettingsMenu.Visibility = Visibility.Visible;
            }
        }

        private void KeyScanWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z && e.Key != Key.P)
            {
                UpdateKeySetting(e.Key);

                switch (activeSetting)
                {
                    case 1:
                        MoveDownLabel.Text = "Move down: " + e.Key.ToString();
                        MoveDownButton.Content = "Set key";
                        break;
                    case 2:
                        MoveLeftLabel.Text = "Move left: " + e.Key.ToString();
                        MoveLeftButton.Content = "Set key";
                        break;
                    case 3:
                        MoveRightLabel.Text = "Move right: " + e.Key.ToString();
                        MoveRightButton.Content = "Set key";
                        break;
                    case 4:
                        DropBlockLabel.Text = "Drop block: " + e.Key.ToString();
                        DropBlockButton.Content = "Set key";
                        break;
                    case 5:
                        RotateCWLabel.Text = "Rotate CW: " + e.Key.ToString();
                        RotateCWButton.Content = "Set key";
                        break;
                    case 6:
                        RotateCCWLabel.Text = "Rotate CCW: " + e.Key.ToString();
                        RotateCCWButton.Content = "Set key";
                        break;
                    default: break;
                }
            }

            activeSetting = 0;
            KeyDown -= KeyScanWindow_KeyDown;
        }

        private void UpdateKeySetting(Key selectedKey)
        {
            switch (activeSetting)
            {
                case 1: tempSettings.MoveDownKey = selectedKey; break;
                case 2: tempSettings.MoveLeftKey = selectedKey; break;
                case 3: tempSettings.MoveRightKey = selectedKey; break;
                case 4: tempSettings.DropBlockKey = selectedKey; break;
                case 5: tempSettings.RotateCWKey = selectedKey; break;
                case 6: tempSettings.RotateCCWKey = selectedKey; break;
                default: break;
            }
        }

        private void SetSettings(TetrisSettings setSettings)
        {
            activeSetting = 0;

            MoveDownLabel.Text = "Move down: " + setSettings.MoveDownKey.ToString();
            MoveLeftLabel.Text = "Move left: " + setSettings.MoveLeftKey.ToString();
            MoveRightLabel.Text = "Move right: " + setSettings.MoveRightKey.ToString();
            DropBlockLabel.Text = "Drop block: " + setSettings.DropBlockKey.ToString();
            RotateCWLabel.Text = "Rotate CW: " + setSettings.RotateCWKey.ToString();
            RotateCCWLabel.Text = "Rotate CCW: " + setSettings.RotateCCWKey.ToString();

            MoveLeftButton.Content = "Set key";
            MoveDownButton.Content = "Set key";
            RotateCCWButton.Content = "Set key";
            MoveRightButton.Content = "Set key";
            DropBlockButton.Content = "Set key";
            RotateCWButton.Content = "Set key";
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            KeyDown -= KeyScanWindow_KeyDown;
            settings = tempSettings;
            SetSettings(tempSettings);
            PlayButton.IsEnabled = true;
            using (TextWriter writer = new StreamWriter("TetrisSettings.xml"))
            {
                serializer.Serialize(writer, settings);
            }
            SettingsMenu.Visibility = Visibility.Hidden;
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            tempSettings = new TetrisSettings(Key.Down, Key.Left, Key.Right, Key.Space, Key.Up, Key.C);
            SetSettings(tempSettings);
        }

        private void MoveDownKey_Click(object sender, RoutedEventArgs e)
        {
            if (activeSetting == 0)
            {
                activeSetting = 1;
                KeyDown += KeyScanWindow_KeyDown;
                MoveDownButton.Content = "Press any key";
            }
        }

        private void MoveLeftKey_Click(object sender, RoutedEventArgs e)
        {
            if (activeSetting == 0)
            {
                activeSetting = 2;
                KeyDown += KeyScanWindow_KeyDown;
                MoveLeftButton.Content = "Press any key";
            }
        }

        private void MoveRightKey_Click(object sender, RoutedEventArgs e)
        {
            if (activeSetting == 0)
            {
                activeSetting = 3;
                KeyDown += KeyScanWindow_KeyDown;
                MoveRightButton.Content = "Press any key";
            }
        }

        private void DropBlockKey_Click(object sender, RoutedEventArgs e)
        {
            if (activeSetting == 0)
            {
                activeSetting = 4;
                KeyDown += KeyScanWindow_KeyDown;
                DropBlockButton.Content = "Press any key";
            }
        }

        private void RotateCWKey_Click(object sender, RoutedEventArgs e)
        {
            if (activeSetting == 0)
            {
                activeSetting = 5;
                KeyDown += KeyScanWindow_KeyDown;
                RotateCWButton.Content = "Press any key";
            }
        }

        private void RotateCCWKey_Click(object sender, RoutedEventArgs e)
        {
            if (activeSetting == 0)
            {
                activeSetting = 6;
                KeyDown += KeyScanWindow_KeyDown;
                RotateCCWButton.Content = "Press any key";
            }
        }
    }
}
