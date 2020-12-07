﻿/*
	Made by Sunny Valle Studio
	(https://svstudio.itch.io)
*/
using SVS;
using SVS.ChessMaze;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class MapBrainGenerator : MonoBehaviour
{
    [SerializeField]
    private bool showLogEachGeneration = false;

    public int indexGenerate = 0;

    //Genetic algorithm parameters
    [SerializeField, Range(10, 1000)]
    public int populationSize = 10;
    [SerializeField, Range(0, 100)]
    public int crossoverRate = 100;
    private double crossoverRatePercent;
    [SerializeField, Range(0, 100)]
    public int mutationRate = 0;
    private double mutationRatePercent;
    [SerializeField, Range(1, 100)]
    public int generatinLimit = 10;

    //algorithm variables
    private List<CandidateMap> currentGeneration;
    private int totalFitnessThisGeneration, bestFitnessScoreAllTime = 0;
    private CandidateMap bestMap = null;
    [HideInInspector]
    public int bestMapGenerationNumber = 0, generationNumber = 1;

    //fitness parameters
    [SerializeField]
    private int fitnessCornerMin = 6, fitnessCornerMax = 12;
    [SerializeField, Range(1, 3)]
    private int fitnessCornerWeight = 1, fitnessNearCornerWeght = 1;
    [SerializeField, Range(1, 5)]
    private int fitnessPathWeight = 1;
    [SerializeField, Range(0.3f, 1f)]
    private float fitnessObstacleWeight = 1;

    //Map start parameters
    [SerializeField, Range(3, 20)]
    private int widthOfMap = 11, lengthOfMap = 11;
    private Vector3 startPosition, exitPosition;
    private MapGrid grid;
    public Direction startPositionEdge = Direction.Left, exitPositionEdge = Direction.Right;
    [SerializeField]
    private bool randomStartAndEnd = false;
    [SerializeField, Range(1, 11)]
    public int numberOfKnightPieces = 7;

    //Visualize grid;
    public MapVisualizer mapVisualizer;
    DateTime startDate, endDate;
    private bool isAlgorithmRunning = false;

    public bool IsAlgorithmRunning { get => isAlgorithmRunning; }

    [SerializeField]
    private List<MetricsData> metricsData = new List<MetricsData>();

    private void Start()
    {
        mutationRatePercent = mutationRate / 100D;
        crossoverRatePercent = crossoverRate / 100D;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
            FindObjectOfType<MapBrainGenerator>().RunAlgorithm();

        if (Input.GetKeyUp(KeyCode.KeypadEnter))
            SaveToFile();
    }

    public void RunAlgorithm()
    {
        UiController.instance.ResetScreen();
        ResetAlgorithmVariables();
        mapVisualizer.ClearMap();

        grid = new MapGrid(widthOfMap, lengthOfMap);

        MapHelper.RandomlyChooseAndSetStartAndExit(grid, ref startPosition, ref exitPosition, randomStartAndEnd, startPositionEdge, exitPositionEdge);

        isAlgorithmRunning = true;
        startDate = DateTime.Now;
        FindOptimalSolution(grid);
    }

    private void ResetAlgorithmVariables()
    {
        totalFitnessThisGeneration = 0;
        bestFitnessScoreAllTime = 0;
        bestMap = null;
        bestMapGenerationNumber = 0;
        generationNumber = 0;
        ++indexGenerate;
    }

    private void FindOptimalSolution(MapGrid grid)
    {
        currentGeneration = new List<CandidateMap>(populationSize);
        for (int i = 0; i < populationSize; i++)
        {
            var candidateMap = new CandidateMap(grid, numberOfKnightPieces);
            candidateMap.CreateMap(startPosition, exitPosition, true);
            currentGeneration.Add(candidateMap);
        }

        StartCoroutine(GeneticAlgorithm());
    }

    private IEnumerator GeneticAlgorithm()
    {
        totalFitnessThisGeneration = 0;
        int bestFitnessScoreThisGeneration = 0;
        CandidateMap bestMapThisGeneration = null;
        foreach (var candidate in currentGeneration)
        {
            candidate.FindPath();
            candidate.Repair();
            var fitness = CalculateFitness(candidate.ReturnMapData());

            totalFitnessThisGeneration += fitness;
            if (fitness > bestFitnessScoreThisGeneration)
            {
                bestFitnessScoreThisGeneration = fitness;
                bestMapThisGeneration = candidate;
            }

        }

        if (bestFitnessScoreThisGeneration > bestFitnessScoreAllTime)
        {
            bestFitnessScoreAllTime = bestFitnessScoreThisGeneration;
            bestMap = bestMapThisGeneration.DeepClone();
            bestMapGenerationNumber = generationNumber;
        }

        generationNumber++;
        yield return new WaitForEndOfFrame();
        UiController.instance.SetLoadingValue(generationNumber / (float)generatinLimit);

        if (showLogEachGeneration)
            Debug.Log("Current generation [ " + generationNumber++ + " ] score: " + bestFitnessScoreAllTime);

        if (generationNumber < generatinLimit)
        {
            List<CandidateMap> nextGeneration = new List<CandidateMap>();

            while (nextGeneration.Count < populationSize)
            {
                var parent1 = currentGeneration[RouletteWheelSelection()];
                var parent2 = currentGeneration[RouletteWheelSelection()];

                CandidateMap child1, child2;

                CrossOverParrents(parent1, parent2, out child1, out child2);

                child1.AddMutation(mutationRatePercent);
                child2.AddMutation(mutationRatePercent);

                nextGeneration.Add(child1);
                nextGeneration.Add(child2);
            }
            currentGeneration = nextGeneration;

            StartCoroutine(GeneticAlgorithm());
        }
        else
        {
            ShowResults();
        }
    }

    private void ShowResults()
    {
        Debug.Log("======== MapBrain::ShowResult() ========");

        isAlgorithmRunning = false;
        Debug.Log("Best solution at generation [" + ++bestMapGenerationNumber + "] with score: " + bestFitnessScoreAllTime);

        var data = bestMap.ReturnMapData();
        mapVisualizer.VisualizeMap(bestMap.Grid, data, true);

        UiController.instance.HideLoadingScreen();

        Debug.Log("Path length: " + data.path.Count);
        Debug.Log("Corners count: " + data.cornersList.Count);

        endDate = DateTime.Now;
        long elapsedTicks = endDate.Ticks - startDate.Ticks;
        TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
        Debug.Log("Time needed to run this genetic optimisation: " + elapsedSpan.TotalSeconds);


        metricsData.Add(new global::MetricsData()
        {
            index = (indexGenerate - 2),
            bestfitnessScore = bestFitnessScoreAllTime,
            bestGenerationIndex = bestMapGenerationNumber,
            corner = data.cornersList.Count,
            path = data.path.Count,
            duration = elapsedSpan.TotalSeconds
        });

        Debug.Log("================ Finish ================");
    }


    private void CrossOverParrents(CandidateMap parent1, CandidateMap parent2, out CandidateMap child1, out CandidateMap child2)
    {
        child1 = parent1.DeepClone();
        child2 = parent2.DeepClone();

        if (Random.value < crossoverRatePercent)
        {
            int numBIts = parent1.ObstaclesArray.Length;

            int crossOverIndex = Random.Range(0, numBIts);

            for (int i = crossOverIndex; i < numBIts; i++)
            {
                child1.PlaceObstacle(i, parent2.IsObstacleAt(i));
                child2.PlaceObstacle(i, parent1.IsObstacleAt(i));
            }
        }
    }

    private int RouletteWheelSelection()
    {
        int randomValue = Random.Range(0, totalFitnessThisGeneration);
        for (int i = 0; i < populationSize; i++)
        {
            randomValue -= CalculateFitness(currentGeneration[i].ReturnMapData());
            if (randomValue <= 0)
            {
                return i;
            }
        }
        return populationSize - 1;
    }

    private int CalculateFitness(MapData mapData)
    {
        int numberOfObstacles = mapData.obstacleArray.Where(isObstacle => isObstacle).Count();
        int score = mapData.path.Count * fitnessPathWeight + (int)(numberOfObstacles * fitnessObstacleWeight);
        int cornersCount = mapData.cornersList.Count;
        if (cornersCount >= fitnessCornerMin && cornersCount <= fitnessCornerMax)
        {
            score += cornersCount * fitnessCornerWeight;
        }
        else if (cornersCount > fitnessCornerMax)
        {
            score -= fitnessCornerWeight * (cornersCount - fitnessCornerMax);
        }
        else if (cornersCount < fitnessCornerMin)
        {
            score -= fitnessCornerWeight * fitnessCornerMin;
        }
        score -= mapData.cornersNearEachOther * fitnessNearCornerWeght;
        return score;
    }

    Dictionary<string, string> autotiles;

    Texture2D selectTexture(Tile[,] map, int x, int y)
    {
        if (autotiles == null)
        {
            autotiles = new Dictionary<string, string>();
            autotiles.Add("0000", "01");
            autotiles.Add("0001", "09");
            autotiles.Add("0010", "05");
            autotiles.Add("0011", "13");
            autotiles.Add("0100", "03");
            autotiles.Add("0101", "11");
            autotiles.Add("0110", "07");
            autotiles.Add("0111", "15");
            autotiles.Add("1000", "02");
            autotiles.Add("1001", "10");
            autotiles.Add("1010", "06");
            autotiles.Add("1011", "14");
            autotiles.Add("1100", "04");
            autotiles.Add("1101", "12");
            autotiles.Add("1110", "08");
            autotiles.Add("1111", "16");
        }

        string name = "";
        if (y == map.GetLength(1) - 1 || map[x, y + 1].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";
        if (x == map.GetLength(0) - 1 || map[x + 1, y].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";
        if (y == 0 || map[x, y - 1].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";
        if (x == 0 || map[x - 1, y].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";

        return Resources.Load<Texture2D>("tiles/tile_" + autotiles[name]);
    }

    public string ToCSV()
    {
        var sb = new StringBuilder("Index,Best Generation Index");
        foreach (var data in metricsData)
        {
            sb.AppendLine();
            sb.Append(data.index.ToString());
            sb.Append(",");
            sb.Append(data.bestGenerationIndex.ToString());
            //sb.Append('/n').Append(frame.Time.ToString()).Append(',').Append(frame.Value.ToString());
        }

        return sb.ToString();
    }
    public void SaveToFile()
    {
        // Use the CSV generation from before
        var content = ToCSV();

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif

        var filePath = Path.Combine(folder, "export.csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log("CSV file success written to");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}

[Serializable]
public class MetricsData
{
    public int index;
    public float bestGenerationIndex;
    public float bestfitnessScore;
    public int path;
    public int corner;
    public double duration;

}