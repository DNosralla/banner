using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BannerflowTest.API.Models.Banner;
using BannerflowTest.Data;
using BannerflowTest.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace BannerflowTest.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BannerController : ControllerBase
	{
		private readonly IMemoryCache _cache;
		private readonly ILogger<BannerController> _logger;
		private readonly Data.IBannerRepository _bannerRepository;

		public BannerController(IMemoryCache cache, ILogger<BannerController> logger, IBannerRepository bannerRepository)
		{
			_cache = cache;
			_logger = logger;
			_bannerRepository = bannerRepository;
		}

		[HttpGet(Name = "List")]
		public async Task<ActionResult<List<Banner>>> Get()
		{
			var result = await _bannerRepository.GetAll();
			return Ok(result);
		}

		[HttpGet("{id:length(24)}", Name = "GetById")]
		public async Task<ActionResult<Banner>> Get(string id)
		{
			var banner = await _bannerRepository.GetById(id);

			if (banner == null)
			{
				return NotFound();
			}

			return Ok(banner);
		}

		[Produces("text/html")]
		[HttpGet("{id:length(24)}/html", Name = "GetHtml")]
		public async Task<IActionResult> GetHtml(string id)
		{
			var html = await _cache.GetOrCreateAsync<string>(id, async (entry) =>
			{
				return await _bannerRepository.GetHtml(id);
			});

			if (html != null) {
				return Content(content: html, contentType: "text/html");
			} else {
				return NotFound();
			}
		}

		[HttpPost]
		public async Task<ActionResult<Banner>> Create(PostBanner post)
		{
			if (post == null || !ModelState.IsValid)
			{
				_logger.LogInformation($"Invalid banner post request");
				return BadRequest(ModelState);
			}

			var newBanner = await _bannerRepository.Create(new Banner()
			{
				Html = post.Html,
				Created = DateTime.Now
			});

			_logger.LogInformation($"New Banner Created with id: {newBanner.Id}");

			return CreatedAtRoute("GetById", new { id = newBanner.Id.ToString() }, newBanner);
		}

		[HttpPut("{id:length(24)}")]
		public async Task<IActionResult> Update(string id, PutBanner put)
		{
			var banner = await _bannerRepository.GetById(id);

			if (banner == null)
			{
				return NotFound();
			}

			if (put == null || !ModelState.IsValid)
			{
				_logger.LogInformation($"Invalid banner put request");
				return BadRequest(ModelState);
			}

			//update banner
			banner.Html = put.Html;
			banner.Modified = DateTime.Now;

			await _bannerRepository.Update(banner);
			_cache.Remove(id);

			_logger.LogInformation($"Banner {banner.Id} updated");

			return NoContent();
		}

		[HttpDelete("{id:length(24)}")]
		public async Task<IActionResult> Delete(string id)
		{
			var banner = await _bannerRepository.GetById(id);

			if (banner == null)
			{
				return NotFound();
			}

			await _bannerRepository.Remove(banner.Id);
			_cache.Remove(id);
			_logger.LogInformation($"Banner {banner.Id} deleted");

			return NoContent();
		}
	}
}