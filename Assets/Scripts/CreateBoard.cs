using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class CreateBoard : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public GameObject housePrefabs;
    public Text score;

    public GameObject treePrefab;
    private GameObject[] tiles;
    private long treeBB = 0;
    private long playerBB = 0;
    private long dirtBB=0; //64 bits
    private long desertBB=0; //64 bits
 
    void Start()
    {
        tiles = new GameObject[64];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int randTile = UnityEngine.Random.Range(0, tilePrefabs.Length);
                Vector3 pos = new Vector3(j, 0, i);
                GameObject tile = Instantiate(tilePrefabs[randTile], pos, Quaternion.identity);
                tile.name = tile.tag + "_" + i + "_" + j;

                tiles[i * 8 + j] = tile;
                if (tile.tag == "Dirt")
                {
                    dirtBB = SetCellState(dirtBB, i, j); 
                }
                if (tile.tag == "Desert")
                {
                    desertBB = SetCellState(desertBB, i, j); 
                }
            }
        }
        Debug.Log("Dirt cells = "+ CellCount(dirtBB));
        
       InvokeRepeating(nameof(PlantTree),.1f,.1f);
    }

    void PlantTree()
    {
        int randRow = UnityEngine.Random.Range(0, 8);
        int randCol = UnityEngine.Random.Range(0, 8);

        if (GetSellState(dirtBB &~playerBB, randRow, randCol))
        {
            GameObject tree = Instantiate(treePrefab);
            tree.transform.parent = tiles[randRow * 8 + randCol].transform;
            tree.transform.localPosition = Vector3.zero;
            treeBB = SetCellState(treeBB, randRow, randCol);
        }

    }
 
    long SetCellState(long bitboard, int row, int col)
    {
        long newBit = 1L << (row * 8 + col);
        return bitboard |= newBit;
    }

    bool GetSellState(long bitboard, int row, int col)
    {
        long mask = 1L << (row * 8 + col);
        return (bitboard &= mask)!=0;
    }

    int CellCount(long bitBoard)
    {
        int count = 0;
        long bb = bitBoard;
        while (bb != 0)
        {
            bb &= bb - 1;
            count++;
        }

        return count;
    }

    void CalculateScore()
    {
        score.text = "Score: " +( CellCount(playerBB & dirtBB)*10 + CellCount(playerBB & desertBB)*2);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                int r = (int) hit.collider.transform.position.z;
                int c = (int) hit.collider.transform.position.x;
                if (GetSellState((dirtBB & ~treeBB)|desertBB, r, c))
                {

                    GameObject house = Instantiate(housePrefabs);
                    house.transform.parent = hit.collider.transform;
                    house.transform.localPosition = Vector3.zero;
                    playerBB = SetCellState(playerBB, r,c);
       
                    CalculateScore();
                }
            }

        }
    }
}
