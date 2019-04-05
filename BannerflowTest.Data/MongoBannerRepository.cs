using BannerflowTest.Domain;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BannerflowTest.Data
{
	public class MongoBannerRepository : IBannerRepository
	{
		private readonly IMongoCollection<Banner> _banners;

		public MongoBannerRepository(IConfiguration config)
		{
			var client = new MongoClient(config.GetConnectionString("BannerDb"));
			var database = client.GetDatabase("BannerDb");
			_banners = database.GetCollection<Banner>("Banners");
		}

		public async Task<List<Banner>> GetAll()
		{
			var query = _banners.Find(banner => true);
			return await query.ToListAsync();
		}

		public async Task<Banner> GetById(string id)
		{
			var query = _banners.Find(b => b.Id == id);
			return await query.FirstOrDefaultAsync();
		}

		public async Task<string> GetHtml(string id)
		{
			var query = _banners.Find(b => b.Id == id);
			query.Project("{Html:1}");

			var banner = await query.FirstOrDefaultAsync();

			if (banner != null)
			{
				return banner.Html;
			}
			else
			{
				return null;
			}
		}

		public async Task<Banner> Create(Banner banner)
		{
			await _banners.InsertOneAsync(banner);
			return banner;
		}

		public async Task Update(Banner banner)
		{
			await _banners.ReplaceOneAsync(b => b.Id == banner.Id, banner);
		}

		public async Task Remove(string id)
		{
			await _banners.DeleteOneAsync(banner => banner.Id == id);
		}
	}
}
