﻿using HpTcgCardBrowser.Business.Models.CardModels;
using HpTcgCardBrowser.Business.Models.LanguageModels;
using HpTcgCardBrowser.Data;

namespace HpTcgCardBrowser.Business.Services.CardServices
{
    public class CardDetailService
    {
        private HpTcgContext _context { get; set; }

        public CardDetailService(HpTcgContext context)
        {
            _context = context;
        }

        public static CardDetailModel GetCardDetailModel(CardDetail cardDetail, Language language)
        {
            return new CardDetailModel()
            {
                CardDetailId = cardDetail.CardDetailId,
                CardId = cardDetail.CardId,
                Language = new LanguageModel()
                {
                    LanguageId = language.LanguageId,
                    Name = language.Name,
                    Code = language.Code,
                    FlagImagePath = language.FlagImagePath,
                    CreatedById = language.CreatedById,
                    CreatedDate = language.CreatedDate,
                    UpdatedById = language.UpdatedById,
                    UpdatedDate = language.UpdatedDate,
                    Deleted = language.Deleted,
                },
                Name = cardDetail.Name,
                Text = cardDetail.Text,
                Effect = cardDetail.Effect,
                Reward = cardDetail.Reward,
                ToSolve = cardDetail.ToSolve,
                Url = cardDetail.Url,
                FlavorText = cardDetail.FlavorText,
                Illustrator = cardDetail.Illustrator,
                Copyright = cardDetail.Copyright,
                CreatedById = cardDetail.CreatedById,
                CreatedDate = cardDetail.CreatedDate,
                UpdatedById = cardDetail.UpdatedById,
                UpdatedDate = cardDetail.UpdatedDate,
                Deleted = cardDetail.Deleted,
            };
        }
    }
}