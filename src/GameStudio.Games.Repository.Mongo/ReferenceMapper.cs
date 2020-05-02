using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using GameStudio.Games;
using GameStudio.Repository.Document.Mongo;

namespace GameStudio.References.Repository.Mongo
{
	public class ReferenceMapper : ReflectionBsonMapper<string, Reference>
	{
        public override void RegisterClassMap(BsonClassMap<Reference> cm)
        {
            base.RegisterClassMap(cm);

            cm.MapMember(c => c.UrlSafeName).SetElementName(MetadataFields.Id);
        }
		public ReferenceMapper(IOptions<MongoOptions> options) : base(options)
		{
		}
	}
}
