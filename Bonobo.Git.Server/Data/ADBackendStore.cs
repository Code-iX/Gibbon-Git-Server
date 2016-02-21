﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections;
using System.Diagnostics;
using System.Web.Hosting;
using Bonobo.Git.Server.Configuration;

namespace Bonobo.Git.Server.Data
{
    public class ADBackendStore<T> : IEnumerable<T> where T : INameProperty
    {
        public T this[string key]
        {
            get
            {
                T result = default(T);

                content.TryGetValue(key, out result);

                return result;
            }

            set
            {
                content.AddOrUpdate(key, value, (k, v) => value);
            }
        }

        private string storagePath;
        private ConcurrentDictionary<string, T> content;
        private readonly string hexchars = "0123456789abcdef";

        public ADBackendStore(string rootpath, string name)
        {
            storagePath = Path.Combine(GetRootPath(rootpath), name);
            content = LoadContent();
        }

        public bool Add(T item)
        {
            return content.TryAdd(item.Id.ToString(), item) && Store(item);
        }

        public bool Remove(string key)
        {
            T removedItem;
            return content.TryRemove(key, out removedItem) && Delete(removedItem);
        }

        public bool Remove(T item)
        {
            return Remove(item.Id.ToString());
        }

        public void Update(T item)
        {
            if (content.TryUpdate(item.Id.ToString(), item, content[item.Id.ToString()]))
            {
                Store(item);
            }
        }

        public void AddOrUpdate(T item)
        {
            content.AddOrUpdate(item.Id.ToString(), item, (k, v) => item);
            Store(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return content.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private bool Store(T item)
        {
            bool result = false;

            try
            {
                string itemFilename = Path.Combine(storagePath, GetItemFilename(item));
                File.WriteAllText(itemFilename, JsonConvert.SerializeObject(item));
                result = true;
            }
            catch(Exception ex)
            {
                Trace.TraceError("ADStoreErr: " + ex);
            }

            return result;
        }

        private bool Delete(T item)
        {
            bool result = false;

            try
            {
                string itemFilename = Path.Combine(storagePath, GetItemFilename(item));
                File.Delete(itemFilename);
                result = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("ADStoreErr: " + ex);
            }

            return result;
        }

        private ConcurrentDictionary<string, T> LoadContent()
        {
            ConcurrentDictionary<string, T> result = new ConcurrentDictionary<string, T>();

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            foreach (string filename in Directory.EnumerateFileSystemEntries(storagePath, "*.json"))
            {
                try
                {
                    T item = JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
                    result.TryAdd(item.Id.ToString(), item);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("ADStoreErr: " + ex);
                }
            }

            return result;
        }

        private string GetRootPath(string path)
        {
            return Path.IsPathRooted(path) ? path : HostingEnvironment.MapPath(path);
        }

        private string GetItemFilename(T item)
        {
            StringBuilder result = new StringBuilder(45);

            byte[] hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(item.Name));
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hexchars[hash[i] >> 4]);
                result.Append(hexchars[hash[i] & 0x0f]);
            }
            result.Append(".json");

            return result.ToString();
        }
    }
}