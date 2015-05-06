using System;
using IdmApi.Models;
using IdmNet.Models;
using IdmNet.SoapModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ObjectCreationAsStatement

namespace IdmApi.Tests
{
    [TestClass]
    public class ETagTests
    {
        [TestMethod]
        public void It_has_a_paremeterless_constructor()
        {
            var it = new ETag();

            Assert.AreEqual("ETag", it.ObjectType);
        }

        [TestMethod]
        public void It_has_a_constructor_that_takes_a_IdmResource_without_a_Creator()
        {
            var idmResource = new IdmResource();
            var it = new ETag(idmResource);

            Assert.AreEqual("ETag", it.ObjectType);
        }

        [TestMethod]
        public void It_has_a_constructor_that_takes_a_IdmResource_WITH_a_Creator()
        {
            var resource = new IdmResource { DisplayName = "myETag", Creator = new Person{DisplayName = "myCreator"}};
            var it = new ETag(resource);

            Assert.AreEqual("myCreator", it.Creator.DisplayName);
        }

        [TestMethod]
        public void It_has_a_constructor_that_takes_a_pagingContext()
        {
            // Arrange
            var pagingContext = new PagingContext
            {
                CurrentIndex = 10,
                EnumerationDirection = "Forwards",
                Expires = "sometime in the distant future",
                Filter = "/ConstantSpecifier",
                Selection = new[] { "DisplayName", "Name" },
                Sorting =
                    new Sorting
                    {
                        Dialect = "The Only Dialect",
                        SortingAttributes =
                            new[]
                            {
                                new SortingAttribute {Ascending = true, AttributeName = "SomeGroupingAttribute"},
                                new SortingAttribute {Ascending = false, AttributeName = "SomeUniqueAttribute"}
                            }
                    }
            };

            // Act
            var it = new ETag(pagingContext);

            // Assert
            Assert.AreEqual("ETag", it.ObjectType);
            Assert.AreEqual(10, it.CurrentIndex);
            Assert.AreEqual("Forwards", it.EnumerationDirection);
            Assert.AreEqual("sometime in the distant future", it.Expires);
            Assert.AreEqual("/ConstantSpecifier", it.Filter);
            Assert.AreEqual("DisplayName,Name", it.Select);
            Assert.AreEqual("The Only Dialect", it.SortingDialect);
            Assert.AreEqual("SomeGroupingAttribute:True,SomeUniqueAttribute:False", it.SortingAttributes);
        }

        [TestMethod]
        public void It_can_convert_to_a_pagingContext()
        {
            // Arrange
            var expectedContext = new PagingContext
            {
                CurrentIndex = 10,
                EnumerationDirection = "Forwards",
                Expires = "sometime in the distant future",
                Filter = "/ConstantSpecifier",
                Selection = new[] { "DisplayName", "Name" },
                Sorting =
                    new Sorting
                    {
                        Dialect = "The Only Dialect",
                        SortingAttributes =
                            new[]
                            {
                                new SortingAttribute {Ascending = true, AttributeName = "SomeGroupingAttribute"},
                                new SortingAttribute {Ascending = false, AttributeName = "SomeUniqueAttribute"}
                            }
                    }
            };
            var it = new ETag
            {
                Filter = expectedContext.Filter,
                SortingAttributes = "SomeGroupingAttribute:True,SomeUniqueAttribute:False",
                CurrentIndex = 10,
                EnumerationDirection = "Forwards",
                Expires = expectedContext.Expires,
                Select = "DisplayName,Name",
                SortingDialect = "The Only Dialect"
            };


            // Act
            PagingContext result = it.ToPagingContext();

            // Assert
            Assert.AreEqual(10, result.CurrentIndex);
            Assert.AreEqual("Forwards", result.EnumerationDirection);
            Assert.AreEqual("sometime in the distant future", result.Expires);
            Assert.AreEqual("/ConstantSpecifier", result.Filter);
            Assert.AreEqual("DisplayName", result.Selection[0]);
            Assert.AreEqual("The Only Dialect", result.Sorting.Dialect);
            Assert.AreEqual("SomeGroupingAttribute", result.Sorting.SortingAttributes[0].AttributeName);
        }




        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void It_throws_when_you_try_to_set_ObjectType_to_anything_other_than_Person()
        {
            new ETag { ObjectType = "foo" };
        }

        [TestMethod]
        public void It_returns_null_for_not_present_properties()
        {
            var it = new ETag();

            Assert.IsNull(it.Filter);
            Assert.IsNull(it.EnumerationDirection);
            Assert.IsNull(it.Expires);
            Assert.IsNull(it.Select);
            Assert.IsNull(it.SortingAttributes);
            Assert.IsNull(it.SortingDialect);

            Assert.IsNull(it.CurrentIndex);
        }

        [TestMethod]
        public void It_can_set_and_get_INT_properties()
        {
            var it = new ETag
            {
                CurrentIndex = 22,
            };

            Assert.AreEqual(22, it.CurrentIndex);
        }

        [TestMethod]
        public void It_can_set_and_get_string_properties()
        {
            var it = new ETag
            {
                Filter = "Test Filter",
                EnumerationDirection = "Test EnumerationDirection",
                Expires = "Test Expires",
                Select = "Test Select",
                SortingAttributes = "Test SortingAttributes",
                SortingDialect = "Test SortingDialect",
            };

            Assert.AreEqual("Test Filter", it.Filter);
            Assert.AreEqual("Test EnumerationDirection", it.EnumerationDirection);
            Assert.AreEqual("Test Expires", it.Expires);
            Assert.AreEqual("Test Select", it.Select);
            Assert.AreEqual("Test SortingAttributes", it.SortingAttributes);
            Assert.AreEqual("Test SortingDialect", it.SortingDialect);
        }
    }
}
