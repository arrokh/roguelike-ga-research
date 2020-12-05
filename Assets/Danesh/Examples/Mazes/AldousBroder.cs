using UnityEngine;
using System.Collections;

public class AldousBroder : AbstractMazeGenerator {

	[Generator]
    public int[,] GenerateMaze(){
        maze = new int[size, size];

        int x = Random.Range(0, size);
        int y = Random.Range(0, size);
        int remaining = size * size; remaining--;

        int rd, nx, ny;
        int tries = 0;
        while(remaining > 0){
            rd = RandomDirection();
            nx = x + DX[rd];
            ny = y + DY[rd];
            if(nx >= 0 && ny >= 0 && nx < size && ny < size){
                if(maze[nx, ny] == 0){
                    Set(x, y, rd);
                    remaining--;
                }
                x = nx;
                y = ny;
            }
        }

        Set(0, 0, START, false);

        Set(size-1, size-1, END, false);

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
