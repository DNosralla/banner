using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BannerflowTest.Domain
{
	public class Banner
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		public string Html { get; set; }
		public DateTime Created { get; set; }
		public DateTime? Modified { get; set; }
	}
}
