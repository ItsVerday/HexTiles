using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using DigitalRuby.Tween;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance = null;

    public static void save()
    {
        SaveManager.saveBoard(Board.instance, Manager.instance.getBoardSaveName());
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static void saveBoard(Board board, string filename)
    {
        Debug.Log("Saving");
        string path = Application.persistentDataPath + "/" + filename;

        SerializableBoard serializedBoard = serializeBoard(board);
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, serializedBoard);
        stream.Close();
    }
    
    public static bool loadBoard(Board board, string filename)
    {
        string path = Application.persistentDataPath + "/" + filename;
        if (!File.Exists(path))
        {
            return false;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        SerializableBoard serializedBoard = (SerializableBoard) formatter.Deserialize(stream);
        stream.Close();

        return deserializeBoard(serializedBoard, board);
    }

    public static SerializablePieceType serializePieceType(Manager.PieceType pieceType)
    {
        if (pieceType == null)
        {
            return new SerializablePieceType("none", 0);
        }

        string name = pieceType.getTypeName();

        switch (name)
        {
            case "standard":
            case "bomb":
                {
                    SerializablePieceType serializablePieceType = new SerializablePieceType(name, 1);
                    serializablePieceType.values[0] = ((Manager.NumberedPieceType) pieceType).number.ToString();
                    return serializablePieceType;
                }
            default:
                {
                    return new SerializablePieceType(name, 0);
                }
        }
    }

    public static Manager.PieceType deserializePieceType(SerializablePieceType pieceType)
    {
        switch (pieceType.type)
        {
            case "standard":
                return new Manager.StandardPieceType(int.Parse(pieceType.values[0]));
            case "wall":
                return new Manager.WallPieceType();
            case "bomb":
                return new Manager.BombPieceType(int.Parse(pieceType.values[0]));
            case "incrementer":
                return new Manager.IncrementerPieceType();
        }

        return null;
    }

    [System.Serializable]
    public class SerializablePieceType
    {
        public string type;
        public string[] values;

        public SerializablePieceType(string type, int length)
        {
            this.type = type;
            values = new string[length];
        }
    }

    public static SerializableBoard serializeBoard(Board board)
    {
        int index = 0;

        SerializableBoard serializableBoard = new SerializableBoard(board.tiles.Count);
        foreach (GameObject tile in board.tiles)
        {
            Tile tileComponent = tile.GetComponent<Tile>();
            GameObject piece = tileComponent.piece;

            Manager.PieceType pieceType = null;
            if (piece != null)
            {
                Piece pieceComponent = tileComponent.piece.GetComponent<Piece>();
                pieceType = pieceComponent.pieceType;
            }

            serializableBoard.pieces[index++] = serializePieceType(pieceType);
        }

        serializableBoard.score = board.score;
        serializableBoard.highestTile = board.highest;
        serializableBoard.spawningScore = board.spawningScore;
        serializableBoard.combo = board.comboManager.combo;
        serializableBoard.comboTime = board.comboManager.comboTime;
        serializableBoard.canMakeMove = board.canMakeMove();

        return serializableBoard;
    }

    public static bool deserializeBoard(SerializableBoard serializableBoard, Board board)
    {
        if (!serializableBoard.canMakeMove)
        {
            return false;
        }

        int index = 0;
        foreach (SerializablePieceType serializablePieceType in serializableBoard.pieces)
        {
            Manager.PieceType pieceType = deserializePieceType(serializablePieceType);

            GameObject tile = board.tiles[index++];
            Tile tileComponent = tile.GetComponent<Tile>();
            GameObject oldPiece = tileComponent.piece;
            tileComponent.unsetPiece();
            
            if (oldPiece != null)
            {
                Destroy(oldPiece);
            }

            if (pieceType != null)
            {
                GameObject piece = Manager.instance.createPiece(pieceType);
                piece.transform.localPosition = tile.transform.position;
                tileComponent.setPiece(piece);
                Piece pieceComponent = piece.GetComponent<Piece>();
                pieceComponent.scaleIn.Stop(TweenStopBehavior.Complete);
            }
        }

        board.score = serializableBoard.score;
        board.highest = serializableBoard.highestTile;
        board.spawningScore = serializableBoard.spawningScore;
        board.comboManager.combo = serializableBoard.combo;
        board.comboManager.comboTime = serializableBoard.comboTime;

        board.displayScore = board.score;
        Color na = new Color();
        Manager.instance.getPieceColors(board.highest, out board.scoreColor, out na);
        board.comboManager.scale = board.comboManager.combo > 0 ? 1 : 0;
        board.comboManager.displayComboTime = serializableBoard.comboTime;
        board.comboManager.comboColor = Manager.instance.getComboColor(Mathf.Max(board.comboManager.combo, 1));

        return true;
    }

    [System.Serializable]
    public class SerializableBoard
    {
        public SerializablePieceType[] pieces;
        public long score;
        public int highestTile;
        public float spawningScore;
        public int combo;
        public float comboTime;
        public bool canMakeMove;

        public SerializableBoard(int tileCount)
        {
            pieces = new SerializablePieceType[tileCount];
        }
    }
}