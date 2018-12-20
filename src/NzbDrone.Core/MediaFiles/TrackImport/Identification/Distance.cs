using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public class Distance
    {
        private Dictionary<string, List<double>> penalties;

        // from beets default config
        private static readonly Dictionary<string, double> weights = new Dictionary<string, double>
        {
            { "source", 2.0 },
            { "artist", 3.0 },
            { "album", 3.0 },
            { "media", 1.0 },
            { "mediums", 1.0 },
            { "year", 1.0 },
            { "country", 0.5 },
            { "label", 0.5 },
            { "catalognum", 0.5 },
            { "albumdisambig", 0.5 },
            { "album_id", 5.0 },
            { "tracks", 2.0 },
            { "missing_tracks", 0.9 },
            { "unmatched_tracks", 0.6 },
            { "track_title", 3.0 },
            { "track_artist", 2.0 },
            { "track_index", 1.0 },
            { "track_length", 2.0 },
            { "recording_id", 5.0 },
        };

        public Distance()
        {
            penalties = new Dictionary<string, List<double>>();
        }

        public double NormalizedDistance => MaxDistance > 0 ? RawDistance / MaxDistance : 0;
        public double MaxDistance => penalties.Select(x => x.Value.Count * weights[x.Key]).Sum();
        public double RawDistance => penalties.Select(x => x.Value.Sum() * weights[x.Key]).Sum();

        public void Add(string key, double dist)
        {
            Ensure.That(dist, () => dist).IsInRange(0, 1);
            if (penalties.ContainsKey(key))
            {
                penalties[key].Add(dist);                    
            }
            else
            {
                penalties[key] = new List<double> { dist };
            }
        }

        public void AddRatio(string key, double value, double target)
        {
            // Adds a distance penalty for value as a ratio of target
            // value is between 0 and target
            var dist = target > 0 ? Math.Max(Math.Min(value, target), 0.0) / target : 0.0;
            Add(key, dist);
        }

        public void AddNumber(string key, int value, int target)
        {
            var diff = Math.Abs(value - target);
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    Add(key, 1.0);
                }
            }
            else
            {
                Add(key, 0.0);
            }
        }

        private static readonly Regex NonWordRegex = new Regex(@"[^a-z0-9]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static string Clean(string input)
        {
            return NonWordRegex.Replace(input.ToLower().RemoveAccent(), string.Empty).Trim();
        }

        public void AddString(string key, string value, string target)
        {
            // Adds a penaltly based on the distance between value and target
            Add(key, 1.0 - Clean(value).LevenshteinCoefficient(Clean(target)));
        }

        public void AddExpr(string key, Func<bool> expr)
        {
            Add(key, expr() ? 1.0 : 0.0);
        }

        public void AddEquality<T>(string key, T value, List<T> options) where T : IEquatable<T>
        {
            Add(key, options.Contains(value) ? 0.0 : 1.0);
        }

        public void AddPriority<T>(string key, T value, List<T> options) where T : IEquatable<T>
        {
            var unit = 1.0 / options.Count > 0 ? options.Count : 1.0;
            var index = options.IndexOf(value);
            if (index == -1)
            {
                Add(key, 1.0);
            }
            else
            {
                Add(key, index * unit);
            }
        }

        public void AddPriority<T>(string key, List<T> values, List<T> options) where T : IEquatable<T>
        {
            for(int i = 0; i < options.Count; i++)
            {
                if (values.Contains(options[i]))
                {
                    Add(key, i / (double)options.Count);
                    return;
                }
            }

            Add(key, 1.0);
        }
    }
}
