﻿using FluentAssertions;
using RetroPipes;
using RetroPipes.Helpers;
using System;
using Xunit;

namespace RetroPipes.LocalStorageTests
{
    public class EncryptionTests
    {
        [Fact(DisplayName = "Helpers.Decrypt() should decode an encrypted string")]
        public void Decrypt_Should_Decode_An_Encrypted_String()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var salt = Guid.NewGuid().ToString();
            var original_value = "lorem ipsum dom dolor sit amet";
            var encrypted_value = CryptographyHelpers.Encrypt(key, salt, original_value);

            // act
            var target = CryptographyHelpers.Decrypt(key, salt, encrypted_value);

            // assert
            target.Should().NotBeNullOrEmpty();
            target.Should().Be(original_value);
        }

        [Fact(DisplayName = "Helpers.Decrypt() should decode an encrypted string with special characters")]
        public void Decrypt_Should_Decode_An_Encrypted_String_With_Special_Characters()
        {
            // arrange
            var key = Guid.NewGuid().ToString("N");
            var salt = Guid.NewGuid().ToString("N");
            var original_value = "Søm€ unicode s-tring+";
            var encrypted_value = CryptographyHelpers.Encrypt(key, salt, original_value);

            // act
            var target = CryptographyHelpers.Decrypt(key, salt, encrypted_value);

            // assert
            target.Should().NotBeNullOrEmpty();
            target.Should().Be(original_value);
        }

        [Fact(DisplayName = "Helpers.Encrypt() should encrypt a string")]
        public void Encryption_Should_Encrypt_String()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var salt = Guid.NewGuid().ToString();
            var text = "lorem ipsum dom dolor sit amet";

            // act
            var target = CryptographyHelpers.Encrypt(key, salt, text);

            // assert
            target.Should().NotBeNullOrEmpty();
            target.Should().NotBe(text);
        }

        [Fact(DisplayName = "LocalStorage.Store() [Encrypted] should persist and retrieve correct type")]
        public void LocalStorage_Store_Encrypted_Should_Persist_And_Retrieve_Correct_Type()
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
            var target = storage.Get<double>(key);
            target.Should().Be(value);
        }

        private LocalStorageConfiguration EncryptedConfiguration()
        {
            return new LocalStorageConfiguration()
            {
                EnableEncryption = true,
                EncryptionSalt = "SALT-N-PEPPA"
            };
        }
    }
}
