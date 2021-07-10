using HelperFunctions;
using Microsoft.FSharp.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KernelDensityEstimation
{
    internal class LinearBinning
    {
        private static void Main(string[] args)
        {
            LinearBinning kde = new();

            SampleDistribution2D.GenerateSampleData(20000);
            kde.CreatePointsSetFromSequences();
            kde.GenerateGridFromSampleData();
            kde.InitiateDataBinning();
        }

        private void GenerateGridFromSampleData()
        {
            Grid g = new(SampleDistribution2D.seq0, SampleDistribution2D.seq1, 100);
            Grid2D = g.Grid2D;
            XAxisSeq = g.XAxis;
            YAxisSeq = g.YAxis;
        }

        private void InitiateDataBinning()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            Parallel.ForEach(Points, point =>
            {
                int[] xNodes = SampleDistribution2D.FindNearestGridNodes(XAxisSeq, point.X);
                int xLowerBoundGridIndex = xNodes[0];
                int xUpperBoundGridIndex = xNodes[1];

                int[] yNodes = SampleDistribution2D.FindNearestGridNodes(YAxisSeq, point.Y);
                int yLowerBoundGridIndex = yNodes[0];
                int yUpperBoundGridIndex = yNodes[1];

                lock (Grid2D)
                {
                    Grid2D[xLowerBoundGridIndex, yUpperBoundGridIndex] += 1;
                    Grid2D[xLowerBoundGridIndex, yLowerBoundGridIndex] += 1;
                    Grid2D[xUpperBoundGridIndex, yLowerBoundGridIndex] += 1;
                    Grid2D[xUpperBoundGridIndex, yUpperBoundGridIndex] += 1;
                }
            });
            watch.Stop();
            System.Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
        }

        private void CreatePointsSetFromSequences()
        {
            Points = new List<CoordinatePoint>();

            for (int i = 0; i < SampleDistribution2D.seq0.Length; i++)
                Points.Add( new CoordinatePoint(SampleDistribution2D.seq0[i], SampleDistribution2D.seq1[i]));
        }

        private List<CoordinatePoint> Points { get; set; }
        private double[,] Grid2D { get; set; }
        private FSharpList<double> XAxisSeq { get; set; }
        private FSharpList<double> YAxisSeq { get; set; }
    }
}