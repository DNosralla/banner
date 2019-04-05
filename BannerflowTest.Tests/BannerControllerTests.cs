using BannerflowTest.API.Controllers;
using BannerflowTest.Data;
using BannerflowTest.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BannerflowTest.Tests
{
	public class BannerControllerTests
	{
		[Fact]
		public async Task Get_ReturnsListOfBanners()
		{
			// Arrange
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(GetTestBanners());
			var controller = new BannerController(null, null, mockRepo.Object);

			// Act
			var response = await controller.Get();

			// Assert
			var result = Assert.IsType<OkObjectResult>(response.Result);
			var list = Assert.IsAssignableFrom<IEnumerable<Banner>>(result.Value);
			Assert.Equal(GetTestBanners().Count, list.Count());
		}

		[Fact]
		public async Task GetById_ReturnsBanner()
		{
			// Arrange
			var testBanner = GetTestBanner();
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetById(testBanner.Id)).ReturnsAsync(GetTestBanner());
			var controller = new BannerController(null, null, mockRepo.Object);

			// Act
			var response = await controller.Get(testBanner.Id);

			// Assert
			var result = Assert.IsType<OkObjectResult>(response.Result);
			var banner = Assert.IsAssignableFrom<Banner>(result.Value);
			Assert.Equal(testBanner.Id, banner.Id);
			Assert.Equal(testBanner.Created, banner.Created);
			Assert.Equal(testBanner.Html, banner.Html);
			Assert.Equal(testBanner.Modified, banner.Modified);
		}

		[Fact]
		public async Task GetById_ReturnsNotFound_ForInvalidBannerId()
		{
			// Arrange
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetById("invalidId")).ReturnsAsync((Banner)null);
			var controller = new BannerController(null, null, mockRepo.Object);

			// Act
			var response = await controller.Get("invalidId");

			// Assert
			var result = Assert.IsType<NotFoundResult>(response.Result);
		}

		[Fact]
		public async Task GetHtml_ReturnsHtml()
		{
			// Arrange
			var testBanner = GetTestBanner();

			var cache = new MemoryCache(new MemoryCacheOptions());

			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetHtml(testBanner.Id)).ReturnsAsync(testBanner.Html);
			var controller = new BannerController(cache, null, mockRepo.Object);

			// Act
			var response = await controller.GetHtml(testBanner.Id);

			// Assert
			var result = Assert.IsType<ContentResult>(response);
			Assert.Equal(testBanner.Html, result.Content);
			Assert.Equal("text/html", result.ContentType);
		}

		[Fact]
		public async Task GetHtml_ReturnsNotFound_ForInvalidBannerId()
		{
			// Arrange
			var testBanner = GetTestBanner();

			var cache = new MemoryCache(new MemoryCacheOptions());

			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetHtml(testBanner.Id)).ReturnsAsync((string)null);
			var controller = new BannerController(cache, null, mockRepo.Object);

			// Act
			var response = await controller.GetHtml(testBanner.Id);

			// Assert
			var result = Assert.IsType<NotFoundResult>(response);
		}

		[Fact]
		public async Task Post_CreatesBanner()
		{
			// Arrange
			var testBanner = GetTestBanner();
			Banner savedBanner = null;
			var mockLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<BannerController>();
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.Create(It.IsAny<Banner>()))
				.Returns(Task.FromResult(testBanner))
				.Callback<Banner>(b=>savedBanner = b)
				.Verifiable();
			var controller = new BannerController(null, mockLogger, mockRepo.Object);

			// Act
			var response = await controller.Create(new API.Models.Banner.PostBanner() { 
				Html = testBanner.Html
			});

			// Assert
			var result = Assert.IsType<CreatedAtRouteResult>(response.Result);
			var banner = Assert.IsAssignableFrom<Banner>(result.Value);
			Assert.Equal(testBanner.Id, banner.Id);
			Assert.Equal(testBanner.Created, banner.Created);
			Assert.Equal(testBanner.Html, banner.Html);
			Assert.Equal(testBanner.Modified, banner.Modified);
			Assert.Null(savedBanner.Modified);
		}

		[Fact]
		public async Task Post_ReturnsBadRequest_ForInvalidHtml()
		{
			// Arrange
			var mockLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<BannerController>();
			var controller = new BannerController(null, mockLogger, null);
			controller.ModelState.AddModelError("Html", "Fake html error");

			// Act
			var response = await controller.Create(new API.Models.Banner.PostBanner()
			{
				Html = "<div>this is not valid"
			});

			// Assert
			var result = Assert.IsType<BadRequestObjectResult>(response.Result);
		}

		[Fact]
		public async Task Put_ModifiesBanner()
		{
			// Arrange
			var testBanner = GetTestBanner();
			var newHtml = "<div>new html</div>";
			Banner savedBanner = null;
			
			var mockLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<BannerController>();
			var mockCache = new Mock<IMemoryCache>();
			
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetById(testBanner.Id)).ReturnsAsync(testBanner);
			mockRepo.Setup(repo => repo.Update(It.IsAny<Banner>()))
				.Callback<Banner>((b)=>savedBanner=b)
				.Returns(Task.FromResult(testBanner))
				.Verifiable();

			var controller = new BannerController(mockCache.Object, mockLogger, mockRepo.Object);

			// Act
			var response = await controller.Update(testBanner.Id, new API.Models.Banner.PutBanner()
			{
				Html = newHtml
			});

			// Assert
			var result = Assert.IsType<NoContentResult>(response);
			Assert.Equal(testBanner.Id, savedBanner.Id);
			Assert.Equal(testBanner.Created, savedBanner.Created);
			Assert.Equal(savedBanner.Html, newHtml);
			Assert.NotNull(savedBanner.Modified);
		}

		[Fact]
		public async Task Put_ReturnsNotFound_ForInvalidId()
		{
			// Arrange
			var testBanner = GetTestBanner();
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetById(testBanner.Id)).ReturnsAsync((Banner)null);
			
			var controller = new BannerController(null, null, mockRepo.Object);

			// Act
			var response = await controller.Update(testBanner.Id, new API.Models.Banner.PutBanner()
			{
				Html = "html"
			});

			// Assert
			var result = Assert.IsType<NotFoundResult>(response);
		}

		[Fact]
		public async Task Put_ReturnsBadRequest_ForInvalidHtml()
		{
			// Arrange
			var mockLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<BannerController>();
			
			var testBanner = GetTestBanner();
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetById(testBanner.Id)).ReturnsAsync(testBanner);
						
			var controller = new BannerController(null, mockLogger, mockRepo.Object);
			controller.ModelState.AddModelError("Html", "Fake html error");

			// Act
			var response = await controller.Update(testBanner.Id, new API.Models.Banner.PutBanner()
			{
				Html = "<div>this is not valid"
			});

			// Assert
			var result = Assert.IsType<BadRequestObjectResult>(response);
		}

		[Fact]
		public async Task Delete_DeletesBanner()
		{
			// Arrange
			var testBanner = GetTestBanner();
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetById(testBanner.Id)).ReturnsAsync(GetTestBanner());
			mockRepo.Setup(repo => repo.Remove(testBanner.Id))
				.Returns(Task.CompletedTask)
				.Verifiable();

			var mockLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<BannerController>();
			var mockCache = new Mock<IMemoryCache>();

			var controller = new BannerController(mockCache.Object, mockLogger, mockRepo.Object);

			// Act
			var response = await controller.Delete(testBanner.Id);

			// Assert
			var result = Assert.IsType<NoContentResult>(response);
		}

		[Fact]
		public async Task Delete_ReturnsNotFound_ForInvalidId()
		{
			// Arrange
			var testBanner = GetTestBanner();
			var mockRepo = new Mock<IBannerRepository>();
			mockRepo.Setup(repo => repo.GetById(testBanner.Id)).ReturnsAsync((Banner)null);

			var controller = new BannerController(null, null, mockRepo.Object);

			// Act
			var response = await controller.Delete(testBanner.Id);

			// Assert
			var result = Assert.IsType<NotFoundResult>(response);
		}

		private List<Banner> GetTestBanners()
		{
			return new List<Banner>() {
				new Banner(){
					Id = "0",
					Created = new DateTime(2019, 1, 1),
					Html = "<div>Banner 0 content</div>"
				},
				new Banner(){
					Id = "1",
					Created = new DateTime(2019, 1, 2),
					Html = "<div>Banner 1 content</div>"
				},
				new Banner(){
					Id = "2",
					Created = new DateTime(2019, 1, 3),
					Modified = new DateTime(2019, 1, 4),
					Html = "<div>Banner 2 content</div>"
				}
			};
		}

		private Banner GetTestBanner(){
			return new Banner()
			{
				Id = "3",
				Created = new DateTime(2019, 1, 4),
				Html = "<div>Test Banner content</div>"
			};
		}
	}
}
