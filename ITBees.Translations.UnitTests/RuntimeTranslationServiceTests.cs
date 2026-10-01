using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using ITBees.ChatGpt;
using ITBees.Interfaces.Repository;
using ITBees.Models.Languages;
using ITBees.Translations.Interfaces;
using ITBees.Translations.Services;
using ITBees.Translations.SqlMigration;
using Moq;
using NUnit.Framework;
// ITBees.Models.Languages declares the languages Is (Icelandic) and It (Italian)
using Is = NUnit.Framework.Is;
using It = Moq.It;

namespace ITBees.Translations.UnitTests
{
    public class RuntimeTranslationServiceTests
    {
        private const string Phrase = "Znaki drogowe";
        private const string QuotaExceededAnswer =
            "API call failed with status code: TooManyRequests \r\n{ \"error\": { \"message\": \"You exceeded your current quota\", \"type\": \"insufficient_quota\" } }";

        private Mock<IReadOnlyRepository<BasePhrase>> _basePhraseRoRepo;
        private Mock<IWriteOnlyRepository<BasePhrase>> _basePhraseRwRepo;
        private Mock<IWriteOnlyRepository<RuntimeTranslation>> _runtimeTranslationRwRepo;
        private Mock<IChatGptConnector> _chatGptConnector;
        private InMemoryCachedTranslations _cachedTranslations;
        private RuntimeTranslationService _runtimeTranslationService;
        private Language _language;

        [SetUp]
        public void SetUp()
        {
            _basePhraseRoRepo = new Mock<IReadOnlyRepository<BasePhrase>>();
            _basePhraseRwRepo = new Mock<IWriteOnlyRepository<BasePhrase>>();
            _runtimeTranslationRwRepo = new Mock<IWriteOnlyRepository<RuntimeTranslation>>();
            _chatGptConnector = new Mock<IChatGptConnector>();
            _cachedTranslations = new InMemoryCachedTranslations();
            _language = new En();

            BasePhraseStoredInDatabase(new BasePhrase { Id = 7, Phrase = Phrase });

            _runtimeTranslationService = new RuntimeTranslationService(
                _basePhraseRoRepo.Object,
                _basePhraseRwRepo.Object,
                _runtimeTranslationRwRepo.Object,
                _chatGptConnector.Object,
                _cachedTranslations);
        }

        [Test]
        public async Task GetTranslation_shouldReturnKeyWithoutPersistingOrCaching_whenChatGptCallThrows()
        {
            ChatGptThrows(new HttpRequestException("API call failed with status code: TooManyRequests"));

            var result = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);

            Assert.That(result, Is.EqualTo(Phrase));
            AssertNothingWasPersistedOrCached();
        }

        [TestCase(QuotaExceededAnswer)]
        [TestCase("\"" + QuotaExceededAnswer + "\"")]
        [TestCase("Empty response from API.")]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public async Task GetTranslation_shouldReturnKeyWithoutPersistingOrCaching_whenChatGptAnswerIsNotATranslation(string answer)
        {
            ChatGptAnswers(answer);

            var result = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);

            Assert.That(result, Is.EqualTo(Phrase));
            AssertNothingWasPersistedOrCached();
        }

        [Test]
        public async Task GetTranslation_shouldAskChatGptAgainOnNextRequest_whenPreviousCallFailed()
        {
            _chatGptConnector
                .SetupSequence(x => x.AskChatGptAsync(It.IsAny<string>(), It.IsAny<ChatGptModel>()))
                .ThrowsAsync(new HttpRequestException("quota exceeded"))
                .ReturnsAsync("Road signs");

            var resultOfFailedCall = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);
            var resultOfRetriedCall = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);
            var resultFromCache = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);

            Assert.That(resultOfFailedCall, Is.EqualTo(Phrase));
            Assert.That(resultOfRetriedCall, Is.EqualTo("Road signs"));
            Assert.That(resultFromCache, Is.EqualTo("Road signs"));
            _chatGptConnector.Verify(x => x.AskChatGptAsync(It.IsAny<string>(), It.IsAny<ChatGptModel>()), Times.Exactly(2));
            _runtimeTranslationRwRepo.Verify(x => x.InsertData(It.IsAny<RuntimeTranslation>()), Times.Once);
        }

        [Test]
        public async Task GetTranslation_shouldPersistAndCacheTranslation_whenChatGptCallSucceeds()
        {
            ChatGptAnswers("Road signs");

            var result = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);

            Assert.That(result, Is.EqualTo("Road signs"));
            _runtimeTranslationRwRepo.Verify(x => x.InsertData(It.Is<RuntimeTranslation>(t =>
                t.BasePhraseId == 7 &&
                t.LanguageId == _language.Id &&
                t.TanslationValue == "Road signs" &&
                t.HasReplicableFields == false)), Times.Once);
            _basePhraseRwRepo.Verify(x => x.InsertData(It.IsAny<BasePhrase>()), Times.Never);

            var cachedTranslation = _cachedTranslations.GetTranslation(Phrase, _language.Id);
            Assert.That(cachedTranslation.Found, Is.True);
            Assert.That(cachedTranslation.Value, Is.EqualTo("Road signs"));
        }

        [Test]
        public async Task GetTranslation_shouldPersistAndCacheTranslationWithoutSurroundingQuotes_whenChatGptAnswerIsQuoted()
        {
            ChatGptAnswers("\"Road signs\"");

            var result = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);

            Assert.That(result, Is.EqualTo("Road signs"));
            _runtimeTranslationRwRepo.Verify(
                x => x.InsertData(It.Is<RuntimeTranslation>(t => t.TanslationValue == "Road signs")), Times.Once);
            Assert.That(_cachedTranslations.GetTranslation(Phrase, _language.Id).Value, Is.EqualTo("Road signs"));
        }

        [Test]
        public async Task GetTranslation_shouldInsertBasePhrase_whenChatGptCallSucceedsAndPhraseIsNotStoredYet()
        {
            BasePhraseStoredInDatabase(null);
            _basePhraseRwRepo
                .Setup(x => x.InsertData(It.IsAny<BasePhrase>()))
                .Returns((BasePhrase basePhrase) => new BasePhrase { Id = 11, Phrase = basePhrase.Phrase });
            ChatGptAnswers("Road signs");

            await _runtimeTranslationService.GetTranslation(Phrase, _language, true);

            _basePhraseRwRepo.Verify(x => x.InsertData(It.Is<BasePhrase>(p => p.Phrase == Phrase)), Times.Once);
            _runtimeTranslationRwRepo.Verify(
                x => x.InsertData(It.Is<RuntimeTranslation>(t => t.BasePhraseId == 11)), Times.Once);
        }

        [Test]
        public async Task GetTranslation_shouldReturnCachedTranslationWithoutAskingChatGpt_whenTranslationIsCached()
        {
            _cachedTranslations.AddTranslation(Phrase, _language.Id, "Road signs");
            _cachedTranslations.AddedTranslations.Clear();

            var result = await _runtimeTranslationService.GetTranslation(Phrase, _language, true);

            Assert.That(result, Is.EqualTo("Road signs"));
            _chatGptConnector.Verify(x => x.AskChatGptAsync(It.IsAny<string>(), It.IsAny<ChatGptModel>()), Times.Never);
            AssertNothingWasPersistedOrCached();
        }

        private void ChatGptAnswers(string answer)
        {
            _chatGptConnector
                .Setup(x => x.AskChatGptAsync(It.IsAny<string>(), It.IsAny<ChatGptModel>()))
                .ReturnsAsync(answer);
        }

        private void ChatGptThrows(Exception exception)
        {
            _chatGptConnector
                .Setup(x => x.AskChatGptAsync(It.IsAny<string>(), It.IsAny<ChatGptModel>()))
                .ThrowsAsync(exception);
        }

        private void BasePhraseStoredInDatabase(BasePhrase basePhrase)
        {
            _basePhraseRoRepo
                .Setup(x => x.GetData(It.IsAny<Expression<Func<BasePhrase, bool>>>(),
                    It.IsAny<Expression<Func<BasePhrase, object>>[]>()))
                .Returns(basePhrase == null ? new List<BasePhrase>() : new List<BasePhrase> { basePhrase });
        }

        private void AssertNothingWasPersistedOrCached()
        {
            _runtimeTranslationRwRepo.Verify(x => x.InsertData(It.IsAny<RuntimeTranslation>()), Times.Never);
            _runtimeTranslationRwRepo.Verify(x => x.InsertData(It.IsAny<ICollection<RuntimeTranslation>>()), Times.Never);
            _basePhraseRwRepo.Verify(x => x.InsertData(It.IsAny<BasePhrase>()), Times.Never);
            Assert.That(_cachedTranslations.AddedTranslations, Is.Empty);
        }

        private class InMemoryCachedTranslations : ICachedTranslationsSingleton
        {
            private readonly Dictionary<string, CachedTranslation> _translations = new Dictionary<string, CachedTranslation>();

            public List<string> AddedTranslations { get; } = new List<string>();

            public CachedTranslation GetTranslation(string key, int languageId)
            {
                return _translations.TryGetValue($"{key}_{languageId}", out var translation)
                    ? translation
                    : new CachedTranslation { Found = false };
            }

            public void AddTranslation(string key, int languageId, string translationValue,
                bool hasReplicableFields = false, string replicableFields = null)
            {
                AddedTranslations.Add(translationValue);
                _translations[$"{key}_{languageId}"] = new CachedTranslation
                {
                    Found = true,
                    Value = translationValue,
                    HasReplicableFields = hasReplicableFields,
                    ReplicableFields = replicableFields
                };
            }
        }
    }
}
