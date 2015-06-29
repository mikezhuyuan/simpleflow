using System;
using System.Collections.Generic;
using System.Linq;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class InMemoryDataStoreTests
    {
        readonly InMemoryDataStore _dataStore = new InMemoryDataStore();

        [Fact]
        public void ShouldGenerateIdentityKey()
        {
            _dataStore.Add(1, 1, typeof (int));
            _dataStore.Add(1, 1, typeof (int));
            _dataStore.Add(1, 1, typeof (int));
            var id = _dataStore.Add(1, 1, typeof (int));

            Assert.Equal(4, id);
        }

        [Fact]
        public void ReturnsDefaultValueIfKeyDoesNotExists()
        {
            Assert.Equal(0, _dataStore.Get<int>(1));
            Assert.Equal(null, _dataStore.Get<string>(1));
            Assert.Equal(null, _dataStore.Get<User>(1));
        }

        [Fact]
        public void SplitValueShouldReuseIdsIfItemIsReferenceType()
        {
            var id1 = _dataStore.Add(1, 1, typeof (int));
            var id2 = _dataStore.Add(1, 2, typeof (int));
            var id3 = _dataStore.AddReferences(1, new[] {id1, id2}, typeof (int));

            var ids = _dataStore.SplitAndGetIds(id3).ToArray();

            Assert.Equal(new[] {id1, id2}, ids);
        }

        [Fact]
        public void SplitValueAndAddingNewItemsIfItemIsNotReferenceType()
        {
            var id1 = _dataStore.Add(1, new[] {new[] {"a"}}, typeof (string[][]));
            var id2 = _dataStore.SplitAndGetIds(id1).Single();
            var id3 = _dataStore.SplitAndGetIds(id2).Single();

            Assert.Equal("a", (string) _dataStore.Get(id3));
        }

        [Fact]
        public void TestAddingDataWithDifferentTypes()
        {
            Assert.Equal(1, (int) _dataStore.Get(_dataStore.Add(1, 1, typeof (int))));
            Assert.Equal("1", (string) _dataStore.Get(_dataStore.Add(1, "1", typeof (string))));
            Assert.Equal(Helpers.Now, (DateTime) _dataStore.Get(_dataStore.Add(1, Helpers.Now, typeof (DateTime))));
            Assert.Equal(new[] {1},
                (IEnumerable<int>) _dataStore.Get(_dataStore.Add(1, new[] {1}, typeof (IEnumerable<int>))));

            var user = new User {Id = 1, Dob = Helpers.Now, Name = "m"};
            Assert.Equal(user, (User) _dataStore.Get(_dataStore.Add(1, user, typeof (User))));
        }

        [Fact]
        public void TestAddingReferenceType()
        {
            var id1 = _dataStore.Add(1, "a", typeof (string));
            var id2 = _dataStore.AddReference(1, id1);
            var id3 = _dataStore.AddReference(1, id2);

            Assert.Equal("a", (string) _dataStore.Get(id3));
        }

        [Fact]
        public void TestAddingGroupOfReferencesWithSameType()
        {
            var id1 = _dataStore.Add(1, "a", typeof (string));
            var id2 = _dataStore.Add(1, "b", typeof (string));
            var id3 = _dataStore.AddReferences(1, new[] {id1, id2}, typeof (string));

            Assert.Equal(new[] {"a", "b"}, (IEnumerable<string>) _dataStore.Get(id3));
        }

        [Fact]
        public void NestedGroupOfReferences()
        {
            var id1 = _dataStore.Add(1, 1, typeof (int));
            var id2 = _dataStore.AddReferences(1, new[] {id1}, typeof (int));
            var id3 = _dataStore.AddReferences(1, new[] {id2}, typeof (IEnumerable<int>));
            var id4 = _dataStore.AddReferences(1, new[] {id3}, typeof (IEnumerable<IEnumerable<int>>));

            Assert.True(_dataStore.Get(id2) is IEnumerable<int>);
            Assert.True(_dataStore.Get(id3) is IEnumerable<IEnumerable<int>>);
            Assert.True(_dataStore.Get(id4) is IEnumerable<IEnumerable<IEnumerable<int>>>);
        }

        [Fact]
        public void TestAddingGroupOfReferencesWithDifferentTypes()
        {
            var id1 = _dataStore.Add(1, "a", typeof (string));
            var id2 = _dataStore.Add(1, 1, typeof (int));
            var id3 = _dataStore.AddReferences(1, new[] {id1, id2}, typeof (object));

            Assert.Equal(new object[] {"a", 1}, (IEnumerable<object>) _dataStore.Get(id3));
        }

        class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Dob { get; set; }

            public override bool Equals(object obj)
            {
                var other = (User) obj;
                return Id == other.Id && Name == other.Name && Dob == other.Dob;
            }
        }
    }
}