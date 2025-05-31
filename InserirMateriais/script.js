function abrirModalCriarMateria() {

    $('#modalCriarMateria').modal('show');
}
function clickInputFile() {
    $("#fileInput").trigger("click")
}
function carregarTextoInseridoPorArquivo(event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const text = e.target.result;
            document.getElementById('texto').textContent = text;
        };
        reader.readAsText(file);

    }
}
function criarMaterial() {
    let texto = $("#texto").val();
    let idMateria = $("#materia").val();
    if (texto && idMateria) {
        try {
            var parametros = {
                "conteudo": texto,
                "idAgente": parseInt(localStorage.idEducador),
                "idMateria": idMateria
            }
            executarRequisicao("api/materiais", parametros, "POST")
            alert("O material foi criado com sucesso!");
        } catch {
            alert("Ocorreu um erro interno. Por favor, tente novamente mais tarde.");
        }
    } else {
        alert("É necessário preencher o texto e a matéria relacionada.");
    }
}

function criarMateria() {
    let materia = $('#nomeMateria').val();
    let inputImagem = document.getElementById('imagemMateria');
    let arquivoImagem = inputImagem.files[0];

    if (!materia) {
        alert("É necessário preencher o nome da matéria.");
        return;
    }

    if (!arquivoImagem) {
        alert("É necessário selecionar uma imagem.");
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        const imagemBase64 = e.target.result;

        const img = new Image();
        img.onload = function () {
            const larguraMaxima = 800;
            const alturaMaxima = 800;

            if (img.width > larguraMaxima || img.height > alturaMaxima) {
                alert(`A imagem excede o tamanho permitido (${larguraMaxima}x${alturaMaxima}px).`);
                return;
            }

            const parametros = {
                nome: materia,
                imagem: imagemBase64
            };

            try {
                executarRequisicao("api/materia", parametros, "POST");

                $('#modalCriarMateria').modal('hide');
                alert("Matéria criada com sucesso!");
                preencherNomesMaterias();
            } catch {
                alert("Ocorreu um erro interno. Por favor, tente novamente mais tarde.");
            }
        };

        img.src = imagemBase64;
    };

    reader.readAsDataURL(arquivoImagem);
}


