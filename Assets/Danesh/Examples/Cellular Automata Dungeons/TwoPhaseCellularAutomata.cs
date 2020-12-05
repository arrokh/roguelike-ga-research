using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    This example is based on a tutorial on Rogue Basin, a great site with information about developing roguelikes.
    http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels
*/

public class TwoPhaseCellularAutomata : MonoBehaviour {

    /*
        These parameters have intentionally vague descriptions!
        See if you can use Danesh to help figure out what effect they
        have on the output.
    */

    [Tunable(MinValue: 0.2f, MaxValue: 0.6f, Name:"Initial Solid Chance")]
    public float ChanceTileWillSpawnAlive = 0.45f;

    [Tunable(MinValue: 0, MaxValue: 8, Name: "Phase 1 Iterations")]
    public int PhaseOneIterations = 5;

    [Tunable(MinValue: 0, MaxValue: 8, Name: "Phase 2 Iterations")]
    public int PhaseTwoIterations = 5;

    [Tunable(MinValue: -1, MaxValue: 9, Name: "Phase 1 R1 Birth Min")]
    public int MinBirthPhase1 = 5;

    [Tunable(MinValue: -1, MaxValue: 9, Name: "Phase 1 R2 Birth Max")]
    public int MaxR2BirthPhase1 = 0;

    [Tunable(MinValue: -1, MaxValue: 9, Name: "Phase 2 R1 Birth Min")]
    public int MinBirthPhase2 = 5;

    [Tunable(MinValue: -1, MaxValue: 9, Name: "Phase 2 R2 Birth Max")]
    public int MaxR2BirthPhase2 = 0;

    public int Width = 40;
    public int Height = 40;

    [Tooltip("Setting this to false will cause the edge of the map to become rounded over time.")]
    // [Tunable(MinValue: false, MaxValue: true, Name: "Map Edges Solid")]
    public bool TreatMapEdgesAsSolid = true;

    GameObject mapSprite;

    [Generator]
    public Tile[,] GenerateLevel(){

        Tile[,] map = new Tile[Width, Height];

        /*
            Randomly initialise the map based on the ChanceSpawnAlive parameter
        */
        for(int i=0; i<Width; i++){
            for(int j=0; j<Height; j++){
                if(Random.Range(0f, 1f) < ChanceTileWillSpawnAlive)
                    map[i,j] = new Tile(i, j, true);
                else
                    map[i,j] = new Tile(i, j, false);
            }
        }

        /*
            Iterate through the map a set number of times, updating each tile
            according to the rules of the cellular automata parameters
        */
        for(int iter=0; iter<PhaseOneIterations; iter++){
            //For each iteration, create a new map that will store the new data
            Tile [,] nextMap = new Tile[Width, Height];
            for(int i=0; i<Width; i++){
                for(int j=0; j<Height; j++){
                    //A tile is updated based on its neighbours.
                    int R1Neighbours = CountBlockingNeighbours(map, i, j, 1);
                    int R2Neighbours = CountBlockingNeighbours(map, i, j, 2);
                    if(R1Neighbours >= MinBirthPhase1 || R2Neighbours <= MaxR2BirthPhase1)
                        nextMap[i,j] = new Tile(i, j, true);
                    else
                        nextMap[i,j] = new Tile(i, j, false);
                }
            }
            //Update the map to point to the next iteration, then continue;
            map = nextMap;
        }

        for(int iter=0; iter<PhaseTwoIterations; iter++){
            //For each iteration, create a new map that will store the new data
            Tile [,] nextMap = new Tile[Width, Height];
            for(int i=0; i<Width; i++){
                for(int j=0; j<Height; j++){
                    //A tile is updated based on its neighbours.
                    int R1Neighbours = CountBlockingNeighbours(map, i, j, 1);
                    int R2Neighbours = CountBlockingNeighbours(map, i, j, 2);
                    if(R1Neighbours >= MinBirthPhase2 || R2Neighbours <= MaxR2BirthPhase2)
                        nextMap[i,j] = new Tile(i, j, true);
                    else
                        nextMap[i,j] = new Tile(i, j, false);
                }
            }
            //Update the map to point to the next iteration, then continue;
            map = nextMap;
        }

        return map;
    }

    /*
        Counts the tiles adjacent to (x,y) that are solid. Considers edge tiles
        to be next to solid tiles depending on the TreatMapEdgesAsSolid param.
    */
    public int CountBlockingNeighbours(Tile[,] map, int x, int y, int dist=1){
        int count = 0;
        for(int i=-dist; i<dist+1; i++){
            for(int j=-dist; j<dist+1; j++){
                int dx = x+i; int dy = y+j;
                if(dx < 0 || dx >= map.GetLength(0) || dy < 0 || dy >= map.GetLength(1)){
                    if(TreatMapEdgesAsSolid)
                        count++;
                }
                else{
                    if(map[dx,dy].BLOCKS_MOVEMENT)
                        count++;
                }
            }
        }
        return count;
    }

    [Visualiser]
    public Texture2D RenderMap(object _m, Texture2D tex){
        Tile[,] map = (Tile[,]) _m;

        tex = new Texture2D (8 * map.GetLength(0), 8 * map.GetLength(1), TextureFormat.ARGB32, false);
        int sf = 8; int Width = map.GetLength(0); int Height = map.GetLength(1);
        // int sf = 10;
        Texture2D gnd = Resources.Load<Texture2D>("tiles/tile_17");

        for(int i=0; i<Width; i++){
            for(int j=0; j<Height; j++){
                if(map[i,j].BLOCKS_MOVEMENT){
                    VisUtils.PaintTexture(tex, i, j, sf, gnd, 8, 8);
                    VisUtils.PaintTexture(tex, i, j, sf, selectTexture(map, i, j), 8, 8);

                }
                else{
                    VisUtils.PaintTexture(tex, i, j, sf, gnd, 8, 8);
                }
            }
        }

        tex.Apply();
        return tex;
    }

    Dictionary<string, string> autotiles;

    Texture2D selectTexture(Tile[,] map, int x, int y){
        if(autotiles == null){
            autotiles = new Dictionary<string, string>();
            autotiles.Add("0000", "01");
            autotiles.Add("0001", "09");
            autotiles.Add("0010", "05");
            autotiles.Add("0011", "13");
            autotiles.Add("0100", "03");
            autotiles.Add("0101", "11");
            autotiles.Add("0110", "07");
            autotiles.Add("0111", "15");
            autotiles.Add("1000", "02");
            autotiles.Add("1001", "10");
            autotiles.Add("1010", "06");
            autotiles.Add("1011", "14");
            autotiles.Add("1100", "04");
            autotiles.Add("1101", "12");
            autotiles.Add("1110", "08");
            autotiles.Add("1111", "16");
        }

        string name = "";
        if(y == map.GetLength(1)-1 || map[x,y+1].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";
        if(x == map.GetLength(0)-1 || map[x+1,y].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";
        if(y == 0 || map[x,y-1].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";
        if(x == 0 || map[x-1,y].BLOCKS_MOVEMENT)
            name += "1";
        else
            name += "0";

        return Resources.Load<Texture2D>("tiles/tile_"+autotiles[name]);
    }

    public Color solidColor = Color.white;
    public Color wallColor = Color.black;

    void PaintPoint(Texture2D tex, int _x, int _y, int scaleFactor, Color c){
        int x = _x*scaleFactor; int y = _y*scaleFactor;
        for(int i=x; i<x+scaleFactor; i++){
            for(int j=y; j<y+scaleFactor; j++){
                tex.SetPixel(i, j, c);
            }
        }
    }
}
