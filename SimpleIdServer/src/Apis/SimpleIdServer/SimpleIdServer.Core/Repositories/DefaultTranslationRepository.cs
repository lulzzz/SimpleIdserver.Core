using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Extensions;

namespace SimpleIdServer.Core.Repositories
{
    internal sealed class DefaultTranslationRepository : ITranslationRepository
    {
        public ICollection<Common.Models.Translation> _translations;

        public DefaultTranslationRepository(ICollection<Common.Models.Translation> translations)
        {
            _translations = translations == null ? new List<Common.Models.Translation>() : translations;
        }

        public Task<Common.Models.Translation> GetAsync(string languageTag, string code)
        {
            var translation = _translations.FirstOrDefault(t => t.LanguageTag == languageTag && t.Code == code);
            if (translation == null)
            {
                return Task.FromResult((Common.Models.Translation)null);
            }

            return Task.FromResult(translation.Copy());
        }

        public Task<ICollection<Common.Models.Translation>> GetAsync(string languageTag)
        {
            ICollection<Common.Models.Translation> result = _translations.Where(t => t.LanguageTag == languageTag).Select(t => t.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<ICollection<string>> GetLanguageTagsAsync()
        {
            ICollection<string> result = _translations.Select(t => t.LanguageTag).ToList();
            return Task.FromResult(result);
        }
    }
}
