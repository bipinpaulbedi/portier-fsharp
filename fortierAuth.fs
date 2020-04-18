module FortierAuth

    open System.Text;
    open System.IO;
    open Microsoft.AspNetCore.Mvc;
    open Microsoft.Azure.WebJobs;
    open Microsoft.Extensions.Logging;
    open Microsoft.Azure.WebJobs.Extensions.Http;
    open Microsoft.AspNetCore.Http;

    [<FunctionName("fortierAuth")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>] req : HttpRequest, log: ILogger) =
        async {
            log.LogInformation("Wow F#")
            use reader = new StreamReader(req.Body, Encoding.UTF8)
            let! body = reader.ReadToEndAsync() |> Async.AwaitTask
            return OkObjectResult body

        } |> Async.StartAsTask