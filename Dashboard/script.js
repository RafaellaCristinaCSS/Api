function redirecionarCadastroUsuario() {
    window.location.href = BaseUrlFront + "Codigo/FRONT/Cadastro/Cadastro.html?Educador=" + localStorage.getItem("idAgente");
}
async function preencherAlunosVinculados() {
    try {
        const alunos = await getAlunos();
        preencherDependentesVinculados(alunos);
        preencherPainelGeralDependentesVinculados(alunos)
        carregarGraficosPorAluno()
    } catch (error) {
        console.error("Erro ao carregar alunos vinculados:", error);
    }
}

function preencherDependentesVinculados(alunos) {
    let html = '';
    for (const aluno of alunos) {
        html += `<li><img src="imagens/daniel.png" class="rounded-circle" width="30"> ${aluno.nome} </li>`;
    }
    $("#dependentesVinculados").html(html);
}
async function carregarGraficosPorAluno() {
    const dados = await executarRequisicao(`api/relatorio/relatorio-desempenho/educador/${parseInt(localStorage.getItem('idEducador'))}`, "", "GET");
    carregarGraficosEducador(dados);
}
async function carregarGraficosEducador(dadosTodos) {
    const container = document.getElementById('graficosContainer');
    container.innerHTML = '';

    if (dadosTodos.length == 0) {
        container.innerHTML = '<h4 style="">Nenhuma Estatistica Registrada</h4>';
    } else {


        dadosTodos.forEach((aluno) => {
            const divAluno = document.createElement('div');
            divAluno.className = 'grafico-aluno card p-2'; // padding menor

            const idDesempenho = `graficoDesempenho_${aluno.idAluno}`;
            const idGeral = `graficoGeral_${aluno.idAluno}`;

            divAluno.innerHTML = `
        <h6 class="btn" onclick="carregarEstatistica(${aluno.idAluno}, '${aluno.nomeAluno}')" style="font-size: 0.9rem;">${aluno.nomeAluno}</h6>
        
        <div style="display: flex; justify-content: center; gap: 0.5rem;">
        <canvas id="${idDesempenho}" style="max-width: 240px; height: 180px;"></canvas>
        <canvas id="${idGeral}" style="max-width: 240px; height: 180px;"></canvas>
        </div>
        
        `;

            divAluno.style.minWidth = '520px'; // reduzido
            divAluno.style.flex = '0 0 auto';

            container.appendChild(divAluno);

            gerarGraficos(aluno.relatorio, idDesempenho, idGeral);
        });
    }
}

function preencherPainelGeralDependentesVinculados(alunos) {
    let html = '';
    for (const aluno of alunos) {
        html += `<div class="text-center">
                            <span class="dot" style="background-color: #4c6ef5;"></span>
                            <p>${aluno.nome}</p>
                            <p>40%</p>
                        </div>`;
    }
    $("#graficoAlunosGeral").html(html);
}
async function gerarGraficos(dados, idCanvasDesempenho = null, idCanvasGeral = null) {
    if (!dados || dados.length === 0) return;

    const materias = dados.map(m => m.nomeMateria.trim());
    const porcentagens = dados.map(m => ((m.totalPontuacaoObtida / m.totalPontuacaoPossivel) * 100).toFixed(1));

    const ctxDesempenho = document.getElementById(idCanvasDesempenho)?.getContext('2d');
    if (ctxDesempenho) {
        new Chart(ctxDesempenho, {
            type: 'bar',
            data: {
                labels: materias,
                datasets: [{
                    label: 'Desempenho (%)',
                    data: porcentagens,
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: { beginAtZero: true, max: 100 }
                },
                responsive: true
            }
        });
    }
    if (idCanvasGeral) {
        const totalObtido = dados.reduce((soma, m) => soma + m.totalPontuacaoObtida, 0);
        const totalPossivel = dados.reduce((soma, m) => soma + m.totalPontuacaoPossivel, 0);

        const ctxGeral = document.getElementById(idCanvasGeral)?.getContext('2d');
        if (ctxGeral) {
            new Chart(ctxGeral, {
                type: 'doughnut',
                data: {
                    labels: ['Pontuação Obtida', 'Pontuação Faltante'],
                    datasets: [{
                        data: [totalObtido, totalPossivel - totalObtido],
                        backgroundColor: ['#4CAF50', '#E0E0E0'],
                    }]
                },
                options: {
                    responsive: true
                }
            });
        }
    }
}
