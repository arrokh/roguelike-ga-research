	using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellularAutomataGenerator : MonoBehaviour {

	[Tunable(MinValue: 0.2f, MaxValue: 0.6f, Name:"Initial Solid Chance")]
	public float ChanceTileWillSpawnAlive = 0.45f;

	[Tunable(MinValue: 0, MaxValue: 8, Name: "No. Of Iterations")]
	public int NumberOfIterations = 5;

	[Tunable(MinValue: 2, MaxValue: 6, Name: "Death Limit")]
	public int StarvationLowerLimit = 2;

	[Tunable(MinValue: 2, MaxValue: 6, Name: "Birth Limit")]
	public int BirthCount = 3;

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
		for(int iter=0; iter<NumberOfIterations; iter++){
			//For each iteration, create a new map that will store the new data
			Tile [,] nextMap = new Tile[Width, Height];
			for(int i=0; i<Width; i++){
				for(int j=0; j<Height; j++){
					//A tile is updated based on its neighbours.
					int NumberOfNeighbours = CountBlockingNeighbours(map, i, j);
					//'Live' cells become dead (empty) if they are too isolated.
					if(map[i,j].BLOCKS_MOVEMENT){
						if(NumberOfNeighbours <= StarvationLowerLimit)
							nextMap[i,j] = new Tile(i, j, false);
						else
							nextMap[i,j] = new Tile(i, j, true);
					}
					//Dead cells become alive (solid) if they are surrounded by enough live cells
					else{
						if(NumberOfNeighbours >= BirthCount)
							nextMap[i,j] = new Tile(i, j, true);
						else
							nextMap[i,j] = new Tile(i, j, false);
					}
				}
			}
			//Update the map to point to the next iteration, then continue;
			map = nextMap;
		}

		Debug.Log(map[20, 20].x + " | " + map[20, 20].y + " | " + map[20, 20].BLOCKS_MOVEMENT);

		return map;
	}

	/*
		Counts the tiles adjacent to (x,y) that are solid. Considers edge tiles
		to be next to solid tiles depending on the TreatMapEdgesAsSolid param.
	*/
	public int CountBlockingNeighbours(Tile[,] map, int x, int y){
		int count = 0;
		for(int i=-1; i<2; i++){
			for(int j=-1; j<2; j++){
				//Don't count yourself
				if(i == 0 && j == 0)
					continue;

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
	public Texture2D RenderMap(object _m){
		Tile[,] map = (Tile[,]) _m;

		Texture2D tex = new Texture2D (8 * map.GetLength(0), 8 * map.GetLength(1), TextureFormat.ARGB32, false);
        int sf = 8; int Width = map.GetLength(0); int Height = map.GetLength(1);
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

    [Visualiser]
    public Texture2D RenderSimpleMap(object _m){
        //Cast the object to the type we expect
        Tile[,] map = (Tile[,]) _m;

        //Create a new texture the right size for our content
        Texture2D tex = new Texture2D (map.GetLength(0), map.GetLength(1), TextureFormat.ARGB32, false);

        //For each tile in the map...
        for(int i=0; i<map.GetLength(0); i++){
            for(int j=0; j<map.GetLength(1); j++){
                //...if it's a wall tile...
                if(map[i,j].BLOCKS_MOVEMENT){
                    //...paint a black pixel on the screen
                    VisUtils.PaintPoint(tex, i, j, 1, Color.black);
                }
                else{
                    //...otherwise, paint a white pixel
                    VisUtils.PaintPoint(tex, i, j, 1, Color.white);
                }
            }
        }

        //Remember to apply the results before we return it
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

	void PaintPoint(Texture2D tex, int _x, int _y, int scaleFactor, Color c){
		int x = _x*scaleFactor; int y = _y*scaleFactor;
		for(int i=x; i<x+scaleFactor; i++){
			for(int j=y; j<y+scaleFactor; j++){
				tex.SetPixel(i, j, c);
			}
		}
	}

	/*
		Counts the tiles adjacent to (x,y) that are solid. Considers edge tiles
		to be next to solid tiles depending on the TreatMapEdgesAsSolid param.
	*/
	bool HasBlockingNeighbour(Tile[,] map, int x, int y){
		for(int i=-1; i<2; i++){
			for(int j=-1; j<2; j++){
				//Don't count yourself
				if(i == 0 && j == 0)
					continue;

				int dx = x+i; int dy = y+j;
				if(dx < 0 || dx >= map.GetLength(0) || dy < 0 || dy >= map.GetLength(1)){
					if(TreatMapEdgesAsSolid)
						return true;
					continue;
				}
				else{
					if(map[dx,dy].BLOCKS_MOVEMENT)
						return true;
				}
			}
		}
		return false;
	}
}
