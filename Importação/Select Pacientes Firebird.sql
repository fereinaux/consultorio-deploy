select
'insert into Participantes(nome,rg,cpf,fone,email,DataNascimento,NomeContato,FoneContato,EstadoCivil,Escolaridade,Congregacao,Filhos,HasMedicacao,
Medicacao, HasAlergia, HasConvenio, HasParente , HasRestricaoAlimentar, HasTeste,Checkin,Sexo,Status,EventoId,Carona)
values (''' || coalesce(p.nome,'')
 ||''',''' || coalesce(p.rg,'')
 ||''',''' || coalesce(p.cpf,'')
  ||''',''' || coalesce(p.celular,'')
    ||''',''' || coalesce(p.email,'')
   ||''', ''' || coalesce(cast(p.dt_nasc as varchar(20)),'') ||''', '''
|| coalesce(p.contato1nome,'') ||''','''
 || coalesce(p.contato1fone,'') ||''','''
 || coalesce(p.estadocivil,'') ||''',''' || coalesce(p.escolaridade,'') ||''', ''' || coalesce(p.religiao,'') ||''', ' || coalesce( cast(p.filhos as varchar(2)),'0') ||', ' || case p.medicacao when 'S' then 1 else 0 end || ',''' || coalesce(p.contato3nome,'') ||''' ,0,0,0,0,0,0,0,1,1,0)'
from pessoas p


