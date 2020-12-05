using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    This generator is inspired by Spelunky's level generator, which combines pre-made
    chunks with metadata that lets the generator know how to slot things together to
    make levels with certain properties (like accessibility of the exit)
*/
public class ChunkBasedPlatformerGenerator : MonoBehaviour {

    public int widthInChunks = 4;
    public int heightInChunks = 4;
    public int tilesPerChunk = 8;

    [Tunable(MinValue: 0f, MaxValue: 1f, Name:"Chance To Block Tiles")]
    public float chanceToMutateTiles = 0.2f;
    [Tunable(MinValue: 0f, MaxValue: 1f, Name:"Chance To Spawn Gold")]
    public float chanceToSpawnGold = 0.2f;
    [Tunable(MinValue: 0f, MaxValue: 1f, Name:"Chance To Spawn Enemy")]
    public float chanceToSpawnEnemy = 0.2f;

    int CODE_START = -1;
    int CODE_EXIT = -2;
    int CODE_GOLD = -3;
    int CODE_ENEMY = -4;

    [Generator]
    public ChunkyLevel GenerateLevel(){
        //First, we generate the direction map. How do we get from the start to the end?
        LevelChunk[,] template = new LevelChunk[widthInChunks, heightInChunks];
        //Pick a random top tile for start and a random bottom tile for exit
        int sx = Random.Range(0, widthInChunks); int sy = 0;
        int osx = sx; int osy = sy;
        int ex = Random.Range(0, widthInChunks); int ey = heightInChunks-1;
        //Now randomly map from the start to the finish. We're basing our approach on how
        //Spelunky did things, but pretty loosely - the general idea is there.
        //If you want to read more: http://tinysubversions.com/spelunkyGen/index.html
        int rounds = 0;
        int dir = Random.Range(-1, 2);
        bool[] lastDir = new bool[]{false, true, false, true};

        //We store this in the level just for convenience so we can calculate some metrics
        List<Vector2> path = new List<Vector2>();

        path.Add(new Vector2(sx, sy));

        while((sx != ex || sy != ey) && rounds++ < 100){
            if(dir < 0 && sx > 0){
                template[sx, sy] = FindChunk(lastDir[0], lastDir[1], lastDir[2], true);
                sx--;
                path.Add(new Vector2(sx, sy));
                lastDir = new bool[]{false, true, false, false};
                if(sy < heightInChunks-1)
                    dir = new int[]{dir, dir, 0}[Random.Range(0, 3)];
            }
            else if(dir > 0 && sx < widthInChunks-1){
                template[sx, sy] = FindChunk(lastDir[0], true, lastDir[2], lastDir[3]);
                sx++;
                path.Add(new Vector2(sx, sy));
                lastDir = new bool[]{false, false, false, true};
                if(sy < heightInChunks-1)
                    dir = new int[]{dir, dir, 0}[Random.Range(0, 3)];
            }
            else{
                if(sy < heightInChunks-1){
                    template[sx, sy] = FindChunk(lastDir[0], lastDir[1], true, lastDir[3]);
                    sy++;
                    path.Add(new Vector2(sx, sy));
                    lastDir = new bool[]{true, false, false, false};
                    if(sy == heightInChunks-1){
                        //Now we should only move towards the exit
                        if(sx < ex){
                            dir = 1;
                        }
                        else{
                            dir = -1;
                        }
                    }
                    else{
                        if(sx == 0)
                            dir = new int[]{1, 1, 0}[Random.Range(0, 3)];
                        else if(sx == widthInChunks-1)
                            dir = new int[]{-1, -1, 0}[Random.Range(0, 3)];
                        else
                            dir = new int[]{-1, -1, 0, 1, 1}[Random.Range(0, 5)];
                    }
                }
            }
        }

        //Pop the last chunk in to set up the exit room properly
        template[sx, sy] = FindChunk(lastDir[0], lastDir[1], lastDir[2], lastDir[3]);

        //Now we've added chunks in, we simply need to fill in the remaining chunks with random stuff
        for(int i=0; i<widthInChunks; i++){
            for(int j=0; j<heightInChunks; j++){
                if(template[i,j] == null){
                    template[i,j] = ChunkCatalogue.StandardChunks[Random.Range(0, ChunkCatalogue.StandardChunks.Length)];
                }
            }
        }

        //Now we make a pass in which we copy the tiles into a real map
        int[,] map = new int[widthInChunks * tilesPerChunk, heightInChunks * tilesPerChunk];
        for(int i=0; i<widthInChunks; i++){
            for(int j=0; j<heightInChunks; j++){
                for(int x=0; x<tilesPerChunk; x++){
                    for(int y=0; y<tilesPerChunk; y++){
                        //Due to the funky way we defined the tiles in the room catalog, note that
                        //the indices into the tiles themselves are reversed (y co-ord first)
                        map[i*tilesPerChunk + x, j*tilesPerChunk + y] = template[i,j].tiles[y,x];
                    }
                }
            }
        }

        //Now the structure has been pasted in, we go over and search for special tiles (code: 2)
        //These tiles have a chance to be turned into blocks, gold or enemies
        //This isn't how Derek does it, but this generator is a bit rougher and readier than the real thing
        for(int i=0; i<widthInChunks; i++){
            for(int j=0; j<heightInChunks; j++){
                for(int x=0; x<tilesPerChunk; x++){
                    for(int y=0; y<tilesPerChunk; y++){
                        if(map[i*tilesPerChunk + x, j*tilesPerChunk + y] == 2 && Random.Range(0f, 1f) < chanceToMutateTiles){
                            map[i*tilesPerChunk + x, j*tilesPerChunk + y] = 1;
                        }
                        if((j*tilesPerChunk) + y+1 < map.GetLength(1) && map[i*tilesPerChunk + x, (j*tilesPerChunk) + y] == 2 && map[i*tilesPerChunk + x, (j*tilesPerChunk) + y + 1] == 1 && Random.Range(0f, 1f) < chanceToSpawnGold){
                            map[i*tilesPerChunk + x, j*tilesPerChunk + y] = CODE_GOLD;
                        }
                        if((j*tilesPerChunk) + y+1 < map.GetLength(1) && map[i*tilesPerChunk + x, (j*tilesPerChunk) + y] == 2 && map[i*tilesPerChunk + x, (j*tilesPerChunk) + y + 1] == 1 && Random.Range(0f, 1f) < chanceToSpawnEnemy){
                            map[i*tilesPerChunk + x, j*tilesPerChunk + y] = CODE_ENEMY;
                        }
                    }
                }
                if(osx == i && osy == j){
                    map = PlaceRandomly(map, i, j, CODE_START);
                }
                if(ex == i && ey == j){
                    map = PlaceRandomly(map, i, j, CODE_EXIT);
                }
            }
        }

        //As cleanup, fill in the map edges
        for(int i=0; i<map.GetLength(0); i++){
            for(int j=0; j<map.GetLength(1); j++){
                if(i == 0 || j == 0 || i == map.GetLength(0)-1 || j == map.GetLength(1)-1){
                    map[i,j] = 1;
                }
            }
        }

        ChunkyLevel level = new ChunkyLevel();
        level.tiles = map;
        level.path = path;
        level.chunks = template;

        return level;
    }

    public int[,] PlaceRandomly(int[,] map, int i, int j, int code){
        //Randomly pick a spot to place the start in
        int rx = Random.Range(0, tilesPerChunk);
        int ry = Random.Range(0, tilesPerChunk);
        for(int x=0; x<tilesPerChunk; x++){
            for(int y=0; y<tilesPerChunk; y++){
                if(((y + ry)%tilesPerChunk) > 1){
                    if(map[i*tilesPerChunk + ((x + rx)%tilesPerChunk), j*tilesPerChunk + ((y + ry)%tilesPerChunk)] == 1
                        && map[i*tilesPerChunk + ((x + rx)%tilesPerChunk), j*tilesPerChunk + ((y-1 + ry)%tilesPerChunk)] == 0){
                        map[i*tilesPerChunk + ((x + rx)%tilesPerChunk), j*tilesPerChunk + ((y-1 + ry)%tilesPerChunk)] = code;
                        return map;
                    }
                }
            }
        }
        return map;
    }

    [Visualiser]
    public Texture2D RenderSimpleMap(object _m){
        //Cast the object to the type we expect
        int[,] map = ((ChunkyLevel) _m).tiles;

        int sf = 16;

        //Create a new texture the right size for our content
        Texture2D tex = new Texture2D (map.GetLength(0)*sf, map.GetLength(1)*sf, TextureFormat.ARGB32, false);

        Texture2D wall = Resources.Load<Texture2D>("tiles/chunky_wall");
        Texture2D empty = Resources.Load<Texture2D>("tiles/chunky_empty");
        Texture2D start = Resources.Load<Texture2D>("tiles/chunky_player");
        Texture2D goal = Resources.Load<Texture2D>("tiles/chunky_goal");
        Texture2D gold = Resources.Load<Texture2D>("tiles/chunky_gold");
        Texture2D enemy = Resources.Load<Texture2D>("tiles/chunky_enemy");

        //For each tile in the map...
        for(int i=0; i<map.GetLength(0); i++){
            for(int j=0; j<map.GetLength(1); j++){
                //...if it's a wall tile...
                if(map[i,j] == 1){
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, wall, sf, sf);
                }
                else if(map[i,j] == CODE_START){
                    //Start point
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, empty, sf, sf);
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, start, sf, sf);
                }
                else if(map[i,j] == CODE_EXIT){
                    //End point
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, empty, sf, sf);
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, goal, sf, sf);
                }
                else if(map[i,j] == CODE_GOLD){
                    //Gold
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, empty, sf, sf);
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, gold, sf, sf);
                }
                else if(map[i,j] == CODE_ENEMY){
                    //Enemy
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, empty, sf, sf);
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, enemy, sf, sf);
                }
                else{
                    VisUtils.PaintTexture(tex, i, map.GetLength(1)-j-1, sf, empty, sf, sf);
                }
            }
        }

        //Remember to apply the results before we return it
        tex.Apply();

        return tex;
    }

    void Start(){
        Texture2D tex = RenderSimpleMap(GenerateLevel());
        GameObject.Find("Plane").GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
    }

    public LevelChunk FindChunk(bool top, bool right, bool down, bool left){
        //Pick a random offset into the array
        int randomOffset = Random.Range(0, ChunkCatalogue.StandardChunks.Length);
        for(int i=0; i<ChunkCatalogue.StandardChunks.Length; i++){
            LevelChunk c = ChunkCatalogue.StandardChunks[(i+randomOffset) % ChunkCatalogue.StandardChunks.Length];
            //We want a chunk that is at least as connected as our requirements (more is ok)
            if( ((c.exitTop && top) || !top) &&
                ((c.exitRight && right) || !right) &&
                ((c.exitBelow && down) || !down) &&
                ((c.exitLeft && left) || !left)){
                return c;
            }
        }
        //oops, we goofed it
        return null;
    }

}
