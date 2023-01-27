﻿using FluentAssertions;
using RetroPipes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions.Common;
using RetroPipes.Helpers;
using Xunit;
using RetroPipes.LocalStorageTests.Stubs;

namespace RetroPipes.LocalStorageTests
{
    public class LocalStorageTests
    {
        [Fact(DisplayName = "LocalStorage should be initializable")]
        public void LocalStorage_Should_Be_Initializable()
        {
            var target = new LocalStorage();
            target.Should().NotBeNull();
        }

        [Fact(DisplayName = "LocalStorage should implement IDisposable")]
        public void LocalStorage_Should_Implement_IDisposable()
        {
            using (var target = new LocalStorage())
            {
                target.Should().NotBeNull();
            }
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist simple string")]
        public void LocalStorage_Store_Should_Persist_Simple_String()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var expectedValue = "I-AM-GROOT";
            var storage = new LocalStorage();

            // act
            storage.Store(key, expectedValue);
            storage.Persist();

            // assert
            var target = storage.Get(key);
            target.Should().BeOfType<string>();
            target.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist simple DateTime as struct")]
        public void LocalStorage_Store_Should_Persist_Simple_DateTime_As_Struct()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var expectedValue = DateTime.Now;
            var storage = new LocalStorage();

            // act
            storage.Store(key, expectedValue);
            storage.Persist();

            // assert
            var target = storage.Get<DateTime>(key);
            target.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist and retrieve correct type")]
        public void LocalStorage_Store_Should_Persist_And_Retrieve_Correct_Type()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var value = (double)42.4m;
            var storage = new LocalStorage();

            // act
            storage.Store(key, value);
            storage.Persist();

            // assert
            var target = storage.Get<double>(key);
            target.Should().Be(value);
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist multiple values")]
        public void LocalStorage_Store_Should_Persist_Multiple_Values()
        {
            // arrange - create multiple values, of different types
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            var key3 = Guid.NewGuid().ToString();
            var value1 = "It was the best of times, it was the worst of times.";
            var value2 = DateTime.Now;
            var value3 = Int32.MaxValue;
            var storage = new LocalStorage();

            // act
            storage.Store(key1, value1);
            storage.Store(key2, value2);
            storage.Store(key3, value3);
            storage.Persist();

            // assert
            var target1 = storage.Get<string>(key1);
            var target2 = storage.Get<DateTime>(key2);
            var target3 = storage.Get<int>(key3);

            target1.Should().Be(value1);
            target2.Should().Be(value2);
            target3.Should().Be(value3);
        }

        [Fact(DisplayName = "LocalStorage.Store() should overwrite existing key")]
        public void LocalStorage_Store_Should_Overwrite_Existing_Key()
        {
            // arrange
            const string key = "I-Will-Be-Used-Twice";
            var storage = new LocalStorage();
            var original_value = new Joke { Id = 1, Text = "Yo mammo is so fat..." };
            storage.Store(key, original_value);
            storage.Persist();
            var expected_value = new Joke { Id = 2, Text = "... she left the house in high heels and when she came back she had on flip flops" };

            // act - overwrite the existing value
            storage.Store(key, expected_value);
            storage.Persist();
            var target = storage.Get<Joke>(key);

            // assert - last stored value should be the truth
            target.Should().NotBeNull();
            target.Equals(expected_value);
        }

        [Fact(DisplayName = "LocalStorage.Clear() should clear all in-memory content")]
        public void LocalStorage_Clear_Should_Clear_All_Content()
        {
            // arrange - make sure something is stored in the LocalStorage
            var storage = new LocalStorage();
            var filepath = FileHelpers.GetLocalStoreFilePath(".localstorage");
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();
            storage.Store(key, value);
            storage.Persist();

            // act - clear the store
            storage.Clear();

            // assert - open the file here and make sure the contents are empty
            storage.Count.Should().Be(0);
        }

        [Fact(DisplayName = "LocalStorage.Persist() should create file with custom filename on filesystem")]
        public void LocalStorage_Persist_Should_Create_File_With_Custom_Filename_On_Filesystem()
        {
            // arrange - create random filename
            var randomCustomFilename = $"{Guid.NewGuid().ToString("N")}.dat";
            var config = new LocalStorageConfiguration() { Filename = randomCustomFilename };
            var storage = new LocalStorage(config);
            var key1 = Guid.NewGuid().ToString();
            var value1 = "My kingdom for a file with a random name.";
            storage.Store(key1, value1);

            // act
            storage.Persist();

            // assert
            var expectedFilepath = FileHelpers.GetLocalStoreFilePath(randomCustomFilename);
            File.Exists(expectedFilepath).Should().BeTrue(because: $"file '{expectedFilepath}'should be created during Persist()");

            // cleanup
            storage.Destroy();
            File.Exists(expectedFilepath).Should().BeFalse(because: $"file '{expectedFilepath} should be deleted after Destroy()");
        }

        [Fact(DisplayName = "LocalStorage.Persist() should create file on filesystem")]
        public void LocalStorage_Persist_Should_Create_File_On_Filesystem()
        {
            // arrange - expect file with default filename
            var defaultFilename = new LocalStorageConfiguration().Filename;
            var expectedFilepath = FileHelpers.GetLocalStoreFilePath(defaultFilename);
            if (File.Exists(expectedFilepath)) File.Delete(expectedFilepath);

            var storage = new LocalStorage();
            var key1 = Guid.NewGuid().ToString();
            var value1 = "My kingdom for a file with a default name.";
            storage.Store(key1, value1);

            // act
            storage.Persist();

            // assert
            File.Exists(expectedFilepath).Should().BeTrue(because: $"file '{expectedFilepath}'should be created during Persist()");

            // cleanup
            storage.Destroy();
            File.Exists(expectedFilepath).Should().BeFalse(because: $"file '{expectedFilepath} should be deleted after Destroy()");
        }

        [Fact(DisplayName = "LocalStorage.Persist() should leave previous entries intact")]
        public void LocalStorage_Persist_Should_Leave_Previous_Entries_Intact()
        {
            // arrange - add an arbitrary item and persist
            var storage = new LocalStorage();
            var key1 = Guid.NewGuid().ToString();
            var value1 = "Some kind of monster";
            storage.Store(key1, value1);
            storage.Persist();

            // act - add a second item
            var key2 = Guid.NewGuid().ToString();
            var value2 = "Some kind of monster";
            storage.Store(key2, value2);
            storage.Persist();

            // assert - prove that both items remain intact
            var target1 = storage.Get<string>(key1);
            var target2 = storage.Get<string>(key2);
            target1.Should().Be(value1);
            target2.Should().Be(value2);
        }

        [Fact(DisplayName = "LocalStorage.Store() should throw exception in readonly mode")]
        public void LocalStorage_Store_Should_Throw_Exception_In_ReadOnly_Mode()
        {
            // arrange - create localstorage in read-only mode
            var storage = new LocalStorage(new LocalStorageConfiguration() { ReadOnly = true });
            var key = Guid.NewGuid().ToString();
            var value = "Macho Man Randy Savage";

            // act + assert
            var target = Assert.Throws<LocalStorageException>(() => storage.Store(key, value));
            target.Message.Should().Be(ErrorMessages.CannotExecuteStoreInReadOnlyMode);
        }

        [Fact(DisplayName = "LocalStorage.Persist() should throw exception in readonly mode")]
        public void LocalStorage_Persist_Should_Throw_Exception_In_ReadOnly_Mode()
        {
            // arrange - create localstorage in read-only mode
            var storage = new LocalStorage(new LocalStorageConfiguration() { ReadOnly = true });

            // act + assert
            var target = Assert.Throws<LocalStorageException>(() => storage.Persist());
            target.Message.Should().Be(ErrorMessages.CannotExecutePersistInReadOnlyMode);
        }

        [Fact(DisplayName = "LocalStorage.Remove() should delete existing key")]
        public void LocalStorage_Remove_Should_Delete_Existing_Key()
        {
            // arrange - add key and verify its in memory
            var storage = new LocalStorage();
            var key = Guid.NewGuid().ToString();
            var value = "Peter Weyland";
            storage.Store(key, value);
            storage.Exists(key).Should().BeTrue();

            // act - remove key
            storage.Remove(key);

            // assert - verify key has been removed
            storage.Exists(key).Should().BeFalse();
        }

        [Fact(DisplayName = "LocalStorage.Remove() should not break on non-existing key")]
        public void LocalStorage_Remove_Should_Not_Break_On_NonExisting_Key()
        {
            // arrange - create instance and verify key doesn't exist already
            var storage = new LocalStorage();
            var key = Guid.NewGuid().ToString();
            storage.Exists(key).Should().BeFalse(because: "expect key not to exist yet");

            // act - remove key that doesn't exist, should still continue
            storage.Remove(key);

            // assert
            storage.Exists(key).Should().BeFalse(because: "key still should not exist");
        }

        [Fact(DisplayName = "LocalStorage should remain intact between multiple instances")]
        public void LocalStorage_Should_Remain_Intact_Between_Multiple_Instances()
        {
            // arrange - add an arbitrary item and persist
            var storage1 = new LocalStorage();
            var key1 = Guid.NewGuid().ToString();
            var value1 = "Robert Baratheon";
            storage1.Store(key1, value1);
            storage1.Persist();

            // act - create a second instance of the LocalStorage,
            // and persist some more stuff
            var storage2 = new LocalStorage();
            var key2 = Guid.NewGuid().ToString();
            var value2 = "Ned Stark";
            storage2.Store(key2, value2);
            storage2.Persist();

            // assert - prove that entries from both instances still exist
            var storage3 = new LocalStorage();
            var target1 = storage3.Get<string>(key1);
            var target2 = storage3.Get<string>(key2);
            target1.Should().Be(value1);
            target2.Should().Be(value2);
        }

        [Fact(DisplayName = "LocalStorage should support unicode")]
        public void LocalStorage_Store_Should_Support_Unicode()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            const string expectedValue = "Juliën's Special Characters: ~!@#$%^&*()œōøęsæ";
            var storage = new LocalStorage();

            // act
            storage.Store(key, expectedValue);
            storage.Persist();

            // assert
            var target = storage.Get(key);
            target.Should().BeOfType<string>();
            target.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "LocalStorage should perform decently with large collections")]
        public void LocalStorage_Should_Perform_Decently_With_Large_Collections()
        {
            // arrange - create a larger collection (100K records)
            var stopwatch = Stopwatch.StartNew();
            var storage = new LocalStorage();
            for (var i = 0; i < 100000; i++)
                storage.Store(Guid.NewGuid().ToString(), i);

            storage.Persist();

            // act - create new instance (e.g. load the larger collection from disk)
            var target = new LocalStorage();
            target.Clear();
            stopwatch.Stop();

            // cleanup - delete the (large!) persisted file
            target.Destroy();

            // assert - make sure the entire operation is done in < 1sec. (psychological boundry, if you will)
            stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(1000);
        }

        [Fact(DisplayName = "LocalStorage should perform decently with many iterations collections")]
        public void LocalStorage_Should_Perform_Decently_With_Many_Opens_And_Writes()
        {
            // arrange - iterate a lot of times through open/persist/close
            for (var i = 0; i < 1000; i++)
            {
                var storage = new LocalStorage();
                // storage.Clear();
                storage.Store(Guid.NewGuid().ToString(), i);
                storage.Persist();
            }

            // cleanup
            var store = new LocalStorage();
            store.Destroy();
        }

        [Fact(DisplayName = "LocalStorage.Exists() should locate existing key")]
        public void LocalStorage_Exists_Should_Locate_Existing_Key()
        {
            // arrange
            var storage = new LocalStorage();
            var expected_key = Guid.NewGuid().ToString();
            storage.Store(expected_key, Guid.NewGuid().ToString());

            // act
            var target = storage.Exists(expected_key);

            // assert
            target.Should().BeTrue();
        }

        [Fact(DisplayName = "LocalStorage.Exists() should ignore non-existing key")]
        public void LocalStorage_Exists_Should_Ignore_NonExisting_Key()
        {
            // arrange
            var storage = new LocalStorage();
            var nonexisting_key = Guid.NewGuid().ToString();

            // act
            var target = storage.Exists(nonexisting_key);

            // assert
            target.Should().BeFalse();
        }

        [Fact(DisplayName = "LocalStorage.Keys() should return collection of all keys")]
        public void LocalStorage_Keys_Should_Return_Collection_Of_Keys()
        {
            // arrange
            var storage = new LocalStorage(TestHelpers.UniqueInstance());
            for (var i = 0; i < 10; i++)
                storage.Store(Guid.NewGuid().ToString(), i);
            var expected_keycount = storage.Count;

            // act
            var target = storage.Keys();

            // assert
            target.Should().NotBeNullOrEmpty();
            target.Count.Should().Be(expected_keycount);
        }

        [Fact(DisplayName = "LocalStorage.Keys() should return 0 on empty collection")]
        public void LocalStorage_Keys_Should_Return_Zero_On_Empty_Collection()
        {
            // arrange
            var storage = new LocalStorage(TestHelpers.UniqueInstance());

            // act
            var target = storage.Keys();

            // assert
            target.Should().NotBeNull();
            target.Should().BeEmpty();
            target.Count.Should().Be(0, because: "nothing is added to the LocalStorage");
        }

        [Fact(DisplayName = "LocalStorage.Query() should cast to a collection")]
        public void LocalStorage_Query_Should_Cast_Response_To_Collection()
        {
            // arrange - persist a collection to storage
            var collection = CarFactory.Create();
            var storage = new LocalStorage();
            var key = Guid.NewGuid().ToString();
            var expected_amount = collection.Count();
            storage.Store(key, collection);

            // act - fetch directly as a collection, passing along a where-clause
            var target = storage.Query<Car>(key);

            // assert
            target.Should().NotBeNull();
            target.Count().Should().Be(expected_amount);
        }

        [Fact(DisplayName = "LocalStorage.Query() should respect a provided predicate")]
        public void LocalStorage_Query_Should_Respect_Provided_Predicate()
        {
            // arrange - persist a collection to storage
            var collection = CarFactory.Create();
            var storage = new LocalStorage();
            var key = Guid.NewGuid().ToString();
            var expected_brand = "BMW";
            var expected_amount = collection.Count(c => c.Brand == expected_brand);
            storage.Store(key, collection);

            // act - fetch directly as a collection, passing along a where-clause
            var target = storage.Query<Car>(key, c => c.Brand == expected_brand);

            // assert
            target.Should().NotBeNull();
            target.Count().Should().Be(expected_amount);
            target.All(c => c.Brand == expected_brand);
        }

        [Fact(DisplayName = "LocalStorage.Destroy() should delete file on disk")]
        public void LocalStorage_Destroy_Should_Delete_File_On_Disk()
        {
            // arrange
            var random_filename = Guid.NewGuid().ToString("N");
            var filepath = FileHelpers.GetLocalStoreFilePath(random_filename);
            var config = new LocalStorageConfiguration()
            {
                Filename = random_filename
            };

            var storage = new LocalStorage(config);
            storage.Persist();

            // act 
            storage.Destroy();

            // assert
            File.Exists(filepath).Should().BeFalse();
        }
    }
}
