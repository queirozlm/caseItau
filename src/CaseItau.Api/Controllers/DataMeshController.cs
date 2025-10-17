using Microsoft.AspNetCore.Mvc;

namespace CaseItau.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataMeshController : ControllerBase
    {
        // GET api/DataMesh/{cpf}
        [HttpGet("{cpf}")]
        public IActionResult GetDadosClienteByCpf(string cpf)
        {
            // Simulação: retorna dados "reais" para o CPF especificado,
            // e um dado genérico para qualquer outro cpf.
            if (cpf == "48331645820")
            {
                return Ok(new
                {
                    cpf,
                    nome = "Lucas Queiroz",
                    produtos = new[] { "Conta Corrente", "Cartão Platinum", "Seguro Vida" },
                    ultimaReclamacao = "App travando na tela de login",
                    relacionamento = "Cliente desde 2015",
                    risco = "Baixo",
                    score = 820
                });
            }

            // fallback genérico
            return Ok(new
            {
                cpf,
                nome = $"Cliente {cpf.Substring(Math.Max(0, cpf.Length - 4))}",
                produtos = new[] { "Conta Corrente" },
                ultimaReclamacao = (string?)null,
                relacionamento = "Cliente (demo)",
                risco = "Indeterminado",
                score = 600
            });
        }

        // GET api/DataMesh/byName?name=Joao%20Silva
        [HttpGet("byName")]
        public IActionResult GetDadosClienteByName([FromQuery] string name)
        {
            // Simula retorno coerente com o nome informado:
            var normalizedName = string.IsNullOrWhiteSpace(name) ? "Cliente Demo" : name.Trim();

            return Ok(new
            {
                cpf = (string?)null,
                nome = normalizedName,
                produtos = new[] { "Conta Corrente", "Cartão" },
                ultimaReclamacao = "Nenhuma registrada (demo)",
                relacionamento = "Cliente (demo)",
                risco = "Indeterminado",
                score = 700
            });
        }
    }
}
