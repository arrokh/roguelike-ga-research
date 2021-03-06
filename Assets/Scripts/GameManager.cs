﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2.0f;
    public float turnDelay = 0.1f;
    public int playerFoodPoints = 100;

    public static GameManager instance = null;

    public bool useAsGA = false;

    public bool playersTurn = true;
    [SerializeField]
    private bool enemiesMoving;

    [SerializeField]
    private int level = 1;
    private BoardManager boardManager;
    private List<Enemy> enemies;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        enemies = new List<Enemy>();

        boardManager = GetComponent<BoardManager>();

        SceneManager.sceneLoaded += OnSceneWasLoaded;
    }

    void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
    {
        level++;
        InitGame();
    }

    void InitGame()
    {
        enemies.Clear();

        if (!useAsGA)
            boardManager.SetupScene(level);
    }

    void Update()
    {
        if (playersTurn || enemiesMoving)
            return;

        StartCoroutine(MoveEnemies());
    }

    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;

        yield return new WaitForSeconds(turnDelay);

        if (enemies.Count == 0)
            yield return new WaitForSeconds(turnDelay);

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();

            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;

        enemiesMoving = false;
    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    public void GameOver()
    {
        enabled = false;
    }
}
