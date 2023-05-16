using System;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Tetris
{
    [Serializable]
    public class TetrisSettings
    {
        [XmlAttribute]
        public Key MoveDownKey { get; set; }
        [XmlAttribute]
        public Key MoveLeftKey { get; set; }
        [XmlAttribute]
        public Key MoveRightKey { get; set; }
        [XmlAttribute]
        public Key DropBlockKey { get; set; }
        [XmlAttribute]
        public Key RotateCWKey { get; set; }
        [XmlAttribute]
        public Key RotateCCWKey { get; set; }

        public TetrisSettings(Key moveDownKey, Key moveLeftKey, Key moveRightKey, Key dropBlockKey, Key rotateCWKey, Key rotateCCWKey)
        {
            MoveDownKey = moveDownKey;
            MoveLeftKey = moveLeftKey;
            MoveRightKey = moveRightKey;
            DropBlockKey = dropBlockKey;
            RotateCWKey = rotateCWKey;
            RotateCCWKey = rotateCCWKey;
        }

        public TetrisSettings() { }
    }
}
