using Arquitetura.Controller;
using AutoMapper;
using Core.Business.Account;
using Core.Business.Caronas;
using Core.Business.Configuracao;
using Core.Business.Equipantes;
using Core.Business.Equipes;
using Core.Business.Eventos;
using Core.Models.Carona;
using SysIgreja.ViewModels;
using System.Linq;
using System.Web.Mvc;
using Utils.Constants;
using Utils.Services;

namespace SysIgreja.Controllers
{

    [Authorize]
    public class CaronaController : SysIgrejaControllerBase
    {
        private readonly ICaronasBusiness caronasBusiness;
        private readonly IEquipesBusiness equipesBusiness;
        private readonly IEquipantesBusiness equipantesBusiness;
        private readonly IMapper mapper;

        public CaronaController(ICaronasBusiness caronasBusiness, IEquipantesBusiness equipantesBusiness, IEquipesBusiness equipesBusiness, IEventosBusiness eventosBusiness, IAccountBusiness accountBusiness, IConfiguracaoBusiness configuracaoBusiness) : base(eventosBusiness, accountBusiness, configuracaoBusiness)
        {
            this.caronasBusiness = caronasBusiness;
            this.equipesBusiness = equipesBusiness;
            this.equipantesBusiness = equipantesBusiness;
            mapper = new MapperRealidade().mapper;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Caronas";
            GetEventos();

            return View();
        }

        [HttpGet]
        public ActionResult GetEquipantes(int EventoId)
        {
            var motoristaList = caronasBusiness.GetCaronas().Where(x => x.EventoId == EventoId).Select(x => x.MotoristaId).ToList();
            var pgList = equipantesBusiness.GetEquipantes().Where(x => !motoristaList.Contains(x.Id)).Select(x => new { x.Id, Nome = x.Nome }).ToList();

            return Json(new { Equipantes = pgList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCaronas(int EventoId)
        {
            var result = caronasBusiness
                .GetCaronas()
                .Where(x => x.EventoId == EventoId)
                .ToList()
                .Select(x => new CaronaViewModel
                {
                    Id = x.Id,
                    Capacidade = $"{caronasBusiness.GetParticipantesByCaronas(x.Id).Count().ToString()}/{x.Capacidade.ToString()}",
                    CapacidadeInt = x.Capacidade,
                    Quantidade = caronasBusiness.GetParticipantesByCaronas(x.Id).Count(),
                    Motorista = x.Motorista.Nome,
                    Latitude = x.Motorista.Latitude,
                    Longitude = x.Motorista.Longitude,
                    MotoristaId = x.MotoristaId.Value
                }).OrderByDescending(x => x.Id);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCarona(int Id)
        {
            var result = caronasBusiness.GetCaronaById(Id);

            return Json(new { Carona = mapper.Map<PostCaronaModel>(result) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostCarona(PostCaronaModel model)
        {
            caronasBusiness.PostCarona(model);

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult DeleteCarona(int Id)
        {
            caronasBusiness.DeleteCarona(Id);

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult DistribuirCaronas(int EventoId)
        {
            caronasBusiness.DistribuirCarona(EventoId);

            return new HttpStatusCodeResult(200);
        }

        [HttpGet]
        public ActionResult GetParticipantesSemCarona(int EventoId)
        {
            return Json(new
            {
                Participantes = caronasBusiness.GetParticipantesSemCarona(EventoId).Select(x => new
                {
                    Id = x.Id,
                    Nome = x.Nome
                }).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCaronasComParticipantes(int EventoId)
        {
            return Json(new
            {
                Caronas = caronasBusiness.GetCaronasComParticipantes(EventoId).ToList().Select(x => new
                {
                    Nome = UtilServices.CapitalizarNome(x.Participante.Nome),
                    Latitude = x.Participante.Latitude,
                    Longitude = x.Participante.Longitude,
                    ParticipanteId = x.ParticipanteId,
                    CaronaId = x.CaronaId,
                    Motorista = x.Carona.Motorista.Nome,
                    Endereco = $"{x.Participante.Logradouro}, {x.Participante.Numero}, {x.Participante.Bairro}, {x.Participante.Cidade}",
                    Quantidade = caronasBusiness.GetParticipantesByCaronas(x.CaronaId).Count(),
                    Capacidade = $"{caronasBusiness.GetParticipantesByCaronas(x.CaronaId).Count().ToString()}/{x.Carona.Capacidade.ToString()}",
                }).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeCarona(int ParticipanteId, int? DestinoId)
        {
            var mensagem = caronasBusiness.ChangeCarona(ParticipanteId, DestinoId);
            if (mensagem == "OK")
            {
                return new HttpStatusCodeResult(200);
            }

            return new HttpStatusCodeResult(400, mensagem);
        }

        [HttpGet]
        public ActionResult GetParticipantesByCarona(int CaronaId)
        {
            var result = caronasBusiness.GetParticipantesByCaronas(CaronaId).ToList().Select(x => new
            {
                Nome = UtilServices.CapitalizarNome(x.Participante.Nome),
                Apelido = UtilServices.CapitalizarNome(x.Participante.Apelido),
                Motorista = UtilServices.CapitalizarNome(x.Carona.Motorista.Nome),
                Fone = x.Participante.Fone,
                Endereco = $"{x.Participante.Logradouro}, {x.Participante.Numero}, {x.Participante.Bairro}, {x.Participante.Cidade}"
            });

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
    }
}