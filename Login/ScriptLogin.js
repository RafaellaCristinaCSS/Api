$(document).ready(() => {
    $("#idTipoAgente").change(() => {
        verificarTipoAgente()
    })
    $("#btnLogin").click(function () {
        var loginData = {
            Login: $("#login").val(),
            Senha: $("#senha").val()
        };

        $.ajax({
            url: "https://localhost:7034/api/agente/login",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(loginData),
            success: function (response) {
                const token = response.token;
                localStorage.setItem("token", token);
                const decoded = jwt_decode(token);

                console.log("TOKEN ::", token);
                localStorage.setItem("tipo", decoded.Tipo);
                localStorage.setItem("nivel", decoded.Nivel);
                localStorage.setItem("idAgente", decoded.Id);
                localStorage.setItem("idEducador", response.idEducador);
                localStorage.setItem("idAluno", response.idAluno);
                localStorage.setItem("login", decoded.unique_name);

                window.location.href = "https://rafaellacristinacss.github.io/ScholaAi/Codigo/FRONT/ScholaAi.html";

            }, error: function (xhr) {
                alert("Erro: " + xhr.responseText);
            }
        });

    });
    $("#novaConta").click(function () {
        window.location.href = "https://rafaellacristinacss.github.io/ScholaAi/Codigo/FRONT/Cadastro/Cadastro.html";
    });
})