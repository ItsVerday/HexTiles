using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.Tween;

public class Piece : MonoBehaviour
{
    public int merge = 0;
    public int combo = 0;
    public float scoreAccumulator = 0;
    public List<Manager.PieceType> mergingPieceTypes;
    public Tween<Vector3> scaleIn;
    public Tween<Vector3> mergeAnimation;
    public Tween<Vector3> moveWhenMergeAnimation;
    public Tween<Vector3> scaleOutWhenMergeAnimation;
    public Tween<Vector3> offsetAnimation;
    public Vector3 animationOffset = new Vector3(0, 0, 0);
    public Vector3 oldPosition = new Vector3(0, 0, 0);

    public Tile tile;
    public Board board;
    public Manager.PieceType pieceType;

    void Awake()
    {
        transform.localScale = new Vector3(0, 0, 0);
        scaleIn = TweenFactory.Tween(null, new Vector3(0, 0, 0), new Vector3(1, 1, 1), 0.3f, TweenScaleFunctions.CubicEaseOut, t =>
        {
            if (this == null)
            {
                return;
            }

            Vector3 scale = t.CurrentValue;
            if (tile == null)
            {
                scale *= Manager.instance.tileScale;
            }

            transform.localScale = scale;
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject display = pieceType.instantiateDisplay(this);
        display.transform.SetParent(transform, false);

        mergingPieceTypes = new List<Manager.PieceType>()
        {
            pieceType
        };
    }

    public void clearAnimations()
    {
        scaleIn?.Stop(TweenStopBehavior.Complete);
        mergeAnimation?.Stop(TweenStopBehavior.Complete);
        moveWhenMergeAnimation?.Stop(TweenStopBehavior.Complete);
        scaleOutWhenMergeAnimation?.Stop(TweenStopBehavior.Complete);
        offsetAnimation?.Stop(TweenStopBehavior.Complete);
        Vector3 scale = new Vector3(1, 1, 1);
        if (tile == null)
        {
            scale *= Manager.instance.tileScale;
        }

        transform.localScale = scale;
    }

    public void explode()
    {
        tile.board.score += (int) Mathf.Floor(pieceType.getPointsForExplode());
        tile.pieceExploded = true;
        tile.unsetPiece();
        clearAnimations();

        TweenFactory.Tween(null, new Vector3(1, 1, 1), new Vector3(0, 0, 0), 0.5f, TweenScaleFunctions.CubicEaseOut, t =>
        {
            if (this == null)
            {
                return;
            }

            Vector3 scale = t.CurrentValue;
            if (tile == null)
            {
                scale *= Manager.instance.tileScale;
            }
            
            scale *= board.transform.localScale.x;

            transform.localScale = scale;
        }, t =>
        {
            if (this != null) {
                Destroy(gameObject);
            }
        });

        Vector2 offset = Random.insideUnitCircle * 3f * board.transform.localScale.x;
        TweenFactory.Tween(null, transform.localPosition, transform.localPosition + (Vector3) offset, 0.5f, TweenScaleFunctions.QuarticEaseOut, t =>
        {
            if (this == null)
            {
                return;
            }

            transform.localPosition = t.CurrentValue;
        });

        float rotation = Random.Range(-540f, 540f);
        TweenFactory.Tween(null, new Vector3(0, 0, 0), new Vector3(0, 0, rotation), 0.5f, TweenScaleFunctions.Linear, t =>
        {
            if (this == null)
            {
                return;
            }

            transform.localEulerAngles = t.CurrentValue;
        });
    }
}