module Hedgehog.Experimental.Tests.GenTests

open Xunit
open Swensen.Unquote
open Hedgehog

[<Fact>]
let ``notIn generates element that is not in list`` () =
    Property.check <| property {
        let! xs =
            Gen.int (Range.linearFrom 0 -100 100)
            |> Gen.list (Range.linear 1 10)
        let! x = Gen.int (Range.linearFrom 0 -100 100) |> GenX.notIn xs
        return not <| List.contains x xs
    }

[<Fact>]
let ``notContains generates list that does not contain element`` () =
    Property.check <| property {
        let! x = Gen.int (Range.linearFrom 0 -100 100)
        let! xs =
            Gen.int (Range.linearFrom 0 -100 100)
            |> Gen.list (Range.linear 1 10)
            |> GenX.notContains x
        return not <| List.contains x xs
    }

[<Fact>]
let ``addElement generates a list with the specified element`` () =
    Property.check <| property {
        let! x = Gen.int (Range.exponentialBounded ())
        let! xs = 
            Gen.int (Range.exponentialBounded ())
            |> Gen.list (Range.linear 0 10)
            |> GenX.addElement x
        return List.contains x xs
    }

[<Fact>]
let ``sorted2 generates a sorted 2-tuple`` () =
    Property.check <| property {
        let! x1, x2 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple
            |> GenX.sorted2
        x1 <=! x2
    }

[<Fact>]
let ``sorted3 generates a sorted 3-tuple`` () =
    Property.check <| property {
        let! x1, x2, x3 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple3
            |> GenX.sorted3
        x1 <=! x2
        x2 <=! x3
    }

[<Fact>]
let ``sorted4 generates a sorted 4-tuple`` () =
    Property.check <| property {
        let! x1, x2, x3, x4 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple4
            |> GenX.sorted4
        x1 <=! x2
        x2 <=! x3
        x3 <=! x4
    }

[<Fact>]
let ``distinct2 generates 2 non-equal elements`` () =
    Property.check <| property {
        let! x1, x2 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple
            |> GenX.distinct2
        [x1; x2] |> List.distinct =! [x1; x2]
    }

[<Fact>]
let ``distinct3 generates 3 non-equal elements`` () =
    Property.check <| property {
        let! x1, x2, x3 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple3
            |> GenX.distinct3
        [x1; x2; x3] |> List.distinct =! [x1; x2; x3]
    }

[<Fact>]
let ``distinct4 generates 4 non-equal elements`` () =
    Property.check <| property {
        let! x1, x2, x3, x4 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple4
            |> GenX.distinct4
        [x1; x2; x3; x4] |> List.distinct =! [x1; x2; x3; x4]
    }

[<Fact>]
let ``increasing2 generates a 2-tuple with strictly increasing elements`` () =
    Property.check <| property {
        let! x1, x2 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple
            |> GenX.increasing2
        x1 <! x2
    }

[<Fact>]
let ``increasing3 generates a 3-tuple with strictly increasing elements`` () =
    Property.check <| property {
        let! x1, x2, x3 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple3
            |> GenX.increasing3
        x1 <! x2
        x2 <! x3
    }

[<Fact>]
let ``increasing4 generates a 4-tuple with strictly increasing elements`` () =
    Property.check <| property {
        let! x1, x2, x3, x4 =
            Gen.int (Range.exponentialBounded ())
            |> Gen.tuple4
            |> GenX.increasing4
        x1 <! x2
        x2 <! x3
        x3 <! x4
    }

[<Fact>]
let ``dateInterval generates two dates spaced no more than the range allows`` () =
    Property.check <| property {
        let! d1, d2 = GenX.dateInterval (Range.linear 0 100)
        (d2-d1).TotalDays <=! 100.
    }

[<Fact>]
let ``dateInterval with positive interval generates increasing dates`` () =
    Property.check <| property {
        let! d1, d2 = GenX.dateInterval (Range.linear 0 100)
        d2 >=! d1
    }

[<Fact>]
let ``dateInterval with negative interval generates increasing dates`` () =
    Property.check <| property {
        let! d1, d2 = GenX.dateInterval (Range.linear 0 -100)
        d2 <=! d1
    }

[<Fact>]
let ``withMapTo is defined for all elements in input list`` () =
    Property.check <| property {
        let! xs, f = 
            Gen.int (Range.exponentialBounded ())
            |> Gen.list (Range.linear 1 50) 
            |> GenX.withMapTo Gen.alphaNum
        xs |> List.map f |> ignore // Should not throw.
    }

[<Fact>]
let ``withDistinctMapTo is defined for all elements in input list`` () =
    Property.check <| property {
        let! xs, f = 
            Gen.int (Range.exponentialBounded ())
            |> Gen.list (Range.linear 1 50) 
            |> GenX.withDistinctMapTo Gen.alphaNum
        xs |> List.map f |> ignore // Should not throw.
    }

[<Fact>]
let ``withDistinctMapTo guarantees that distinct input values map to distinct output values`` () =
    Property.check <| property {
        let! xs, f = 
            Gen.int (Range.exponentialBounded ())
            |> Gen.list (Range.linear 1 50) 
            |> GenX.withDistinctMapTo Gen.alphaNum
        let xsDistinct = xs |> List.distinct
        xsDistinct |> List.map f |> List.distinct |> List.length =! xsDistinct.Length
    }


type RecOption =
  {X: RecOption option}
  member this.Depth =
    match this.X with
    | None -> 0
    | Some x -> x.Depth + 1

[<Fact>]
let ``auto with recursive option members does not cause stack overflow using default settings`` () =
    Property.check <| property {
        let! _ = GenX.auto<RecOption>()
        return true
    }

[<Fact>]
let ``auto with recursive option members respects max recursion depth`` () =
    Property.check <| property {
        let! depth = Gen.int <| Range.exponential 0 5
        let! x = GenX.autoWith<RecOption> {GenX.defaults with RecursionDepth = depth}
        x.Depth <=! depth
    }

[<Fact>]
let ``auto with recursive option members generates some values with max recursion depth`` () =
    Property.check' 10<tests> <| property {
        let! depth = Gen.int <| Range.linear 1 5
        let! xs = GenX.autoWith<RecOption> {GenX.defaults with RecursionDepth = depth}
                  |> (Gen.list (Range.singleton 100))
        test <@ xs |> List.exists (fun x -> x.Depth = depth) @>
    }


type RecArray =
  {X: RecArray array}
  member this.Depth =
    match this.X with
    | [||] -> 0
    | xs -> xs |> Array.map (fun x -> x.Depth + 1) |> Array.max

[<Fact>]
let ``auto with recursive array members does not cause stack overflow using default settings`` () =
    Property.check <| property {
        let! _ = GenX.auto<RecArray>()
        return true
    }

[<Fact>]
let ``auto with recursive array members respects max recursion depth`` () =
    Property.check <| property {
        let! depth = Gen.int <| Range.exponential 0 5
        let! x = GenX.autoWith<RecArray> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 0 5}
        x.Depth <=! depth
    }

[<Fact>]
let ``auto with recursive array members generates some values with max recursion depth`` () =
    Property.check' 10<tests> <| property {
        let! depth = Gen.int <| Range.linear 1 5
        let! xs = GenX.autoWith<RecArray> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 1 5}
                  |> (Gen.list (Range.singleton 100))
        test <@ xs |> List.exists (fun x -> x.Depth = depth) @>
    }


type RecList =
  {X: RecList list}
  member this.Depth =
    match this.X with
    | [] -> 0
    | xs -> xs |> List.map (fun x -> x.Depth + 1) |> List.max

[<Fact>]
let ``auto with recursive list members does not cause stack overflow using default settings`` () =
    Property.check <| property {
        let! _ = GenX.auto<RecList>()
        return true
    }

[<Fact>]
let ``auto with recursive list members respects max recursion depth`` () =
    Property.check <| property {
        let! depth = Gen.int <| Range.exponential 0 5
        let! x = GenX.autoWith<RecList> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 0 5}
        x.Depth <=! depth
    }

[<Fact>]
let ``auto with recursive list members generates some values with max recursion depth`` () =
    Property.check' 10<tests> <| property {
        let! depth = Gen.int <| Range.linear 1 5
        let! xs = GenX.autoWith<RecList> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 1 5}
                  |> (Gen.list (Range.singleton 100))
        test <@ xs |> List.exists (fun x -> x.Depth = depth) @>
    }


type RecSet =
  {X: Set<RecSet>}
  member this.Depth =
    if this.X.IsEmpty then 0
    else
      this.X |> Seq.map (fun x -> x.Depth + 1) |> Seq.max

[<Fact>]
let ``auto with recursive set members does not cause stack overflow using default settings`` () =
    Property.check <| property {
        let! _ = GenX.auto<RecSet>()
        return true
    }

[<Fact>]
let ``auto with recursive set members respects max recursion depth`` () =
    Property.check <| property {
        let! depth = Gen.int <| Range.exponential 0 5
        let! x = GenX.autoWith<RecSet> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 0 5}
        x.Depth <=! depth
    }

[<Fact>]
let ``auto with recursive set members generates some values with max recursion depth`` () =
    Property.check' 10<tests> <| property {
        let! depth = Gen.int <| Range.linear 1 5
        let! xs = GenX.autoWith<RecSet> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 1 5}
                  |> (Gen.list (Range.singleton 100))
        test <@ xs |> List.exists (fun x -> x.Depth = depth) @>
    }


type RecMap =
  {X: Map<RecMap, RecMap>}
  member this.Depth =
    if this.X.IsEmpty then 0
    else
      this.X |> Map.toSeq |> Seq.map (fun (k, v)  -> max (k.Depth + 1) (v.Depth + 1)) |> Seq.max

[<Fact>]
let ``auto with recursive map members does not cause stack overflow using default settings`` () =
    Property.check <| property {
        let! _ = GenX.auto<RecMap>()
        return true
    }

[<Fact>]
let ``auto with recursive map members respects max recursion depth`` () =
    Property.check <| property {
        let! depth = Gen.int <| Range.exponential 0 5
        let! x = GenX.autoWith<RecMap> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 0 5}
        x.Depth <=! depth
    }

[<Fact>]
let ``auto with recursive map members generates some values with max recursion depth`` () =
    Property.check' 10<tests> <| property {
        let! depth = Gen.int <| Range.linear 1 5
        let! xs = GenX.autoWith<RecMap> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 1 5}
                  |> (Gen.list (Range.singleton 100))
        test <@ xs |> List.exists (fun x -> x.Depth = depth) @>
    }


type MutuallyRecursive1 =
  {X: MutuallyRecursive2 option}
  member this.Depth =
    match this.X with
    | None -> 0
    | Some {X = []} -> 0
    | Some {X = mc1s} -> 
        mc1s
        |> List.map (fun mc1 -> mc1.Depth + 1)
        |> List.max

and MutuallyRecursive2 =
  {X: MutuallyRecursive1 list}
  member this.Depth =
    if this.X.IsEmpty then 0
    else
      let depths = 
        this.X
        |> List.choose (fun mc1 -> mc1.X)
        |> List.map (fun mc2 -> mc2.Depth + 1)
      if depths.IsEmpty then 0 else List.max depths

[<Fact>]
let ``auto with mutually recursive types does not cause stack overflow using default settings`` () =
    Property.check <| property {
        let! _ = GenX.auto<MutuallyRecursive1>()
        let! _ = GenX.auto<MutuallyRecursive2>()
        return true
    }

[<Fact>]
let ``auto with mutually recursive types respects max recursion depth`` () =
    Property.check <| property {
        let! depth = Gen.int <| Range.exponential 0 5
        let! x1 = GenX.autoWith<MutuallyRecursive1> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 0 5}
        let! x2 = GenX.autoWith<MutuallyRecursive2> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 0 5}
        x1.Depth <=! depth
        x2.Depth <=! depth
    }

[<Fact>]
let ``auto with mutually recursive types generates some values with max recursion depth`` () =
    Property.check' 10<tests> <| property {
        let! depth = Gen.int <| Range.linear 1 5
        let! xs1 = GenX.autoWith<MutuallyRecursive1> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 1 5}
                  |> (Gen.list (Range.singleton 100))
        let! xs2 = GenX.autoWith<MutuallyRecursive2> {GenX.defaults with RecursionDepth = depth; SeqRange = Range.exponential 1 5}
                  |> (Gen.list (Range.singleton 100))
        test <@ xs1 |> List.exists (fun x -> x.Depth = depth) @>
        test <@ xs2 |> List.exists (fun x -> x.Depth = depth) @>
    }
