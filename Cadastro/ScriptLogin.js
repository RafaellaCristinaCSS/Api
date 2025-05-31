

$(document).ready(() => {
    verificarTipoAgente()
    preencherLoginsEducadores()

    $("#idTipoAgente").change(() => {
        verificarTipoAgente()
    })
    $("#cadastrar").click(() => {
        realizarCadastro()
    })
})
function setarValores() {

    let urlParams = new URLSearchParams(window.location.search);
    let idEducador = urlParams.get("Educador");
    if (idEducador) {
        $("#idTipoAgente").val(1)
        $("#idEducador").val(idEducador)
    }
}
async function realizarCadastro() {
    var parametros = {
        "login": $("#login").val(),
        "senha": $("#senha").val(),
        "nome": $("#nome").val(),
        "idTipoAgente": parseInt($("#idTipoAgente").val()),
        "dataNascimento": converterData($("#dataNascimento").val()),
        "idEducador": parseInt($("#idEducador").val())
    }
    await executarRequisicao("api/Agente", parametros, "POST")
    window.location.href = "https://rafaellacristinacss.github.io/ScholaAi/Codigo/FRONT/ScholaAi.html";
}
async function preencherLoginsEducadores() {
    try {
        const educadores = await executarRequisicao("api/Agente/buscarLoginEducadores", "", "GET");

        let html = '<option value="">Selecione um educador</option>';
        for (const educador of educadores) {
            html += `<option value="${educador.id}">${educador.login}</option>`;
        }
        $("#idEducador").html(html);
        setarValores()
    } catch (error) {
        console.error("Erro ao carregar educadores:", error);
    }
}
function verificarTipoAgente() {
    if ($("#idTipoAgente").val() == 1) {
        $("#dataNascimento").closest("div").show()
        $("#idEducador").closest("div").show()
    } else {
        $("#dataNascimento").closest("div").hide()
        $("#idEducador").closest("div").hide()
    }
}


function converterData(data) {
    if (data) {
        return `${data}T00:00:00`;
    } else {
        return '2000-05-20T00:00:00'
    }
}