using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Manager : MonoBehaviour
{
    public static Manager instance = null;
    public GameObject tilePrefab;
    public GameObject piecePrefab;

    public GameObject standardPiecePrefab;
    public GameObject wallPiecePrefab;
    public GameObject bombPiecePrefab;
    public GameObject incrementerPiecePrefab;

    public Board board;
    public GameMode gameMode = new NormalGameMode();

    public int boardSize;
    public float tileScale;

    void Awake()
    {
        if (instance == null)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string mode;
                URLParameters.GetSearchParameters().TryGetValue("mode", out mode);
                if (mode == null)
                {
                    mode = "normal";
                }

                if (mode.ToLower() == "normal")
                {
                    gameMode = new NormalGameMode();
                } else if (mode.ToLower() == "hardcore")
                {
                    gameMode = new HardcoreGameMode();
                } else if (mode.ToLower() == "zen")
                {
                    gameMode = new ZenGameMode();
                }
            }

            instance = this;
            board.scale = 5f / (2 * boardSize + 1);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public GameObject createTile()
    {
        return Instantiate(tilePrefab);
    }

    public void setTilePiece(GameObject tile, GameObject piece)
    {
        Tile tileComponent = tile.GetComponent<Tile>();
        tileComponent.piece = piece;
    }

    public GameObject createPiece(PieceType pieceType)
    {
        GameObject piece = Instantiate(piecePrefab);
        Piece pieceComponent = piece.GetComponent<Piece>();
        pieceComponent.pieceType = pieceType;

        return piece;
    }

    public static List<Color> PIECE_COLORS = new List<Color>()
    {
        new Color(24 / 255f, 147 / 255f, 211 / 255f),
        new Color(43 / 255f, 192 / 255f, 97 / 255f),
        new Color(245 / 255f, 157 / 255f, 28 / 255f),
        new Color(226 / 255f, 20 / 255f, 44 / 255f),
        new Color(231 / 255f, 85 / 255f, 152 / 255f),
        new Color(173 / 255f, 62 / 255f, 217 / 255f),
        new Color(244 / 255f, 203 / 255f, 35 / 255f),
        new Color(189 / 255f, 238 / 255f, 50 / 255f),
        new Color(126 / 255f, 129 / 255f, 134 / 255f),
        new Color(89 / 255f, 221 / 255f, 230 / 255f),
        new Color(228 / 255f, 235 / 255f, 255 / 255f),
        new Color(97 / 255f, 35 / 255f, 181 / 255f)
    };

    public static List<Color> END_PIECE_COLORS = new List<Color>()
    {
        new Color(58 / 255f, 47 / 255f, 140 / 255f),
        new Color(21 / 255f, 110 / 255f, 101 / 255f),
        new Color(227 / 255f, 85 / 255f, 39 / 255f),
        new Color(166 / 255f, 41 / 255f, 79 / 255f),
        new Color(240 / 255f, 158 / 255f, 236 / 255f),
        new Color(100 / 255f, 29 / 255f, 171 / 255f),
        new Color(255 / 255f, 232 / 255f, 120 / 255f),
        new Color(56 / 255f, 217 / 255f, 121 / 255f),
        new Color(216 / 255f, 221 / 255f, 227 / 255f),
        new Color(170 / 255f, 242 / 255f, 211 / 255f),
        new Color(174 / 255f, 141 / 255f, 214 / 255f),
        new Color(246 / 255f, 166 / 255f, 175 / 255f)
    };

    public Color getPieceColor(int number)
    {
        float blend = Mathf.Floor((number - 1) / PIECE_COLORS.Count);
        int numberMod = (number - 1) % PIECE_COLORS.Count;
        blend %= 5;
        blend /= 4;
        Color lerped = PIECE_COLORS[numberMod] * (1 - blend) + END_PIECE_COLORS[numberMod] * blend;
        float h, s, v;
        Color.RGBToHSV(lerped, out h, out s, out v);
        h += 0.618f * 0.25f * (number / 60);
        h %= 1;
        return Color.HSVToRGB(h, s, v);
    }

    public static List<Color> COMBO_COLORS = new List<Color>()
    {
        new Color(18 / 255f, 193 / 255f, 91 / 255f),
        new Color(162 / 255f, 255 / 255f, 40 / 255f),
        new Color(255 / 255f, 237 / 255f, 0 / 255f),
        new Color(248 / 255f, 167 / 255f, 21 / 255f),
        new Color(255 / 255f, 105 / 255f, 0 / 255f),
        new Color(241 / 255f, 16 / 255f, 24 / 255f),
        new Color(243 / 255f, 39 / 255f, 110 / 255f),
        new Color(224 / 255f, 23 / 255f, 205 / 255f),
        new Color(150 / 255f, 15 / 255f, 207 / 255f),
        new Color(60 / 255f, 21 / 255f, 183 / 255f),
        new Color(51 / 255f, 104 / 255f, 240 / 255f),
        new Color(0 / 255f, 221 / 255f, 255 / 255f)
    };

    public Color getComboColor(int number)
    {
        float lighten = Mathf.Floor((number - 1) / COMBO_COLORS.Count);
        lighten = Mathf.Pow(0.9f, lighten);

        number = (number - 1) % COMBO_COLORS.Count;
        return COMBO_COLORS[number] * lighten + Color.white * (1 - lighten);
    }

    public enum Direction
    {
        UP_RIGHT, RIGHT, DOWN_RIGHT, DOWN_LEFT, LEFT, UP_LEFT
    }

    public Direction opposite(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP_RIGHT: return Direction.DOWN_LEFT;
            case Direction.RIGHT: return Direction.LEFT;
            case Direction.DOWN_RIGHT: return Direction.UP_LEFT;
            case Direction.DOWN_LEFT: return Direction.UP_RIGHT;
            case Direction.LEFT: return Direction.RIGHT;
            default: return Direction.DOWN_RIGHT;
        }
    }

    public Vector2 directionVector(Direction direction)
    {
        float theta = angle(direction) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
    }

    public float angle(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP_RIGHT: return 60f;
            case Direction.RIGHT: return 0f;
            case Direction.DOWN_RIGHT: return -60f;
            case Direction.DOWN_LEFT: return -120f;
            case Direction.LEFT: return 180f;
            default: return 120f;
        }
    }

    public abstract class PieceType
    {
        public abstract string getTypeName();
        public abstract bool canMerge(PieceType pieceType);
        public abstract PieceType mergeResult(PieceType pieceType);
        public abstract void mergeBehavior(Piece piece);
        public abstract float getPointsForMerge();
        public abstract float getPointsForExplode();
        public abstract bool contribuesToCombo();
        public abstract GameObject instantiateDisplay(Piece piece);
        public abstract PieceType clone();
    }

    public abstract class NumberedPieceType : PieceType
    {
        public override bool canMerge(PieceType pieceType)
        {
            if (!(pieceType is NumberedPieceType))
            {
                return false;
            }

            NumberedPieceType numberedPieceType = pieceType as NumberedPieceType;
            return numberedPieceType.getNumber() == getNumber();
        }

        public override PieceType mergeResult(PieceType pieceType)
        {
            if (pieceType is IncrementerPieceType)
            {
                return pieceType.mergeResult(this);
            }

            if (!(pieceType is NumberedPieceType))
            {
                return new StandardPieceType(getNumber() + 1);
            }

            NumberedPieceType numberedPieceType = pieceType as NumberedPieceType;
            return new StandardPieceType(Mathf.Max(numberedPieceType.getNumber(), getNumber()) + 1);
        }

        public int number;

        public NumberedPieceType(int number)
        {
            this.number = number;
        }

        public int getNumber()
        {
            return number;
        }

        public void setNumber(int number)
        {
            this.number = number;
        }
    }

    public class StandardPieceType : NumberedPieceType
    {
        public StandardPieceType(int number) : base(number)
        { }

        public override string getTypeName()
        {
            return "standard";
        }

        public override void mergeBehavior(Piece piece)
        { }

        public override float getPointsForMerge()
        {
            return number;
        }

        public override float getPointsForExplode()
        {
            return number * 10f;
        }

        public override bool contribuesToCombo()
        {
            return true;
        }

        public override GameObject instantiateDisplay(Piece piece)
        {
            GameObject gameObject = Instantiate(instance.standardPiecePrefab);
            updateDisplay(gameObject.transform.Find("Piece Top").gameObject, gameObject.transform.Find("Piece Sides").gameObject, gameObject.transform.Find("Number").gameObject.GetComponent<TextMeshPro>());

            return gameObject;
        }

        public override PieceType clone()
        {
            return new StandardPieceType(number);
        }

        public void updateDisplay(GameObject top, GameObject sides, TextMeshPro text)
        {
            Color color = getColor();
            Color darker = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f);
            top.GetComponent<SpriteRenderer>().color = color;
            sides.GetComponent<SpriteRenderer>().color = darker;
            text.color = darker;
            text.text = getText();
        }

        public string getText()
        {
            if (number < 1)
            {
                return "Err";
            }

            return number.ToString();
        }

        public Color getColor()
        {
            if (number < 1)
            {
                return Color.red;
            }

            return instance.getPieceColor(number);
        }
    }

    public class WallPieceType : PieceType
    {
        public override string getTypeName()
        {
            return "wall";
        }

        public override bool canMerge(PieceType pieceType)
        {
            return pieceType is WallPieceType;
        }

        public override PieceType mergeResult(PieceType pieceType)
        {
            return new WallPieceType();
        }

        public override void mergeBehavior(Piece piece)
        { }

        public override float getPointsForMerge()
        {
            return 0f;
        }

        public override float getPointsForExplode()
        {
            return 0f;
        }

        public override bool contribuesToCombo()
        {
            return false;
        }

        public override GameObject instantiateDisplay(Piece piece)
        {
            GameObject gameObject = Instantiate(instance.wallPiecePrefab);
            return gameObject;
        }

        public override PieceType clone()
        {
            return new WallPieceType();
        }
    }

    public class BombPieceType : NumberedPieceType
    {
        public BombPieceType(int number) : base(number)
        { }

        public override string getTypeName()
        {
            return "bomb";
        }

        public override void mergeBehavior(Piece piece)
        {
            if (piece == null || piece.tile == null)
            {
                return;
            }

            foreach (GameObject neighbor in piece.tile.neighbors.Values)
            {
                if (neighbor == null)
                {
                    continue;
                }

                neighbor.GetComponent<Tile>().explode();
            }

            piece.explode();
        }

        public override float getPointsForMerge()
        {
            return number * 3f;
        }

        public override float getPointsForExplode()
        {
            return number * 15f;
        }

        public override bool contribuesToCombo()
        {
            return true;
        }

        public override GameObject instantiateDisplay(Piece piece)
        {
            GameObject gameObject = Instantiate(instance.bombPiecePrefab);
            updateDisplay(gameObject.transform.Find("Piece Top").gameObject, gameObject.transform.Find("Piece Sides").gameObject, gameObject.transform.Find("Number").gameObject.GetComponent<TextMeshPro>(), gameObject.transform.Find("Bomb").gameObject.GetComponent<SpriteRenderer>());

            return gameObject;
        }

        public void updateDisplay(GameObject top, GameObject sides, TextMeshPro text, SpriteRenderer bombSprite)
        {
            Color color = getColor();
            Color darker = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f);
            top.GetComponent<SpriteRenderer>().color = color;
            sides.GetComponent<SpriteRenderer>().color = darker;
            text.color = darker;
            text.text = getText();
            bombSprite.color = darker;
        }

        public string getText()
        {
            if (number < 1)
            {
                return "Err";
            }

            return number.ToString();
        }

        public Color getColor()
        {
            if (number < 1)
            {
                return Color.red;
            }

            return instance.getPieceColor(number);
        }

        public override PieceType clone()
        {
            return new BombPieceType(number);
        }
    }

    public class IncrementerPieceType : PieceType
    {
        public override string getTypeName()
        {
            return "incrementer";
        }

        public override bool canMerge(PieceType pieceType)
        {
            return pieceType is NumberedPieceType;
        }

        public override PieceType mergeResult(PieceType pieceType)
        {
            if (!(pieceType is NumberedPieceType))
            {
                return pieceType;
            }

            NumberedPieceType numberedPieceType = (NumberedPieceType)pieceType.clone();
            numberedPieceType.setNumber(numberedPieceType.getNumber() + 1);
            return numberedPieceType;
        }

        public override void mergeBehavior(Piece piece)
        { }

        public override float getPointsForMerge()
        {
            return 100;
        }

        public override float getPointsForExplode()
        {
            return 200;
        }

        public override bool contribuesToCombo()
        {
            return true;
        }

        public override GameObject instantiateDisplay(Piece piece)
        {
            GameObject gameObject = Instantiate(instance.incrementerPiecePrefab);
            return gameObject;
        }

        public override PieceType clone()
        {
            return new IncrementerPieceType();
        }
    }

    public string getBoardSaveName()
    {
        return "board_" + gameMode.getName();
    }

    public abstract class GameMode
    {
        public abstract string getName();
        public abstract PieceType spawnPiece(Board board);
        public abstract float getComboTime(int combo);
        public abstract float getComboMultiplier(int combo);
    }

    public class NormalGameMode : GameMode
    {
        public override string getName()
        {
            return "normal";
        }

        public override PieceType spawnPiece(Board board)
        {
            float highest = board.highest;
            float f = highest - 0.5f * Mathf.Pow(Mathf.Log(highest), 1.1f);
            float rangeLow = Mathf.Floor(f - 0.4f * Mathf.Pow(f, 0.47f));
            float rangeHigh = Mathf.Ceil(f);
            int number = (int) Mathf.Max(Random.Range(rangeLow, rangeHigh), 1f);

            Vector2Int lowestPiece = board.lowestPieceCount();
            if (lowestPiece.x < rangeLow && lowestPiece.y == 1)
            {
                if (Random.value < (rangeLow - lowestPiece.x) * 0.05f)
                {
                    number = lowestPiece.x;
                }
            }

            PieceType pieceType = new StandardPieceType(number);

            if (Random.value < 0.045f && highest >= 40)
            {
                pieceType = new BombPieceType(number);
            }
            else if (Random.value < 0.035f && highest >= 20)
            {
                pieceType = new BombPieceType(number);
            }
            else if (Random.value < 0.025f && highest >= 12)
            {
                pieceType = new BombPieceType(number);
            }

            if (Random.value < 0.16f && highest >= 30)
            {
                pieceType = new WallPieceType();
            }
            else if (Random.value < 0.13f && highest >= 18)
            {
                pieceType = new WallPieceType();
            }
            else if (Random.value < 0.1f && highest >= 7)
            {
                pieceType = new WallPieceType();
            }

            if (Random.value < 0.1f && highest >= 50)
            {
                pieceType = new IncrementerPieceType();
            }
            else if (Random.value < 0.07f && highest >= 35)
            {
                pieceType = new IncrementerPieceType();
            }
            else if (Random.value < 0.04f && highest >= 19)
            {
                pieceType = new IncrementerPieceType();
            }

            return pieceType;
        }

        public override float getComboTime(int combo)
        {
            return Mathf.Max(4f - combo * 0.1f, 1.5f);
        }

        public override float getComboMultiplier(int combo)
        {
            return Mathf.Min(Mathf.Pow(combo + 1f, 0.5f), 15f);
        }
    }

    public class HardcoreGameMode : GameMode
    {
        public override string getName()
        {
            return "hardcore";
        }

        public override PieceType spawnPiece(Board board)
        {
            float highest = board.highest * 0.9f;
            float f = highest - 0.6f * Mathf.Pow(Mathf.Log(highest), 1.3f);
            float rangeLow = Mathf.Floor(f - 0.5f * Mathf.Pow(f, 0.49f));
            float rangeHigh = Mathf.Ceil(f);
            int number = (int) Mathf.Max(Random.Range(rangeLow, rangeHigh), 1f);

            PieceType pieceType = new StandardPieceType(number);

            if (Random.value < 0.04f && highest >= 25)
            {
                pieceType = new BombPieceType(number);
            }
            else if (Random.value < 0.03f && highest >= 14)
            {
                pieceType = new BombPieceType(number);
            }
            else if (Random.value < 0.02f && highest >= 9)
            {
                pieceType = new BombPieceType(number);
            }

            if (Random.value < 0.15f && highest >= 20)
            {
                pieceType = new WallPieceType();
            }
            else if (Random.value < 0.11f && highest >= 11)
            {
                pieceType = new WallPieceType();
            }
            else if (Random.value < 0.08f && highest >= 4)
            {
                pieceType = new WallPieceType();
            }

            if (Random.value < 0.05f && highest >= 40)
            {
                pieceType = new IncrementerPieceType();
            }
            else if (Random.value < 0.035f && highest >= 29)
            {
                pieceType = new IncrementerPieceType();
            }
            else if (Random.value < 0.02f && highest >= 20)
            {
                pieceType = new IncrementerPieceType();
            }

            return pieceType;
        }

        public override float getComboTime(int combo)
        {
            return 3.5f * Mathf.Pow(0.95f, combo);
        }

        public override float getComboMultiplier(int combo)
        {
            return Mathf.Min(Mathf.Pow(combo + 1f, 0.7f), 20f);
        }
    }

    public class ZenGameMode : GameMode
    {
        public override string getName()
        {
            return "zen";
        }

        public override PieceType spawnPiece(Board board)
        {
            float highest = board.highest;
            float f = highest - 0.4f * Mathf.Pow(Mathf.Log(highest), 0.75f);
            float rangeLow = Mathf.Floor(f - 0.3f * Mathf.Pow(f, 0.45f));
            float rangeHigh = Mathf.Ceil(f);
            int number = (int) Mathf.Max(Random.Range(rangeLow, rangeHigh), 1f);

            Vector2Int lowestPiece = board.lowestPieceCount();
            if (lowestPiece.x < rangeLow && lowestPiece.y == 1)
            {
                if (Random.value < (rangeLow - lowestPiece.x) * 0.1f)
                {
                    number = lowestPiece.x;
                }
            }

            PieceType pieceType = new StandardPieceType(number);

            if (Random.value < 0.05f && highest >= 28)
            {
                pieceType = new BombPieceType(number);
            }
            if (Random.value < 0.04f && highest >= 18)
            {
                pieceType = new BombPieceType(number);
            }
            else if (Random.value < 0.03f && highest >= 10)
            {
                pieceType = new BombPieceType(number);
            }

            if (Random.value < 0.04f && highest >= 30)
            {
                pieceType = new WallPieceType();
            }
            else if (Random.value < 0.03f && highest >= 20)
            {
                pieceType = new WallPieceType();
            }
            else if (Random.value < 0.02f && highest >= 12)
            {
                pieceType = new WallPieceType();
            }

            if (Random.value < 0.14f && highest >= 40)
            {
                pieceType = new IncrementerPieceType();
            }
            else if (Random.value < 0.1f && highest >= 25)
            {
                pieceType = new IncrementerPieceType();
            }
            else if (Random.value < 0.07f && highest >= 14)
            {
                pieceType = new IncrementerPieceType();
            }

            return pieceType;
        }

        public override float getComboTime(int combo)
        {
            return Mathf.Max(10f - combo * 0.05f, 5f);
        }

        public override float getComboMultiplier(int combo)
        {
            return Mathf.Min(Mathf.Pow(combo + 1f, 0.3f), 12f);
        }
    }
}