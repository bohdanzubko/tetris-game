namespace Tetris
{
    public class GameState
    {
        private Block currentBlock;

        public Block CurrentBlock
        {

        }

        public GameGrid GameGrid { get; }
        public BlockQueue BlockQueue { get; }
        public bool GameOver { get; private set; }
        public int Score { get; private set; }

        public GameState()
        {

        }

        private bool BlockFits()
        {

        }

        public void RotateBlockCW()
        {

        }

        public void RotateBlockCCW()
        {

        }

        public void MoveBlockLeft()
        {

        }

        public void MoveBlockRight()
        {

        }

        private bool IsGameOver()
        {
            
        }

        private void PlaceBlock()
        {

        }

        public void MoveBlockDown()
        {
        }
    }
}