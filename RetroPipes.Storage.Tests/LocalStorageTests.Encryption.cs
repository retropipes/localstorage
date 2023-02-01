// MARS Web App by Rockwell Automation, Inc. (C) 2019-present

using FluentAssertions;
using RetroPipes.Storage.Helpers;
using System;
using Xunit;

namespace RetroPipes.Storage.Tests;

public class EncryptionTests
{
    [Fact(DisplayName = "Helpers.Decrypt() should decode an encrypted string")]
    public void DecryptShouldDecodeAnEncryptedString()
    {
        // arrange
        var key = Guid.NewGuid().ToString();
        var salt = Guid.NewGuid().ToString();
        var original_value = "lorem ipsum dom dolor sit amet";
        var encrypted_value = CryptographyHelpers.Encrypt(key, salt, original_value);

        // act
        var target = CryptographyHelpers.Decrypt(key, salt, encrypted_value);

        // assert
        _ = target.Should().NotBeNullOrEmpty();
        _ = target.Should().Be(original_value);
    }

    [Fact(DisplayName = "Helpers.Decrypt() should decode an encrypted string with special characters")]
    public void DecryptShouldDecodeAnEncryptedStringWithSpecialCharacters()
    {
        // arrange
        var key = Guid.NewGuid().ToString("N");
        var salt = Guid.NewGuid().ToString("N");
        var original_value = "Søm€ unicode s-tring+";
        var encrypted_value = CryptographyHelpers.Encrypt(key, salt, original_value);

        // act
        var target = CryptographyHelpers.Decrypt(key, salt, encrypted_value);

        // assert
        _ = target.Should().NotBeNullOrEmpty();
        _ = target.Should().Be(original_value);
    }

    [Fact(DisplayName = "Helpers.Encrypt() should encrypt a string")]
    public void EncryptionShouldEncryptString()
    {
        // arrange
        var key = Guid.NewGuid().ToString();
        var salt = Guid.NewGuid().ToString();
        var text = "lorem ipsum dom dolor sit amet";

        // act
        var target = CryptographyHelpers.Encrypt(key, salt, text);

        // assert
        _ = target.Should().NotBeNullOrEmpty();
        _ = target.Should().NotBe(text);
    }

    [Fact(DisplayName = "LocalStorage.Store() [Encrypted] should persist and retrieve correct type")]
    public void LocalStorageStoreEncryptedShouldPersistAndRetrieveCorrectType()
    {
        // arrange
        var key = Guid.NewGuid().ToString();
        var value = (double)42.4m;
        var password = Guid.NewGuid().ToString();
        var storage = new LocalStorage(EncryptedConfiguration(), password);

        // act
        storage.Store(key, value);
        storage.Persist();

        // assert
        var target = storage.Load<double>(key);
        _ = target.Should().Be(value);
    }

    private static LocalStorageConfiguration EncryptedConfiguration() => new()
    {
        EnableEncryption = true,
        EncryptionSalt = "SALT-N-PEPPA"
    };
}
