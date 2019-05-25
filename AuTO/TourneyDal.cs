namespace IHateNamingThings
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    class TourneyDal
    {
        readonly string apiKey;
        readonly HttpClient client;
        readonly string tourneyId;

        public TourneyDal(string apiKey, string tourneyId)
        {
            this.apiKey = apiKey;
            this.tourneyId = tourneyId;

            client = new HttpClient();
        }

        public async Task UpdateWinnerAsync(string matchId, string winnerId, string score)
        {
            string uri = $"https://{apiKey}api.challonge.com/v1/tournaments/{tourneyId}/matches/{matchId}.json?match[winner_id]={winnerId}&match[scores_csv]={score}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, uri);
            await client.SendAsync(request);
        }

        public async Task<JObject> GetOpenMatchesAsync()
        {
            string uri = $"https://api.challonge.com/v1/tournaments/{tourneyId}/matches.json?state=open";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsAsync<JObject>();
        }

        public async Task<JObject> GetOpenMatchByIdAsync(string playerId)
        {
            string uri = $"https://api.challonge.com/v1/tournaments/{tourneyId}/matches.json?participant_id={playerId}&state=open";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsAsync<JObject>();
        }
    }
}