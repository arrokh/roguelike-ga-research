using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBrainMetrics : MonoBehaviour
{
    [Metric("Best Map Generation Number")]
    public static float MetricBestMapGenerationNumber(object map)
    {
        //return FindObjectOfType<MapBrainGenerator>().bestMapGenerationNumber;

        return 0.0f;
    }

    [Metric("Best Map Generation Number")]
    public static float MetricAnother(object map)
    {
        //return FindObjectOfType<MapBrainGenerator>().bestMapGenerationNumber;

        return 0.2f;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("===> RunAlgorithm()");
            FindObjectOfType<MapBrainGenerator>().RunAlgorithm();
        }
    }
}
