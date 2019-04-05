using BannerflowTest.API.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BannerflowTest.API.Models.Banner
{
	public class PostBanner
	{
		[Required]
		[ValidHtml]
		public string Html { get; set; }
	}
}
