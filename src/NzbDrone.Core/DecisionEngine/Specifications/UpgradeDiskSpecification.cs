using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IPreferredWordService _preferredWordServiceCalculator;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification qualityUpgradableSpecification,
            IMediaFileService mediaFileService,
            IPreferredWordService preferredWordServiceCalculator,
            Logger logger)
        {
            _upgradableSpecification = qualityUpgradableSpecification;
            _mediaFileService = mediaFileService;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {

            foreach (var album in subject.Albums)
            {
                var trackFiles = _mediaFileService.GetFilesByAlbum(album.Id);

                if (trackFiles.Any())
                {
                    var lowestQuality = trackFiles.Select(c => c.Quality).OrderBy(c => c.Quality.Id).First();

                    if (!_upgradableSpecification.IsUpgradable(subject.Artist.QualityProfile,
                                                               subject.Artist.LanguageProfile,
                                                               lowestQuality,
                                                               trackFiles[0].Language,
                                                               _preferredWordServiceCalculator.Calculate(subject.Artist, trackFiles[0].GetSceneOrFileName()),
                                                               subject.ParsedAlbumInfo.Quality,
                                                               subject.ParsedAlbumInfo.Language,
                                                               subject.PreferredWordScore))
                    {
                        return Decision.Reject("Existing file on disk is of equal or higher preference: {0} - {1}", lowestQuality, trackFiles[0].Language);
                    }
                }

            }

            return Decision.Accept();
        }
    }
}
