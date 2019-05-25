using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using WorldsFirst.Schemas;
using System.Threading;
using System.Threading.Tasks;
using AuTO.BsonSchema;
using System.Linq;
using MongoDB.Bson;

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
            client = new MongoClient("mongodb+srv://idk:idk@cluster0-nz2zj.azure.mongodb.net/WorldsFirstSmash?retryWrites=true");
            db = client.GetDatabase("WorldsFirstSmash");
            participantCollection = db.GetCollection<ParticipantBson>("Participants");
            matchesCollection = db.GetCollection<MatchListBson>("MessagedMatches");
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

        public async Task UpdateMatchListAsync(Matches matches, string newMatchId)
        {
            List<string> newMatchList = matches.MatchIds;
            newMatchList.Add(newMatchId);

            var filter = Builders<MatchListBson>.Filter.Eq("_id", matches.Id);
            var update = Builders<MatchListBson>.Update.Set("matches", newMatchList);

            UpdateResult result = await matchesCollection.UpdateOneAsync(filter, update);

            return;
        }
    }
}
