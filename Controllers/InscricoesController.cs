
using AutoMapper;
using Core.Business.Categorias;
using Core.Business.Configuracao;
using Core.Business.Equipantes;
using Core.Business.Equipes;
using Core.Business.Eventos;
using Core.Business.Lancamento;
using Core.Business.MeioPagamento;
using Core.Business.Newsletter;
using Core.Business.Participantes;
using Core.Models.Equipantes;
using Core.Models.Participantes;
using Data.Entities;
using SysIgreja.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Utils.Enums;
using Utils.Extensions;
//using MercadoPago.Config;
//using MercadoPago.Client.Preference;
//using MercadoPago.Resource.Preference;
using System.Collections.Generic;
using Core.Business.Notificacao;
using Utils.Services;
using System.Drawing.Imaging;

namespace SysIgreja.Controllers
{
    public class InscricoesController : Controller
    {
        private readonly IParticipantesBusiness participantesBusiness;
        private readonly ICategoriaBusiness categoriaBusiness;
        private readonly INotificacaoBusiness notificacaoBusiness;
        private readonly IConfiguracaoBusiness configuracaoBusiness;
        private readonly ILancamentoBusiness lancamentoBusiness;
        private readonly IEquipesBusiness equipesBusiness;
        private readonly IMeioPagamentoBusiness meioPagamentoBusiness;
        private readonly IEventosBusiness eventosBusiness;
        private readonly IEquipantesBusiness equipantesBusiness;
        private readonly INewsletterBusiness newsletterBusiness;
        private readonly IImageService imageService;
        private readonly IMapper mapper;

        public InscricoesController(IParticipantesBusiness participantesBusiness, IImageService imageService, INotificacaoBusiness notificacaoBusiness, ICategoriaBusiness categoriaBusiness, IEquipesBusiness equipesBusiness, IEquipantesBusiness equipantesBusiness, IConfiguracaoBusiness configuracaoBusiness, IEventosBusiness eventosBusiness, INewsletterBusiness newsletterBusiness, ILancamentoBusiness lancamentoBusiness, IMeioPagamentoBusiness meioPagamentoBusiness)
        {
            this.participantesBusiness = participantesBusiness;
            this.notificacaoBusiness = notificacaoBusiness;
            this.imageService = imageService;
            this.categoriaBusiness = categoriaBusiness;
            this.equipesBusiness = equipesBusiness;
            this.configuracaoBusiness = configuracaoBusiness;
            this.equipantesBusiness = equipantesBusiness;
            this.meioPagamentoBusiness = meioPagamentoBusiness;
            this.lancamentoBusiness = lancamentoBusiness;
            this.eventosBusiness = eventosBusiness;
            this.newsletterBusiness = newsletterBusiness;
            mapper = new MapperRealidade().mapper;
            //MercadoPagoConfig.AccessToken = "APP_USR-6615847238519666-091214-ae37fc001760151942cc1fa7ca689e3b-221658192";
        }

        [HttpPost]
        public ActionResult PostInscricao(PostInscricaoModel model)
        {
            var evento = eventosBusiness.GetEventoById(model.EventoId);
     
            return Json(Url.Action("InscricaoConcluida", new { Id = participantesBusiness.PostInscricao(model) }));
        }
    }
}