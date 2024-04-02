using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var requestBody = JObject.Parse(request?.Body ?? "{}");
        var repositoryName = (string)requestBody["repository"]?["name"] ?? "Unknown Repository";
        var pusherName = (string)requestBody["pusher"]?["name"] ?? "Unknown Pusher";

        if (requestBody["issue"] != null && requestBody["action"].ToString() == "opened")
        {
            var issueUrl = (string)requestBody["issue"]["html_url"];
            string payload = $"{{'text':'Issue Created: {issueUrl}'}}";

            var client = new HttpClient();
            var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
            var webRequest = new HttpRequestMessage(HttpMethod.Post, slackUrl)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = client.Send(webRequest);
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = "Webhook processed successfully."
        };
    }
}
