/*
	Made by Sunny Valle Studio
	(https://svstudio.itch.io)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SVS.ChessMaze
{
    public class MapVisualizer : MonoBehaviour
    {
        private Transform parent;
        public Color startColor, exitColor;

        public GameObject roadStraight, roadTileCorner, tileEmpty, startTile, exitTile, enemyTile;
        public GameObject[] environmentTiles;

        Dictionary<Vector3, GameObject> dictionaryOfObstacles = new Dictionary<Vector3, GameObject>();

        public bool animate;

        private void Awake()
        {
            parent = this.transform;
        }

        public void VisualizeMap(MapGrid grid, MapData data, bool visualizeUsingPrefabs)
        {
            if (visualizeUsingPrefabs)
            {
                VisualizeUsingPrefabs(grid, data);
            }
            else
            {
                VisualizeUsingPrimitives(grid, data);
            }

            StartCoroutine("RunSS");
        }
        private IEnumerator RunSS()
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot("./Assets/SS/" + (FindObjectOfType<MapBrainGenerator>().indexGenerate - 2) + " - Generation.png");
        }

        private void VisualizeUsingPrefabs(MapGrid grid, MapData data)
        {
            double maxGenerateEnemy = Math.Floor(((Math.Log(Math.Sqrt(grid.Width * grid.Length)) / 8) * 10));

            for (int i = 0; i < data.path.Count; i++)
            {
                var position = data.path[i];
                if (position != data.exitPosition)
                {
                    grid.SetCell(position.x, position.z, CellObjectType.Road);
                }
            }
            for (int col = 0; col < grid.Width; col++)
            {
                for (int row = 0; row < grid.Length; row++)
                {
                    var cell = grid.GetCell(col, row);
                    var position = new Vector3(cell.X, cell.Z, 0);

                    var index = grid.CalculateIndexFromCoordinates(position.x, position.y);
                    if (data.obstacleArray[index] && cell.IsTaken == false)
                    {
                        cell.ObjectType = CellObjectType.Obstacle;
                    }
                    Direction previousDirection = Direction.None;
                    Direction nextDirection = Direction.None;
                    switch (cell.ObjectType)
                    {
                        case CellObjectType.Empty:
                            int showEnemy = Random.Range(0, 2);
                            GameObject tile = tileEmpty;
                            if (showEnemy == 1 && maxGenerateEnemy > 0)
                            {
                                --maxGenerateEnemy;
                                tile = enemyTile;
                            }
                            CreateIndicator(position, tile, Quaternion.Euler(0, 0, 0));
                            break;
                        case CellObjectType.Road:
                            CreateIndicator(position, roadStraight, Quaternion.Euler(0, 0, 0));
                            break;
                        case CellObjectType.Obstacle:

                            int randomIndex = Random.Range(0, environmentTiles.Length);
                            CreateIndicator(position, environmentTiles[randomIndex], Quaternion.Euler(0, 0, 0));
                            break;
                        case CellObjectType.Start:
                            if (data.path.Count > 0)
                                nextDirection = GetDirectionFromVectors(data.path[0], position);
                            CreateIndicator(position, startTile, Quaternion.Euler(0, 0, 0));
                            break;
                        case CellObjectType.Exit:
                            if (data.path.Count > 0)
                                previousDirection = GetDirectionOfPreviousCell(position, data);
                            CreateIndicator(position, exitTile, Quaternion.Euler(0, 0, 0));
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        private Direction GetDicrectionOfNextCell(Vector3 position, MapData data)
        {
            int index = data.path.FindIndex(a => a == position);
            var nextCellPosition = data.path[index + 1];
            return GetDirectionFromVectors(nextCellPosition, position);
        }

        private Direction GetDirectionOfPreviousCell(Vector3 position, MapData data)
        {
            var index = data.path.FindIndex(a => a == position);
            var previousCellPosition = Vector3.zero;
            if (index > 0)
            {
                previousCellPosition = data.path[index - 1];
            }
            else
            {
                previousCellPosition = data.startPosition;
            }
            return GetDirectionFromVectors(previousCellPosition, position);
        }

        private Direction GetDirectionFromVectors(Vector3 positionToGoTo, Vector3 position)
        {
            if (positionToGoTo.x > position.x)
            {
                return Direction.Right;
            }
            else if (positionToGoTo.x < position.x)
            {
                return Direction.Left;
            }
            else if (positionToGoTo.z < position.y)
            {
                return Direction.Down;
            }
            return Direction.Up;
        }

        private void CreateIndicator(Vector3 position, GameObject prefab, Quaternion rotation = new Quaternion())
        {
            var placementPosition = position + new Vector3(.5f, .5f, .5f);
            var element = Instantiate(prefab, placementPosition, rotation);
            element.transform.parent = parent;
            dictionaryOfObstacles.Add(position, element);
            if (animate)
            {
                element.AddComponent<DropTween>();
                DropTween.IncreaseDropTime();
            }
        }

        private void VisualizeUsingPrimitives(MapGrid grid, MapData data)
        {
            PlaceStartAndExitPoints(data);
            for (int i = 0; i < data.obstacleArray.Length; i++)
            {
                if (data.obstacleArray[i])
                {
                    var positionOnGrid = grid.CalculateCoordinatesFromIndex(i);
                    if (positionOnGrid == data.startPosition || positionOnGrid == data.exitPosition)
                    {
                        continue;
                    }
                    grid.SetCell(positionOnGrid.x, positionOnGrid.z, CellObjectType.Obstacle);
                    if (PlaceKnightObstacle(data, positionOnGrid))
                    {
                        continue;
                    }
                    if (dictionaryOfObstacles.ContainsKey(positionOnGrid) == false)
                    {
                        CreateIndicator(positionOnGrid, Color.white, PrimitiveType.Cube);
                    }

                }
            }
        }

        private bool PlaceKnightObstacle(MapData data, Vector3 positionOnGrid)
        {
            foreach (var knight in data.knightPiecesList)
            {
                if (knight.Position == positionOnGrid)
                {
                    CreateIndicator(positionOnGrid, Color.red, PrimitiveType.Cube);
                    return true;
                }
            }
            return false;
        }

        private void PlaceStartAndExitPoints(MapData data)
        {
            CreateIndicator(data.startPosition, startColor, PrimitiveType.Sphere);
            CreateIndicator(data.exitPosition, exitColor, PrimitiveType.Sphere);
        }

        private void CreateIndicator(Vector3 position, Color color, PrimitiveType sphere)
        {
            var element = GameObject.CreatePrimitive(sphere);
            dictionaryOfObstacles.Add(position, element);
            element.transform.position = position + new Vector3(.5f, .5f, .5f);
            element.transform.parent = parent;
            var renderer = element.GetComponent<Renderer>();
            renderer.material.SetColor("_Color", color);
            if (animate)
            {
                element.AddComponent<DropTween>();
                DropTween.IncreaseDropTime();
            }
        }

        public void ClearMap()
        {
            foreach (var obstacle in dictionaryOfObstacles.Values)
            {
                Destroy(obstacle);
            }
            dictionaryOfObstacles.Clear();

            if (animate)
                DropTween.ResetTime();
        }
    }
}

