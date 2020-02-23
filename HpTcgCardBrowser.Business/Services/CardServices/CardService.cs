﻿using HpTcgCardBrowser.Business.Models.CardModels;
using HpTcgCardBrowser.Business.Models.ImportModels;
using HpTcgCardBrowser.Business.Models.LanguageModels;
using HpTcgCardBrowser.Business.Models.LessonModels;
using HpTcgCardBrowser.Business.Services.LanguageServices;
using HpTcgCardBrowser.Business.Services.LessonServices;
using HpTcgCardBrowser.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HpTcgCardBrowser.Business.Services.CardServices
{
    public class CardService
    {
        private HpTcgContext _context { get; set; }
        private CardSetService _cardSetService { get; set; }
        private CardTypeService _cardTypeService { get; set; }
        private CardRarityService _cardRarityService { get; set; }
        private LanguageService _languageService { get; set; }
        public LessonService _lessonService { get; set; }

        public CardService(HpTcgContext context, CardSetService cardSetService, CardTypeService cardTypeService,
                           CardRarityService cardRarityService, LanguageService languageService, LessonService lessonService)
        {
            _context = context;
            _cardSetService = cardSetService;
            _cardTypeService = cardTypeService;
            _cardRarityService = cardRarityService;
            _languageService = languageService;
            _lessonService = lessonService;
        }

        public List<CardModel> SearchCards(Guid? cardSetId, Guid? cardTypeId, Guid? cardRarityId, Guid languageId, int? lessonCost, string searchText)
        {
            var cards = (from card in _context.Card
                         join cardDetail in _context.CardDetail on card.CardId equals cardDetail.CardId
                         join language in _context.Language on cardDetail.LanguageId equals language.LanguageId
                         join cardSet in _context.CardSet on card.CardSetId equals cardSet.CardSetId
                         join cardRarity in _context.CardRarity on card.CardRarityId equals cardRarity.CardRarityId
                         join cardType in _context.CardType on card.CardTypeId equals cardType.CardTypeId
                         where !card.Deleted && !cardSet.Deleted && !cardRarity.Deleted && !cardType.Deleted &&
                               language.LanguageId == languageId && !string.IsNullOrEmpty(cardDetail.Url)
                         select new
                         {
                             card,
                             cardDetail,
                             cardSet,
                             cardRarity,
                             cardType,
                             language
                         });

            if (cardSetId != null && cardSetId != Guid.Empty)
            {
                cards = cards.Where(x => x.card.CardSetId == cardSetId);
            }
            if (cardTypeId != null && cardTypeId != Guid.Empty)
            {
                cards = cards.Where(x => x.card.CardTypeId == cardTypeId);
            }
            if (cardRarityId != null && cardRarityId != Guid.Empty)
            {
                cards = cards.Where(x => x.card.CardRarityId == cardRarityId);
            }
            if (lessonCost != null && lessonCost >= 0)
            {
                cards = cards.Where(x => x.card.LessonCost == lessonCost);
            }
            if (!string.IsNullOrEmpty(searchText))
            {
                cards = from card in cards
                        where EF.Functions.Like(card.cardDetail.Name, $"%{searchText}%") || EF.Functions.Like(card.cardDetail.Text, $"%{searchText}%")
                        select card;
            }

            var cardModels = cards.Select(x => GetCardModel(x.card, x.cardSet, x.cardRarity, x.cardType, x.cardDetail, x.language)).ToList();

            return cardModels != null ? cardModels : new List<CardModel>();
        }
        public List<CardModel> GetAllCards()
        {
            var cards = (from card in _context.Card
                         join cardDetail in _context.CardDetail on card.CardId equals cardDetail.CardId
                         join language in _context.Language on cardDetail.LanguageId equals language.LanguageId
                         join cardSet in _context.CardSet on card.CardSetId equals cardSet.CardSetId
                         join cardRarity in _context.CardRarity on card.CardRarityId equals cardRarity.CardRarityId
                         join cardType in _context.CardType on card.CardTypeId equals cardType.CardTypeId
                         where !card.Deleted && !cardDetail.Deleted && !language.Deleted && !cardSet.Deleted && !cardRarity.Deleted &&
                               !cardType.Deleted
                         select new
                         {
                             card,
                             cardDetail,
                             cardSet,
                             cardRarity,
                             cardType,
                             language
                         });

            var cardModels = cards.Select(x => GetCardModel(x.card, x.cardSet, x.cardRarity, x.cardType, x.cardDetail, x.language)).ToList();

            return cardModels != null ? cardModels : new List<CardModel>();
        }
        public void ImportCardsFromSets(List<ImportSetModel> sets)
        {
            var cardCache = GetAllCards();
            var setCache = _cardSetService.GetSets();
            var cardTypeCache = _cardTypeService.GetCardTypes();
            var rarityCache = _cardRarityService.GetCardRarities();
            var lessonCache = _lessonService.GetLessonTypes();

            foreach (var set in sets)
            {
                foreach (var importCard in set.Cards)
                {
                    //Between the JSON and my DB, the only way to determine if a card is a duplicate, is to compare
                    //the names between both objects
                    var cardExists = cardCache.Any(x => x.Detail.Name.ToLower() == importCard.Name.ToLower());
                    if (!cardExists)
                    {
                        var card = GetCard(importCard, set.Name, setCache, cardTypeCache, rarityCache, lessonCache);
                        var cardDetail = GetCardDetail(card.CardId, importCard);

                        _context.Card.Add(card);
                        _context.CardDetail.Add(cardDetail);
                    }
                }
            }

            _context.SaveChanges();
        }

        public static CardModel GetCardModel(Card card, CardSet cardSet, CardRarity cardRarity, CardType cardType, CardDetail cardDetail, Language language)
        {
            //TODO: Add lesson type
            return new CardModel()
            {
                CardId = card.CardId,
                CardSet = CardSetService.GetCardSetModel(cardSet),
                CardType = CardTypeService.GetCardTypeModel(cardType),
                Rarity = CardRarityService.GetCardRarityModel(cardRarity),
                Detail = CardDetailService.GetCardDetailModel(cardDetail, language),
                CardNumber = card.CardNumber,
                CssSizeClass = card.CssSizeClass,
                CreatedById = card.CreatedById,
                CreatedDate = card.CreatedDate,
                UpdatedById = card.UpdatedById,
                UpdatedDate = card.UpdatedDate,
                Deleted = card.Deleted,
            };
        }
        public Card GetCard(ImportCardModel importCardModel, string setName, List<CardSetModel> cardSetCache, List<CardTypeModel> cardTypesCache,
                            List<CardRarityModel> raritiesCache, List<LessonTypeModel> lessonTypeCache)
        {
            /*
             * As a general rule, each item's (set, type, raritiy), name should match up between the DB and the JSON.
             */

            var cardSet = cardSetCache.SingleOrDefault(x => x.Name.ToLower() == setName?.ToLower());
            var cardType = cardTypesCache.SingleOrDefault(x => x.Name.ToLower() == importCardModel.Type?.ToLower());
            var cardRarity = raritiesCache.SingleOrDefault(x => x.Name.ToLower() == importCardModel.Rarity?.ToLower());
            var lessonType = lessonTypeCache.SingleOrDefault(x => x.Name.ToLower() == importCardModel.LessonType?.ToLower());

            //Cards typically cost 1 action. But some cards cost more. From the official game, only adventures and characters
            //cost 2 actions. We don't want to assume the card costs 1 action because someone might screw up and forget to assign
            //a type to a card with more than 1 action. In the website, we'll hide the action cost if it's null.
            int? actionCost = null;
            if (cardType != null)
            {
                var typeOfCard = _cardTypeService.GetTypeOfCard(cardType.CardTypeId);
                if (typeOfCard == TypeOfCard.Adventure || typeOfCard == TypeOfCard.Character)
                    actionCost = 2;
                else
                    actionCost = 1;
            }

            return new Card()
            {
                CardId = Guid.NewGuid(),
                CardSetId = cardSet?.CardSetId,
                CardTypeId = cardType?.CardTypeId,
                CardRarityId = cardRarity?.CardRarityId,
                LessonTypeId = lessonType?.LessonTypeId,
                LessonCost = importCardModel.Cost == null ? null : (int?)Convert.ToInt32(importCardModel.Cost),
                ActionCost = actionCost,
                CardNumber = importCardModel.Number,
                CssSizeClass = "card-size-vertical", //Most cards are vertical. We'll set this to vertical by default and change it manually later
                CreatedById = Guid.Empty,
                CreatedDate = DateTime.UtcNow,
                UpdatedById = Guid.Empty,
                UpdatedDate = DateTime.UtcNow,
                Deleted = false,
            };
        }
        public CardDetail GetCardDetail(Guid cardId, ImportCardModel importCardModel)
        {
            //English is the only language for the moment
            var languageId = _languageService.GetLanguageId(TypeOfLanguage.English);

            return new CardDetail()
            {
                CardDetailId = Guid.NewGuid(),
                CardId = cardId,
                LanguageId = languageId,
                Name = importCardModel.Name,
                Text = importCardModel.Description.Text,
                Effect = importCardModel.Description.Effect,
                ToSolve = importCardModel.Description.ToSolve,
                Reward = importCardModel.Description.Reward,
                Url = null, //The URL to the card is maintained in a CDN, separate from the JSON.
                FlavorText = importCardModel.FlavorText,
                Illustrator = importCardModel.Artists != null ? string.Join(", ", importCardModel.Artists) : null,
                Copyright = null, //Copyright isn't maintained in the JSON
                CreatedById = Guid.Empty,
                CreatedDate = DateTime.UtcNow,
                UpdatedById = Guid.Empty,
                UpdatedDate = DateTime.UtcNow,
                Deleted = false,
            };
        }
    }
}