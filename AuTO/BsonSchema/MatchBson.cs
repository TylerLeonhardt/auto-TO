using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuTO.BsonSchema
{
    public class MatchListBson
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("matches")]
        public string[] Matches { get; set; }
    }
}
