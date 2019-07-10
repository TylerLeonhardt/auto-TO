using System.Collections.Generic;
using AuTO.BsonSchema;
using MongoDB.Bson;

namespace WorldsFirst.Schemas
{
    public class Matches
    {
        public Matches()
        { }

        public Matches(MatchListBson bson)
        {
            MatchIds.AddRange(bson.Matches);
            Id = bson.Id;
        }

        internal ObjectId Id { get; set; }

        internal List<string> MatchIds { get; } = new List<string>();

    }
}
