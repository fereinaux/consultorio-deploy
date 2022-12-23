using Arquitetura.Controller;
using Core.Business.Account;
using Core.Business.Configuracao;
using Core.Business.Equipes;
using Core.Business.Eventos;
using Core.Business.Reunioes;
using Core.Models.Reunioes;
using SysIgreja.ViewModels;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Utils.Constants;
using Utils.Extensions;

namespace SysIgreja.Controllers
{

    [Authorize]
    public class ConsultaController : SysIgrejaControllerBase
    {
        private readonly IReunioesBusiness reuniaosBusiness;
        private readonly IEquipesBusiness equipesBusiness;

        public ConsultaController(IReunioesBusiness ReuniaosBusiness, IEquipesBusiness equipesBusiness, IEventosBusiness eventosBusiness, IConfiguracaoBusiness configuracaoBusiness, IAccountBusiness accountBusiness) : base(eventosBusiness, accountBusiness, configuracaoBusiness)
        {
            this.reuniaosBusiness = ReuniaosBusiness;
            this.equipesBusiness = equipesBusiness;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Consultas";
            GetEventos();

            return View();
        }

        [HttpGet]
        public ActionResult GetReunioes(int EventoId)
        {
            var result = reuniaosBusiness
                .GetReunioes()
                .Where(x => x.EventoId == EventoId)
                .ToList()
                .Select(x => new
                {
                    Id = x.Id,
                    DataReuniao = x.DataReuniao.ToString("yyyy-MM-ddTHH:mm"),
                    Pauta = x.Pauta,
                    PacienteId = x.ParticipanteId,
                    Paciente = x.Participante?.Nome,
                    Etiquetas = x.ConsultaEtiquetas.Select(y => y.Etiqueta.Id)

                });

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetReunioesByPaciente(int pacienteId)
        {
            var result = reuniaosBusiness
                .GetReunioes()
                .Where(x => x.ParticipanteId == pacienteId)
                .ToList()
                .Select(x => new
                {
                    Id = x.Id,
                    DataReuniao = x.DataReuniao.ToString("dd/MM/yyyy"),
                    Pauta = x.Pauta,
                    PacienteId = x.ParticipanteId,
                    Paciente = x.Participante?.Nome

                });

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetReuniao(int Id)
        {

            var result = reuniaosBusiness.GetReunioes().Where(x => x.Id == Id).ToList()
                .Select(x => new
                {
                    Id = x.Id,
                    DataReuniao = x.DataReuniao.ToString("dd/MM/yyyy HH:mm"),
                    Pauta = x.Pauta,
                    PacienteId = x.ParticipanteId,
                    Paciente = x.Participante?.Nome,
                    Etiquetas = x.ConsultaEtiquetas.Select(y => y.Etiqueta.Id)

                }).FirstOrDefault();

            return Json(new { Reuniao = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostReuniao(PostReuniaoModel model)
        {
            reuniaosBusiness.PostReuniao(model);

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult DeleteReuniao(int Id)
        {
            reuniaosBusiness.DeleteReuniao(Id);

            return new HttpStatusCodeResult(200);
        }
    }
}