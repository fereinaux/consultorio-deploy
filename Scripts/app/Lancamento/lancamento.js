﻿function CarregarTabelaLancamento(url, id, tipo) {
    var ajaxOptions = {
        url: url,
        data:
        {
            EventoId: $('#lancamento-eventoid').val(),
            MeioPagamentoId: $('#busca-meiopagamento').val() == 0 ? null : $('#busca-meiopagamento').val(),
            CentroCustoId: $('#busca-centrocusto').val() == 0 ? null : $('#busca-centrocusto').val()
        },
        datatype: "json",
        type: "POST"
    };

    var tableLancamentoConfig = {
        language: languageConfig,
        lengthMenu: [200, 500, 1000],
        colReorder: false,
        serverSide: false,
        deferloading: 0,
        orderCellsTop: true,
        fixedHeader: true,
        filter: true,
        orderMulti: false,
        responsive: true, stateSave: true, stateSaveCallback: stateSaveCallback, stateLoadCallback: stateLoadCallback,
        destroy: true,
        dom: domConfig,
        buttons: getButtonsConfig('A Receber'),
        columns: [
            { data: "Descricao", name: "Descricao", autoWidth: true },
            { data: "Origem", name: "Origem", autoWidth: true },
            { data: "CentroCusto", name: "CentroCusto", autoWidth: true },
            { data: "FormaPagamento", name: "FormaPagamento", autoWidth: true },
            { data: "Valor", name: "Valor", autoWidth: true },
            { data: "DataLancamento", name: "DataLancamento", autoWidth: true },
            {
                data: "Id", name: "Id", orderable: false, width: "10%",
                "render": function (data, type, row) {
                    return `${GetAnexosButton('AnexosLancamento', JSON.stringify(row), row.QtdAnexos)}
                            ${GetButton('EditLancamento', JSON.stringify({ Id: data, Tipo: tipo }), 'blue', 'fa-edit', 'Editar')}   
                            ${row.Observacao != null ? GetButton('PrintRecibo', data, 'yellow', 'fa-print', 'Imprimir Recibo') : ""}
                            ${GetButton('DeleteLancamento', data, 'red', 'fa-trash', 'Excluir')}`;
                }
            }
        ],
        order: [
            [1, "asc"]
        ],
        ajax: ajaxOptions,
        drawCallback: function (row, data, start, end, display) {
            var api = this.api(), data;
            var intVal = function (i) {
                return typeof i === 'string' ?
                    Number(i.replace('R$', '').replace('.', '').replace(',', '.')) * 1 :
                    typeof i === 'number' ?
                        i : 0;
            };

            total = api
                .column(4, { selected: true, search: 'applied' })
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0);


            $(api.column(4).footer()).html(
                total.toLocaleString('pt-BR', { minimumFractionDigits: 2, style: 'currency', currency: 'BRL' })
            );
        }
    };

    $(id).DataTable(tableLancamentoConfig);
}

function GetLancamento(id) {
    if (id > 0) {
        $.ajax({
            url: "/Lancamento/GetLancamento/",
            data: { Id: id },
            datatype: "json",
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                handleSelect()
                $("#lancamento-id").val(data.Lancamento.Id);
                $("#lancamento-centrocusto").val(data.Lancamento.CentroCustoId);
                $("#lancamento-descricao").val(data.Lancamento.Descricao);
                $("#lancamento-origem").val(data.Lancamento.Origem);
                $("#lancamento-observacao").val(data.Lancamento.Observacao);
                $("#lancamento-meiopagamento").val(data.Lancamento.MeioPagamentoId);
                $("#lancamento-data").val(moment(data.Lancamento.DataLancamento).format('DD/MM/YYYY'));
                $("#lancamento-contabancaria").val(data.Lancamento.ContaBancariaId > 0 ? data.Lancamento.ContaBancariaId : 0);
                $("#lancamento-valor").val(data.Lancamento.Valor);
            }
        });
    }
    else {
        handleSelect()
        $("#lancamento-id").val(0);
        $("#lancamento-centrocusto").val($("#lancamento-centrocusto option").not(".d-none").first().val());
        $("#lancamento-descricao").val("");
        $("#lancamento-origem").val("");
        $("#lancamento-observacao").val("");
        $("#lancamento-meiopagamento").val($("#lancamento-meiopagamento option:first").val());
        $("#lancamento-valor").val(0);
    }
}

function EditLancamento(params) {
    $("#lancamento-tipo").val(params.Tipo == "Receber" ? 1 : 2);
    GetLancamento(params.Id);
   
}

function handleSelect() {
    if ($("#lancamento-tipo").val() == 1) {
        $('#modal-lancamentos .opt-centrocusto[data-tipo="Despesa"]').addClass('d-none');
        $('#modal-lancamentos .opt-centrocusto[data-tipo="Receita"]').removeClass('d-none');
    } else {
        $('#modal-lancamentos .opt-centrocusto[data-tipo="Despesa"]').removeClass('d-none');
        $('#modal-lancamentos .opt-centrocusto[data-tipo="Receita"]').addClass('d-none');
    }
    $("#modal-lancamentos").modal();
}

function DeleteLancamento(id) {
    ConfirmMessageDelete().then((result) => {
        if (result) {
            $.ajax({
                url: "/Lancamento/DeletePagamento/",
                datatype: "json",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(
                    {
                        Id: id
                    }),
                success: function () {
                    SuccessMesageDelete();
                    Filtrar();
                }
            });
        }
    });
}

function PostLancamento() {
    if (ValidateForm(`#form-lancamento`)) {
        $.ajax({
            url: "/Lancamento/PostLancamento/",
            datatype: "json",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(
                {
                    Id: $("#lancamento-id").val(),
                    Descricao: $("#lancamento-descricao").val(),
                    Origem: $("#lancamento-origem").val(),
                    Observacao: $("#lancamento-observacao").val(),
                    Tipo: $("#lancamento-tipo").val(),
                    Data: moment($("#lancamento-data").val(), 'DD/MM/YYYY', 'pt-br').toJSON(),
                    MeioPagamentoId: $("#lancamento-meiopagamento").val(),
                    EventoId: $('#lancamento-eventoid').val(),
                    CentCustoId: $("#lancamento-centrocusto").val(),
                    ContaBancariaId: $('.contabancaria').hasClass('d-none') ? 0 : $("#lancamento-contabancaria").val(),
                    Valor: Number($("#lancamento-valor").val())
                }),
            success: function () {
                SuccessMesageOperation();
                Filtrar();
                $("#modal-lancamentos").modal("hide");
            }
        });
    }
}

$(document).ready(function () {
    Filtrar();
});

function Filtrar() {
    CarregarTabelaLancamento('/Lancamento/GetLancamentoReceber', "#table-lancamentos-receber", 'Receber');
    CarregarTabelaLancamento('/Lancamento/GetLancamentoPagar', "#table-lancamentos-pagar", 'Pagar');
}

var lancamento = {}

function AnexosLancamento(row) {
    lancamento = row
    $("#LancamentoId").val(row.Id);
    GetAnexos(row.Id);
    $("#modal-anexos").modal();
}

function GetAnexos(id) {
    const tableArquivoConfig = {
        language: languageConfig,
        lengthMenu: [200, 500, 1000],
        colReorder: false,
        serverSide: false,
        deferloading: 0,
        orderCellsTop: true,
        fixedHeader: true,
        filter: true,
        orderMulti: false,
        responsive: true, stateSave: true, stateSaveCallback: stateSaveCallback, stateLoadCallback: stateLoadCallback,
        destroy: true,
        dom: domConfigNoButtons,
        columns: [
            { data: "Nome", name: "Nome", autoWidth: true },
            { data: "Extensao", name: "Extensao", autoWidth: true },
            {
                data: "Id", name: "Id", orderable: false, width: "15%",
                "render": function (data, type, row) {
                    return `${GetButton('GetArquivo', data, 'blue', 'fa-download', 'Download')}
                            ${GetButton('DeleteArquivo', data, 'red', 'fa-trash', 'Excluir')}`;
                }
            }
        ],
        order: [
            [0, "asc"]
        ],
        ajax: {
            url: '/Arquivo/GetArquivosLancamento',
            data: { Id: id },
            datatype: "json",
            type: "POST"
        }
    };

    $("#table-anexos").DataTable(tableArquivoConfig);
}

function GetArquivo(id) {
    window.open(`/Arquivo/GetArquivo/${id}`)
}

function DeleteArquivo(id) {
    ConfirmMessageDelete().then((result) => {
        if (result) {
            $.ajax({
                url: "/Arquivo/DeleteArquivo/",
                datatype: "json",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(
                    {
                        Id: id
                    }),
                success: function () {
                    SuccessMesageDelete();
                    GetAnexos($("#LancamentoId").val());
                }
            });
        }
    });
}

function PostArquivo() {
    var dataToPost = new FormData($('#frm-upload-arquivos')[0]);
    dataToPost.set('ParticipanteId', lancamento.ParticipanteId)

    $.ajax(
        {
            processData: false,
            contentType: false,
            type: "POST",
            data: dataToPost,
            url: "Arquivo/PostArquivo",
            success: function () {
                GetAnexos($("#LancamentoId").val());
            }
        });
}

$("#arquivo").change(function () {
    PostArquivo();
});


$("#modal-anexos").on('hidden.bs.modal', function () {
    Filtrar();
});


function PrintRecibo(id) {
    $.ajax({
        url: "/Lancamento/GetLancamento/",
        data: { Id: id },
        datatype: "json",
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            var doc = CriarPDFA4();
            var text = `Recebi do ${$("#lancamento-eventoid option:selected").text()}, a importância de ${data.Lancamento.Valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })} referente a ${data.Lancamento.Descricao}.`;
            var valor = data.Lancamento.Valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
            moment.locale("pt-br");
            var dataExtenso = `${moment().format('D')} de ${moment().format('MMMM')} de ${moment().format('YYYY')}`;

            doc.setFont('helvetica', "bold")
            doc.text(94, 20, "RECIBO");
            doc.text(150, 30, `Valor: ${valor}`);

            doc.setFont('helvetica', "normal")
            splitText = doc.splitTextToSize(text, 160);
            doc.text(20, 50, splitText);
            doc.text(110, 83, `Recife, ${dataExtenso}`);

            doc.line(20, 98, 100, 98);
            doc.text(20, 105, data.Lancamento.Observacao);

            printDoc(doc);
        }
    });
}