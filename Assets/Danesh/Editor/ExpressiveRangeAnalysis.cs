using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class ExpressiveRangeAnalysis {

    public GameObject image;

    public Color solidColor = Color.white;
    public Color wallColor = Color.black;

    Danesh danesh;

    public string firstMetricName = "CalculateDensity";
    public string secondMetricName = "CalculateOpenness";
    public void SetMetricName1(string name){firstMetricName = name;}
    public void SetMetricName2(string name){secondMetricName = name;}

    public int numberOfAttempts = 200;
    public int numberOfAttemptsRandom = 2000;

    public Gradient heatGradient;

    public ExpressiveRangeAnalysis(int numGenerators){
        ERAData = new List<List<float>>[numGenerators];
        RERAData = new List<List<float>>[numGenerators];

        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        heatGradient = new Gradient();
        gck = new GradientColorKey[4];
        gck[0].color = Color.blue;
        gck[0].time = 0.0F;
        gck[1].color = Color.green;
        gck[1].time = 0.33F;
        gck[2].color = Color.yellow;
        gck[2].time = 0.66F;
        gck[3].color = Color.red;
        gck[3].time = 1.0F;
        gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0F;
        gak[0].time = 0.0F;
        gak[1].alpha = 1.0F;
        gak[1].time = 1.0F;
        heatGradient.SetKeys(gck, gak);
    }

    public List<List<GeneratorSample[,]>> eraSamples;

    List<List<float>> SampleExpressiveRangeRandomly(int totalAttempts, Danesh gen){
        float progressBar = 0f;
        EditorUtility.DisplayProgressBar("Computing Randomised Expressive Range Histogram", "Working...", progressBar);
        List<List<float>> res = new List<List<float>>();

        List<GeneratorMetric> metrics = danesh.GetMetricsForActiveGenerator();

        /*
            Current:
            eraSample[metric1][metric2] = object[,] of samples representing the histogram
            Lot of data duplication here, not the most efficient way to do it
        */
        eraSamples = new List<List<GeneratorSample[,]>>();

        for(int i=0; i<metrics.Count; i++){
            List<GeneratorSample[,]> secondmetric = new List<GeneratorSample[,]>();
            for(int j=0; j<metrics.Count; j++){
                GeneratorSample[,] sampleH = new GeneratorSample[100,100];
                secondmetric.Add(sampleH);
            }
            eraSamples.Add(secondmetric);
        }

        List<GeneratorParameter> genParams = gen.GetParametersForActiveGenerator();

        for(int att=0; att<totalAttempts; att++){

            //Hold this so we can find this version later
            object[] ps = new object[genParams.Count];
            //Randomly parameterise the generator
            for(int i=0; i<genParams.Count; i++){
                GeneratorParameter p = genParams[i];
                p.RandomiseValue();
                ps[i] = p.GetValue();
            }

            object map = danesh.GenerateContent();

            List<float> nums = new List<float>();
            for(int i=0; i<metrics.Count; i++){
                float score = (float)danesh.GetMetric(i, new object[]{map});
                nums.Add(score);
            }

            //Update the samples list
            for(int i=0; i<nums.Count; i++){
                int index1 = (int)Mathf.Floor(nums[i]*100f);
                if(index1 < 0) index1 = 0;
                if(index1 >99) index1 = 99;
                for(int j=0; j<nums.Count; j++){
                    int index2 = (int)Mathf.Floor(nums[j]*100f);
                    if(index2 < 0) index2 = 0;
                    if(index2 >99) index2 = 99;
                    eraSamples[i][j][index1,index2] = new GeneratorSample(map, ps);
                }
            }

            res.Add(nums);
            EditorUtility.DisplayProgressBar("Computing Randomised Expressive Range Histogram", "Evaluating random expressive range... "+(100*(float)att/(float)totalAttempts).ToString("F0")+" percent complete", (float)att/(float)totalAttempts);
        }
        EditorUtility.ClearProgressBar();
        return res;
    }

    List<List<float>> SampleExpressiveRange(int totalAttempts, Danesh gen){
        float progressBar = 0f;
        EditorUtility.DisplayProgressBar("Computing Expressive Range", "Working...", progressBar);
        List<List<float>> res = new List<List<float>>();

        List<GeneratorMetric> metrics = danesh.GetMetricsForActiveGenerator();

        List<GeneratorParameter> genParams = gen.GetParametersForActiveGenerator();
        //Hold this so we can find this version later
        object[] ps = new object[genParams.Count];
        for(int i=0; i<genParams.Count; i++){
            ps[i] = genParams[i].currentValue;
        }

        eraSamples = new List<List<GeneratorSample[,]>>();
        for(int i=0; i<metrics.Count; i++){
            List<GeneratorSample[,]> secondmetric = new List<GeneratorSample[,]>();
            for(int j=0; j<metrics.Count; j++){
                GeneratorSample[,] sampleH = new GeneratorSample[100,100];
                secondmetric.Add(sampleH);
            }
            eraSamples.Add(secondmetric);
        }

        for(int att=0; att<totalAttempts; att++){
            object map = danesh.GenerateContent();
            List<float> nums = new List<float>();
            for(int i=0; i<metrics.Count; i++){
                float score = 0f;
                try{
                    score = (float) metrics[i].method.Invoke(null, new object[]{map});
                }
                catch{

                }
                nums.Add(score);
            }
            res.Add(nums);

            //Update the samples list
            for(int i=0; i<nums.Count; i++){
                int index1 = (int)Mathf.Floor(nums[i]*100f);
                if(index1 < 0) index1 = 0;
                if(index1 >99) index1 = 99;
                for(int j=0; j<nums.Count; j++){
                    int index2 = (int)Mathf.Floor(nums[j]*100f);
                    if(index2 < 0) index2 = 0;
                    if(index2 >99) index2 = 99;
                    eraSamples[i][j][index1,index2] = new GeneratorSample(map, ps);
                }
            }

            EditorUtility.DisplayProgressBar("Computing Expressive Range", "Evaluating expressive range... "+(100*(float)att/(float)totalAttempts).ToString("F0")+" percent complete", (float)att/(float)totalAttempts);
        }
        EditorUtility.ClearProgressBar();
        return res;
    }

    public List<List<float>>[] ERAData;
    public List<List<float>>[] RERAData;

    public void StartRERA(Danesh gen, int num, int genId){
        danesh = gen;
        numberOfAttemptsRandom = num;
        RERAData[genId] = SampleExpressiveRangeRandomly(num, gen);
    }

    public void StartERA(Danesh gen, int num, int genId){
        danesh = gen;
        numberOfAttempts = num;
        ERAData[genId] = SampleExpressiveRange(num, gen);
    }

    public Texture2D GenerateERAGraphForAxes(int generator, int x, int y){
        return GenerateGraphFromData(ERAData[generator], x, y);
    }

    public Texture2D GenerateRERAGraphForAxes(int generator, int x, int y){
        return GenerateGraphFromData(RERAData[generator], x, y, true);
    }

    public Texture2D GenerateGraphFromData(List<List<float>> LastERA, int x, int y, bool isRERA=false){
        if(LastERA == null){
            return new Texture2D (1000, 1000, TextureFormat.ARGB32, false);
        }
        else{
            int[,] data = new int[100,100];
            for(int att=0; att<LastERA.Count; att++){
                int m1 = (int)Mathf.Floor(LastERA[att][x]*100f);
                int m2 = (int)Mathf.Floor(LastERA[att][y]*100f);
                if(m1 < 0)
                    m1 = 0;
                if(m2 < 0)
                    m2 = 0;
                if(m1 >= 0 && m2 >= 0 && m1 < data.GetLength(0) && m2 < data.GetLength(1)){
                    data[m1,m2]++;
                }
            }
            int sf = 5;

            Texture2D newTex = new Texture2D (500, 500, TextureFormat.ARGB32, false);

            float max = 0;
            for(int i=0; i<data.GetLength(0); i++){
                for(int j=0; j<data.GetLength(1); j++){
                    if((float)data[i,j] > max){
                        max = (float)data[i,j];
                    }
                }
            }

            //Render the map
            for(int i=0; i<data.GetLength(0); i++){
                for(int j=0; j<data.GetLength(1); j++){
                    float amt = (float)data[i,j]/max;///(float)numberOfAttempts * 50;
                    if(amt == 0){
                        PaintPoint(newTex, i, j, 5, Color.black);//new Color(amt, amt, amt, 1.0f)
                    }
                    else if(!isRERA){
                        PaintPoint(newTex, i, j, 5, heatGradient.Evaluate(amt));//new Color(amt, amt, amt, 1.0f)
                    }
                    else{
                        amt = (float)data[i,j]/(float)numberOfAttempts * 50;
                        PaintPoint(newTex, i, j, 5, new Color(amt, amt, amt, 1.0f));
                    }
                }
            }

            //Replace texture
            newTex.Apply();

            return newTex;
        }
    }

    public Texture2D GeneratedGraph;

    void PaintPoint(Texture2D tex, int _x, int _y, int scaleFactor, Color c){
        int x = _x*scaleFactor; int y = _y*scaleFactor;
        for(int i=x; i<x+scaleFactor; i++){
            for(int j=y; j<y+scaleFactor; j++){
                tex.SetPixel(i, j, c);
            }
        }
    }
}
