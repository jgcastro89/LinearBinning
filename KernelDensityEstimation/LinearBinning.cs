using HelperFunctions;
using Microsoft.FSharp.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KernelDensityEstimation
{
    class LinearBinning
    {
        static void Main(string[] args)
        {
            LinearBinning kde = new();

            kde.CreatePointsSetFromSequences();
            kde.GenerateGridFromSampleData();
            kde.InitiateDataBinning();
        }

        public void GenerateGridFromSampleData()
        {
            Grid _ = new(SampleDistribution2D.seq0, SampleDistribution2D.seq1, 100);
            Grid2D = _.Grid2D;
            XAxisSeq = _.XAxis;
            YAxisSeq = _.YAxis;
        }

        public void InitiateDataBinning()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            Parallel.ForEach(Points, point =>
            {
                int[] xNodes = SampleDistribution2D.FindGridNodes(XAxisSeq, point.X);
                int XLowerBoundGridIndex = xNodes[0];
                int XUpperBoundGridIndex = xNodes[1];

                int[] yNodes = SampleDistribution2D.FindGridNodes(YAxisSeq, point.Y);
                int YLowerBoundGridIndex = yNodes[0];
                int YUpperBoundGridIndex = yNodes[1];

                lock (Grid2D)
                {
                    Grid2D[XLowerBoundGridIndex, YUpperBoundGridIndex] += 1;
                    Grid2D[XLowerBoundGridIndex, YLowerBoundGridIndex] += 1;
                    Grid2D[XUpperBoundGridIndex, YLowerBoundGridIndex] += 1;
                    Grid2D[XUpperBoundGridIndex, YUpperBoundGridIndex] += 1;
                }
            });
            watch.Stop();
            System.Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
        }

        public void CreatePointsSetFromSequences()
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