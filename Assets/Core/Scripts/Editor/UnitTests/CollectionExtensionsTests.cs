using System.Collections.Generic;
using CoreDomain.Scripts.Extensions;
using NUnit.Framework;

namespace CoreDomain.Scripts.Editor.UnitTests
{
    public class CollectionExtensionsTests
    {
        [Test]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenCollectionIsNull()
        {
            List<int> nullList = null;
            Assert.IsTrue(nullList.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenCollectionIsEmpty()
        {
            var emptyList = new List<string>();
            Assert.IsTrue(emptyList.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnFalse_WhenCollectionHasItems()
        {
            var list = new List<float> { 1f };
            Assert.IsFalse(list.IsNullOrEmpty());
        }
        
        [Test]
        public void RemoveElements_ShouldRemoveExistingItems()
        {
            var list = new List<string> { "a", "b", "c", "d" };
            var toRemove = new List<string> { "b", "d" };

            list.RemoveElements(toRemove);

            CollectionAssert.AreEqual(new[] { "a", "c" }, list);
        }

        [Test]
        public void RemoveElements_ShouldIgnoreItemsNotInList()
        {
            var list = new List<int> { 1, 2, 3 };
            var toRemove = new List<int> { 4, 5 };

            list.RemoveElements(toRemove);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void RemoveElements_ShouldHandleDuplicatesCorrectly()
        {
            var list = new List<int> { 1, 1, 2, 3 };
            var toRemove = new List<int> { 1 };

            list.RemoveElements(toRemove);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void RemoveElements_ShouldDoNothing_WhenRemoveListIsEmpty()
        {
            var list = new List<string> { "a", "b" };
            var toRemove = new List<string>();

            list.RemoveElements(toRemove);

            CollectionAssert.AreEqual(new[] { "a", "b" }, list);
        }
    }
}