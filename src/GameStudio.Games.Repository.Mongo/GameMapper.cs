using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using GameStudio.Repository.Document.Mongo;


namespace GameStudio.Games.Repository.Mongo
{
    public class GameMapper : ReflectionBsonMapper<string,Game>
    {
        public override void RegisterClassMap(BsonClassMap<Game> cm)
        {
            base.RegisterClassMap(cm);

            cm.MapMember(c => c.UrlSafeName).SetElementName(MetadataFields.Id);
        }

        public GameMapper(IOptions<MongoOptions> options) : base(options)
        {
        }
    }
}
