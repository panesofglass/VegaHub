// Include dependencies
#I """..\packages"""
#r """Owin.1.0\lib\net40\Owin.dll"""
#r """Microsoft.Owin.2.1.0-rc1\lib\net45\Microsoft.Owin.dll"""
#r """Microsoft.Owin.FileSystems.2.1.0-rc1\lib\net40\Microsoft.Owin.FileSystems.dll"""
#r """Microsoft.Owin.Hosting.2.1.0-rc1\lib\net45\Microsoft.Owin.Hosting.dll"""
#r """Microsoft.Owin.Security.2.1.0-rc1\lib\net45\Microsoft.Owin.Security.dll"""
#r """Microsoft.Owin.StaticFiles.2.1.0-rc1\lib\net40\Microsoft.Owin.StaticFiles.dll"""
#r """Microsoft.Owin.Host.HttpListener.2.1.0-rc1\lib\net45\Microsoft.Owin.Host.HttpListener.dll"""
#r """Newtonsoft.Json.5.0.6\lib\net45\Newtonsoft.Json.dll"""
#r """Microsoft.AspNet.SignalR.Core.2.0.1\lib\net45\Microsoft.AspNet.SignalR.Core.dll"""
#r """ImpromptuInterface.6.2.2\lib\net40\ImpromptuInterface.dll"""
#r """ImpromptuInterface.FSharp.1.2.13\lib\net40\ImpromptuInterface.FSharp.dll"""
#r """FSharp.Data.1.1.10\lib\net40\FSharp.Data.dll"""

// Reference VegaHub
#r """..\src\bin\Debug\VegaHub.dll"""

// Reference MathNet.Numerics
#r """MathNet.Numerics.2.6.2\lib\net40\MathNet.Numerics.dll"""
#r """MathNet.Numerics.FSharp.2.6.0\lib\net40\MathNet.Numerics.FSharp.dll"""

open System
open System.Text
open System.Text.RegularExpressions
open VegaHub
open VegaHub.Grammar
open VegaHub.Basics
open FSharp.Data
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double

// ACQUIRING DATA

type Sample = CsvProvider<"ads.csv">
type Ad = Sample.Row


// LINEAR REGRESSION MODEL

let rng = Random(42)

let  reducedTheta (theta: Vector) = 
    theta 
    |> Vector.mapi (fun i x -> if i = 0 then 0. else x)

let cost (x: DenseMatrix) (y: DenseVector) (w: DenseVector) (theta: DenseVector) (lambda: float) = 
    let m = y.Count |> (float)
    let theta' = reducedTheta theta
    ((y - x * theta).PointwiseMultiply(w) * (y - x * theta)) / (1. * m) 
    + (lambda / (2. * m)) * (theta' * theta')

let  descentUpdate (x: DenseMatrix) (y: DenseVector) (w: DenseVector) (theta: DenseVector) (alpha: float) (lambda: float) =
    let m = y.Count |> (float)
    let gradient = 
        ((1. / m) * (y - (x * theta)).PointwiseMultiply(w) * x
        - (1. / m) * lambda * (reducedTheta theta)) :?> DenseVector
    (theta + alpha * gradient) 

type Convergence = {
    Theta:DenseVector;
    Cost:float;
    Alpha:float;
    Iteration:int;
    Decreases:int;
    Limit:int; }

type Adjustment =
    | Done of (DenseVector*float)
    | Next of Convergence

type Tuner = Convergence -> (DenseVector*float) -> Adjustment
 
let noTuner iters convergence (theta,cost) =
    if convergence.Iteration > iters
    then Done(theta,cost)
    else Next({convergence with Iteration = convergence.Iteration + 1; Cost = cost; Theta = theta})

let basicTuner iters (c:Convergence) (theta,cost) =
    if c.Iteration > iters
    then Done(theta,cost)
    else
        let theta', cost', alpha' = 
            if cost > c.Cost 
            then c.Theta, c.Cost, c.Alpha * 0.5
            else theta, cost, c.Alpha
        Next({c with Theta = theta'; Cost = cost'; Alpha = alpha'; Iteration = c.Iteration + 1; })

let selfTuner iters (c:Convergence) (theta,cost) =
    if c.Iteration > iters
    then Done(theta,cost)
    elif abs ((c.Cost-cost)/c.Cost) < 0.0001 then Done(theta,cost)
    else
        let theta', cost', alpha', steps', limit' = 
            if cost > c.Cost 
            then c.Theta, c.Cost, c.Alpha * 0.5, 0, c.Limit
            else
                if (c.Decreases > c.Limit)
                then theta, cost, c.Alpha * 1.5, 0, c.Limit + 1
                else theta, cost, c.Alpha, c.Decreases + 1, c.Limit
        Next({Theta = theta'; Cost = cost'; Alpha = alpha'; Iteration = c.Iteration + 1; Decreases = steps'; Limit = limit' })

let  run (x: DenseMatrix) (y: DenseVector) (w: DenseVector) 
                (intercept:bool) (alpha: float) (lambda: float) (iters: int)
                (handler: (DenseVector * float) -> unit) =

    if not (x.RowCount = y.Count) 
    then failwith "Inconsistent sizes" 
    else
        let features = x.ColumnCount
        let x' =
            if intercept 
            then x.InsertColumn(0, DenseVector.create y.Count 1.) :?> DenseMatrix
            else  x.InsertColumn(0, DenseVector.create y.Count 0.) :?> DenseMatrix
        let theta = DenseVector(features + 1)
        
        let conv = { Theta = theta; Alpha = alpha; Iteration = 0; Cost = Double.MaxValue; Decreases = 0; Limit = 1 }
        let tuner = selfTuner iters 

        let rec search (c:Convergence) (t:Tuner) =
            let theta' = descentUpdate x' y w (c.Theta) (c.Alpha) lambda
            let cost' = cost x' y w theta' lambda
            handler (theta', cost')
            printfn "%i Cost %f Alpha %f" c.Iteration cost' c.Alpha
            match (t c (theta',cost')) with
            | Done(x) -> x
            | Next(x) -> search x t

        search conv tuner |> fst

let linear (x: DenseVector) (th: DenseVector) = (Vector.insert 0 1. x * th)
  
let estimate (data: float [] * float [][] * float []) // output, inputs, obs weights
                intercept alpha lambda iters (handler: (DenseVector * float) -> unit) =

    let Ys, Xs, Ws = data

    let features = Xs.[0].Length
    let sample = Ys.Length
    
    // convert to algebra form
    let y = DenseVector(sample)
    let x = DenseMatrix(sample, features)
    let w = DenseVector(sample)
    
    // fill in matrices & vectors
    for row in 0 .. (sample - 1) do
        for col in 0 .. (features - 1) do
            x.[row,col] <- Xs.[row].[col]
        y.[row] <- Ys.[row]
        w.[row] <- Ws.[row]

    run x y w intercept alpha lambda iters handler 

// LINEAR REGRESSION BLOCK END


let shuffle data = 
    let result = (Array.length data) |> Array.zeroCreate
    result.[0] <- data.[0]
    for i in 1 .. (data.Length - 1) do
        let j = rng.Next(0, (i + 1))
        if i <> j then result.[i] <- result.[j]
        result.[j] <- data.[i]
    result

// Mean Absolute Percent Error
let mape (data: (float * float) seq) =
    data |> Seq.averageBy (fun (act, pred) -> abs ((act - pred) / act))

// Mean Absolute Error
let mae (data: (float * float) seq) =
    data |> Seq.averageBy (fun (act, pred) -> abs (act - pred))


let options = RegexOptions.Compiled ||| RegexOptions.IgnoreCase

// Regular Expression matching full words, case insensitive.
let matchWords = new Regex(@"\w+", options)

let vocabulary (text: string) =
    matchWords.Matches(text)
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Value)
    |> Set.ofSeq

let logit p = log (p / (1. - p))
let logitToProba l = 1. / (1. + exp(-l))

// SPECIFIC ANALYSIS

let fullText (ad: Ad) =  ad.Content

let  clampCtr (ad: Ad) =
    let raw = float ad.Clicks / float ad.Impressions
    if raw < 0.00001 then 0.00001 
    elif raw > 0.99999 then 0.99999 
    else raw


// Function to determine the relative observation weight,
// from CTR, tokens, impressions
type ObservationWeights = Ad [] -> (Ad -> float)

// Every observation is equally weigthed
let equalWeights data = fun (ad: Ad) -> 1.

// Observations are weighted proportional to impressions
let impressionsWeights (data: Ad seq) =
    let max = 
        data 
        |> Seq.map (fun ad -> float ad.Impressions) 
        |> Seq.max
    fun (ad: Ad) -> float ad.Impressions / max

let fullWeights data =
    let max = 
        data 
        |> Seq.map (fun ad -> clampCtr ad * float ad.Impressions) 
        |> Seq.max
    fun (ad: Ad) -> clampCtr ad * (float ad.Impressions) / max

// From a dataset, separate 2 random subsets,
// the training and validation sets,
// with a proportion p going to validation 
let crossValidation data p =
    let shuffled = data |> shuffle
    let validationSize = Array.length data |> float |> (*) p |> int
    shuffled.[validationSize ..], shuffled.[.. validationSize - 1]


type AdTokenizer = Ad -> string Set

let basicTokenizer =
    fun (ad: Ad) -> fullText ad |> vocabulary

let extractTokens (ads: Ad []) (t: AdTokenizer) =
    ads 
    |> Array.map (fun ad -> fullText ad |> vocabulary)
    |> Array.reduce (fun acc t -> Set.union acc t)

type Predictor = Ad -> float

// Create predictor function
let predictor (tokenizer: AdTokenizer) (tokens: string []) theta =
    fun (ad:Ad) ->        
        let vocab = tokenizer ad
        let vector = [| 
            for t in tokens do
                if vocab.Contains t then yield 1. else yield 0. |]
        linear (DenseVector(vector)) theta
    
        
let Analyze (trainingSet: Ad []) 
            (tokenizer: AdTokenizer)
            (weights: ObservationWeights) 
            alpha lambda iters 
            (handler: (DenseVector * float) -> unit)=
    
    // Estimate parameters on the training set

    let tokens = 
        extractTokens trainingSet tokenizer 
        |> Set.toArray

    let ws = weights trainingSet

    let averageCtr = trainingSet |> Array.averageBy (fun ad -> clampCtr ad)

    let toModel ad = 0.5 - averageCtr + clampCtr ad |> logit
    let fromModel y = y |> logitToProba |> fun x -> x - 0.5 + averageCtr

    let (Ys, Xs, Ws) =
        trainingSet
        |> Array.map (fun ad -> 
            let adTokens = tokenizer ad
            ad |> toModel,
            tokens 
            |> Array.map (fun t -> if Set.contains t adTokens then 1. else 0.),
            ws ad)
        |> Array.unzip3

    let estTheta = estimate (Ys, Xs, Ws) false alpha lambda iters handler
    let predictor ad = (predictor tokenizer tokens estTheta ad) |> fromModel

    estTheta, predictor, tokens


// Running the model

let tokenizer = basicTokenizer

// up.csv
// pb.csv
// ws.csv
let dataset = Sample.Load("pb.csv").Data |> Seq.toArray

let training, validation = crossValidation dataset 0.2 // 30% validation

let createpredictor (data:Ad[]) (tokenizer:AdTokenizer) (theta:DenseVector) =
    let tokens = 
        extractTokens data tokenizer 
        |> Set.toArray
    let averageCtr = data |> Array.averageBy (fun ad -> clampCtr ad)
    let fromModel y = y |> logitToProba |> fun x -> x - 0.5 + averageCtr
    fun (ad:Ad) ->        
        let vocab = tokenizer ad
        let vector = [| 
            for t in tokens do
                if vocab.Contains t then yield 1. else yield 0. |]
        linear (DenseVector(vector)) theta |> fromModel

let disposable = Vega.connect "http://localhost:8081" __SOURCE_DIRECTORY__

let handler ((theta:DenseVector), (cost:float)) =
    let pred = createpredictor training tokenizer theta
    let maxCtr = training |> Seq.map (fun ad -> ad.Impressions) |> Seq.max |> float
    let size (ad:Ad) = 10. + ((float ad.Impressions) / maxCtr) * 1000.
    let eval =
        [  
           for ad in training -> clampCtr ad, pred ad, "Train", size ad
           for ad in validation -> clampCtr ad, pred ad, "Valid", size ad ]
//    printfn "Updated values"
//    printfn "Cost: %f" cost
    VegaHub.Basics.scatterplot eval 
                               ((fun (x,_,_,_) -> x), 
                               (fun (_,y,_,_) -> y), 
                               (fun (_,_,c,_) -> c), 
                               (fun (_,_,_,s) -> s))
    |> Vega.send

let obsWeightsStrat = equalWeights // equalWeights // impressionsWeights // 
let the, pred, toks = Analyze training tokenizer obsWeightsStrat 1.0 0. 100 handler

disposable.Dispose()
