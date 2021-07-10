namespace HelperFunctions

open FSharp.Stats
open FSharp.Stats.Distributions

type Random2DSample (mu: float, std: float, size: int) =
    // normal distributions with differing standard deviations
    let normal_0 = Continuous.normal mu std
    let normal_1 = Continuous.normal mu (std/2.)

    member this.sample0 = Array.init size (fun _ -> normal_0.Sample())
    member this.sample1 = Array.init size (fun _ -> normal_1.Sample())

type CoordinatePoint (x: float, y:float) =
    struct
        member this.X = x
        member this.Y = y
    end

module SampleDistribution2D = 
    let mutable seed = 1
    let mutable Seq0 = [||]
    let mutable Seq1 = [||]
    let mutable Points = [||]

    Random.SetSampleGenerator(Random.RandThreadSafe(seed))

    let Step (arr, size:float) = 
        ((arr |> Array.max) - (arr |> Array.min)) / size

    let LinSpace (arr, size:float) = 
        let axisLegnth = size - 1.0
        [(arr |> Array.min) .. ((arr, axisLegnth) |> Step) .. (arr |> Array.max)]

    let GenerateSampleData sampleSize =
        let random2DSample = new Random2DSample(0.0, 1.0, sampleSize)
        Seq0 <- Array.map2 (+) random2DSample.sample0 random2DSample.sample1
        Seq1 <- Array.map2 (-) random2DSample.sample0 random2DSample.sample1
        Seq0, Seq1

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

