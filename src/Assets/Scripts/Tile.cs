using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.Tween;

public class Tile : MonoBehaviour
{
    public Dictionary<Manager.Direction, GameObject> neighbors = new Dictionary<Manager.Direction, GameObject>();
    public GameObject piece = null;
    public bool pieceExploded = false;

    public Board board;

    void Start()
    {
        TweenFactory.Tween(null, new Vector3(0, 0, 0), new Vector3(1, 1, 1), 0.6f, TweenScaleFunctions.CubicEaseOut, t =>
        {
            transform.localScale = t.CurrentValue * Manager.instance.tileScale;
        });
    }

    public void setPiece(GameObject piece)
    {
        if (piece != null)
        {
            unsetPiece();
        }

        this.piece = piece;
        Piece pieceComponent = piece.GetComponent<Piece>();
        pieceComponent.tile = this;

        piece.transform.SetParent(transform);
        piece.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void unsetPiece()
    {
        if (piece != null)
        {
            Piece pieceComponent = piece.GetComponent<Piece>();

            piece.transform.parent = null;
            // piece.transform.localScale = new Vector3(Manager.instance.tileScale, Manager.instance.tileScale, Manager.instance.tileScale);
            pieceComponent.tile = null;
            piece = null;
        }
    }

    public void explode()
    {
        pieceExploded = true;

        if (piece != null)
        {
            piece.GetComponent<Piece>().explode();
        }
    }
}