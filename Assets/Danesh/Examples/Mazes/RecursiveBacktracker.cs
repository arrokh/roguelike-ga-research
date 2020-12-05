using UnityEngine;
using System.Collections;

public class RecursiveBacktracker : AbstractMazeGenerator {

    public int endX = -1;
    public int endY = -1;

    [Generator]
    public int[,] GenerateMaze(){
        maze = new int[size, size];

        if(endX < 0){
            endX = size-1;
            endY = size-1;
        }

        CarveSpace(startX, startY);

        Set(0, 0, START, false);

        Set(endX, endY, END, false);

        return maze;
    }

    void CarveSpace(int x, int y){
        int[] ds = RandomDirectionList();
        int nx, ny;
        for(int i=0; i<ds.Length; i++){
            nx = x + DX[ds[i]];
            ny = y + DY[ds[i]];

            if(nx >= 0 && nx < maze.GetLength(0) && ny >= 0 && ny < maze.GetLength(1) && maze[nx, ny] == 0){
                //Mark the route as clear
                Set(x, y, ds[i]);
                //Continue carving
                CarveSpace(nx, ny);
            }
        }
    }

}
