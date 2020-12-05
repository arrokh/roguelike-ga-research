using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeMetrics : AbstractMazeGenerator {

    [Metric("Dead End %")]
    public static float Terminals(object _map){
        int[,] maze = (int[,]) _map;

        float totalSize = maze.GetLength(0) * maze.GetLength(1);
        float deadEnds = 0;

        for(int i=0; i<maze.GetLength(0); i++){
            for(int j=0; j<maze.GetLength(1); j++){
                if(CountExits(i, j, maze) == 1)
                    deadEnds++;
            }
        }

        return deadEnds/totalSize;
    }

    static int CountExits(int x, int y, int[,] maze){
        int cnt = 0;
        for(int i=0; i<4; i++){
            if(AbstractMazeGenerator.CanMove(x, y, AbstractMazeGenerator.directions[i], maze)){
                cnt++;
            }
        }
        return cnt;
    }

    [Metric("Branch %")]
    public static float Branches(object _map){
        int[,] maze = (int[,]) _map;

        float totalSize = maze.GetLength(0) * maze.GetLength(1);
        float branches = 0;

        for(int i=0; i<maze.GetLength(0); i++){
            for(int j=0; j<maze.GetLength(1); j++){
                if(CountExits(i, j, maze) > 2)
                    branches++;
            }
        }

        return branches/totalSize;
    }

    [Metric("Corridor %")]
    public static float Corridors(object _map){
        int[,] maze = (int[,]) _map;

        float totalSize = maze.GetLength(0) * maze.GetLength(1);
        float branches = 0;

        for(int i=0; i<maze.GetLength(0); i++){
            for(int j=0; j<maze.GetLength(1); j++){
                if(CountExits(i, j, maze) == 2)
                    branches++;
            }
        }

        return branches/totalSize;
    }

    [Metric("Path Length")]
    public static float Path(object _map){
        int[,] maze = (int[,]) _map;

        Point start = new Point(-1, -1);
        Point end = new Point(-1, -1);

        for(int i=0; i<maze.GetLength(0); i++){
            for(int j=0; j<maze.GetLength(1); j++){
                if((maze[i,j] & START) != 0){
                    start = new Point(i, j);
                }
                if((maze[i,j] & END) != 0){
                    end = new Point(i, j);
                }
            }
        }

        List<Point> openList = new List<Point>();
        List<Point> closedList = new List<Point>();

        openList.Add(start);

        float pathLength = 0f;

        while(openList.Count > 0){
            Point p = openList[0];
            openList.RemoveAt(0); closedList.Add(p);

            if(p.x == end.x && p.y == end.y){
                pathLength = p.g;
                break;
            }

            List<Point> ns = GetNeighbours(p, maze, openList, closedList);
            foreach(Point n in ns){
                openList.Add(n);
            }

            openList.Sort(
                delegate(Point p1, Point p2)
                {
                    float f1 = p1.g + Manhattan(p1, end);
                    float f2 = p2.g + Manhattan(p2, end);
                    return f1.CompareTo(f2);
                }
            );
        }

        return pathLength / (maze.GetLength(0)*maze.GetLength(1));
    }

    public static int Manhattan(Point p, Point x){
        return (int) (Mathf.Abs(p.x - x.x) + Mathf.Abs(p.y - x.y));
    }

    public static List<Point> GetNeighbours(Point p, int[,] maze, List<Point> openList, List<Point> closedList){
        List<Point> ps = new List<Point>();
        foreach(int d in directions){
            if(CanMove(p.x, p.y, d, maze)){
                Point np = new Point(p.x + DX[d], p.y + DY[d], p);

                if(closedList.Contains(np)){
                    continue;
                }

                np.g = p.g+1;

                bool ad = true;
                foreach(Point op in openList){
                    if(op == np){
                        ad = false;
                        if(np.g < op.g){
                            op.g = np.g;
                            op.parent = np.parent;
                        }
                        break;
                    }
                }

                if(ad){
                    ps.Add(np);
                }
            }
        }
        return ps;
    }

    public class Point{
        public Point parent;
        public int x;
        public int y;

        public float g;

        public Point(int x, int y){
            this.x = x;
            this.y = y;
        }

        public Point(int x, int y, Point parent){
            this.x = x;
            this.y = y;
            this.parent = parent;
        }

        public override bool Equals(object obj)
        {
            Point pt = (Point) obj;
            return pt.x == this.x && pt.y == this.y;
        }
    }

}
