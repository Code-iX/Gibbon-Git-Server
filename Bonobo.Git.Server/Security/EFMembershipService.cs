﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Bonobo.Git.Server.Data;
using System.Data;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Security
{
    public class EFMembershipService : IMembershipService
    {
        public bool ValidateUser(string username, string password)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            username = username.ToLowerInvariant();
            using (var database = new BonoboGitServerContext())
            {
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                return user != null && ComparePassword(password, username, user.Password);
            }
        }

        public bool CreateUser(string username, string password, string name, string surname, string email)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", "name");
            if (String.IsNullOrEmpty(surname)) throw new ArgumentException("Value cannot be null or empty.", "surname");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            username = username.ToLowerInvariant();
            using (var database = new BonoboGitServerContext())
            {
                var user = new User
                {
                    Username = username,
                    Password = GetSaltedHash(password, username),
                    Name = name,
                    Surname = surname,
                    Email = email,
                };
                database.Users.Add(user);
                try
                {
                    database.SaveChanges();
                }
                catch (UpdateException)
                {
                    return false;
                }
            }

            return true;
        }

        public IList<UserModel> GetAllUsers()
        {
            using (var db = new BonoboGitServerContext())
            {
                return db.Users.Include("Roles").ToList().Select(item => new UserModel
                {
                    Username = item.Username,
                    Name = item.Name,
                    Surname = item.Surname,
                    Email = item.Email,
                    Roles = item.Roles.Select(i => i.Name).ToArray(),
                }).ToList();
            }
        }

        public UserModel GetUser(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "username");

            username = username.ToLowerInvariant();
            using (var db = new BonoboGitServerContext())
            {
                var user = db.Users.FirstOrDefault(i => i.Username == username);
                return user == null ? null : new UserModel
                {
                    Username = user.Username,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Roles = user.Roles.Select(i => i.Name).ToArray(),
                 };
            }
        }

        public void UpdateUser(string username, string name, string surname, string email, string password)
        {
            using (var database = new BonoboGitServerContext())
            {
                username = username.ToLowerInvariant();
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                if (user != null)
                {
                    user.Name = name ?? user.Name;
                    user.Surname = surname ?? user.Surname;
                    user.Email = email ?? user.Email;
                    user.Password = password != null ? GetSaltedHash(password, username) : user.Password;
                    database.SaveChanges();
                }
            }
        }

        public void DeleteUser(string username)
        {
            using (var database = new BonoboGitServerContext())
            {
                username = username.ToLowerInvariant();
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                if (user != null)
                {
                    user.AdministratedRepositories.Clear();
                    user.Roles.Clear();
                    user.Repositories.Clear();
                    user.Teams.Clear();
                    database.Users.Remove(user);
                    database.SaveChanges();
                }
            }
        }

        private bool ComparePassword(string password, string salt, string hash)
        {
            return GetSaltedHash(password, salt) == hash;
        }

        // todo embix: hash related stuff should be injected
        private readonly Func<HashAlgorithm> _getHashProvider = ()=>new SHA512CryptoServiceProvider();

        private string GetHash(string content)
        {
            using (var hashProvider = _getHashProvider())
            {
                var data = System.Text.Encoding.UTF8.GetBytes(content);
                data = hashProvider.ComputeHash(data);
                return BitConverter.ToString(data).Replace("-", ""); 
            }
        }

        // as the username is fixed and unique for each user
        // it seams the least bad salt without breaking the db abstraction
        internal string GetSaltedHash(string password, string salt)
        {
            var hashedSalt = GetHash(salt);
            return GetHash(GetHash(hashedSalt + password + hashedSalt));
        }
    }
}