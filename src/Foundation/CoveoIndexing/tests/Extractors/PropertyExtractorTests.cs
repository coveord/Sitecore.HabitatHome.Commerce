using System;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.Tests.Extractors
{
    [TestFixture]
    public class PropertyExtractorTests
    {
        private const string INPUT_PROPERTY_NAME = "Name";
        private const string OUTPUT_METADATA_NAME = "output";

        private JToken m_CommerceEntity;
        private PropertyExtractor m_PropertyExtractor;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_CommerceEntity = JToken.Parse(Constants.SELLABLE_ITEM_JSON_FROM_REPOSITORY);
        }

        [SetUp]
        public void SetUp()
        {
            m_PropertyExtractor = new PropertyExtractor(INPUT_PROPERTY_NAME, OUTPUT_METADATA_NAME);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(null, OUTPUT_METADATA_NAME)]
        [TestCase(INPUT_PROPERTY_NAME, null)]
        public void GivenNullAttributes_WhenCreatingObject_ExpectArgumentNullException(string p_InputPropertyName,
                                                                                       string p_OutputMetadataName)
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyExtractor(p_InputPropertyName, p_OutputMetadataName));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("", OUTPUT_METADATA_NAME)]
        [TestCase(INPUT_PROPERTY_NAME, "")]
        public void GivenEmptyAttributes_WhenCreatingObject_ExpectArgumentException(string p_InputPropertyName,
                                                                                    string p_OutputMetadataName)
        {
            Assert.Throws<ArgumentException>(() => new PropertyExtractor(p_InputPropertyName, p_OutputMetadataName));
        }

        [Test]
        public void GivenNullCommerceEntity_WhenExtractingProperty_ExpectArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => m_PropertyExtractor.Extract(null));
        }

        [Test]
        public void GivenCommerceEntityWithSpecificName_WhenExtractingProperty_ExpectNameIsExtracted()
        {
            object extractedValue = m_PropertyExtractor.Extract(m_CommerceEntity);

            Assert.AreEqual(Constants.SELLABLE_ITEM_NAME, extractedValue);
        }
    }
}