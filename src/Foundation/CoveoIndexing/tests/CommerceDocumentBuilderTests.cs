using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.Tests
{
    [TestFixture]
    public class CommerceDocumentBuilderTests
    {
        private CommerceDocumentBuilder m_DocumentBuilder;
        private JToken m_SellableItem;
        private Mock<IExtractor> m_Extractor;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_SellableItem = JToken.Parse(Constants.SELLABLE_ITEM_JSON_FROM_REPOSITORY);
        }

        [SetUp]
        public void SetUp()
        {
            m_Extractor = new Mock<IExtractor>();
            IEnumerable<IExtractor> extractors = new List<IExtractor> {
                m_Extractor.Object
            };
            
            m_DocumentBuilder = new CommerceDocumentBuilder(extractors);
        }

        [Test]
        public void GivenNullExtractors_WhenCreatingObject_ExpectNoException()
        {
            Assert.DoesNotThrow(() => new CommerceDocumentBuilder(null));
        }

        [Test]
        public void GivenNullExtractors_WhenBuildingDocument_ExpectNoException()
        {
            m_DocumentBuilder = new CommerceDocumentBuilder(null);
            Assert.DoesNotThrow(() => BuildDocument());
        }

        [Test]
        public void GivenSellableItem_WhenBuildingDocument_ExpectICoveoIndexableCommerceItem()
        {
            var indexableItem = BuildDocument();

            Assert.IsInstanceOf<ICoveoIndexableCommerceItem>(indexableItem);
        }

        [Test]
        public void GivenNullSellableItem_WhenBuildingDocument_ExpectArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => m_DocumentBuilder.Build(null));
        }

        [Test]
        public void GivenExtractors_WhenBuildingDocument_ExpectExtractorsAreRun()
        {
            BuildDocument();

            m_Extractor.Verify(extractor => extractor.Extract(It.IsAny<JToken>()), Times.Once);
        }

        [Test]
        public void GivenNullExtractor_WhenBuildingDocument_ExpectNoException()
        {
            IEnumerable<IExtractor> extractors = new List<IExtractor> {
                null
            };
            m_DocumentBuilder = new CommerceDocumentBuilder(extractors);

            Assert.DoesNotThrow(() => BuildDocument());
        }

        private ICoveoIndexableCommerceItem BuildDocument()
        {
            return m_DocumentBuilder.Build(m_SellableItem);
        }
    }
}