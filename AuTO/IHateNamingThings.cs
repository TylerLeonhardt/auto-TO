namespace IHateNamingThings
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Twilio.TwiML;

    public class IHateNamingThings
    {
        [FunctionName("HttpTriggerCSharp")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] 
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var data = await req.ReadAsStringAsync();

            var formValues = data.Split('&')
                .Select(value => value.Split('='))
                .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "),
                              pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

            var sender = formValues["From"];

            // lookup the bracket
            // get the sender's game
            // assume sender won
            // send sender to winner parent
            // send other player to loser parent
            // update database
            // Perform calculations, API lookups, etc. here

            var response = new MessagingResponse()
                .Message($"You said: {formValues["Body"]}");
            var twiml = response.ToString();
            twiml = twiml.Replace("utf-16", "utf-8");

            return new HttpResponseMessage
            {
                Content = new StringContent(twiml, Encoding.UTF8, "application/xml")
            };
        }
    }
}
