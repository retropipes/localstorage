// MARS Web App by Rockwell Automation, Inc. (C) 2019-present

using FluentAssertions;
using RetroPipes.Storage.Helpers;
using RetroPipes.Storage.Tests.Stubs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace RetroPipes.Storage.Tests;

public class LocalStorageTests
{
    [Fact(DisplayName = "LocalStorage should be initializable")]
    public void LocalStorageShouldBeInitializable()
    {
        var target = new LocalStorage();
        _ = target.Should().NotBeNull();
    }

    [Fact(DisplayName = "LocalStorage should implement IDisposable")]
    public void LocalStorageShouldImplementIDisposable()
    {
        using (var target = new LocalStorage())
        {
            _ = target.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "LocalStorage.Store() should persist simple string")]
    public void LocalStorageStoreShouldPersistSimpleString()
    {
        // arrange
        var key = Guid.NewGuid().ToString();
        var expectedValue = "I-AM-GROOT";
        var storage = new LocalStorage();

        // act
        storage.Store(key, expectedValue);
        storage.Persist();

        // assert
        var target = storage.Load<string>(key);
        _ = target.Should().BeOfType<string>();
        _ = target.Should().Be(expectedValue);
    }

    [Fact(DisplayName = "LocalStorage.Store() should persist simple DateTime as struct")]
    public void LocalStorageStoreShouldPersistSimpleDateTimeAsStruct()
    {
        // arrange
        var key = Guid.NewGuid().ToString();
        var expectedValue = DateTime.Now;
        var storage = new LocalStorage();

        // act
        storage.Store(key, expectedValue);
        storage.Persist();

        // assert
        var target = storage.Load<DateTime>(key);
        _ = target.Should().Be(expectedValue);
    }

    [Fact(DisplayName = "LocalStorage.Store() should persist and retrieve correct type")]
    public void LocalStorageStoreShouldPersistAndRetrieveCorrectType()
    {
        // arrange
        var key = Guid.NewGuid().ToString();
        var value = (double)42.4m;
        var storage = new LocalStorage();

        // act
        storage.Store(key, value);
        storage.Persist();

        // assert
        var target = storage.Load<double>(key);
        _ = target.Should().Be(value);
    }

    [Fact(DisplayName = "LocalStorage.Store() should persist multiple values")]
    public void LocalStorageStoreShouldPersistMultipleValues()
    {
        // arrange - create multiple values, of different types
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();
        var key3 = Guid.NewGuid().ToString();
        var value1 = "It was the best of times, it was the worst of times.";
        var value2 = DateTime.Now;
        var value3 = int.MaxValue;
        var storage = new LocalStorage();

        // act
        storage.Store(key1, value1);
        storage.Store(key2, value2);
        storage.Store(key3, value3);
        storage.Persist();

        // assert
        var target1 = storage.Load<string>(key1);
        var target2 = storage.Load<DateTime>(key2);
        var target3 = storage.Load<int>(key3);

        _ = target1.Should().Be(value1);
        _ = target2.Should().Be(value2);
        _ = target3.Should().Be(value3);
    }

    [Fact(DisplayName = "LocalStorage.Store() should overwrite existing key")]
    public void LocalStorageStoreShouldOverwriteExistingKey()
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
        var target = storage.Load<Joke>(key);

        // assert - last stored value should be the truth
        _ = target.Should().NotBeNull();
        _ = target.Equals(expected_value);
    }

    [Fact(DisplayName = "LocalStorage.Clear() should clear all in-memory content")]
    public void LocalStorageClearShouldClearAllContent()
    {
        // arrange - make sure something is stored in the LocalStorage
        var storage = new LocalStorage();
        _ = FileHelpers.GetLocalStoreFilePath(".localstorage");
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid();
        storage.Store(key, value);
        storage.Persist();

        // act - clear the store
        storage.Clear();

        // assert - open the file here and make sure the contents are empty
        _ = storage.Count.Should().Be(0);
    }

    [Fact(DisplayName = "LocalStorage.Persist() should create file with custom filename on filesystem")]
    public void LocalStoragePersistShouldCreateFileWithCustomFilenameOnFilesystem()
    {
        // arrange - create random filename
        var randomCustomFilename = $"{Guid.NewGuid():N}.dat";
        var config = new LocalStorageConfiguration() { Filename = randomCustomFilename };
        var storage = new LocalStorage(config);
        var key1 = Guid.NewGuid().ToString();
        var value1 = "My kingdom for a file with a random name.";
        storage.Store(key1, value1);

        // act
        storage.Persist();

        // assert
        var expectedFilepath = FileHelpers.GetLocalStoreFilePath(randomCustomFilename);
        _ = File.Exists(expectedFilepath).Should().BeTrue(because: $"file '{expectedFilepath}'should be created during Persist()");

        // cleanup
        storage.Destroy();
        _ = File.Exists(expectedFilepath).Should().BeFalse(because: $"file '{expectedFilepath} should be deleted after Destroy()");
    }

    [Fact(DisplayName = "LocalStorage.Persist() should create file on filesystem")]
    public void LocalStoragePersistShouldCreateFileOnFilesystem()
    {
        // arrange - expect file with default filename
        var defaultFilename = new LocalStorageConfiguration().Filename;
        var expectedFilepath = FileHelpers.GetLocalStoreFilePath(defaultFilename);
        if (File.Exists(expectedFilepath))
        {
            File.Delete(expectedFilepath);
        }

        var storage = new LocalStorage();
        var key1 = Guid.NewGuid().ToString();
        var value1 = "My kingdom for a file with a default name.";
        storage.Store(key1, value1);

        // act
        storage.Persist();

        // assert
        _ = File.Exists(expectedFilepath).Should().BeTrue(because: $"file '{expectedFilepath}'should be created during Persist()");

        // cleanup
        storage.Destroy();
        _ = File.Exists(expectedFilepath).Should().BeFalse(because: $"file '{expectedFilepath} should be deleted after Destroy()");
    }

    [Fact(DisplayName = "LocalStorage.Persist() should leave previous entries intact")]
    public void LocalStoragePersistShouldLeavePreviousEntriesIntact()
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
        var target1 = storage.Load<string>(key1);
        var target2 = storage.Load<string>(key2);
        _ = target1.Should().Be(value1);
        _ = target2.Should().Be(value2);
    }

    [Fact(DisplayName = "LocalStorage.Store() should throw exception in readonly mode")]
    public void LocalStorageStoreShouldThrowExceptionInReadOnlyMode()
    {
        // arrange - create localstorage in read-only mode
        var storage = new LocalStorage(new LocalStorageConfiguration() { ReadOnlyMode = true });
        var key = Guid.NewGuid().ToString();
        var value = "Macho Man Randy Savage";

        // act + assert
        var target = Assert.Throws<LocalStorageException>(() => storage.Store(key, value));
        _ = target.Message.Should().Be(ErrorMessages.CannotExecuteStoreInReadOnlyMode);
    }

    [Fact(DisplayName = "LocalStorage.Persist() should throw exception in readonly mode")]
    public void LocalStoragePersistShouldThrowExceptionInReadOnlyMode()
    {
        // arrange - create localstorage in read-only mode
        var storage = new LocalStorage(new LocalStorageConfiguration() { ReadOnlyMode = true });

        // act + assert
        var target = Assert.Throws<LocalStorageException>(storage.Persist);
        _ = target.Message.Should().Be(ErrorMessages.CannotExecutePersistInReadOnlyMode);
    }

    [Fact(DisplayName = "LocalStorage.Remove() should delete existing key")]
    public void LocalStorageRemoveShouldDeleteExistingKey()
    {
        // arrange - add key and verify its in memory
        var storage = new LocalStorage();
        var key = Guid.NewGuid().ToString();
        var value = "Peter Weyland";
        storage.Store(key, value);
        _ = storage.Exists(key).Should().BeTrue();

        // act - remove key
        storage.Remove(key);

        // assert - verify key has been removed
        _ = storage.Exists(key).Should().BeFalse();
    }

    [Fact(DisplayName = "LocalStorage.Remove() should not break on non-existing key")]
    public void LocalStorageRemoveShouldNotBreakOnNonExistingKey()
    {
        // arrange - create instance and verify key doesn't exist already
        var storage = new LocalStorage();
        var key = Guid.NewGuid().ToString();
        _ = storage.Exists(key).Should().BeFalse(because: "expect key not to exist yet");

        // act - remove key that doesn't exist, should still continue
        storage.Remove(key);

        // assert
        _ = storage.Exists(key).Should().BeFalse(because: "key still should not exist");
    }

    [Fact(DisplayName = "LocalStorage should remain intact between multiple instances")]
    public void LocalStorageShouldRemainIntactBetweenMultipleInstances()
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
        var target1 = storage3.Load<string>(key1);
        var target2 = storage3.Load<string>(key2);
        _ = target1.Should().Be(value1);
        _ = target2.Should().Be(value2);
    }

    [Fact(DisplayName = "LocalStorage should support unicode")]
    public void LocalStorageStoreShouldSupportUnicode()
    {
        // arrange
        var key = Guid.NewGuid().ToString();
        const string expectedValue = "Juliën's Special Characters: ~!@#$%^&*()œōøęsæ";
        var storage = new LocalStorage();

        // act
        storage.Store(key, expectedValue);
        storage.Persist();

        // assert
        var target = storage.Load<string>(key);
        _ = target.Should().BeOfType<string>();
        _ = target.Should().Be(expectedValue);
    }

    [Fact(DisplayName = "LocalStorage should perform decently with large collections")]
    public void LocalStorageShouldPerformDecentlyWithLargeCollections()
    {
        // arrange - create a larger collection (100K records)
        var stopwatch = Stopwatch.StartNew();
        var storage = new LocalStorage();
        for (var i = 0; i < 100000; i++)
        {
            storage.Store(Guid.NewGuid().ToString(), i);
        }

        storage.Persist();

        // act - create new instance (e.g. load the larger collection from disk)
        var target = new LocalStorage();
        target.Clear();
        stopwatch.Stop();

        // cleanup - delete the (large!) persisted file
        target.Destroy();

        // assert - make sure the entire operation is done in < 1sec. (psychological boundry, if you will)
        _ = stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(1000);
    }

    [Fact(DisplayName = "LocalStorage should perform decently with many iterations collections")]
    public void LocalStorageShouldPerformDecentlyWithManyOpensAndWrites()
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
    public void LocalStorageExistsShouldLocateExistingKey()
    {
        // arrange
        var storage = new LocalStorage();
        var expected_key = Guid.NewGuid().ToString();
        storage.Store(expected_key, Guid.NewGuid().ToString());

        // act
        var target = storage.Exists(expected_key);

        // assert
        _ = target.Should().BeTrue();
    }

    [Fact(DisplayName = "LocalStorage.Exists() should ignore non-existing key")]
    public void LocalStorageExistsShouldIgnoreNonExistingKey()
    {
        // arrange
        var storage = new LocalStorage();
        var nonexisting_key = Guid.NewGuid().ToString();

        // act
        var target = storage.Exists(nonexisting_key);

        // assert
        _ = target.Should().BeFalse();
    }

    [Fact(DisplayName = "LocalStorage.Keys() should return collection of all keys")]
    public void LocalStorageKeysShouldReturnCollectionOfKeys()
    {
        // arrange
        var storage = new LocalStorage(TestHelpers.UniqueInstance());
        for (var i = 0; i < 10; i++)
        {
            storage.Store(Guid.NewGuid().ToString(), i);
        }

        var expected_keycount = storage.Count;

        // act
        var target = storage.Keys();

        // assert
        _ = target.Should().NotBeNullOrEmpty();
        _ = target.Count.Should().Be(expected_keycount);
    }

    [Fact(DisplayName = "LocalStorage.Keys() should return 0 on empty collection")]
    public void LocalStorageKeysShouldReturnZeroOnEmptyCollection()
    {
        // arrange
        var storage = new LocalStorage(TestHelpers.UniqueInstance());

        // act
        var target = storage.Keys();

        // assert
        _ = target.Should().NotBeNull();
        _ = target.Should().BeEmpty();
        _ = target.Count.Should().Be(0, because: "nothing is added to the LocalStorage");
    }

    [Fact(DisplayName = "LocalStorage.Query() should cast to a collection")]
    public void LocalStorageQueryShouldCastResponseToCollection()
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
        _ = target.Should().NotBeNull();
        _ = target.Count().Should().Be(expected_amount);
    }

    [Fact(DisplayName = "LocalStorage.Query() should respect a provided predicate")]
    public void LocalStorageQueryShouldRespectProvidedPredicate()
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
        _ = target.Should().NotBeNull();
        _ = target.Count().Should().Be(expected_amount);
        _ = target.All(c => c.Brand == expected_brand);
    }

    [Fact(DisplayName = "LocalStorage.Destroy() should delete file on disk")]
    public void LocalStorageDestroyShouldDeleteFileOnDisk()
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
        _ = File.Exists(filepath).Should().BeFalse();
    }
}
