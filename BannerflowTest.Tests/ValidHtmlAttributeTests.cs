using BannerflowTest.API.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BannerflowTest.Tests
{
	public class ValidHtmlAttributeTests
	{

		[Fact]
		public void Validates_ValidHtml()
		{
			// Arrange
			var testModel = new TestModel()
			{
				Html = "<div>This is some valid html</div>"
			};

			// Act
			var result = ValidateModel(testModel);

			// Assert
			Assert.Equal(0, result.Count);
		}

		[Fact]
		public void Validates_PureText()
		{
			// Arrange
			var testModel = new TestModel()
			{
				Html = "This is just text"
			};

			// Act
			var result = ValidateModel(testModel);

			// Assert
			Assert.Equal(0, result.Count);
		}

		[Fact]
		public void DoesNotValidate_UnclosedElements()
		{
			// Arrange
			var testModel = new TestModel()
			{
				Html = "<div>This is not valid"
			};

			// Act
			var result = ValidateModel(testModel);

			// Assert
			Assert.Equal(1, result.Count);
		}

		private IList<ValidationResult> ValidateModel(object model)
		{
			var validationResults = new List<ValidationResult>();
			var ctx = new ValidationContext(model, null, null);
			Validator.TryValidateObject(model, ctx, validationResults, true);
			return validationResults;
		}

	}

	class TestModel
	{
		[ValidHtml]
		public string Html { get; set; }
	}
}
