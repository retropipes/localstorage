// MARS Web App by Rockwell Automation, Inc. (C) 2019-present

using FluentAssertions;
using RetroPipes.Storage.Helpers;
using System;
using System.IO;
using Xunit;

namespace RetroPipes.Storage.Tests;

public class LocalStorageConfigurationTests
{
    [Fact(DisplayName = "LocalStorage should not be initializable with null for configuration")]
    public void LocalStorageShouldNotBeInitializableWithArgumentNull() => Assert.Throws<ArgumentNullException>(() =>
                                                                                          {
                                                                                              var target = new LocalStorage(null);
                                                                                          });

    [Fact(DisplayName = "LocalStorageConfiguration should respect custom filename")]
    public void LocalStorageConfigurationShouldRespectCustomFilename()
    {
        // arrange - configure localstorage to use a custom filename
        var random_filename = Guid.NewGuid().ToString("N");
        var config = new LocalStorageConfiguration()
        {
            Filename = random_filename
        };

        // act - store the container
        var storage = new LocalStorage(config);
        storage.Persist();
        var target = FileHelpers.GetLocalStoreFilePath(random_filename);

        // assert
        _ = File.Exists(target).Should().BeTrue();

        // cleanup
        storage.Destroy();
    }
}
