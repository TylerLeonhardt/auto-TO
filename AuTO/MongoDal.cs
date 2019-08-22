using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuTO.BsonSchema;
using MongoDB.Bson;
using MongoDB.Driver;
using WorldsFirst.Schemas;

namespace WorldsFirst
{
    public class MongoDal
    {
        MongoClient client;
        IMongoDatabase db;
        IMongoCollection<ParticipantBson> participantCollection;
        IMongoCollection<MatchListBson> matchesCollection;

        public MongoDal()
        {
            string secret = Environment.GetEnvironmentVariable("mongoClientSecret");
            client = new MongoClient(secret);
            db = client.GetDatabase("WorldsFirstSmash");
            participantCollection = db.GetCollection<ParticipantBson>("Participants");
            matchesCollection = db.GetCollection<MatchListBson>("MessagedMatches");
        }

        public async Task<Participant> GetParticipantByNameAsync(string name)
        {
            var filter = Builders<ParticipantBson>.Filter.Eq("name", name);
            var result = await participantCollection.Find(filter).ToListAsync();

            ParticipantBson bson = result.FirstOrDefault();

            return new Participant(bson);
        }

        public async Task<Participant> GetParticipantByPhoneNumberAsync(string phoneNumber)
        {
            var filter = Builders<ParticipantBson>.Filter.Eq("phoneNumber", phoneNumber);
            var result = await participantCollection.Find(filter).ToListAsync();

            ParticipantBson bson = result.FirstOrDefault();

            return new Participant(bson);
        }

        public async Task<Participant> GetParticipantByChallongeIdAsync(string challongeId)
        {
            var filter = Builders<ParticipantBson>.Filter.Eq("challongeId", challongeId);
            var result = await participantCollection.Find(filter).ToListAsync();

            ParticipantBson bson = result.FirstOrDefault();

            return new Participant(bson);
        }

        public async Task<Matches> GetMatchesAsync()
        {
            var result = await matchesCollection.Find(new BsonDocument()).ToListAsync();
            return new Matches(result.FirstOrDefault());
        }

        public async Task UpdateUserChallongeId(string name, string challongeId)
        {
            var filter = Builders<ParticipantBson>.Filter.Eq("name", name);
            var update = Builders<ParticipantBson>.Update.Set("challongeId", challongeId);

            await participantCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateMatchListAsync(Matches matches, string newMatchId)
        {
            // TODO: Remove race condition because it's only one object
            List<string> newMatchList = matches.MatchIds;
            newMatchList.Add(newMatchId);

            var filter = Builders<MatchListBson>.Filter.Eq("_id", matches.Id);
            var update = Builders<MatchListBson>.Update.Set("matches", newMatchList);

            await matchesCollection.UpdateOneAsync(filter, update);
        }
    }
}
