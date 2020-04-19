module Run

    open Microsoft.Azure.WebJobs;
    open Microsoft.Extensions.Logging;
    open Microsoft.Azure.WebJobs.Extensions.Http;
    open Microsoft.AspNetCore.Http;
    open Giraffe
    open System.Threading.Tasks
    open FSharp.Control.Tasks.V2

    type Auth = { Name : string }

    let auth = { Name = "Yes" }

    let loggingHandler: HttpHandler =
        fun next ctx -> task {
            let logger = ctx.GetLogger()
            use _ = logger.BeginScope(dict ["foo", "bar"])
            logger.LogInformation("Logging {Level}", "Information")
            return! Successful.OK "ok" next ctx
        }        

    let app : HttpHandler =
        choose [
            GET >=> route "/api/auth" >=> negotiate auth
            POST >=> route "/api/auth" >=> negotiate auth
            // GET >=> route "/api/demo" >=> htmlFile "index.htm"
            // GET >=> route "/api/demo/logging" >=> loggingHandler
            // GET >=> route "/api/demo/failing" >=> warbler (fun _ -> failwith "FAILURE")
            RequestErrors.NOT_FOUND "Not Found"
        ]    

    let errorHandler (ex : exn) (logger : ILogger) =
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse
        >=> ServerErrors.INTERNAL_ERROR ex.Message

    [<FunctionName "Giraffe">]
    let run ([<HttpTrigger (AuthorizationLevel.Anonymous, Route = "{*any}")>] req : HttpRequest, context : ExecutionContext, log : ILogger) =
        let hostingEnvironment = req.HttpContext.GetHostingEnvironment()
        hostingEnvironment.ContentRootPath <- context.FunctionAppDirectory
        let func = Some >> Task.FromResult
        { new Microsoft.AspNetCore.Mvc.IActionResult with
            member _.ExecuteResultAsync(ctx) = 
                task {
                    try
                        return! app func ctx.HttpContext :> Task
                    with exn ->
                        return! errorHandler exn log func ctx.HttpContext :> Task
                }
                :> Task }                  