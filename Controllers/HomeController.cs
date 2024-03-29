﻿using Arquitetura.Controller;
using Core.Business.Account;
using Core.Business.Arquivos;
using Core.Business.Configuracao;
using Core.Business.Equipes;
using Core.Business.Eventos;
using Core.Business.Lancamento;
using Core.Business.Participantes;
using Core.Business.Reunioes;
using Core.Models;
using Newtonsoft.Json;
using SysIgreja.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Utils.Constants;
using Utils.Enums;
using Utils.Extensions;
using Utils.Services;

namespace SysIgreja.Controllers
{

    [Authorize]
    public class HomeController : SysIgrejaControllerBase
    {
        private readonly IEquipesBusiness equipesBusiness;
        private readonly IParticipantesBusiness participantesBusiness;
        private readonly ILancamentoBusiness lancamentoBusiness;
        private readonly IImageService imageService;
        private readonly IReunioesBusiness reunioesBusiness;
        private readonly IEventosBusiness eventosBusiness;
        private readonly IArquivosBusiness arquivosBusiness;
        private readonly IAccountBusiness accountBusiness;

        public HomeController(IEquipesBusiness equipesBusiness, IImageService imageService, IParticipantesBusiness participantesBusiness, IArquivosBusiness arquivosBusiness, ILancamentoBusiness lancamentoBusiness, IEventosBusiness eventosBusiness, IAccountBusiness accountBusiness, IReunioesBusiness reunioesBusiness, IConfiguracaoBusiness configuracaoBusiness) : base(eventosBusiness, accountBusiness, configuracaoBusiness)
        {
            this.lancamentoBusiness = lancamentoBusiness;
            this.imageService = imageService;
            this.participantesBusiness = participantesBusiness;
            this.equipesBusiness = equipesBusiness;
            this.eventosBusiness = eventosBusiness;
            this.accountBusiness = accountBusiness;
            this.reunioesBusiness = reunioesBusiness;
            this.arquivosBusiness = arquivosBusiness;
        }

        [Authorize]
        public ActionResult Admin()
        {
            ViewBag.Title = "Sistema de Gestão";

            GetEventos(new string[] { "Financeiro", "Admin", "Geral", "Administrativo" });
            return View();
        }

        [HttpGet]
        public ActionResult GetResultadosGeral()
        {
            var eventos = eventosBusiness.GetEventosComImagens().ToList().Select(x => new
            {
                Id = x.Id,
                Evento = $"{(x.Numeracao > 0 ? $"{x.Numeracao}º " : string.Empty)}{x.Configuracao.Titulo}",
                Logo = x.Configuracao?.Logo != null ? imageService.ResizeImage(x.Configuracao?.Logo?.Conteudo, 50) : "",
                Background = x.Configuracao?.Background != null ? imageService.ResizeImage(x.Configuracao?.Background?.Conteudo, 150) : "",
                Cor = x.Configuracao.CorBotao,
                CorHover = x.Configuracao.CorHoverBotao,
            });

            var result = new
            {
                Eventos = eventos
            };
            return Json(new
            {
                result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetResultadosAdmin(int EventoId, DateTime? dataIni, DateTime? dataFim)
        {
            var evento = eventosBusiness.GetEventoById(EventoId);

            var result = new
            {

                Evento = evento.Status.GetDescription(),

                Total = participantesBusiness.GetParticipantesByEvento(EventoId).Where(x => x.Status != StatusEnum.Cancelado && x.Status != StatusEnum.Espera).Count(),
                Meninos = participantesBusiness.GetParticipantesByEvento(EventoId).Where(x => x.Sexo == SexoEnum.Masculino && x.Status != StatusEnum.Cancelado && x.Status != StatusEnum.Espera).Count(),
                Meninas = participantesBusiness.GetParticipantesByEvento(EventoId).Where(x => x.Sexo == SexoEnum.Feminino && x.Status != StatusEnum.Cancelado && x.Status != StatusEnum.Espera).Count(),
                MeiosPagamento = lancamentoBusiness.GetLancamentos().Where(x => x.EventoId == EventoId && x.Valor > 0).Select(x => x.MeioPagamento.Descricao).Distinct(),
                Financeiro = lancamentoBusiness.GetLancamentos().Where(x => x.EventoId == EventoId && x.Valor > 0 && (!(dataIni.HasValue && dataFim.HasValue) || x.DataCadastro >= dataIni && x.DataCadastro <= dataFim)).Select(x => new
                {
                    MeioPagamento = x.MeioPagamento.Descricao,
                    Valor = x.Valor,
                    Tipo = x.Tipo,
                    Data = x.DataCadastro
                }).GroupBy(x => new
                {
                    x.Tipo,
                    x.MeioPagamento,
                    x.Data.Value.Month
                })
                .Select(x => new
                {
                    Tipo = x.Key.Tipo,
                    MeioPagamento = x.Key.MeioPagamento,
                    Mes = x.Key.Month,
                    Valor = x.Sum(y => y.Valor)
                })
                .ToList()
                .Select(x => new
                {
                    Tipo = x.Tipo.GetDescription(),
                    MeioPagamento = x.MeioPagamento,
                    Mes = x.Mes.ToString(),
                    Valor = x.Valor
                })
                .OrderByDescending(x => x.Tipo),
                UltimosInscritos = participantesBusiness.GetParticipantesByEvento(EventoId).Where(x => x.Status != StatusEnum.Cancelado)
                .OrderByDescending(x => x.DataCadastro).Take(5).ToList().Select(x => new ParticipanteViewModel
                {
                    Nome = UtilServices.CapitalizarNome(x.Nome),
                    Sexo = x.Sexo.GetDescription(),
                    Idade = UtilServices.GetAge(x.DataNascimento)
                }).ToList(),
            };

            return Json(new
            {
                result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDetalhamentoEvento(int EventoId)
        {
            var result = new
            {
                Equipantes = equipesBusiness
                .GetEquipantesEvento(EventoId)
                .OrderBy(x => x.Equipe.Nome)
                .ThenBy(x => x.Tipo)
                .ThenBy(x => x.Equipante.Nome)
                .ToList()
                .Select(x => new
                {
                    Equipe = x.Equipe.Nome,
                    Nome = x.Equipante.Nome,
                    Tipo = x.Tipo.GetDescription(),
                    Fone = x.Equipante.Fone
                })
            };


            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CoordenadorGet(int eventoId)
        {

            var user = GetApplicationUser();
            var equipanteEvento = equipesBusiness.GetEquipanteEventoByUser(eventoId, user.Id);
            var membrosEquipe = equipesBusiness.GetMembrosEquipe(eventoId, equipanteEvento.EquipeId.Value);
            var result = new
            {
                Equipe = equipanteEvento.Equipe.Nome,
                EquipeEnum = equipanteEvento.EquipeId,
                QtdMembros = membrosEquipe.Count(),
                Configuracao = new
                {
                    Titulo = equipanteEvento.Evento.Configuracao.Titulo,
                    Id =
                equipanteEvento.Evento.ConfiguracaoId.Value,
                    Cor = equipanteEvento.Evento.Configuracao.CorBotao,
                    Logo = equipanteEvento.Evento.Configuracao.Logo != null ? Convert.ToBase64String(equipanteEvento.Evento.Configuracao.Logo.Conteudo) : ""
                },
                Membros = membrosEquipe.ToList().Select(x => new EquipanteViewModel
                {
                    Id = x.Equipante.Id,
                    Sexo = x.Equipante.Sexo.GetDescription(),
                    Fone = x.Equipante.Fone,
                    Idade = UtilServices.GetAge(x.Equipante.DataNascimento),
                    Nome = x.Equipante.Nome,
                    Faltas = reunioesBusiness.GetFaltasByEquipanteId(x.EquipanteId.Value, eventoId),
                    Oferta = lancamentoBusiness.GetPagamentosEquipante(x.EquipanteId.Value, x.EventoId.Value).Any(),
                })
            };


            var jsonRes = Json(new { result }, JsonRequestBehavior.AllowGet);
            jsonRes.MaxJsonLength = Int32.MaxValue;
            return jsonRes;
        }
        public ActionResult Coordenador()
        {
            ViewBag.Title = "Coordenador";
            var user = GetApplicationUser();
            var equipanteEvento = equipesBusiness.GetCoordByUser(user.Id);
            if (equipanteEvento.Count() > 0)
            {

                ViewBag.Eventos = equipanteEvento.Select(x => x.Evento);
                ViewBag.Equipante = equipanteEvento.Select(x => x.Equipante).FirstOrDefault();
                return View();
            }
            else
            {
                return NaoAutorizado();
            }
        }

        [HttpGet]
        public ActionResult GetPresenca(int ReuniaoId)
        {
            var presenca = equipesBusiness.GetPresenca(ReuniaoId).Select(x => x.EquipanteEventoId).ToList();

            var user = GetApplicationUser();
            var eventoId = reunioesBusiness.GetReuniaoById(ReuniaoId).EventoId;

            var result = equipesBusiness
                .GetMembrosEquipe(eventoId, equipesBusiness.GetEquipanteEventoByUser(eventoId, user.Id).EquipeId.Value).ToList().Select(x => new PresencaViewModel
                {
                    Id = x.Id,
                    Nome = x.Equipante.Nome,
                    Presenca = presenca.Contains(x.Id)
                });

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TogglePresenca(int EquipanteEventoId, int ReuniaoId)
        {
            equipesBusiness.TogglePresenca(EquipanteEventoId, ReuniaoId);

            return new HttpStatusCodeResult(200);
        }

        public ActionResult Index()
        {
            var user = GetApplicationUser();
            var permissoes = JsonConvert.DeserializeObject<List<Permissoes>>(user.Claims.Where(y => y.ClaimType == "Permissões").FirstOrDefault().ClaimValue);

            if (permissoes.Any(x => new string[] { "Admin", "Geral", }.Contains(x.Role) || (x.Eventos != null && x.Eventos.Any(y => new string[] { "Admin", "Geral", "Administrativo", "Financeiro" }.Contains(y.Role)))))
            {
                return RedirectToAction("Admin", "Home");
            }
            else if (permissoes.Any(x => new string[] { "Membro" }.Contains(x.Role) || x.Eventos.Any(y => new string[] { "Membro" }.Contains(y.Role))))
            {
                return NaoAutorizado();
            }
            else if (permissoes.Any(x => new string[] { "Padrinho" }.Contains(x.Role) || x.Eventos.Any(y => new string[] { "Padrinho" }.Contains(y.Role))))
            {
                return RedirectToAction("Index", "Participante");
            }
            else
            {
                return RedirectToAction("Coordenador", "Home");
            }

        }
    }
}