using Microsoft.AspNetCore.Mvc;
using CaseItau.Domain.Interfaces;
using CaseItau.Infrastructure.Classification;

namespace CaseItau.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassificationController : ControllerBase
    {
        private readonly ICategoryClassifier _classifier;

        public ClassificationController()
        {
            // Melhoria futura: Mover para banco ou arquivo JSON
            var categorias = new Dictionary<string, IEnumerable<string>>
            {
                ["imobiliário"] = new[] { "credito imobiliario", "casa", "apartamento" },
                ["seguros"] = new[] { "resgate", "capitalizacao", "socorro" },
                ["cobrança"] = new[] { "fatura", "cobranca", "valor", "indevido" },
                ["acesso"] = new[] { "acessar", "login", "senha" },
                ["aplicativo"] = new[] { "app", "aplicativo", "travando", "erro" },
                ["fraude"] = new[] { "fatura", "nao reconhece divida", "fraude" }
            };

            _classifier = new KeywordClassifier(categorias);
        }

        /// <summary>
        /// Classifica automaticamente o texto de uma reclamação em uma ou mais categorias.
        /// </summary>
        /// <param name="request">Objeto JSON com o campo "reclamacao".</param>
        /// <returns>Lista de categorias detectadas com nível de confiança.</returns>
        [HttpPost]
        public IActionResult Classify([FromBody] ClassifyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Reclamacao))
                return BadRequest(new { message = "O campo 'reclamacao' é obrigatório." });

            var resultado = _classifier.Classify(request.Reclamacao);

            return Ok(resultado);
        }

        public class ClassifyRequest
        {
            public string Reclamacao { get; set; } = string.Empty;
        }
    }
}