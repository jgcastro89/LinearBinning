namespace HelperFunctions

open FSharp.Stats
open FSharp.Stats.Distributions
open System.Threading.Tasks

module Message =
    let mutable message = "Hello World"

type Random2DSample (mu: float, std: float, size: int) =
    // normal distributions with differing standard deviations
    let normal_0 = Continuous.normal mu std
    let normal_1 = Continuous.normal mu (std/2.)

    member this.sample0 = Array.init size (fun _ -> normal_0.Sample())
    member this.sample1 = Array.init size (fun _ -> normal_1.Sample())

type CoordinatePoint (x: float, y:float) =
    struct
        member _.X = x
        member _.Y = y
    end

module SampleDistribution2D = 
    let mutable seed = 1
    Random.SetSampleGenerator(Random.RandThreadSafe(seed))

    let private random2DSample = new Random2DSample(0.0, 1.0, 20000000)
    let seq0 = Array.map2 (+) random2DSample.sample0 random2DSample.sample1
    let seq1 = Array.map2 (-) random2DSample.sample0 random2DSample.sample1

    let Step arr size:float = 
        ((arr |> Array.max) - (arr |> Array.min)) / size

    let LinSpace (arr, size:float) = 
        let AxisSize = size - 1.0
        [(arr |> Array.min) .. (Step arr AxisSize) .. (arr |> Array.max)]

    let print arr = 
        printfn "%A" arr

    let numberOfPoints =
        seq0 |> Array.length

    let FindGridNodes (axisSeq: List<float>, point:float) =
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

        [|lowerBound; upperBound|]

type Grid (arr0, arr1, size) =
    let GridSize = size-1.0

    member this.YAxis = (arr1, GridSize) |> SampleDistribution2D.LinSpace
    member this.XAxis = (arr0, GridSize) |> SampleDistribution2D.LinSpace

    member this.Grid2D = 
        Array2D.init (this.XAxis.Length + 1) (this.YAxis.Length + 1) (fun x y -> 0.0)