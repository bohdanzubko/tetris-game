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

        }

        public void RotateCW()
        {

        }

        public void RotateCCW()
        {

        }

        public void Move(int rows, int columns)
        {

        }

        public void Reset()
        {

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
