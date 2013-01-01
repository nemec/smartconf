using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartConf.Validation;

namespace SmartConf.UnitTest
{
    [TestClass]
    public class RuleBasedValidatorUnitTest
    {
        private const string FailingName = "SteveWithLongName";

        private class Config
        {
            public string Name { get; set; }
        }

        private static bool CustomBooleanRule(Config obj)
        {
            return obj.Name.Length < 10;
        }

        private static void CustomComplexRule(Config obj)
        {
            if (!CustomBooleanRule(obj))
            {
                throw new RuleBasedValidationException("Complex rule");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(RuleBasedValidationException))]
        public void Validate_WithBooleanRuleFailingValidation_ThrowsException()
        {
            // Arrange
            var config = new Config
            {
                Name = FailingName
            };

            var validator = new RuleBasedValidator<Config>();

            validator.AddRule((BooleanRule<Config>) CustomBooleanRule);

            // Act
            validator.Validate(config);
        }

        [TestMethod]
        [ExpectedException(typeof(RuleBasedValidationException))]
        public void Validate_WithComplexRuleFailingValidation_ThrowsException()
        {
            // Arrange
            var config = new Config
            {
                Name = FailingName
            };

            var validator = new RuleBasedValidator<Config>();

            validator.AddRule((ComplexRule<Config>)CustomComplexRule);

            // Act
            validator.Validate(config);
        }
    }
}
