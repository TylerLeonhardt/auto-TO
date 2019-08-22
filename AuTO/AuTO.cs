using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;
using WorldsFirst.Schemas;

namespace WorldsFirst
{
    public class AuTO
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

            string tournamentOrganizerNumber = Environment.GetEnvironmentVariable("tournamentOrganizerNumber");
            string tourneyId = Environment.GetEnvironmentVariable("ChallongeTournamentId");
            string apiKey = Environment.GetEnvironmentVariable("ChallongeApiKey");

            var tourneyDal = new TourneyDal(apiKey, tourneyId);
            var mongoDal = new MongoDal();

            string senderNumber = "+" + formValues["From"].Substring(1);

            if(formValues["Body"].Trim().ToLower() == "master message" && senderNumber == tournamentOrganizerNumber)
            {
                await UpdateAllPlayerChallongeIdsAsync(tourneyId, tourneyDal, mongoDal);
                var message = new MessagingResponse()
                    .Message($"Enjoy the tournament!  You should be ready to play.");
                var messageTwiml = message.ToString();
                messageTwiml = messageTwiml.Replace("utf-16", "utf-8");

                return new HttpResponseMessage
                {
                    Content = new StringContent(messageTwiml.ToString(), Encoding.UTF8, "application/xml")
                };
            }

            if(formValues["Body"].Trim().ToLower() != "i won")
            {
                var error = new MessagingResponse()
                    .Message($"DON'T @ ME!!  You sent {formValues["Body"]}.  I only respect 'I won' (no quotes)");
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
            Participant senderParticipant = await mongoDal.GetParticipantByPhoneNumberAsync(senderNumber);

            // get the sender's game
            JArray openGamesForSender = await tourneyDal.GetOpenMatchesAsync(senderParticipant.ChallongeId);
            var senderGame = (JObject)openGamesForSender.Single();

            string loserId;
            string orderedScore;
            if (senderGame["match"]["player1_id"].Value<string>() == senderParticipant.ChallongeId)
            {
                loserId = senderGame["match"]["player2_id"].Value<string>();
                orderedScore = "2-0";
            }
            else
            {
                loserId = senderGame["match"]["player1_id"].Value<string>();
                orderedScore = "0-2";
            }

            // get the loser to send them a message
            Participant loserParticipant = await mongoDal.GetParticipantByChallongeIdAsync(loserId);

            // assume sender won and update that game
            await tourneyDal.UpdateWinnerAsync(
                senderGame["match"]["id"].Value<string>(),
                senderParticipant.ChallongeId,
                orderedScore);

            // get matches that have been notified before
            Matches matches = await mongoDal.GetMatchesAsync();
            var notifiedMatchIds = new HashSet<string> (matches.MatchIds, StringComparer.OrdinalIgnoreCase);

            JObject nextMatch = await GetNextMatchToPlayAsync(tourneyDal, notifiedMatchIds);

            Participant player1 = await mongoDal.GetParticipantByChallongeIdAsync(
                nextMatch["match"]["player1_id"].Value<string>());
            Participant player2 = await mongoDal.GetParticipantByChallongeIdAsync(
                nextMatch["match"]["player2_id"].Value<string>());

            // text next players
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
                to: new PhoneNumber(loserParticipant.PhoneNumber));

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

            return new HttpResponseMessage
            {
            };
        }

        static async Task UpdateAllPlayerChallongeIdsAsync(
            string tourneyId, TourneyDal tourneyDal, MongoDal mongoDal)
        {
            JArray participants = await tourneyDal.GetAllParticipantsAsync(tourneyId);

            foreach (JToken participant in participants)
            {
                await mongoDal.UpdateUserChallongeId(participant["participant"]["name"].ToString(), participant["participant"]["id"].ToString());
            }
        }

        static async Task<JObject> GetNextMatchToPlayAsync(
            TourneyDal tourneyDal, HashSet<string> playedMatchIds)
        {
            JArray openMatches = await tourneyDal.GetOpenMatchesAsync();
            IList<JToken> nonMessagedMatches =
                openMatches.Where(match =>
                    !playedMatchIds.Contains(match["match"]["id"].Value<string>())).ToList();
            IList<JToken> sortedNonMessagedMatches =
                nonMessagedMatches.OrderBy(openMatch =>
                    openMatch["match"]["suggested_play_order"].Value<int>()).ToList();
            return (JObject)sortedNonMessagedMatches.First();
        }
    }
}