using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkyLevelMetrics : MonoBehaviour {

    [Metric("Main Path Length")]
    public static float CalcPathLength(object map){
        ChunkyLevel sl = (ChunkyLevel) map;
        return (float)sl.path.Count / (float) ((sl.chunks.GetLength(0) * sl.chunks.GetLength(1)));
    }

    [Metric("Bombless Accessible Percentage")]
    public static float CalcAccessiblePC(object map){
        ChunkyLevel sl = (ChunkyLevel) map;
        int sx = 0; int sy = 0;
        float emptyCount = 0;
        for(int i=0; i<sl.tiles.GetLength(0); i++){
            for(int j=0; j<sl.tiles.GetLength(1); j++){
                if(sl.tiles[i,j] == -1){
                    sx = i;
                    sy = j;
                }
                if(sl.tiles[i,j] == 0){
                    emptyCount++;
                }
            }
        }

        //Flood out from the start point
        bool[,] markers = new bool[sl.tiles.GetLength(0), sl.tiles.GetLength(1)];
        List<Point> open = new List<Point>();
        open.Add(new Point(sx, sy));

        markers[sx,sy] = true;

        float accessCount = 0;
        int rounds = 0;
        int pointer = 0;

        while(pointer < open.Count && rounds++ < 1000){
            Point p = open[pointer];
            pointer++;
            for(int i=-1; i<2; i++){
                for(int j=-1; j<2; j++){
                    if(i != 0 || j != 0){
                        int dx = p.x + i;
                        int dy = p.y + j;
                        if(dx >= 0 && dy >= 0 && dx < sl.tiles.GetLength(0) && dy < sl.tiles.GetLength(1) &&
                            sl.tiles[dx,dy] == 0 && !markers[dx,dy]){
                            open.Add(new Point(dx, dy));
                            markers[dx,dy] = true;
                        }
                    }
                }
            }
        }
        return (float) pointer / emptyCount;
    }

    class Point{
        public int x;
        public int y;
        public Point(int x, int y){this.x = x; this.y = y;}
    }
}
