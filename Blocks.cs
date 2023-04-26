using System.Collections.Generic;

namespace TetrisGame
{
   public abstract class Block
    {
        protected abstract Position[][] Tiles { get; }
        protected abstract Position StartOffset { get; }
        public abstract int Id { get; }

        private int rotationState;
        private Position offset;

        public Block()
        {
            offset = new Position(StartOffset.Row, StartOffset.Column);
        }

        public void RotateCW()
        {
            rotationState = (rotationState + 1) % Tiles.Length;
        }

        public void RotateCCW()
        {
            if (rotationState == 0)
            {
                rotationState = Tiles.Length - 1;
            }
            else
            {
                rotationState--;
            }
        }

        public void Move(int rows, int columns)
        {
            offset.Row += rows;
            offset.Column += columns;
        }

        public void Reset()
        {
            rotationState = 0;
            offset.Row = StartOffset.Row;
            offset.Column = StartOffset.Column;
        }
    }

    public class IBlock : Block
    {

    }

    public class JBlock : Block
    {

    }

    public class LBlock : Block
    {

    }

    public class OBlock : Block
    {

    }

    public class SBlock : Block
    {

    }

    public class TBlock : Block
    {

    }

    public class ZBlock : Block
    {

    }
}
