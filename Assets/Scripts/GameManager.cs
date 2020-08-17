using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    [HideInInspector]
    public BoardManager boardManager;


    public int playerFoodPoints = 0;

    [HideInInspector]
    public bool playersTurn = true;

    [SerializeField]
    private int level = 3;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        boardManager = GetComponent<BoardManager>();
        InitGame();
    }

    void InitGame()
    {
        boardManager.SetupScene(level);
    }

    public void GameOver()
    {
        enabled = false;
    }
}
