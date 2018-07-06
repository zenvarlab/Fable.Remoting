namespace Fable.Remoting.Server

open System 
open System.Reflection 

type IAsyncBoxer =
    abstract BoxAsyncResult : obj -> Async<obj>

type AsyncBoxer<'T>() =
    interface IAsyncBoxer with
        member __.BoxAsyncResult(boxedAsync: obj) : Async<obj> =
            match boxedAsync with
            | :? Async<'T> as unboxedAsyncOfGenericValueT ->
                async {
                    // this is of type 'T
                    let! unwrappedGenericValueTfromAsync  = unboxedAsyncOfGenericValueT
                    return box unwrappedGenericValueTfromAsync
                }
            | otherValue -> failwithf "Invalid boxed value of type '%s'" (otherValue.GetType().FullName) 

/// Distinguish between records fields that are simple async values and fields that are functions with input and output
type RecordFunctionType = 
    | NoArguments of output: Type
    | SingleArgument of input: Type * output: Type 
    | ManyArguments of input: Type list * output: Type

/// Combines information about dynamic functions
type RecordFunctionInfo = {
    FunctionName: string 
    Type: RecordFunctionType
    PropertyInfo: PropertyInfo 
    ActualFunctionOrValue: obj
    InvokeMethodInfo: Option<MethodInfo> 
}

type ProtocolImplementationMetadata = Type * RecordFunctionInfo list

/// Route information that is propagated to error handler when exceptions are thrown
type RouteInfo<'ctx> = {
    path: string
    methodName: string
    httpContext: 'ctx
}

type CustomErrorResult<'a> =
    { error: 'a;
      ignored: bool;
      handled: bool; }

/// The ErrorResult lets you choose whether you want to propagate a custom error back to the client or to ignore it. Either case, an exception is thrown on the call-site from the client
type ErrorResult =
    | Ignore
    | Propagate of obj

type ErrorHandler<'context> = System.Exception -> RouteInfo<'context> -> ErrorResult

type IoCContainer = 
    abstract Resolve<'t> : unit -> 't  

/// A protocol implementation can be a static value provided or it can be generated from the Http context on every request.
type ProtocolImplementation<'context, 'serverImpl> = 
    | Empty 
    | StaticValue of 'serverImpl 
    | FromContext of ('context -> 'serverImpl)

type RemotingOptions<'context, 'serverImpl> = {
    Implementation: ProtocolImplementation<'context, 'serverImpl> 
    RouteBuilder : string -> string -> string 
    ErrorHandler : ErrorHandler<'context> option 
    DiagnosticsLogger : (string -> unit) option 
    IoCContainer : IoCContainer option 
}

type RequestMethod = GET | POST 

type InternalRequest = {
    Method : RequestMethod
    InputBody   : string 
    Route  : string 
}

type InternalResponse = {
    OutputBody : string
    StatusCode : int 
    NoCache : bool 
}