using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using WorldsFirst.Schemas;
using System.Threading;
using System.Threading.Tasks;
using AuTO.BsonSchema;
using System.Linq;

namespace IHateNamingThings
{

    public class MongoDal
    {
        MongoClient client;
        IMongoDatabase db;
        IMongoCollection<ParticipantBson> collection;

        public MongoDal()
        {
            client = new MongoClient("mongodb+srv://idk:idk@cluster0-nz2zj.azure.mongodb.net/WorldsFirstSmash?retryWrites=true");
            db = client.GetDatabase("WorldsFirstSmash");
            collection = db.GetCollection<ParticipantBson>("Participants");
        }

        public async Task<Participant> GetParticipantByPhoneNumber(string phoneNumber)
        {
            var filter = Builders<ParticipantBson>.Filter.Eq("phoneNumber", phoneNumber);
            var result = await collection.Find(filter).ToListAsync();

            ParticipantBson bson = result.FirstOrDefault();

            return new Participant(bson);
        }

        public async Task<Participant> GetParticipantByChallongeId(string challongeId)
        {
            var filter = Builders<ParticipantBson>.Filter.Eq("challongeId", challongeId);
            var result = await collection.Find(filter).ToListAsync();

            ParticipantBson bson = result.FirstOrDefault();

            return new Participant(bson);
        }
    }
}
