async function alterarNivelEducador(idEducador, nivelVisibilidade){
    var parametros ={
        "nivelVisibilidade": nivelVisibilidade
    }

    let resposta = await executarRequisicao(`api/Agente/educador/${idEducador}/editarNivelVisibilidade`, parametros, "PUT")
    console.log(resposta)
    preencherListEducadores()
    alert("Mudança realizada com sucesso!");
}
async function preencherListEducadores() {
    try {
        const educadores = await buscarEducadores()
        let html = '';

        for (const educador of educadores) {

            html += `<li>
                       <div class="educador-info">
                           <img src="imagens/maria.png" alt="Maria" class="educador-icon">
                           <span>${educador.nomeEducador}</span>
                       </div>
                       <div class="nivel-container">
                           <span class="nivel">Nível ${educador.nivelVisibilidade}</span>
                           <button class="edit"><i class="fa fa-edit"></i></button>
                           <button class="mover alterarNivelEducador" onclick='alterarNivelEducador(${educador.idEducador}, ${(educador.nivelVisibilidade == 1 ? 2 : 1)})' >Mover para Nível ${(educador.nivelVisibilidade == 1 ? 2 : 1)}</button>
                       </div>
                   </li>`;
        }
        $("#listEducadores").html(html);

    } catch (error) {
        console.error("Erro ao carregar educadores:", error);
    }
}