async function preencherListMateriais() {
    try {
        const response = await fetch('https://localhost:7034/api/materiais');
        const materiais = await response.json();
        let html = '';

        for (const material of materiais) {
            html += `
                <li>
                    <span class="material-nome">${material.conteudo}</span>
                    <div class="material-actions">
                        <button class="btn edit" onclick="editarMaterial(${material.id})">
                            <i class="fa fa-edit"></i>
                        </button>
                        <button class="btn delete" onclick="excluirMaterial(${material.id})">
                            <i class="fa fa-trash"></i>
                        </button>
                    </div>
                </li>
            `;
        }

        document.getElementById("listMateriais").innerHTML = html;
    } catch (error) {
        console.error("Erro ao carregar materiais:", error);
    }
}

async function excluirMaterial(id) {
    if (confirm("Tem certeza que deseja excluir este material?")) {
        const response = await fetch(`https://localhost:7034/api/materiais/${id}`, {
            method: 'DELETE'
        });
        if (response.ok) {
            preencherListMateriais();
        } else {
            alert("Erro ao excluir material.");
        }
    }
}

async function editarMaterial(id) {
    const novoConteudo = prompt("Digite o novo conteúdo do material:");
    if (!novoConteudo || novoConteudo.trim() === "") {
        alert("O conteúdo não pode estar vazio.");
        return;
    }

    const dados = { conteudo: novoConteudo };

    try {
        const response = await fetch(`https://localhost:7034/api/materiais/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dados)
        });

        if (response.ok) {
            alert("Material atualizado com sucesso!");
            preencherListMateriais();
        } else {
            const erro = await response.text();
            alert("Erro ao editar material: " + erro);
        }
    } catch (error) {
        console.error("Erro ao editar material:", error);
        alert("Erro inesperado.");
    }
}

preencherListMateriais();
