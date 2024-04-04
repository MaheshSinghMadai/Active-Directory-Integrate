using GetAllADUsers.Dto;
using System;
using System.Collections.Generic;
using System.DirectoryServices;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter your username:");
        string username = Console.ReadLine();

        Console.WriteLine("Enter your password:");
        string password = ReadPassword();

        string ldapPath = "LDAP://AD.nav.com"; // Replace with your LDAP path

        try
        {
            // Attempt to authenticate user credentials
            bool isAuthenticated = AuthenticateUser(ldapPath, username, password);

            if (isAuthenticated)
            {
                // If authentication is successful, retrieve and display all AD users
                List<ADUser> users = GetADUsers(ldapPath);
                Console.WriteLine("Active Directory users:");
                foreach (var user in users)
                {
                    Console.WriteLine($"UserId: {user.UserId}");
                    Console.WriteLine($"Username: {user.Username}");
                    Console.WriteLine($"First Name: {user.FirstName}");
                    Console.WriteLine($"Last Name: {user.LastName}");
                    Console.WriteLine($"Email: {user.Email}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(); // Wait for user input before exiting
    }

    static bool AuthenticateUser(string ldapPath, string username, string password)
    {
        try
        {
            // Attempt to bind with the directory using the provided credentials
            using (DirectoryEntry entry = new DirectoryEntry(ldapPath, username, password))
            {
                // If binding succeeds, the username and password are correct
                object nativeObject = entry.NativeObject;
                return true;
            }
        }
        catch (DirectoryServicesCOMException)
        {
            // If binding fails, the provided username or password is incorrect
            return false;
        }
    }

    static List<ADUser> GetADUsers(string ldapPath)
    {
        List<ADUser> users = new List<ADUser>();

        using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
        {
            using (DirectorySearcher searcher = new DirectorySearcher(entry))
            {
                // Set the filter to retrieve all user objects
                searcher.Filter = "(objectCategory=user)";
                searcher.PageSize = 1000; // Set a reasonable page size to improve performance

                // Perform the search and retrieve the results
                SearchResultCollection results = searcher.FindAll();

                // Iterate through the results and populate user data
                foreach (SearchResult result in results)
                {
                    // Retrieve the DirectoryEntry for the current search result
                    DirectoryEntry userEntry = result.GetDirectoryEntry();

                    // Create a new ADUser object and populate it with user properties
                    ADUser user = new ADUser
                    {
                        UserId = userEntry.Properties["DistinguishedName"].Value.ToString(),
                        Username = userEntry.Properties["samAccountName"].Value?.ToString(),
                        FirstName = userEntry.Properties["givenName"].Value?.ToString(),
                        LastName = userEntry.Properties["sn"].Value?.ToString(),
                        Email = userEntry.Properties["mail"].Value?.ToString()
                    };

                    // Add the user to the list
                    users.Add(user);
                }
            }
        }

        return users;
    }

    // Method to read password without showing characters on the console
    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            // Ignore any key other than Backspace or Enter
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*"); // Display * instead of the actual character
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, (password.Length - 1));
                Console.Write("\b \b"); // Move cursor back and replace with space to simulate backspace
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine(); // Add a newline after entering password
        return password;
    }
}

