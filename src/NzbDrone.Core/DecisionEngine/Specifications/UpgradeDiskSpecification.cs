using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly ITrackService _trackService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification qualityUpgradableSpecification,
                                        IMediaFileService mediaFileService,
                                        ITrackService trackService,
                                        Logger logger)
        {
            _upgradableSpecification = qualityUpgradableSpecification;
            _mediaFileService = mediaFileService;
            _trackService = trackService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {

            foreach (var album in subject.Albums)
            {
                var tracksMissing =_trackService.TracksWithoutFiles(album.Id).Any();
                var trackFiles = _mediaFileService.GetFilesByAlbum(album.Id);

                if (!tracksMissing && trackFiles.Any())
                {
                    var lowestQuality = trackFiles.Select(c => c.Quality).OrderBy(c => c.Quality.Id).First();

                    if (!_upgradableSpecification.IsUpgradable(subject.Artist.Profile,
                                                               subject.Artist.LanguageProfile,
                                                               lowestQuality,
                                                               trackFiles[0].Language,
                                                               subject.ParsedAlbumInfo.Quality,
                                                               subject.ParsedAlbumInfo.Language))
                    {
                        return Decision.Reject("Quality for existing file on disk is of equal or higher preference: {0}", lowestQuality);
                    }
                }

            }

            return Decision.Accept();
        }
    }
}
