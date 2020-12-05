using UnityEngine;
using System.Collections;

public class ChunkCatalogue {

    /*
        This is a list of chunks that can be used for level generation.
        If you were really building a game like Spelunky you probably wouldn't
        define them like this. We're doing it because it's quick and simple
        for a self-contained example generator.

        Add your own chunks and see them instantly used in the generator!
    */


    // 0 - Empty Space
    // 1 - Solid Wall
    // 2 - Variable Tile (used for walls, gold, and enemies based on chance rolls)
    public static LevelChunk[] StandardChunks = new LevelChunk[]{
        new LevelChunk(new int[,]{
                {1,1,1,1,1,1,1,1},
                {2,1,0,2,2,0,1,2},
                {2,1,0,0,0,0,1,2},
                {0,1,1,0,0,1,1,0},
                {0,2,2,0,0,2,2,0},
                {0,0,0,0,0,0,0,0},
                {0,2,2,0,0,2,2,0},
                {1,1,1,0,0,1,1,1},
            //Final parameters are accessibility, clockwise from top
            }, false, true, true, true),
        new LevelChunk(new int[,]{
                {1,1,1,1,1,1,1,1},
                {0,0,0,1,1,0,0,0},
                {0,0,2,1,1,2,0,0},
                {0,2,1,1,1,1,2,0},
                {0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0},
                {0,2,2,0,0,2,2,0},
                {1,1,1,0,0,1,1,1},
            }, false, true, true, true),
        new LevelChunk(new int[,]{
                {1,1,1,1,1,1,1,1},
                {0,0,0,2,2,0,0,0},
                {0,0,0,0,0,0,0,0},
                {0,0,0,2,2,0,0,0},
                {0,0,2,1,1,2,0,0},
                {0,0,1,1,1,1,0,0},
                {0,1,1,2,0,0,0,2},
                {1,1,1,0,0,1,1,1},
            }, false, true, true, true),
        new LevelChunk(new int[,]{
                {1,1,1,1,1,1,1,1},
                {0,1,1,1,1,1,1,0},
                {0,0,1,1,1,1,0,0},
                {0,0,2,2,2,2,0,0},
                {0,0,0,0,0,0,0,0},
                {0,0,1,1,1,1,0,0},
                {0,1,1,1,1,1,1,0},
                {1,1,1,1,1,1,1,1},
        }, false, true, false, true),
        new LevelChunk(new int[,]{
                {1,1,1,1,1,1,1,1},
                {2,2,2,0,0,2,2,2},
                {1,1,1,0,0,1,1,1},
                {2,2,2,0,0,2,2,2},
                {0,0,0,0,0,0,0,0},
                {0,0,1,2,2,1,0,0},
                {0,2,1,2,2,1,2,0},
                {1,1,1,1,1,1,1,1},
        }, false, true, false, true),
        new LevelChunk(new int[,]{
                {1,0,0,0,0,0,2,1},
                {1,0,0,0,0,2,2,1},
                {1,0,0,1,1,1,1,1},
                {1,2,0,0,0,2,2,1},
                {1,2,2,0,0,0,2,1},
                {1,1,1,1,1,0,0,1},
                {1,2,2,0,0,0,0,1},
                {1,2,0,0,0,0,0,1},
        }, true, false, true, false),
        new LevelChunk(new int[,]{
                {1,0,0,0,0,0,0,1},
                {1,0,0,1,1,1,1,1},
                {1,0,0,0,0,0,2,1},
                {1,0,1,1,0,0,2,1},
                {1,0,0,1,1,1,1,1},
                {1,2,0,0,0,2,2,1},
                {1,2,2,0,0,0,2,1},
                {1,1,1,1,1,0,0,1},
        }, true, false, true, false),
        new LevelChunk(new int[,]{
                {1,0,0,0,0,0,0,1},
                {0,0,0,0,0,0,0,0},
                {0,0,2,2,2,2,0,0},
                {0,2,1,1,1,1,2,0},
                {0,2,1,1,1,1,2,0},
                {0,0,2,2,2,2,0,0},
                {0,0,0,1,1,0,0,0},
                {1,1,1,1,1,1,1,1},
        }, true, true, false, true),
        new LevelChunk(new int[,]{
                {1,1,1,1,1,1,1,1},
                {0,0,0,2,2,2,2,1},
                {0,0,0,0,0,0,2,1},
                {0,0,0,2,2,0,2,1},
                {0,0,0,1,1,0,0,1},
                {0,0,1,1,2,0,0,1},
                {0,1,1,1,2,0,1,1},
                {1,1,1,1,2,0,0,1},
        }, false, false, true, true),
        new LevelChunk(new int[,]{
                {1,1,1,1,1,1,1,1},
                {1,2,2,0,0,0,0,0},
                {1,2,0,0,0,0,0,0},
                {1,2,0,1,0,0,0,0},
                {1,2,0,0,1,0,0,0},
                {1,1,1,0,0,1,0,0},
                {1,0,0,0,0,0,1,0},
                {1,0,1,1,1,1,1,1},
        }, false, true, true, false),
    };

}
