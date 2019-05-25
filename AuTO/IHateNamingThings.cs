namespace IHateNamingThings
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.TwiML;
    using Twilio.Types;
    using WorldsFirst.Schemas;

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

            // Isabela-proofing
            string authorized = Environment.GetEnvironmentVariable("nextPlayer1");

            if(sender.Substring(1) != authorized.Substring(1))
            {
                var error = new MessagingResponse()
                    .Message($"DON'T @ ME!!  You sent {formValues["Body"]}");
                var errorTwiml = error.ToString();
                errorTwiml = errorTwiml.Replace("utf-16", "utf-8");

                return new HttpResponseMessage
                {
                    Content = new StringContent(errorTwiml.ToString(), Encoding.UTF8, "application/xml")
                };
            }

            string accountId = Environment.GetEnvironmentVariable("accountId");
            string authToken = Environment.GetEnvironmentVariable("authToken");
            var myNumber = new PhoneNumber(Environment.GetEnvironmentVariable("myNumber"));
            var nextPlayer1 = new PhoneNumber(Environment.GetEnvironmentVariable("nextPlayer1"));
            var nextPlayer2 = new PhoneNumber(Environment.GetEnvironmentVariable("nextPlayer2"));
            string nextTV = "";

            TwilioClient.Init(accountId, authToken);

            var p2notification = MessageResource.Create(
                body: $"You're up next on tv {nextTV} against {nextPlayer1}",
                from: myNumber,
                to: nextPlayer2);

            var p1notification = MessageResource.Create(
                body: $"You're up next on tv {nextTV} against {nextPlayer2}",
                from: myNumber,
                to: nextPlayer1);

            TwilioClient.Invalidate();
            // lookup the bracket

            string tourneyId = Environment.GetEnvironmentVariable("ChallongeTournamentId");
            string apiKey = Environment.GetEnvironmentVariable("ChallongeApiKey");
            TourneyDal tourneyDal = new TourneyDal(apiKey, tourneyId);
            MongoDal mongoDal = new MongoDal();

            Participant player = await mongoDal.GetParticipantByPhoneNumber("");

            Matches matches = await mongoDal.GetMatchesAsync();
            
            // get the sender's game
            
            // assume sender won


            // send sender to winner parent
            // send other player to loser parent
            // update database
            // Perform calculations, API lookups, etc. here
            
            var congrats = new MessagingResponse()
                .Message($"Congrats on winning!  You sent {formValues["Body"]}");
            var congratsTwiml = congrats.ToString();
            congratsTwiml = congratsTwiml.Replace("utf-16", "utf-8");
            
            return new HttpResponseMessage
            {
                Content = new StringContent(congratsTwiml.ToString(), Encoding.UTF8, "application/xml")
            };
        }
    }
}
