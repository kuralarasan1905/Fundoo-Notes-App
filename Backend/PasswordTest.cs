using BCrypt.Net;

namespace fundoo_notes.Tests
{
    public static class PasswordTest
    {
        public static void TestPasswordHashing()
        {
            Console.WriteLine("=== Password Hashing Test ===");

            string testPassword = "YourTestPassword123!";

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(testPassword); // Fully qualify the namespace to avoid ambiguity.
            Console.WriteLine($"Original Password: {testPassword}");
            Console.WriteLine($"Hashed Password: {hashedPassword}");
            Console.WriteLine($"Hash Length: {hashedPassword.Length}");

            // Verify the password
            bool isValid = BCrypt.Net.BCrypt.Verify(testPassword, hashedPassword); // Fully qualify the namespace to resolve the error.
            Console.WriteLine($"Verification Result: {isValid}");

            // Test with wrong password
            bool isInvalid = BCrypt.Net.BCrypt.Verify("WrongPassword", hashedPassword); // Fully qualify the namespace to resolve the error.
            Console.WriteLine($"Wrong Password Test: {isInvalid}");

            // Test with spaces
            bool withSpaces = BCrypt.Net.BCrypt.Verify(" " + testPassword + " ", hashedPassword); // Fully qualify the namespace to resolve the error.
            Console.WriteLine($"Password with spaces: {withSpaces}");

            Console.WriteLine("=== Test Complete ===");
        }
    }
}
