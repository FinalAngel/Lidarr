using NzbDrone.Core.Music;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using System.IO;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalAlbumRelease
    {
        public LocalAlbumRelease()
        {
            LocalTracks = new List<LocalTrack>();

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public LocalAlbumRelease(List<LocalTrack> tracks)
        {
            LocalTracks = tracks;

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public List<LocalTrack> LocalTracks;
        public int TrackCount => LocalTracks.Count;

        public TrackMapping TrackMapping;
        public Distance Distance;
        public AlbumRelease AlbumRelease;

        public void PopulateMatch()
        {
            if (AlbumRelease != null)
            {
                foreach (var localTrack in LocalTracks)
                {
                    localTrack.Release = AlbumRelease;
                    localTrack.Album = AlbumRelease.Album.Value;

                    if (TrackMapping.Mapping.ContainsKey(localTrack))
                    {
                        var track = TrackMapping.Mapping[localTrack];
                        localTrack.Tracks = new List<Track> { track };
                        localTrack.Artist = track.Artist.Value;
                    }
                }
            }
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", LocalTracks.Select(x => Path.GetDirectoryName(x.Path)).Distinct()) + "]";
        }
    }

    public class TrackMapping
    {
        public TrackMapping()
        {
            Mapping = new Dictionary<LocalTrack, Track>();
        }
        
        public Dictionary<LocalTrack, Track> Mapping { get; set; }
        public List<LocalTrack> LocalExtra { get; set; }
        public List<Track> MBExtra { get; set; }
    }
}
