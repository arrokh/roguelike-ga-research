/*
	Made by Sunny Valle Studio
	(https://svstudio.itch.io)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SVS.ChessMaze
{
    public class MapBrain : MonoBehaviour
    {
        //Genetic algorithm parameters
        [Tunable(MinValue: 20, MaxValue: 100, Name: "Population Size")]
        [SerializeField, Range(20, 100)]
        private int populationSize = 20;
        [Tunable(MinValue: 0, MaxValue: 100, Name: "Crossover Rate")]
        [SerializeField, Range(0, 100)]
        private int crossoverRate = 100;
        private double crossoverRatePercent;
        [Tunable(MinValue: 0, MaxValue: 100, Name: "Mutation Rate")]
        [SerializeField, Range(0, 100)]
        private int mutationRate = 0;
        private double mutationRatePercent;
        [Tunable(MinValue: 1, MaxValue: 100, Name: "Generation Limit")]
        [SerializeField, Range(1, 100)]
        private int generatinLimit = 10;

        //algorithm variables
        private List<CandidateMap> currentGeneration;
        private int totalFitnessThisGeneration, bestFitnessScoreAllTime = 0;
        private CandidateMap bestMap = null;
        private int bestMapGenerationNumber = 0, generationNumber = 1;

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


        private void Start()
        {
            mutationRatePercent = mutationRate / 100D;
            crossoverRatePercent = crossoverRate / 100D;
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
            Debug.Log("================ Finish ================");
        }


        [Generator]
        public object GenerateGAResult2Danesh()
        {
            RunAlgorithm();

            var width = bestMap.Grid.Width;
            var height = bestMap.Grid.Length;

            Tile[,] map = new Tile[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var cell = bestMap.Grid.GetCell(i, j);
                    map[i, j] = new Tile(cell.X, cell.Z, cell.IsTaken);
                }
            }

            return map;
        }

        [Visualiser]
        public Texture2D RenderMap(object _m)
        {
            Tile[,] map = (Tile[,])_m;

            Texture2D tex = new Texture2D(8 * map.GetLength(0), 8 * map.GetLength(1), TextureFormat.ARGB32, false);
            int sf = 8; int Width = map.GetLength(0); int Height = map.GetLength(1);
            Texture2D gnd = Resources.Load<Texture2D>("tiles/tile_17");

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (map[i, j].BLOCKS_MOVEMENT)
                    {
                        VisUtils.PaintTexture(tex, i, j, sf, gnd, 8, 8);
                        VisUtils.PaintTexture(tex, i, j, sf, selectTexture(map, i, j), 8, 8);

                    }
                    else
                    {
                        VisUtils.PaintTexture(tex, i, j, sf, gnd, 8, 8);
                    }
                }
            }

            tex.Apply();
            return tex;
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
    }
}

