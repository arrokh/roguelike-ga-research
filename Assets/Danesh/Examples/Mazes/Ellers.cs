using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ellers : AbstractMazeGenerator {

    [Tunable(MinValue: 0.1f, MaxValue: 0.9f, Name:"Chance To Merge")]
    public float mergeChance = 0.5f;

    List<List<int>> sets;
    List<int>[] columns;

    [Generator]
    public int[,] GenerateMaze(){
        maze = new int[size, size];

        //We use this to keep track of the sets each column is in
        sets = new List<List<int>>();
        //We use this to keep track of the columns each set contains
        columns = new List<int>[size];

        //Initially, each column is in its own set
        for(int i=0; i<size; i++){
            sets.Add(new List<int>());
            sets[i].Add(i);
            columns[i] = sets[i];
        }

        //For each row in the maze, we work downwards
        for(int i=0; i<size-1; i++){
            //Merge random adjacent cells, providing they don't share a set
            for(int j=0; j<size-1; j++){
                if(columns[j] != columns[j+1] && UnityEngine.Random.Range(0f, 1f) < mergeChance){
                    //Link right
                    Set(j, i, EAST);
                    //Mark both as in the same set
                    MergeSet(j, j+1);
                }
            }

            //Now, randomly make downward connections, ensuring there's at least one per set
            List<int>[] newcolumns = new List<int>[size];
            foreach(List<int> ss in sets){
                for(int j=0; j<Random.Range(1, ss.Count); j++){
                    int link = ss[Random.Range(0, ss.Count)];
                    //Link down
                    Set(link, i, SOUTH);
                    newcolumns[link] = ss;
                }
                //Clear the set
                ss.Clear();
                //This set now contains the new row's members only
                for(int j=0; j<newcolumns.Length; j++){
                    if(newcolumns[j] == ss){
                        ss.Add(j);
                    }
                }
            }

            //Any column that didn't get a vertical link is now it's own set
            for(int j=0; j<newcolumns.Length; j++){
                if(newcolumns[j] == null){
                    List<int> newset = new List<int>();
                    newset.Add(j);
                    newcolumns[j] = newset;
                    sets.Add(newset);
                }
            }

            columns = newcolumns;
        }

        //For the final row, just merge adjacent cells
        for(int j=0; j<size-1; j++){
            if(columns[j] != columns[j+1]){
                //Link right
                Set(j, size-1, EAST);
                //Mark both as in the same set
                MergeSet(j, j+1);
            }
        }

        Set(0, 0, START, false);
        Set(size-1, size-1, END, false);

        return maze;
    }

    public void MergeSet(int i, int j){
        List<int> js = columns[j];
        sets.Remove(js);
        foreach(int n in js){
            columns[i].Add(n);
            columns[n] = columns[i];
        }
    }

}
