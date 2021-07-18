using HelperFunctions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KernelDensityEstimation
{
    internal class LinearBinning
    {
        private static void Main(string[] args)
        {
            LinearBinning kde = new();

            SampleDistribution2D.GenerateSampleData(50000000);
            kde.Generate2DGridFromSampleData(200);
            kde.CreatePointsSetFromSequences();

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            kde.DivideAndConquer();

            watch.Stop();
            System.Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            SampleDistribution2D.Chart3D(kde.GridMap2D, kde.XAxisSeq, kde.YAxisSeq);
            SampleDistribution2D.ChartContour(kde.GridMap2D);
        }

        private void Generate2DGridFromSampleData(int gridSize)
        {
            var grid = SampleDistribution2D.Create2DGrid(SampleDistribution2D.xCoordinate, SampleDistribution2D.yCoordinate, gridSize);

            XAxisSeq = grid.Item1;
            YAxisSeq = grid.Item2;
            GridMap2D = grid.Item3;
        }

        private void DivideAndConquer()
        {
            Parallel.ForEach(Points, point =>
            {
                Tuple<int, int> xNodes = SampleDistribution2D.FindNearestGridNodes(XAxisSeq, point.X);
                int xLowerBoundGridIndex = xNodes.Item1;
                int xUpperBoundGridIndex = xNodes.Item2;

                Tuple<int, int> yNodes = SampleDistribution2D.FindNearestGridNodes(YAxisSeq, point.Y);
                int yLowerBoundGridIndex = yNodes.Item1;
                int yUpperBoundGridIndex = yNodes.Item2;

                lock (GridMap2D)
                {
                    GridMap2D[xLowerBoundGridIndex, yUpperBoundGridIndex] ++;
                    GridMap2D[xLowerBoundGridIndex, yLowerBoundGridIndex] ++;
                    GridMap2D[xUpperBoundGridIndex, yLowerBoundGridIndex] ++;
                    GridMap2D[xUpperBoundGridIndex, yUpperBoundGridIndex] ++;
                }
            });
        }
        private void CreatePointsSetFromSequences()
        {
            Points = new List<CoordinatePoint>();

            for (int i = 0; i < SampleDistribution2D.xCoordinate.Length; i++)
                Points.Add(new CoordinatePoint(SampleDistribution2D.xCoordinate[i], SampleDistribution2D.yCoordinate[i]));
        }

        private List<CoordinatePoint> Points { get; set; }
        private double[,] GridMap2D { get; set; }
        private double[] XAxisSeq { get; set; }
        private double[] YAxisSeq { get; set; }
    }
}