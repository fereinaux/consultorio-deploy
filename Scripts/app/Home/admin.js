+$(document).ready(() => {
    $('#data-ini').val(moment().startOf('quarter').format('DD/MM/YYYY'))
    $('#data-fim').val(moment().endOf('quarter').format('DD/MM/YYYY'))
    $('#data-fim,#data-ini').change(GetResultadosAdmin)
    carregarEtiquetas()
    HideMenu();
    GetResultadosAdmin();
    GetParticipantes()
    onChangePagamentos()
    carregarConsultas()
    changeArquivoConsulta()
    //GetResultadosGeral();
});

function getEquipantesExcel() {
    $.ajax({
        url: "/Equipante/getEquipantesExcel/",
        datatype: "json",
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(
            { EventoId: $("#eventoid").val() }),
        success: (data) => {

            window.location = `DownloadTempFile?fileName=Equipe ${$("#eventoid option:selected").text()}.xlsx&g=` + data;
        }
    });
}

function carregarConsultas() {
    $.ajax({
        url: '/Consulta/GetReunioes',
        datatype: "json",
        data: { EventoId: $("#eventoid").val(), },
        type: "GET",
        success: (data) => {
            var calendarEl = document.getElementById('calendar-consultas');
            var calendar = new FullCalendar.Calendar(calendarEl, {
                customButtons: {
                    new: {
                        text: 'Nova consulta',
                        click: function () {
                            EditReuniao(0)
                        }
                    }
                },
                initialView: 'listDay',
                locale: 'pt-br',
                headerToolbar: {
                    start: 'prev,next,today', // will normally be on the left. if RTL, will be on the right
                    center: 'title',
                    end: 'new' // will normally be on the right. if RTL, will be on the left
                },
                eventTimeFormat: {
                    hour: '2-digit',
                    minute: '2-digit',
                    meridiem: false
                },
                eventContent: function (arg) {
                    let div = document.createElement('div')

                    $(div).html(`${arg.event.title} <div> ${arg.event.extendedProps.etiquetas.map(
                        etiqueta => {
                            cor = $(`#reuniao-marcadores option[value=${etiqueta}]`).data('cor')
                            titulo = $(`#reuniao-marcadores option[value=${etiqueta}]`).text()
                            return `<span class="badge m-r-xs" style="background-color:${cor};color:#fff">${titulo}</span>`
                        }).join().replace(/,/g, '')}`)


                    let arrayOfDomNodes = [div]
                    return { domNodes: arrayOfDomNodes }
                },
                events: data.data.map(e => {
                    return {
                        title: e.Paciente ?? "Agenda Bloqueada",
                        start: e.DataReuniao,
                        backgroundColor: 'transparent',
                        className: e.Paciente ? null : "no-pacient",
                        extendedProps: {
                            id: e.Id,
                            etiquetas: e.Etiquetas
                        },
                    }
                }),
                eventClick: function (info) {
                    EditReuniao(info.event._def.extendedProps.id)
                },
            });
            calendar.render();
        }
    })
}

function GetResultadosGeral() {
    if ($('.bloco-adm-geral').length > 0) {


        $.ajax({
            url: '/Home/GetResultadosGeral',
            datatype: "json",
            data: { EventoId: $("#eventoid").val(), dataIni: moment($("#data-ini").val(), 'DD/MM/YYYY', 'pt-br').toJSON(), dataFim: moment($("#data-fim").val(), 'DD/MM/YYYY', 'pt-br').toJSON(), },
            type: "GET",
            success: (data) => {
                result = data.result;
                var calendarEl = document.getElementById('calendar');
                var calendar = new FullCalendar.Calendar(calendarEl, {
                    initialView: 'listMonth',
                    locale: 'pt-br',
                    height: 400,
                    buttons: {
                        today: false
                    },
                    events: result.Eventos.map(e => {
                        return {
                            title: e.Evento,
                            start: e.Data,
                            color: 'transparent',
                            extendedProps: {
                                logo: e.Logo,
                                bg: e.Background,
                                cor: e.Cor,
                                hover: e.CorHover,
                                id: e.Id
                            }
                        }
                    }),
                    eventContent: function (arg) {
                        let div = document.createElement('div')
                        let overlay = document.createElement('div')
                        let spanContainer = document.createElement('div')
                        let span = document.createElement('span')
                        let img = document.createElement('img')
                        let bg = document.createElement('img')
                        img.classList.add('img-calendar')
                        bg.classList.add('bg-calendar')
                        div.classList.add('div-calendar')
                        overlay.classList.add('overlay-calendar')
                        div.dataset.id = arg.event.extendedProps.id
                        span.classList.add('span-calendar')
                        spanContainer.classList.add('span-container-calendar')
                        span.innerHTML = arg.event.title
                        img.src = `data:image/png;base64,${arg.event.extendedProps.logo}`
                        bg.src = `data:image/png;base64,${arg.event.extendedProps.bg}`
                        if (arg.event.extendedProps.logo) {

                            div.append(img)
                        }
                        if (arg.event.extendedProps.bg) {

                            div.append(bg)
                            div.append(overlay)
                        }
                        spanContainer.append(span)
                        div.append(spanContainer)
                        return { domNodes: [div] }
                    }
                    //eventContent: function (arg) {
                    //    console.log(arg);
                    //    var element = $(arg.el);
                    //    console.log(element);

                    //    let img = document.createElement('img')

                    //    img.src = `data:image/png;base64,${arg.event.extendedProps.logo}`
                    //    element.append(img)
                    //}

                });
                calendar.render();
            }
        })


    }
}

$('body').on('DOMNodeInserted', '.div-calendar', function () {
    var element = $(this)
    element.parent().parent().css('cursor', 'pointer')
    element.parent().parent().css('background-color', 'transparent')
    element.parent().parent().css('border-color', 'transparent')

    element.parent().parent().click(function () {
        $('#eventoid').val($(this).find('.div-calendar').data('id')).trigger('change');

    })


});

let chartFinanc


function GetResultadosAdmin() {
    if ($('#eventoid option:selected').data('role') == "Administrativo") {
        $('#resumo-financeiro-bloco').css('display', 'none')
    } else {
        $('#resumo-financeiro-bloco').css('display', 'block')
    }
    $.ajax({
        url: '/Home/GetResultadosAdmin',
        datatype: "json",
        data: { EventoId: $("#eventoid").val(), dataIni: moment($("#data-ini").val(), 'DD/MM/YYYY', 'pt-br').toJSON(), dataFim: moment($("#data-fim").val(), 'DD/MM/YYYY', 'pt-br').toJSON(), },
        type: "GET",
        success: (data) => {
            result = data.result;
            if (result.Total == 0) {
                $('.zero-participantes').css('display', 'none')
            } else {
                $('.zero-participantes').css('display', 'block')
            }
            $("#total").text(result.Total);
            $("#espera").text(result.Espera);
            $("#confirmados").text(result.Confirmados);
            $("#isencoes").text(result.Isencoes);
            $("#presentes").text(result.Presentes);
            $("#cancelados").text(result.Cancelados);
            $("#meninos").text(result.Meninos);
            $("#meninas").text(result.Meninas);
            $("#equipe-male").text(result.EquipeMeninos);
            $("#equipe-female").text(result.EquipeMeninas);
            $("#boletos").text(result.Boletos);
            $("#contatos").text(result.Contatos);
            $("#saldo").text(result.SaldoGeral);
            $("#saldodinheiro").text(result.SaldoDinheir);
            $("#saldopix").text(result.SaldoPix);
            $("#receita").text(result.TotalReceber);
            $("#despesa").text(result.TotalPagar);

            htmlResumoFinanc = ''
            receberTotal = 0
            pagarTotal = 0
            result.MeiosPagamento.map(mp => {
                receber = result.Financeiro.filter(f => f.Tipo == 'Receber' && f.MeioPagamento == mp).reduce((a, b) => ({ MeioPagamento: a.MeioPagamento, Valor: a.Valor + b.Valor }), { Valor: 0 })
                pagar = result.Financeiro.filter(f => f.Tipo == 'Pagar' && f.MeioPagamento == mp).reduce((a, b) => ({ MeioPagamento: a.MeioPagamento, Valor: a.Valor + b.Valor }), { Valor: 0 })
                receberValor = receber ? receber.Valor : 0
                receberTotal += receberValor
                pagarValor = pagar ? pagar.Valor : 0
                pagarTotal += pagarValor
                saldo = receberValor - pagarValor
                htmlResumoFinanc += `<tr>                        
                        <td>${mp}</td>
                        <td>R$ ${receberValor.toLocaleString('pt-br', { minimumFractionDigits: 2 })}</td>
<td>R$ ${pagarValor.toLocaleString('pt-br', { minimumFractionDigits: 2 })}</td>
<td>R$ ${saldo.toLocaleString('pt-br', { minimumFractionDigits: 2 })}</td>
                    </tr>$`
            })
            $('#resumo-financeiro').html(htmlResumoFinanc);
            $('#receberTotal').text(`R$ ${receberTotal.toLocaleString('pt-br', { minimumFractionDigits: 2 })}`)
            $('#pagarTotal').text(`R$ ${pagarTotal.toLocaleString('pt-br', { minimumFractionDigits: 2 })}`)
            $('#saldoTotal').text(`R$ ${(receberTotal - pagarTotal).toLocaleString('pt-br', { minimumFractionDigits: 2 })}`)


            htmlInscritos = '';
            $(result.UltimosInscritos).each((i, element) => {
                htmlInscritos += `<tr>                        
                        <td>${element.Nome}</td>                        
                        <td>${element.Idade}</td>                                                
                    </tr>`;
            });

            $('#ultimos-inscritos').html(htmlInscritos);
            if (chartFinanc) {
                chartFinanc.destroy()
            }

            months = getMonthsBetweenDates($("#data-ini").val(), $("#data-fim").val())
            chartFinanc = new Chart(
                document.getElementById('chart-financeiro'),
                {
                    type: 'line',
                    options: {
                        scales: {
                            y: {
                                ticks: {
                                    // Include a dollar sign in the ticks
                                    callback: function (value, index, ticks) {
                                        return `R$ ${value.toLocaleString('pt-br', { minimumFractionDigits: 2 })}`;
                                    }
                                }
                            },
                         
                        },
                        responsive: true,
                        plugins: {
                            legend: {
                                position: 'top',
                            },
                            title: {
                                display: false
                            },
                            tooltip: {
                                callbacks: {
                                    label: function (context) {
                                        let label = context.dataset.label || '';

                                        if (label) {
                                            label += ': ';
                                        }
                                        if (context.parsed.y !== null) {
                                            label +=`R$ ${context.parsed.y.toLocaleString('pt-br', { minimumFractionDigits: 2 })}`;
                                        }
                                        return label;
                                    }
                                }
                            }
                        }
                    },
                    data: {
                        labels: months,
                        datasets: [
                            {
                                label: 'Receber',
                                backgroundColor: "#1ebd7b70",
                                borderColor: "#1ebd7b",
                                data: months.map(mes => result.Financeiro.filter(x => x.Tipo == "Receber" && x.Mes == moment(mes, "MMM YYYYY").locale('pt-br').month() + 1)).map(valorMes => valorMes.reduce((a, b) => ({ Valor: a.Valor + b.Valor }), { Valor: 0 })).map(valor =>  valor.Valor)
                            },   
                            {
                                label: 'Pagar',
                                backgroundColor: "#d5343487",
                                borderColor: "#d53434",
                                data: months.map(mes => result.Financeiro.filter(x => x.Tipo == "Pagar" && x.Mes == moment(mes, "MMM YYYYY").locale('pt-br').month() + 1)).map(valorMes => valorMes.reduce((a, b) => ({ Valor: a.Valor + b.Valor }), { Valor: 0 })).map(valor => valor.Valor)
                            },  
                        ]
                    }
                }
            );

            htmlEquipes = '';
            htmlEquipesMobile = '';
            totalEquipe = 0;
            $(result.Equipes).each((i, element) => {
                totalEquipe += element.QuantidadeMembros;
                htmlEquipesMobile += `  <div class="col col-xs-6">
                                <div class="equipe-mobile black-bg">
                                    <div class="mobile-content">
                                        <h3>${element.QuantidadeMembros}</h3>
                                        <p>${element.Equipe}</p>
                                    </div>
                                </div>
                            </div>`
                htmlEquipes += `<tr>                        
                        <td>${element.Equipe}</td>                        
                        <td>${element.QuantidadeMembros}</td>                                                
                    </tr>`;
            });

            htmlEquipesMobile += `  <div class="col col-xs-6">
                                <div class="equipe-mobile navy-bg">
                                    <div class="mobile-content">
                                        <h3>${totalEquipe}</h3>
                                        <p>Total</p>
                                    </div>
                                </div>
                            </div>`

            $('#tb-equipes').html(htmlEquipes);
            $('.equipe-mobile-container').html(htmlEquipesMobile);
            $('#totalEquipe').text(totalEquipe);

            htmlReunioes = '';
            $(result.Reunioes).each((i, element) => {
                htmlReunioes += `<tr>
                        <td>${element.Titulo}</td>
                        <td>${moment(element.DataReuniao).format('DD/MM/YYYY HH:mm')}</td>                        
                        <td>${element.Presenca}</td>                                                
                    </tr>`;
            });
            $('#tb-reunioes').html(htmlReunioes);

            var randomColorGenerator = function () {
                return '#' + (Math.random().toString(16) + '0000000').slice(2, 8);
            };

        }
    });
}
