using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;

public class Board : MonoBehaviour
{
    public RectTransform container;
    public ControlManager controlManager;
    public ComboManager comboManager;
    public AudioSource impact;
    public TextMeshPro scoreText;
    public float scale;

    public static Board instance;

    public bool forceNewSave = false;

    public List<GameObject> tiles = new List<GameObject>();
    public Dictionary<Vector2Int, GameObject> tileGrid = new Dictionary<Vector2Int, GameObject>();

    public int highest = 1;
    public float spawningScore = 1f;
    public long score = 0;
    public double displayScore = 0;
    public Color scoreColor = Manager.PIECE_COLORS[0];

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for (int y = -Manager.instance.boardSize; y <= Manager.instance.boardSize; y++)
        {
            for (int x = 0; x <= Manager.instance.boardSize * 2 - Mathf.Abs(y); x++)
            {
                float trueY = y * Mathf.Sqrt(0.75f);
                float trueX = x + Mathf.Abs(y) / 2f - Manager.instance.boardSize;

                GameObject tile = Manager.instance.createTile();
                tile.GetComponent<Tile>().board = this;
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(trueX, trueY);
                float scale = Manager.instance.tileScale;
                tile.transform.localScale = new Vector3(scale, scale, scale);

                tiles.Add(tile);
                tileGrid.Add(new Vector2Int(x, y), tile);
            }
        }

        for (int y = -Manager.instance.boardSize; y <= Manager.instance.boardSize; y++)
        {
            for (int x = 0; x <= Manager.instance.boardSize * 2 - Mathf.Abs(y); x++)
            {
                Tile tile = getTile(x, y).GetComponent<Tile>();

                tile.neighbors[Manager.Direction.RIGHT] = getTile(x + 1, y);
                tile.neighbors[Manager.Direction.LEFT] = getTile(x - 1, y);

                if (y >= 0)
                {
                    tile.neighbors[Manager.Direction.UP_LEFT] = getTile(x - 1, y + 1);
                    tile.neighbors[Manager.Direction.UP_RIGHT] = getTile(x, y + 1);
                }
                else
                {
                    tile.neighbors[Manager.Direction.UP_LEFT] = getTile(x, y + 1);
                    tile.neighbors[Manager.Direction.UP_RIGHT] = getTile(x + 1, y + 1);
                }

                if (y <= 0)
                {
                    tile.neighbors[Manager.Direction.DOWN_LEFT] = getTile(x - 1, y - 1);
                    tile.neighbors[Manager.Direction.DOWN_RIGHT] = getTile(x, y - 1);
                }
                else
                {
                    tile.neighbors[Manager.Direction.DOWN_LEFT] = getTile(x, y - 1);
                    tile.neighbors[Manager.Direction.DOWN_RIGHT] = getTile(x + 1, y - 1);
                }
            }
        }

        try
        {
            if (!forceNewSave && !Manager.forceReset && SaveManager.loadBoard(this, Manager.instance.getBoardSaveName()))
            {
                Debug.Log("Save loaded");
                save();
            }
            else
            {
                Debug.Log("No save found");
                newGame();
            }
        }
        catch
        {
            Debug.Log("Error loading save");
            newGame();
        }

        /*
        // For testing purposes
        int i = 1;
        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            tile.unsetPiece();
            tile.setPiece(createPiece(i++));
        }
        */
    }
    
    public void newGame()
    {
        placeRandomPiece(new Manager.StandardPieceType(1));
        placeRandomPiece(new Manager.StandardPieceType(1));
        placeRandomPiece(new Manager.StandardPieceType(1));

        save();
    }

    GameObject getTile(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (tileGrid.ContainsKey(position))
        {
            return tileGrid[position];
        }

        return null;
    }

    void Update()
    {
        controlManager.move(this);

        float scale = this.scale * Mathf.Min(768f, container.rect.width / container.rect.height * 768f) / 768f;
        transform.localScale = new Vector3(scale, scale, scale);

        float displayScoreLerp = 1.0f - Mathf.Pow(1.0f - 0.95f, Time.deltaTime);
        displayScore = displayScore * (1.0f - displayScoreLerp) + score * displayScoreLerp;
        if (score - displayScore < 0.5f)
        {
            displayScore = score;
        }

        float scoreColorLerp = 1.0f - Mathf.Pow(1.0f - 0.999f, Time.deltaTime);
        Color pieceColor, na = new Color();
        Manager.instance.getPieceColors(highest, out pieceColor, out na);
        scoreColor = scoreColor * (1.0f - scoreColorLerp) + pieceColor * scoreColorLerp;

        scoreText.text = string.Format("{0:#,##0.##}", Math.Floor(displayScore));
        scoreText.color = scoreColor;
    }

    public void placeRandomPiece()
    {
        placeRandomPiece(Manager.instance.gameMode.spawnPiece(this));
    }

    public void placeRandomPiece(Manager.PieceType pieceType)
    {
        List<Tile> empty = new List<Tile>();
        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece == null && !tile.pieceExploded)
            {
                empty.Add(tile);
            }
        }

        if (empty.Count == 0)
        {
            return;
        }

        Tile emptyTile = empty[(int) Mathf.Floor(UnityEngine.Random.value * empty.Count)];

        GameObject piece = createPiece(pieceType);
        emptyTile.setPiece(piece);
    }

    public GameObject createPiece(int number)
    {
        return createPiece(new Manager.StandardPieceType(number));
    }

    public GameObject createPiece(Manager.PieceType pieceType)
    {
        if (pieceType is Manager.NumberedPieceType)
        {
            Manager.NumberedPieceType numberedPieceType = (Manager.NumberedPieceType) pieceType;

            if (numberedPieceType.getNumber() > highest)
            {
                highest = numberedPieceType.getNumber();
            }
        }

        return Manager.instance.createPiece(pieceType);
    }

    public Vector2Int lowestPieceCount()
    {
        int lowestPiece = 999999999;
        int count = 0;

        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece != null)
            {
                Piece pieceComponent = tile.piece.GetComponent<Piece>();
                Manager.PieceType pieceType = pieceComponent.pieceType;
                if (!(pieceType is Manager.NumberedPieceType))
                {
                    continue;
                }

                int number = ((Manager.NumberedPieceType) pieceType).getNumber();

                if (number < lowestPiece)
                {
                    lowestPiece = number;
                    count = 0;
                }

                if (number == lowestPiece)
                {
                    count++;
                }
            }
        }

        return new Vector2Int(lowestPiece, count);
    }

    public void move(Manager.Direction direction)
    {
        saveOldPositions();
        bool didCompress = compress(direction);

        if (!merge(direction, didCompress) && !didCompress)
        {
            afterMove(false);
            return;
        }
        else
        {
            playImpact();
        }

        afterMove(true);
    }

    public void saveOldPositions()
    {
        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece != null)
            {
                GameObject piece = tile.piece;
                Piece pieceComponent = piece.GetComponent<Piece>();
                pieceComponent.oldPosition = tile.transform.localPosition;
            }
        }
    }

    public void afterMove(bool success)
    {
        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            tile.pieceExploded = false;
        }

        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece != null)
            {
                Piece pieceComponent = tile.piece.GetComponent<Piece>();
                Manager.PieceType pieceType = pieceComponent.pieceType;

                if (pieceType is Manager.NumberedPieceType)
                {
                    Manager.NumberedPieceType numberedPieceType = (Manager.NumberedPieceType) pieceType;

                    if (numberedPieceType.getNumber() > highest)
                    {
                        highest = numberedPieceType.getNumber();
                    }
                }
            }
        }

        if (success)
        {
            if (highest > spawningScore)
            {
                spawningScore = highest;
            }

            spawningScore += Manager.instance.gameMode.getProgressionSpeed();
            placeRandomPiece();
        }

        save();
    }

    public void save()
    {
        SaveManager.save();
    }

    public bool compress(Manager.Direction direction)
    {
        bool change = true;
        bool looped = false;

        while (true)
        {
            change = false;

            foreach (GameObject gameObject in tiles)
            {
                Tile tile = gameObject.GetComponent<Tile>();
                if (tile.piece != null)
                {
                    GameObject neighbor = tile.neighbors[direction];
                    if (neighbor != null)
                    {
                        Tile neighborTile = neighbor.GetComponent<Tile>();
                        if (neighborTile.piece == null && !neighborTile.pieceExploded)
                        {
                            change = true;
                            GameObject piece = tile.piece;
                            tile.unsetPiece();
                            neighborTile.setPiece(piece);

                            Vector2 vector = Manager.instance.directionVector(direction) * 0.1f;
                            Piece pieceComponent = piece.GetComponent<Piece>();
                            pieceComponent.animationOffset = vector;
                        }
                    }
                }
            }
            
            if (!change)
            {
                return looped;
            }

            looped = true;
        }
    }

    public bool merge(Manager.Direction direction, bool didCompress)
    {
        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece != null)
            {
                GameObject piece = tile.piece;
                Piece pieceComponent = piece.GetComponent<Piece>();
                pieceComponent.merge = 0;
            }
        }

        bool change = true;
        bool looped = false;

        while (change)
        {
            change = false;

            foreach (GameObject gameObject in tiles)
            {
                Tile tile = gameObject.GetComponent<Tile>();
                if (tile.piece != null)
                {
                    Piece piece = tile.piece.GetComponent<Piece>();
                    Manager.PieceType pieceType = piece.pieceType;

                    GameObject neighborAhead = tile.neighbors[direction];
                    Tile neighborAheadTile = neighborAhead != null ? neighborAhead.GetComponent<Tile>() : null;
                    Piece neighborAheadPiece = neighborAheadTile != null && neighborAheadTile.piece != null ? neighborAheadTile.piece.GetComponent<Piece>() : null;
                    Manager.PieceType neighborAheadPieceType = neighborAheadPiece != null ? neighborAheadPiece.pieceType : null;
                    GameObject neighborBehind = tile.neighbors[Manager.instance.opposite(direction)];
                    Tile neighborBehindTile = neighborBehind != null ? neighborBehind.GetComponent<Tile>() : null;
                    Piece neighborBehindPiece = neighborBehindTile != null  && neighborBehindTile.piece != null ? neighborBehindTile.piece.GetComponent<Piece>() : null;
                    Manager.PieceType neighborBehindPieceType = neighborBehindPiece != null ? neighborBehindPiece.pieceType : null;

                    if (neighborAheadTile != null && neighborAheadPiece != null && canMerge(pieceType, neighborAheadPieceType) && (neighborBehindTile == null || neighborBehindPiece == null || !canMerge(pieceType, neighborBehindPieceType)))
                    {
                        change = true;

                        neighborAheadPiece.merge = piece.merge + 1;
                        neighborAheadPiece.combo = pieceType.contribuesToCombo() ? 1 : 0;
                        neighborAheadPiece.scoreAccumulator = piece.scoreAccumulator + piece.pieceType.getPointsForMerge();
                        List<Manager.PieceType> mergingPieceTypes = new List<Manager.PieceType>();
                        mergingPieceTypes.AddRange(piece.mergingPieceTypes);
                        mergingPieceTypes.Add(neighborAheadPiece.pieceType);
                        neighborAheadPiece.mergingPieceTypes = mergingPieceTypes;
                        GameObject pieceObject = tile.piece;
                        Piece pieceToAnimate = pieceObject.GetComponent<Piece>();
                        tile.unsetPiece();

                        Vector3 position = piece.oldPosition;
                        Vector3 tilePosition = tile.transform.localPosition;
                        float scaleBy = transform.localScale.x;
                        Vector2 vector = Manager.instance.directionVector(direction) * 0.4f * scaleBy;
                        pieceToAnimate.clearAnimations();
                        pieceToAnimate.moveWhenMergeAnimation = TweenFactory.Tween(null, new Vector3(0, 0, -3) + position * scaleBy, new Vector3(vector.x, vector.y, -3) + position * scaleBy, 0.25f, TweenScaleFunctions.CubicEaseOut, t =>
                        {
                            if (pieceObject == null)
                            {
                                return;
                            }

                            pieceObject.transform.localPosition = t.CurrentValue;
                        }, t => 
                        {
                            Destroy(pieceObject);
                        });

                        pieceToAnimate.scaleOutWhenMergeAnimation = TweenFactory.Tween(null, new Vector3(Manager.instance.tileScale, Manager.instance.tileScale, Manager.instance.tileScale), new Vector3(0, 0, 0), 0.25f, TweenScaleFunctions.CubicEaseOut, t =>
                        {
                            if (pieceObject == null)
                            {
                                return;
                            }

                            pieceObject.transform.localScale = t.CurrentValue * scaleBy;
                        });
                    }
                }
            }

            if (!change && !looped)
            {
                animateOffset();

                return false;
            }

            looped = true;
        }

        float toAddScore = 0;
        int maxCombo = 0;

        Dictionary<Tile, GameObject> newPieces = new Dictionary<Tile, GameObject>();
        List<MergeBehaviorPair> mergeBehaviors = new List<MergeBehaviorPair>();

        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece != null)
            {
                GameObject piece = tile.piece;
                Piece pieceComponent = piece.GetComponent<Piece>();
                int combo = pieceComponent.combo;
                int merge = pieceComponent.merge;

                if (combo > maxCombo)
                {
                    maxCombo = combo;
                }

                toAddScore += pieceComponent.scoreAccumulator;
                if (merge > 0)
                {
                    Manager.PieceType pieceType = pieceComponent.mergingPieceTypes[0];
                    for (int i = 1; i < pieceComponent.mergingPieceTypes.Count; i++)
                    {
                        pieceType = pieceComponent.mergingPieceTypes[i].mergeResult(pieceType);
                    }

                    GameObject newPiece = createPiece(pieceType);
                    Piece newPieceComponent = newPiece.GetComponent<Piece>();
                    newPieceComponent.scaleIn.Stop(TweenStopBehavior.Complete);

                    tile.unsetPiece();
                    newPieces[tile] = newPiece;
                    newPieceComponent.tile = tile;

                    for (int i = 0; i < pieceComponent.mergingPieceTypes.Count; i++)
                    {
                        mergeBehaviors.Add(new MergeBehaviorPair(pieceComponent.mergingPieceTypes[i], newPieceComponent));
                    }

                    Destroy(piece);

                    newPieceComponent.clearAnimations();
                    newPieceComponent.mergeAnimation = TweenFactory.Tween(null, new Vector3(1.15f, 1.15f, 1.15f), new Vector3(1f, 1f, 1f), 0.2f, TweenScaleFunctions.QuadraticEaseIn, t =>
                    {
                        if (newPiece == null)
                        {
                            return;
                        }

                        Vector3 scale = t.CurrentValue;
                        if (newPieceComponent.tile == null)
                        {
                            scale *= Manager.instance.tileScale;
                        }

                        newPiece.transform.localScale = scale;
                    });
                }
            }
        }

        foreach (Tile tile in newPieces.Keys)
        {
            tile.setPiece(newPieces[tile]);
        }

        foreach (MergeBehaviorPair behavior in mergeBehaviors)
        {
            behavior.execute();
        }

        comboManager.addCombo(maxCombo);
        toAddScore *= comboManager.getComboMultiplier();
        score += (int) Mathf.Floor(toAddScore);

        compress(direction);
        animateOffset();

        return true;
    }

    public void animateOffset()
    {
        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece != null)
            {
                GameObject piece = tile.piece;
                Piece pieceComponent = piece.GetComponent<Piece>();

                if (pieceComponent.animationOffset.magnitude > 0)
                {
                    pieceComponent.offsetAnimation = TweenFactory.Tween(null, pieceComponent.animationOffset, new Vector3(0, 0, 0), 0.2f, TweenScaleFunctions.QuadraticEaseOut, t => 
                    {
                        if (piece == null)
                        {
                            return;
                        }

                        piece.transform.localPosition = t.CurrentValue;
                    });

                    pieceComponent.animationOffset = new Vector3(0, 0, 0);
                }
            }
        }
    }

    public void playImpact()
    {
        impact.volume = UnityEngine.Random.value * 0.2f + 0.3f;
        impact.pitch = UnityEngine.Random.value * 0.2f + 0.8f;
        impact.time = 0.04f;
        impact.Play();
    }

    public bool canMerge(Manager.PieceType a, Manager.PieceType b)
    {
        return a.canMerge(b) || b.canMerge(a);
    }

    public bool canMakeMove()
    {
        foreach (GameObject gameObject in tiles)
        {
            Tile tile = gameObject.GetComponent<Tile>();
            if (tile.piece == null)
            {
                return true;
            }

            Piece piece = tile.piece.GetComponent<Piece>();
            foreach (GameObject neighbor in tile.neighbors.Values)
            {
                if (neighbor == null)
                {
                    continue;
                }

                Tile neighborTile = neighbor.GetComponent<Tile>();
                if (neighborTile.piece == null)
                {
                    return true;
                }

                Piece neighborPiece = neighborTile.piece.GetComponent<Piece>();

                if (canMerge(piece.pieceType, neighborPiece.pieceType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public struct MergeBehaviorPair
    {
        private Manager.PieceType pieceType;
        private Piece newPiece;

        public MergeBehaviorPair(Manager.PieceType pieceType, Piece newPiece)
        {
            this.pieceType = pieceType;
            this.newPiece = newPiece;
        }

        public void execute()
        {
            pieceType.mergeBehavior(newPiece);
        }
    }
}