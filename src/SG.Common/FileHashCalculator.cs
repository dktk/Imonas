// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Security.Cryptography;
using System.Text;

namespace SG.Common
{
    public class FileHashCalculator
    {
        public static string ComputeFileHash(byte[] content, string hashAlgorithmName = "SHA256")
        {
            // Validate algorithm
            using var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName)
                ?? throw new ArgumentException($"Unsupported hash algorithm: {hashAlgorithmName}");

            // Open file stream for reading without loading entire file into memory
            var hashBytes = hashAlgorithm.ComputeHash(content);

            // Convert hash bytes to hexadecimal string
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
