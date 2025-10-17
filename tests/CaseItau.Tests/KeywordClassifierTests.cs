using System.Collections.Generic;
using CaseItau.Domain.Interfaces;
using CaseItau.Infrastructure.Classification;
using Xunit;

namespace CaseItau.Tests
{
    public class KeywordClassifierTests
    {
        [Fact]
        public void Deve_Classificar_Reclamacao_Corretamente()
        {
            var categorias = new Dictionary<string, IEnumerable<string>>
            {
                ["imobiliário"] = new[] { "credito imobiliario", "casa", "apartamento" },
                ["seguros"] = new[] { "resgate", "capitalizacao", "socorro" },
                ["cobrança"] = new[] { "fatura", "cobranca", "valor", "indevido" },
                ["acesso"] = new[] { "acessar", "login", "senha" },
                ["aplicativo"] = new[] { "app", "aplicativo", "travando", "erro" },
                ["fraude"] = new[] { "fatura", "nao reconhece divida", "fraude" }
            };

            ICategoryClassifier classifier = new KeywordClassifier(categorias);

            var reclamacao = "Estou com problemas para acessar minha conta e o aplicativo está travando muito.";

            var resultado = classifier.Classify(reclamacao);

            Assert.NotEmpty(resultado);
            Assert.Contains(resultado, r => r.Category == "acesso");
            Assert.Contains(resultado, r => r.Category == "aplicativo");
        }
    }
}
