using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BannerflowTest.API.Validation
{
	public class ValidHtmlAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var html = (string)value;

			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);

			if (doc.ParseErrors.Count() > 0)
			{
				//Invalid HTML
				var errors = doc.ParseErrors.Select(e => $"{e.Reason} at line {e.SourceText}");
				return new ValidationResult(String.Join("Invalid html, errors: ", errors));
			} else {
				return ValidationResult.Success;
			}
		}

	}
}
