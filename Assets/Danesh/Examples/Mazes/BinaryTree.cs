using UnityEngine;
using System.Collections;

public class BinaryTree : AbstractMazeGenerator {

    [Generator]
    public int[,] GenerateMaze(){
        maze = new int[size, size];

        bool canSouth = false;
        bool canEast = false;
        for(int i=0; i<size; i++){
            for(int j=0; j<size; j++){
                canEast = i < (size-1);
                canSouth = j < (size-1);
                if(canEast && canSouth){
                    canEast = Random.Range(0f, 1f) < 0.5f;
                    canSouth = !canEast;
                }
                if(canEast){
                    Set(i, j, EAST);
                }
                if(canSouth){
                    Set(i, j, SOUTH);
                }
            }
        }

        Set(0, 0, START, false);

        Set(size-1, size-1, END, false);

        return maze;
    }

}
