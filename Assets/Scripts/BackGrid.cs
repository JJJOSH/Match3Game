using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGrid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        BUBBLE,
        COUNT,
    }

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    public int yDim;
    public int xDim;


    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;
    public float fillTime;
    private bool inverse = false;
    private GamePiece[,] pieces;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    // Start is called before the first frame update
    void Start()
    {
        piecePrefabDict = new Dictionary<PieceType, GameObject>();

        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < xDim; y++)
            {
                GameObject background = (GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
            }
        }

        pieces = new GamePiece[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y, PieceType.EMPTY);
            }
        }

        Destroy(pieces[1, 4].gameObject);
        SpawnNewPiece(1, 4, PieceType.BUBBLE);

        Destroy(pieces[2, 4].gameObject);
        SpawnNewPiece(2, 4, PieceType.BUBBLE);

        Destroy(pieces[3, 4].gameObject);
        SpawnNewPiece(3, 4, PieceType.BUBBLE);

        Destroy(pieces[5, 4].gameObject);
        SpawnNewPiece(5, 4, PieceType.BUBBLE);

        Destroy(pieces[6, 4].gameObject);
        SpawnNewPiece(6, 4, PieceType.BUBBLE);

        Destroy(pieces[7, 4].gameObject);
        SpawnNewPiece(7, 4, PieceType.BUBBLE);

        Destroy(pieces[4, 0].gameObject);
        SpawnNewPiece(4, 0, PieceType.BUBBLE);

        StartCoroutine(Fill());


        public IEnumerator Fill()
        {
            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }
        }

        public bool FillStep()
        {
            bool movedPiece = false;

            for (int y = yDim - 2; y >= 0; y--)
            {
                for (int loopX = 0; loopX < xDim; loopX++)
                {
                    int x = loopX;

                    if (inverse)
                    {
                        x = xDim - 1 - loopX;
                    }

                    GamePPiece piece = pieces[x, y];

                    if (piece.IsMovable())
                    {
                        GamePPiece pieceBelow = pieces[x, y + 1];

                        if (pieceBelow.Type == PieceType.EMPTY)
                        {
                            Destroy(pieceBelow.gameObject);
                            piece.MovableComponent.Move(x, y + 1, fillTime);
                            pieces[x, y + 1] = piece;
                            SpawnNewPiece(x, y, PieceType.EMPTY);
                            movedPiece = true;
                        }
                        else
                        {
                            for (int diag = -1; diag <= 1; diag++)
                            {
                                if (diag != 0)
                                {
                                    int diagX = x + diag;

                                    if (inverse)
                                    {
                                        diagX = x - diag;
                                    }

                                    if (diagX >= 0 && diagX < xDim)
                                    {
                                        GamePPiece diagonalPiece = pieces[diagX, y + 1];

                                        if (diagonalPiece.Type == PieceType.EMPTY)
                                        {
                                            bool hasPieceAbove = true;

                                            for (int aboveY = y; aboveY >= 0; aboveY--)
                                            {
                                                GamePPiece pieceAbove = pieces[diagX, aboveY];

                                                if (pieceAbove.IsMovable())
                                                {
                                                    break;

                                                }
                                                else if (!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
                                                {
                                                    hasPieceAbove = false;
                                                    break;
                                                }
                                            }

                                            if (!hasPieceAbove)
                                            {
                                                Destroy(diagonalPiece.gameObject);
                                                piece.MovableComponent.Move(diagX, y + 1, fillTime);
                                                pieces[diagX, y + 1] = piece;
                                                SpawnNewPiece(x, y, PieceType.EMPTY);
                                                movedPiece = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
     
        for (int x = 0; x < xDim; x++)
        {
            GamePiece pieceBelow = pieces[x, 0];

            if (pieceBelow.Type == PieceType.EMPTY)
            {
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
                newPiece.transform.parent = transform;

                pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));
                movedPiece = true;
            }
        }

        return movedPiece;
    }  
    

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x,
            transform.position.y + yDim / 2.0f - y);
    }

    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);

        return pieces[x, y];
    }
}
