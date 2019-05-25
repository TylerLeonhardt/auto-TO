using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuTO.BsonSchema
{
    public class ParticipantBson
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("challongeId")]
        public string ChallongeId { get; set; }

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

    }
}
