function exibirPerfilAluno() {
    $("#main").load(`./Perfil/index.html`);
    executarScriptsEspecificos("Perfil")
}
async function getDadosAluno() {
    const resposta = await executarRequisicao(`api/Agente/aluno/${parseInt(localStorage.getItem("idAluno"))}`, "", "GET");
    if (!resposta) {
        console.error("Aluno não encontrado");
        return null;
    } else {
        $("#estiloAprendizagem").val((resposta.estiloAprendizagem ? resposta.estiloAprendizagem : ""));
        $("#dataNascimento").val((resposta.dataNascimento ? resposta.dataNascimento : ""));
        $("#generoLiterarioFavorito").val((resposta.generoLiterarioFavorito ? resposta.generoLiterarioFavorito : ""));
        $("#modeloEnsino").val((resposta.modeloEnsino ? resposta.modeloEnsino : ""));
        $("#horasEstudo").val((resposta.horasEstudo ? resposta.horasEstudo : ""));
        $("#hobbies").val((resposta.hobbies ? resposta.hobbies : ""));
        $("#informacaoAdicional").val((resposta.informacaoAdicional ? resposta.informacaoAdicional : ""));
    }
}
async function enviarInformacoesAluno() {
    const dados = {
        "idAgente": parseInt(localStorage.getItem("idAgente")),
        "estiloAprendizagem": document.getElementById("estiloAprendizagem").value,
        "generoLiterarioFavorito": document.getElementById("generoLiterarioFavorito").value,
        "modeloEnsino": document.getElementById("modeloEnsino").value,
        "horasEstudo": document.getElementById("horasEstudo").value,
        "hobbies": document.getElementById("hobbies").value,
        "informacaoAdicional": document.getElementById("informacaoAdicional").value
    };
    await executarRequisicao(`api/Agente/complementar-informacoes`, dados, "POST");
    alert("Informações salvas com sucesso!");
}