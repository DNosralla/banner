using System.Collections.Generic;
using System.Threading.Tasks;
using BannerflowTest.Domain;

namespace BannerflowTest.Data
{
	public interface IBannerRepository
	{
		Task<List<Banner>> GetAll();
		Task<Banner> GetById(string id);
		Task<string> GetHtml(string id);
		Task<Banner> Create(Banner banner);
		Task Update(Banner banner);
		Task Remove(string id);
	}
}