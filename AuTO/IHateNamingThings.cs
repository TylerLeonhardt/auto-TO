namespace WorldsFirst
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
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

            string senderNumber = "+" + formValues["From"].Substring(1);

            if(formValues["Body"].Trim().ToLower() != "i won")
            {
                var error = new MessagingResponse()
                    .Message($"DON'T @ ME!!  You sent {formValues["Body"]}.  I only respect 'i won' (no quotes)");
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

            // lookup the sender

            string tourneyId = Environment.GetEnvironmentVariable("ChallongeTournamentId");
            string apiKey = Environment.GetEnvironmentVariable("ChallongeApiKey");
            TourneyDal tourneyDal = new TourneyDal(apiKey, tourneyId);
            MongoDal mongoDal = new MongoDal();

            Participant senderParticipant = await mongoDal.GetParticipantByPhoneNumberAsync(senderNumber);

            // get the sender's game
            JArray openGamesForSender = await tourneyDal.GetOpenMatchByIdAsync(senderParticipant.ChallongeId);
            JObject senderGame = (JObject)openGamesForSender.Single();

            // assume sender won
            await tourneyDal.UpdateWinnerAsync(senderGame["match"]["id"].Value<string>(), senderParticipant.ChallongeId, "2-0");

            // get matches that have been notified before
            Matches matches = await mongoDal.GetMatchesAsync();
            var playedMatchIds = new HashSet<string> (matches.MatchIds, StringComparer.OrdinalIgnoreCase);

            // get open matches to choose the next one to play
            JArray openGames = await tourneyDal.GetOpenMatchesAsync();
            IList<JToken> sortedOpenMatches = openGames.OrderBy(openGame => openGame["match"]["suggested_play_order"].Value<int>()).ToList();
            IList<JToken> sortedNonMessagedMatches = sortedOpenMatches.Where(match => !playedMatchIds.Contains(match["match"]["id"].Value<string>())).ToList();
            JObject nextMatch = (JObject)sortedNonMessagedMatches.First();

            Participant player1 = await mongoDal.GetParticipantByChallongeIdAsync(nextMatch["match"]["player1_id"].Value<string>());
            Participant player2 = await mongoDal.GetParticipantByChallongeIdAsync(nextMatch["match"]["player2_id"].Value<string>());

            // test next players
            var nextPlayer1 = new PhoneNumber(player1.PhoneNumber);
            var nextPlayer2 = new PhoneNumber(player2.PhoneNumber);

            TwilioClient.Init(accountId, authToken);

            var congrats = MessageResource.Create(
                body: $"Congrats on winning! :D",
                from: myNumber,
                to: new PhoneNumber(senderNumber));

            var sorry = MessageResource.Create(
                body: $"Tough match :/",
                from: myNumber,
                to: new PhoneNumber(senderNumber));

            var p2notification = MessageResource.Create(
                body: $"You're up now against {player1.Name}. Please go find the open TV.",
                from: myNumber,
                to: nextPlayer2);

            var p1notification = MessageResource.Create(
                body: $"You're up now against {player2.Name}. Please go find the open TV.",
                from: myNumber,
                to: nextPlayer1);

            TwilioClient.Invalidate();

            // update database
            await mongoDal.UpdateMatchListAsync(matches, nextMatch["match"]["id"].Value<string>());

            //var congrats = new MessagingResponse()
            //    .Message($"Congrats on winning!  You sent {formValues["Body"]}");
            //var congratsTwiml = congrats.ToString();
            //congratsTwiml = congratsTwiml.Replace("utf-16", "utf-8");
            
            return new HttpResponseMessage
            {
            };
        }
    }
}
