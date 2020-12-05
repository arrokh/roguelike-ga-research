using UnityEngine;
using System.Collections;

public class LevelChunk  {

    public bool exitTop = false;
    public bool exitRight = false;
    public bool exitBelow = false;
    public bool exitLeft = false;

    public int[,] tiles;

    public LevelChunk(int[,] tiles, bool exitTop, bool exitRight, bool exitBelow, bool exitLeft){
        this.tiles = tiles;
        this.exitTop = exitTop;
        this.exitRight = exitRight;
        this.exitBelow = exitBelow;
        this.exitLeft = exitLeft;
    }

}
