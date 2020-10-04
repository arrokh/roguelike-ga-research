/*
	Made by Sunny Valle Studio
	(https://svstudio.itch.io)
*/
using UnityEngine;

namespace SVS
{
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Vector3 offsett = Vector3.zero;
        public GameObject groudPrefab;

        public void VisualizeGrid(int width, int length)
        {
            // Vector3 position = new Vector3(width / 2f, length / 2f, 3);
            // Quaternion rotation = Quaternion.Euler(0, 0, 0);
            // var board = Instantiate(groudPrefab, position, rotation);
            // board.transform.localScale = new Vector3(width, length, 1);

            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < length; row++)
                {
                    var position = new Vector3(col, row, 2) + offsett;

                    // Vector3 position = new Vector3(width / 2f, length / 2f, 3);
                    Quaternion rotation = Quaternion.Euler(0, 0, 0);
                    var board = Instantiate(groudPrefab, position, rotation);
                }
            }
        }

    }
}

