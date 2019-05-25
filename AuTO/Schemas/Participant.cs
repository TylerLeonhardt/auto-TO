using AuTO.BsonSchema;

namespace WorldsFirst.Schemas
{
    public class Participant
    {
        public Participant()
        { }

        public Participant(ParticipantBson bson)
        {
            this.ChallongeId = bson.ChallongeId;
            this.PhoneNumber = bson.PhoneNumber;
            this.Name = bson.Name;
        }

        public string ChallongeId { get; set; }

        public string PhoneNumber { get; set; }

        public string Name { get; set; }
    }
}
