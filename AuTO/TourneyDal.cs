using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WorldsFirst
{
    class TourneyDal
    {
        static readonly IList<MediaTypeFormatter> Formatters = new[]
        {
            new JsonMediaTypeFormatter()
        };

        readonly string apiKey;
        readonly HttpClient client;
        readonly string tourneyId;

        public TourneyDal(string apiKey, string tourneyId)
        {
            this.apiKey = apiKey;
            this.tourneyId = tourneyId;

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<JArray> GetAllParticipantsAsync(string tourneyId)
        {
            string uri = $"https://api.challonge.com/v1/tournaments/{tourneyId}/participants.json?api_key={apiKey}";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            HttpResponseMessage response = null;
            for(int i = 0; i <3; i++)
            {
                response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }

            return JArray.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task UpdateWinnerAsync(string matchId, string winnerId, string score)
        {
            string uri = $"https://api.challonge.com/v1/tournaments/{tourneyId}/matches/{matchId}.json?api_key={apiKey}&match[winner_id]={winnerId}&match[scores_csv]={score}";
            var request = new HttpRequestMessage(HttpMethod.Put, uri);

            for(int i = 0; i <3; i++)
            {
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }
        }

        public async Task<JArray> GetOpenMatchesAsync(string playerId = "")
        {
            string uri = $"https://api.challonge.com/v1/tournaments/{tourneyId}/matches.json?api_key={apiKey}&state=open";
            uri += playerId == "" ? "" : $"&participant_id={playerId}";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            HttpResponseMessage response = null;
            for(int i = 0; i <3; i++)
            {
                response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }

            return JArray.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}