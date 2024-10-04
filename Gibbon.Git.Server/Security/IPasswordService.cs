namespace Gibbon.Git.Server.Security;

/// <summary>
/// Provides methods for generating and validating secure passwords and tokens using cryptographic techniques.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Generates a new cryptographically secure salt for password hashing.
    /// </summary>
    /// <returns>A Base64-encoded string representing the generated salt.</returns>
    /// <remarks>
    /// The salt is used to add randomness to the hashing process, which helps prevent attacks like rainbow table attacks. 
    /// It should be stored alongside the hashed password and reused during the validation process.
    /// </remarks>
    string GenerateSalt();

    /// <summary>
    /// Creates a secure hash of the provided value (e.g., a password) using a given salt.
    /// </summary>
    /// <param name="salt">The Base64-encoded salt used to hash the value. It must be the same salt used during validation.</param>
    /// <param name="value">The value (e.g., password) to be hashed.</param>
    /// <returns>A Base64-encoded string representing the hashed value.</returns>
    /// <remarks>
    /// This method uses the PBKDF2 algorithm with SHA256 and a configurable number of iterations to generate a secure hash. 
    /// The hash is combined with the salt and stored. During validation, the same salt and algorithm must be used.
    /// </remarks>
    string GenerateHash(string salt, string value);

    /// <summary>
    /// Generates a secure token based on the input value using the PBKDF2 algorithm with SHA256.
    /// </summary>
    /// <param name="value">The input string (e.g., a username or password) used in the key derivation function.</param>
    /// <returns>
    /// A Base64-encoded string containing the salt and the derived subkey. 
    /// The first byte represents the format version, followed by the salt and the subkey.
    /// </returns>
    /// <remarks>
    /// This method generates a cryptographically secure token that includes both the salt and the derived subkey. 
    /// The token can be used to validate the input in future operations, ensuring that it has not been tampered with. 
    /// The PBKDF2 algorithm uses SHA256 for additional security, and the number of iterations can be configured.
    /// </remarks>
    string GenerateToken(string value);

    /// <summary>
    /// Compares a hashed value (e.g., a stored password hash) with the hash of a provided value using the same salt.
    /// </summary>
    /// <param name="salt">The Base64-encoded salt that was used to create the known hash.</param>
    /// <param name="value">The value (e.g., password) to hash and compare against the known hash.</param>
    /// <param name="knownHash">The Base64-encoded hash that is being compared against.</param>
    /// <returns>
    /// <c>true</c> if the hash of the provided value matches the known hash; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method hashes the provided value using the same salt and algorithm that was used to generate the known hash, 
    /// then compares the two hashes for equality. It ensures that the provided value matches the stored value.
    /// </remarks>
    bool CompareHash(string salt, string value, string knownHash);
}
