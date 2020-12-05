using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    With thanks to http://weblog.jamisbuck.org/2011/2/7/maze-generation-algorithm-recap
*/
public class AbstractMazeGenerator : MonoBehaviour {

    public int[,] maze;

    public int startX = 0;
    public int startY = 0;

    [Tunable(MinValue: 3, MaxValue: 40, Name:"Maze Size")]
    public int size = 10;

    /*
        These constants are all powers of 2, which will help us write compact code to test
        directions and walls. Don't worry if you've not come across this stuff before - you
        won't need it to understand the maze algorithms.
    */
    public static int NORTH = 1;
    public static int EAST = 2;
    public static int SOUTH = 4;
    public static int WEST = 8;

    public static int START = 16;
    public static int END = 32;

    public static int[] directions = new int[]{1,2,4,8};

    public static Dictionary<int, int> OPPOSITE = new Dictionary<int, int>
    {
        { 1,4 },
        { 4,1 },
        { 2,8 },
        { 8,2 },
    };

    public static Dictionary<int, int> DX = new Dictionary<int, int>
    {
        { 1,0 },
        { 4,0 },
        { 2,1 },
        { 8,-1 },
    };

    public static Dictionary<int, int> DY = new Dictionary<int, int>
    {
        { 1,-1 },
        { 4,1 },
        { 2,0 },
        { 8,0 },
    };

    public void Set(int x, int y, int code, bool mirror=true){
        maze[x,y] |= code;
        if(mirror)
            maze[x+DX[code], y+DY[code]] |= OPPOSITE[code];
    }

    public static bool CanMove(int x, int y, int dir, int[,] _maze){
        return (_maze[x,y] & dir) != 0;
    }

    public int RandomDirection(){
        return directions[Random.Range(0, 4)];
    }

    public int[] RandomDirectionList(){
        int[] res = new int[4];
        for(int i=0; i<4; i+=0){
            int n = Random.Range(0, 4);
            bool ad = true;
            for(int j=0; j<i; j++){
                if(res[j] == directions[n]){
                    ad = false;
                    break;
                }
            }
            if(ad){
                res[i] = directions[n];
                i++;
            }
        }
        return res;
    }

    [Visualiser]
    public Texture2D RenderMap(object _m){

        int[,] _maze = (int[,]) _m;

        int sf = 4;
        Texture2D tex = new Texture2D (sf * _maze.GetLength(0), sf * _maze.GetLength(1), TextureFormat.ARGB32, false);
        int Width = _maze.GetLength(0); int Height = _maze.GetLength(1);

        for(int i=0; i<Width; i++){
            for(int j=0; j<Height; j++){
                if((_maze[i,j] & START) != 0){
                    VisUtils.PaintSquare(tex, sf*i +1, sf*j +1, sf-1, sf-1, Color.green);
                }
                else if((_maze[i,j] & END) != 0){
                    VisUtils.PaintSquare(tex, sf*i +1, sf*j +1, sf-1, sf-1, Color.red);
                }
            }
        }

        for(int i=0; i<Width; i++){
            for(int j=0; j<Height; j++){
                if(!CanMove(i, j, NORTH, _maze)){
                    VisUtils.PaintLine(tex, sf*i, sf*j, sf+1, false, Color.black);
                }
                if(!CanMove(i, j, WEST, _maze)){
                    VisUtils.PaintLine(tex, sf*i, sf*j, sf+1, true, Color.black);
                }
            }
        }

        VisUtils.PaintLine(tex, 0, 0, sf*_maze.GetLength(0), false, Color.black);
        VisUtils.PaintLine(tex, 0, 0, sf*_maze.GetLength(1), true, Color.black);
        VisUtils.PaintLine(tex, 0, sf*(_maze.GetLength(0))-1, sf*_maze.GetLength(0), false, Color.black);
        VisUtils.PaintLine(tex, sf*(_maze.GetLength(0))-1, 0, sf*_maze.GetLength(1), true, Color.black);

        tex.Apply();
        return tex;
    }

}
