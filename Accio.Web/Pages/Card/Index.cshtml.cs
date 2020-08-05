﻿using System;
using System.Collections.Generic;
using Accio.Business.Models.CardModels;
using Accio.Business.Models.RulingModels;
using Accio.Business.Models.SourceModels;
using Accio.Business.Services.CardServices;
using Accio.Business.Services.LessonServices;
using Accio.Business.Services.SourceServices;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Accio.Web.Pages.Card
{
    public class IndexModel : PageModel
    {
        public CardModel Card { get; set; }
        public List<RulingModel> Rules { get; set; }

        public string SetShortName { get; set; }
        public string CardNumber { get; set; }

        public bool ShowCardData { get; set; } = false;

        private SourceService _sourceService { get; set; }
        private CardRulingService _cardRulingService { get; set; }
        private SingleCardService _singleCardService { get; set; }
        public LessonService _lessonService { get; set; }
        public TypeService _cardTypeService { get; set; }

        public IndexModel(SingleCardService singleCardService, SourceService sourceService, CardRulingService cardRulingService,
                          LessonService lessonService, TypeService typeService)
        {
            _singleCardService = singleCardService;
            _sourceService = sourceService;
            _cardRulingService = cardRulingService;
            _lessonService = lessonService;
            _cardTypeService = typeService;
        }

        public void OnGet(string setShortName, string cardNumber, string cardName)
        {
            SetShortName = setShortName;
            CardNumber = cardNumber;

            if (string.IsNullOrEmpty(SetShortName) || string.IsNullOrEmpty(CardNumber))
            {
                ShowCardData = false;
            }
            else
            {
                try
                {
                    var websiteSource = _sourceService.GetSource(SourceType.Website);
                    var param = new SingleCardParameters()
                    {
                        SetShortName = SetShortName,
                        CardNumber = CardNumber,
                    };

                    Card = _singleCardService.GetCard(param);
                    Rules = _cardRulingService.GetCardRules(Card.CardId);

                    ShowCardData = Card != null;
                }
                catch (Exception ex)
                {
                    //TODO: idk, something
                }
            }
        }
    }
}