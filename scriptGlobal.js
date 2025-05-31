let BaseUrlFront = 'http://127.0.0.1:5500/';
let BaseUrlBack = 'https://localhost:7034/';

async function buscarEducadores() {
    return educadores = await executarRequisicao("api/Agente/buscarDadosEducadores", "", "GET");
}
async function executarScriptsEspecificos(nome) {

    switch (nome) {
        case "ListaEducadores":
            preencherListEducadores()
            break;
        case "Dashboard":
            let tipoAgente = localStorage.getItem('tipo');
            if (tipoAgente == '2') {
                preencherAlunosVinculados()
            }
            break;
        case "InserirMateriais":
            preencherNomesMaterias()
            break;
        case "Materiais":
            preencherListMateriais();
            break;
        case "Atividades":
            preencherNomesMaterias()
            preencherAtividadesPorAgente()
            break;
        case "NovaAtividade":
            preencherCheckboxALunoPorEducador()
            preencherNomesMaterias()
            break;
        case "Estatistica":
            if (localStorage.getItem("tipo") == "1") gerarRelatorio(parseInt(localStorage.getItem('idAluno')))
            else criarSelectAlunos()

            break;
        case "Perfil":
            getDadosAluno();
            break;
    }
}
async function preencherSelectALunoPorEducador() {
    try {
        const alunos = await executarRequisicao(`api/Agente/buscarAlunosPorEducador/${localStorage.getItem("idAgente")}`, "", "GET");
        let html = '<option value="">Selecione um Aluno</option>';
        for (const aluno of alunos) {
            html += `<option value="${aluno.idAluno}">${aluno.nome}</option>`;
        }
        $("#aluno").html(html);
    } catch (error) {
        console.error("Erro - preencherSelectALunoPorEducador:", error);
    }
}
async function preencherCheckboxALunoPorEducador() {

    try {
        const alunos = await executarRequisicao(`api/Agente/buscarAlunosPorEducador/${localStorage.getItem("idAgente")}`, "", "GET");
        let html = '';
        for (const aluno of alunos) {
            html += `<label><input type="checkbox" value="${aluno.idAluno}">${aluno.nome}</label>`;
        }
        $("#aluno").html(html);
    } catch (error) {
        console.error("Erro - preencherCheckboxALunoPorEducador:", error);
    }
}
async function preencherAtividadesPorAgente() {
    const dados = await executarRequisicao(`api/atividade/atividadesPorAgente/${localStorage.getItem("idAgente")}`, "", "GET");
    if (dados.tipoAgente == 'educador') $(".cabecalhoEducador").show();

    const lista = document.getElementById('lista-atividades');
    lista.innerHTML = "";

    dados.atividades.forEach(atividade => {
        const li = document.createElement('li');
        li.className = "card btn editarAtividade";
        li.setAttribute("data-materia", atividade.materia.nome);
        li.setAttribute("id", `atividade-${atividade.id}`);

        const imagemBase64 = atividade.materia.imagem;
        const imagemSrc = imagemBase64
            ? `${imagemBase64}`
            : "caminho/para/imagem-default.png";

        li.innerHTML = `
            <div class="d-flex flex-column">
                <img src="${imagemSrc}" alt="Capa da Matéria" id="imagem-atividade-${atividade.id}">

                 <div class="card-title">${atividade.nome}</div>
                <small class="text-muted hide">${atividade.materia.nome}</small>
        
                <div class="card-description">${atividade.informacaoExtra}     </div>
                <button type="button" class="btn btn-sm btn-outline-danger deletarAtividade d-none">
                    <i class="fa fa-trash"></i>
                </button>
            </div>
        `;

        lista.appendChild(li);
    });
}

async function preencherNomesMaterias() {
    try {
        const materias = await executarRequisicao(`api/materia`, "", "GET");

        let html = '<option value="">Selecione uma Materia</option>';
        for (const materia of materias) {
            html += `<option value="${materia.id}">${materia.nome}</option>`;
        }
        $("#materia").html(html);

    } catch (error) {
        console.error("Erro - preencherNomesMaterias:", error);
    }
}
async function executarRequisicao(rota, parametros, tipo) {
    console.log(localStorage.getItem("token"))
    try {
        const response = await $.ajax({
            type: tipo,
            url: BaseUrlBack + rota,
            headers: {
                "Authorization": "Bearer " + localStorage.getItem("token")
            },
            crossDomain: true,
            data: JSON.stringify(parametros),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
        });

        console.log("Resposta da API:", response);
        return response;

    } catch (error) {
        console.error("Erro na requisição:", error);
        throw error; // Propaga o erro para quem chamar essa função
    }
}
async function getAlunos() {
    return await executarRequisicao(`api/Agente/buscarAlunosPorEducador/${localStorage.getItem("idAgente")}`, "", "GET");
}