module App

open Fable.Core
open Fable.Core.JsInterop
open Fable.Remoting.Client
open SharedTypes

let server = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.buildProxy<IServer>

QUnit.registerModule "Fable.Remoting"

QUnit.testCase "Proxy.combineWithBaseUrlWorks" <| fun test -> 
    let route = "/IMusicStore/getLength"
    ["http://api.example.com"; "http://api.example.com/"]
    |> List.map (Some >> Proxy.combineRouteWithBaseUrl route)
    |> List.distinct
    |> function 
        | [ "http://api.example.com/IMusicStore/getLength" ] -> test.pass()
        | otherwise -> test.fail() 
        
QUnit.testCaseAsync "IServer.getLegth" <| fun test ->
    async {
        let! result = server.getLength "hello"
        do test.equal result 5
    }

QUnit.testCaseAsync "IServer.echoAnonymousRecord" <| fun test -> 
    async {
        let! result = server.echoAnonymousRecord (Just {| name = "John" |})
        match result with 
        | Just record -> test.equal "John" record.name 
        | otherwise -> test.failWith "Unexpected result"
    }

QUnit.testCaseAsync "IServer.echoNestedAnonRecord" <| fun test -> 
    async {
        let! result = server.echoNestedAnonRecord (Just {| nested  = {| name = "John" |} |})
        match result with 
        | Just record -> test.equal "John" record.nested.name 
        | otherwise -> test.failWith "Unexpected result"
    }

QUnit.testCaseAsync "IServer.binaryContent" <| fun test ->
    async {
        let! result = server.binaryContent()
        test.equal 3 result.Length
        test.equal true (result = [| byte 1; byte 2; byte 3|])
    }

QUnit.testCaseAsync "IServer.echoRemoteWorkEntity" <| fun test ->
    async {
        let entity = { RemoteWork = RequiredInput.validOrFail (ValidUserInput (RemoteWork "Fully Remote")) }
        let! echoedEntity = server.echoRemoteWorkEntity entity 
        test.equal true (echoedEntity.RemoteWork = RemoteWork "Fully Remote")
    }
 
QUnit.testCaseAsync "IServer.binaryContent" <| fun test ->
    async {
        let input = [| byte 1; byte 2; byte 3|]
        let! output = server.binaryInputOutput input
        test.equal 3 output.Length
        test.equal true (input = output) 
    }

QUnit.testCaseAsync "ISever.echoInteger" <| fun test ->
    async {
        let! fstResult = server.echoInteger 20
        let! sndResult = server.echoInteger 15
        do test.equal fstResult 20
        do test.equal sndResult 15
    }

QUnit.testCaseAsync "IServer.simpleUnit" <| fun test -> 
    async {
        let! result = server.simpleUnit() 
        do test.equal result 42
    }

QUnit.testCaseAsync "IServer.echoString" <| fun test ->
    async {
        let! result1 = server.echoString ""
        let! result2 = server.echoString "this one"
        let! result3 = server.echoString null
        do test.equal result1 ""
        do test.equal result2 "this one"
        do test.equal true (isNull result3)
    }

QUnit.testCaseAsync "IServer.echoBool" <| fun test ->
    async {
        let! fstTrue = server.echoBool true
        let! fstFalse = server.echoBool false
        do test.equal fstTrue true
        do test.equal fstFalse false
    }

QUnit.testCaseAsync "IServer.mapRecordAsKey" <| fun test -> 
    async {
        let! result = server.mapRecordAsKey()
        result 
        |> Map.toList 
        |> function 
            | [ { Key = 1; Value = "Value" }, 1 ] -> test.pass()
            | otherwise -> test.failWith (sprintf "%A" otherwise)
    } 

open System

let datesEqual (test: QUnit.Asserter) (x: DateTime) (y: DateTime) =
    test.equal x.Year y.Year
    test.equal x.Day y.Day
    test.equal x.Month y.Month
    test.equal x.Hour y.Hour
    test.equal x.Minute y.Minute
    test.equal x.Second y.Second


QUnit.testCaseAsync "IServer.echoIntOption" <| fun test ->
    async {
        let! fstResult = server.echoIntOption (Some 5)
        let! sndResult = server.echoIntOption None
        do test.equal true (fstResult = Some 5)
        do test.equal true (sndResult = None)
    }

QUnit.testCaseAsync "IServer.echoStringOption" <| fun test ->
    async {
        let! fstResult = server.echoStringOption (Some "hello")
        let! sndResult = server.echoStringOption None
        do test.equal true (fstResult = Some "hello")
        do test.equal true (sndResult = None)
    }

QUnit.testCaseAsync "IServer.echoPrimitiveLong" <| fun test ->
    async {
        let! fstResult = server.echoPrimitiveLong (20L)
        let! sndResult = server.echoPrimitiveLong 0L
        let! thirdResult = server.echoPrimitiveLong -20L
        do test.equal true (fstResult = 20L)
        do test.equal true (sndResult = 0L)
        do test.equal true (thirdResult = -20L)
    }

QUnit.testCaseAsync "IServer.echoPrimitiveLong with large values" <| fun test -> 
    async {
        let! fstResult = server.echoPrimitiveLong System.Int64.MaxValue 
        let! sndResult = server.echoPrimitiveLong System.Int64.MinValue
        do test.equal true (fstResult = System.Int64.MaxValue)
        do test.equal true (sndResult = System.Int64.MinValue)
    }

QUnit.testCaseAsync "IServer.echoComplexLong" <| fun test -> 
    async {
        let input = { Value = 20L; OtherValue = 10 }
        let! output = server.echoComplexLong input 
        do test.equal true (input = output)
    }

QUnit.testCaseAsync "IServer.echoOptionalLong" <| fun test ->
    async {
        let! fstResult = server.echoOptionalLong (Some 20L)
        let! sndResult = server.echoOptionalLong None
        do test.equal true (fstResult = (Some 20L))
        do test.equal true (sndResult = None)
    }

QUnit.testCaseAsync "IServer.echoSingleDULong" <| fun test ->
    async {
        let! output = server.echoSingleDULong (SingleLongCase 20L)
        do test.equal true (output = (SingleLongCase 20L))
    }

QUnit.testCaseAsync "IServer.echoLongInGenericUnion" <| fun test ->
    async {
        let! output = server.echoLongInGenericUnion (Just 20L)
        let! result = server.echoLongInGenericUnion Nothing 
        do test.equal true (output = Just 20L)
        do test.equal true (result = Nothing)
    }
    
QUnit.testCaseAsync "IServer.echoSimpleUnionType" <| fun test ->
    async {
        let! result1 = server.echoSimpleUnionType One
        let! result2 = server.echoSimpleUnionType Two
        do test.equal true (result1 = One)
        do test.equal true (result2 = Two)
    }

QUnit.testCaseAsync "IServer.echoGenericUnionInt" <| fun test ->
    async {
        let! result1 = server.echoGenericUnionInt (Just 5)
        let! result2 = server.echoGenericUnionInt (Just 10)
        let! result3 = server.echoGenericUnionInt Nothing

        do test.equal true (result1 = Just 5)
        do test.equal true (result2 = Just 10)
        do test.equal true (result3 = Nothing)
    }

QUnit.testCaseAsync "IServer.echoGenericUnionString" <| fun test ->
    async {
        let! result1 = server.echoGenericUnionString (Just "")
        let! result2 = server.echoGenericUnionString (Just null)
        let! result3 = server.echoGenericUnionString (Just "you")
        let! result4 = server.echoGenericUnionString Nothing

        do test.equal true (result1 = Just "")
        do test.equal true (result2 = Just null)
        do test.equal true (result3 = Just "you")
        do test.equal true (result4 = Nothing)
    }


QUnit.testCaseAsync "IServer.echoRecord" <| fun test ->
    let record1 = { Prop1 = "hello"; Prop2 = 10; Prop3 = None }
    let record2 = { Prop1 = ""; Prop2 = 20; Prop3 = Some 10 }
    let record3 = { Prop1 = null; Prop2 = 30; Prop3 = Some 20  }
    async {
        let! result1 = server.echoRecord record1
        let! result2 = server.echoRecord record2
        let! result3 = server.echoRecord record3

        do test.equal true (result1 = record1)
        do test.equal true (result2 = record2)
        do test.equal true (result3 = record3)
    }



QUnit.setTimeout 5000

QUnit.testCaseAsync "IServer.echoHighScores" <| fun test -> 
    let input = [|
        { Name = "alfonsogarciacaro"; Score =  100 }
        { Name = "theimowski"; Score =  28 }
    |]

    async {
        let! result = server.echoHighScores input 
        do test.equal "alfonsogarciacaro" result.[0].Name 
        do test.equal 100 result.[0].Score 
        do test.equal "theimowski" result.[1].Name 
        do test.equal 28 result.[1].Score 
    }

QUnit.testCaseAsync "IServer.echoHighScores without do" <| fun test -> 
    let input = [|
        { Name = "alfonsogarciacaro"; Score =  100 }
        { Name = "theimowski"; Score =  28 }
    |]

    async {
        let! result = server.echoHighScores input 
        test.equal "alfonsogarciacaro" result.[0].Name 
        test.equal 100 result.[0].Score 
        test.equal "theimowski" result.[1].Name 
        test.equal 28 result.[1].Score 
    }

QUnit.testCaseAsync "IServer.echoHighScores" <| fun test -> 
    async {
        let! result = server.getHighScores() 
        do test.equal "alfonsogarciacaro" result.[0].Name 
        do test.equal 100 result.[0].Score 
        do test.equal "theimowski" result.[1].Name 
        do test.equal 28 result.[1].Score 
    }

QUnit.testCaseAsync "IServer.echoHighScores without do" <| fun test -> 
    async {
        let! result = server.getHighScores() 
        test.equal "alfonsogarciacaro" result.[0].Name 
        test.equal 100 result.[0].Score 
        test.equal "theimowski" result.[1].Name 
        test.equal 28 result.[1].Score 
    }


QUnit.testCaseAsync "IServer.echoNestedGeneric" <| fun test ->

    let input : GenericRecord<Maybe<int option>> = {
        Value = Just (Some 5)
        OtherValue = 2
    }

    let input2 : GenericRecord<Maybe<int option>> = {
        Value = Just (None)
        OtherValue = 2
    }

    async {
        let! result1 = server.echoNestedGeneric input
        let! result2 = server.echoNestedGeneric input2
        do test.equal true (input = result1)
        do test.equal true (input2 = result2)
    }


QUnit.testCaseAsync "IServer.echoIntList" <| fun test ->
    async {
        let! output = server.echoIntList [1 .. 5]
        do test.equal true (output = [1;2;3;4;5])

        let! echoedList = server.echoIntList []
        do test.equal true (List.isEmpty echoedList)
    }

QUnit.testCaseAsync "IServer.echoSingleCase" <| fun test ->
    async { 
        let! output = server.echoSingleCase (SingleCase 10)
        match output with 
        | SingleCase 10 -> test.pass()
        | other -> test.fail()
    }

QUnit.testCaseAsync "IServer.echoStringList" <| fun test ->
    async {
        let! output = server.echoStringList ["one"; "two"; null]
        do test.equal true (output = ["one"; "two"; null])

        let! echoedList = server.echoStringList []
        do test.equal true (List.isEmpty echoedList)
    }

QUnit.testCaseAsync "IServer.echoBoolList" <| fun test ->
    async {
        let! output = server.echoBoolList [true; false; true]
        do test.equal true (output = [true; false; true])

        let! echoedList = server.echoStringList []
        do test.equal true (List.isEmpty echoedList)
    }


QUnit.testCaseAsync "IServer.echoListOfListsOfStrings" <| fun test ->
    async {
        let! output = server.echoListOfListsOfStrings [["1"; "2"]; ["3"; "4";"5"]]
        do test.equal true (output =  [["1"; "2"]; ["3"; "4";"5"]])
    }

QUnit.testCaseAsync "IServer.echoResult for Result<int, string>" <| fun test ->
    async {
        let! outputOk = server.echoResult (Ok 15)
        match outputOk with
        | Ok 15 -> test.pass()
        | otherwise -> test.fail()

        let! outputError = server.echoResult (Error "hello")
        match outputError with
        | Error "hello" -> test.pass()
        | otherwise -> test.fail()
    }

QUnit.testCaseAsync "IServer.echoMap" <| fun test ->
    async {
        let input = ["hello", 1] |> Map.ofList
        let! output = server.echoMap input
        match input = output with
        | true -> test.pass()
        | false -> test.fail()
    }

QUnit.testCaseAsync "IServer.echoBigInteger" <|
    fun test ->
        async {
            let n = 1I
            let! output = server.echoBigInteger n
            test.equal true (output = n)

            let n = 2I
            let! output = server.echoBigInteger n
            test.equal true (output = n)

            let n = -1I
            let! output = server.echoBigInteger n
            test.equal true (output = n)

            let n = -2I
            let! output = server.echoBigInteger n
            test.equal true (output = n)

            let n = 100I
            let! output = server.echoBigInteger n
            test.equal true (output = n)
        }

QUnit.testCaseAsync "IServer.throwError" <| fun test ->
    async {
        let! result = Async.Catch (server.throwError())
        match result with
        | Choice1Of2 output -> test.fail()
        | Choice2Of2 error ->
            match error with 
            | :? ProxyRequestException as ex ->
                if ex.ResponseText.Contains("Generating custom server error")
                then test.pass()
                else test.fail() 
            | otherwise -> test.fail()
    }

QUnit.testCaseAsync "IServer.mutliArgFunc" <| fun test ->
    async {
        let! output = server.multiArgFunc "hello" 10 false
        test.equal 15 output

        let! sndOutput = server.multiArgFunc "byebye" 5 true
        test.equal 12 sndOutput
    }

QUnit.testCaseAsync "IServer.mutliArgFunc partially applied" <| fun test ->
    async {
        let partialFunc = server.multiArgFunc "hello" 10
        let! output =  partialFunc false
        test.equal 15 output

        let otherPartialFunc = server.multiArgFunc "byebye"
        let! sndOutput = otherPartialFunc 5 true
        test.equal 12 sndOutput
    }

QUnit.testCaseAsync "IServer.pureAsync" <| fun test ->
    async {
        let! output = server.pureAsync
        test.equal 42 output
    }

QUnit.testCaseAsync "IServer.asyncNestedGeneric" <| fun test ->
    async {
        let! result = server.asyncNestedGeneric
        test.equal true (result = { OtherValue = 10; Value = Just (Some "value") })
    }

QUnit.testCaseAsync "IServer.multiArgComplex" <| fun test -> 
    async {
        let input = { OtherValue = 10; Value = Just (Some "value") }
        let! output = server.multiArgComplex false input
        test.equal true (input = output)
    }

QUnit.testCaseAsync "IServer.getSeq" <| fun test ->
    async {
        let! output = server.getSeq()
        let maybes = List.ofSeq output
        match maybes with 
        | [ Just 5; Nothing ] -> test.equal true true 
        | _ -> test.equal false true
    }

QUnit.testCaseAsync "IServer.echoGenericMap" <| fun test ->
    async {
        let input = Map.ofList [ "firstKey", Just 5; "secondKey", Nothing ]
        let! output = server.echoGenericMap input 
        test.equal true (input = output)
    }

QUnit.testCaseAsync "IServer.echoRecursiveRecord" <| fun test ->
    async {
        let input = {
            Name = "root" 
            Children = [ 
                { Name = "Child 1"; Children = [ { Name = "Grandchild"; Children = [ ] } ] } 
                { Name = "Child 1"; Children = [ ] } 
            ]
        }

        let! output = server.echoRecursiveRecord input 
        test.equal true (output = input)
    }

QUnit.testCaseAsync "IServer.echoTree (recursive union)" <| fun test ->
    async {
        let input = Branch(Branch(Leaf 10, Leaf 5), Leaf 5)
        let! output = server.echoTree input 
        test.equal true (input = output)
    }

QUnit.testCaseAsync "IServer.multiArgComplex partially applied" <| fun test -> 
    async {
        let input = { OtherValue = 10; Value = Just (Some "value") }
        let partialF = fun x -> server.multiArgComplex false x
        let! output = partialF input 
        test.equal true (input = output) 
    }

QUnit.testCaseAsync "IServer.tuplesAndLists" <| fun test ->
    async {
        let inputDict = Map.ofList [ "hello", 5 ]
        let inputStrings = [ "there!" ]
        let! outputDict = server.tuplesAndLists (inputDict, inputStrings) 

        let expected = Map.ofList [ "hello", 5; "there!", 6 ] 
        test.equal true (expected = outputDict)  
    }

let cookieServer =
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.buildProxy<ICookieServer>

[<Emit("document.cookie")>]
let currentDocumentCookie : string = jsNative

QUnit.testCaseAsync "ICookieServer.checkCookie" <| fun test ->
    async {
        // Cookie not set yet
        let! firstCall = cookieServer.checkCookie ()
        test.equalWithMsg false firstCall "Cookie should not be set yet"

        // Cookie should now be set and sent back to server
        let! secondCall = cookieServer.checkCookie ()
        test.equalWithMsg true secondCall "Cookie should have been set and sent"

        // Cookie should not be visible to javascript (HttpOnly)
        let notInJs = not (currentDocumentCookie.Contains("httpOnly-test-cookie"))
        test.equalWithMsg true notInJs "Cookie should not be visible to javascript"
    }

let resolveAccessToken n = 
    async {
        let request = Http.get (sprintf "/IAuthServer/token/%d" n)
        let! response = Http.send request
        return response.ResponseBody
    }

let createSecureApi (accessToken: string) = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.withAuthorizationHeader accessToken
    |> Remoting.buildProxy<IAuthServer>

let authorizedServer n f = 
    async {
        let! accessToken = resolveAccessToken n
        let authServer = createSecureApi accessToken 
        return! f authServer
    }

QUnit.testCaseAsync "IAuthServer can be used by resolving access tokens" <| fun test ->
    async {
        let! firstResponse = authorizedServer 1 (fun api -> api.getSecureValue())
        let! secondResponse = authorizedServer 2 (fun api -> api.getSecureValue())
        test.equalWithMsg firstResponse 1 "The returned values are the access tokens resolved from server"
        test.equalWithMsg secondResponse 2 "The returned values are the access tokens resolved from server"
    } 