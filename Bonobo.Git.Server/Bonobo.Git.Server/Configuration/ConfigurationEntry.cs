﻿using System.Configuration;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{
    public abstract class ConfigurationEntry<Entry> where Entry : ConfigurationEntry<Entry>, new()
    {
        private static Entry _current = null;
        private static readonly object _sync = new object();
        private static readonly string _configPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["UserConfiguration"]);
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Entry));


        public static Entry Current { get { return _current ?? Load(); } }

                
        private static Entry Load()
        {
            if (_current == null)
            {
                lock (_sync)
                {
                    if (_current == null)
                    {
                        try
                        {
                            using (var stream = File.Open(_configPath, FileMode.Open))
                            {
                                _current = _serializer.Deserialize(stream) as Entry;
                            }
                        }
                        catch
                        {
                            _current = new Entry();
                        }
                    }
                }
            }

            return _current;
        }

        public void Save()
        {
            if (_current != null)
            {
                lock (_sync)
                {
                    if (_current != null)
                    {
                        using (var stream = File.Open(_configPath, FileMode.Create))
                        {
                            _serializer.Serialize(stream, _current);
                        }
                    }
                }
            }
        }
    }
}