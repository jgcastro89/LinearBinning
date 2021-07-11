namespace HelperFunctions

open FSharp.Stats
open Plotly.NET
open MxNet

type CoordinatePoint (x: float, y:float) =
    struct
        member this.X = x
        member this.Y = y
    end

module SampleDistribution2D = 
    let mutable seed = 10
    let mutable Seq0 = [||]
    let mutable Seq1 = [||]
    let mutable Points = [||]

    Random.SetSampleGenerator(Random.RandThreadSafe(seed))

    let Step (arr, size:float) = 
        ((arr |> Array.max) - (arr |> Array.min)) / size

    let LinSpace (arr, size:float) = 
        let axisLegnth = size - 1.0
        [(arr |> Array.min) .. ((arr, axisLegnth) |> Step) .. (arr |> Array.max)]

    let Chart2D (xData: seq<float>, yData: seq<float>) =
        Chart.Point(xData, yData)
        |> Chart.withTitle "Kernel Density Estimate"
        |> Chart.withSize(width=2600., height=1400.)
        |> Chart.Show

    let Chart3D (grid: float[,], xAxis: List<float>, yAxis: List<float>) =
        let gridAxisLegnth = int (sqrt(float grid.Length)) - 1
        let data = seq {for i in 0 .. gridAxisLegnth do 
                        seq { for j in 0 .. gridAxisLegnth 
                            do yield grid.[i, j]} }

        Chart.Heatmap(data, xAxis, yAxis)
        |> Chart.withTitle "Kernel Density Estimate"
        |> Chart.withSize(width=2600., height=1400.)
        |> Chart.Show

    let ChartContour (grid: float[,]) =
        let gridAxisLegnth = int (sqrt(float grid.Length)) - 1
        let data = seq {for i in 0 .. gridAxisLegnth do 
                        seq { for j in 0 .. gridAxisLegnth 
                            do yield grid.[i, j]} }
        data 
        |> Chart.Contour
        |> Chart.withSize(2600., 1400.)
        |> Chart.Show

    let GenerateSampleData (size:int) =
        let mutable sample0 = Array.init size (fun _ -> 0.0)
        let mutable sample1 = Array.init size (fun _ -> 0.0)

        nd.Random.Normal(shape=new Shape(size)).ArrayData.CopyTo(sample0, 0)
        nd.Random.Normal(shape=new Shape(size), scale=float32 0.5).ArrayData.CopyTo(sample1, 0)

        Seq0 <- Array.map2 (+) sample0 sample1
        Seq1 <- Array.map2 (-) sample0 sample1        
        Seq0, Seq1

    let GenerateAndViewSampleData sampleSize =
        sampleSize
        |> GenerateSampleData
        |> Chart2D

    let FindNearestGridNodes (axisSeq: List<float>, point:float) =
        let mutable pivot = axisSeq.Length / 2
        let mutable upperBound = axisSeq.Length
        let mutable lowerBound = 0

        while (upperBound - lowerBound > 1) do
            if point <= axisSeq.[pivot] then
                upperBound <- pivot
                pivot <- (pivot - (pivot - lowerBound) /2)
            elif point >= axisSeq.[pivot] then
                lowerBound <- pivot
                pivot <- (pivot + (upperBound - pivot) / 2) 

        lowerBound, upperBound

    let CreatePointSetFromSampleData (seq0: array<float>, seq1: array<float>) = 
        let listSize = seq0 |> Array.length
        Points <- Array.init<CoordinatePoint> listSize (fun i -> new CoordinatePoint(seq0.[i], seq1.[i]))

    let Create2DGrid (arr0, arr1, size) = 
        let GridSize = size-1.0
        
        let YAxis = (arr1, GridSize) |> LinSpace
        let XAxis = (arr0, GridSize) |> LinSpace
        
        let GridMap2D = Array2D.init (XAxis.Length + 1) (YAxis.Length + 1) (fun x y -> 0.0)

        XAxis, YAxis, GridMap2D