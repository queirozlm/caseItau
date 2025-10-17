using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CaseItau.Domain.Interfaces;

namespace CaseItau.Infrastructure.Classification
{
    // Essa classe é o coração da classificação — ela pega uma reclamação,
    // varre o texto, e tenta entender em qual categoria ela se encaixa.
    // O "truque" aqui é fazer isso apenas com palavras-chave e expressões conhecidas.
    public class KeywordClassifier : ICategoryClassifier
    {
        // Criei essa classe interna só pra organizar as informações das palavras-chave.
        // Ela guarda a palavra em si, o peso (pra frases mais relevantes)
        // e o regex que ajuda a procurar isso dentro do texto.
        private class KeywordInfo
        {
            public string Keyword { get; init; } = "";
            public double Weight { get; init; }
            public Regex Pattern { get; init; } = null!;
        }

        // Aqui ficam as palavras separadas por categoria — tipo “aplicativo”, “acesso”, etc.
        private readonly Dictionary<string, List<KeywordInfo>> _byCategory;

        // Guarda o peso total de cada categoria (serve pra calcular a confiança depois)
        private readonly Dictionary<string, double> _categoryTotalWeight;

        // Esses dois valores definem o “peso” de uma palavra isolada ou de uma frase composta.
        private readonly double _wordWeight;
        private readonly double _phraseWeight;

        // O construtor recebe o dicionário de categorias e palavras,
        // e já monta os padrões de regex pra facilitar as buscas depois.
        public KeywordClassifier(IDictionary<string, IEnumerable<string>> categories, double wordWeight = 1.0, double phraseWeight = 2.0)
        {
            _wordWeight = wordWeight;
            _phraseWeight = phraseWeight;

            _byCategory = new(StringComparer.OrdinalIgnoreCase);
            _categoryTotalWeight = new(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in categories)
            {
                var category = kv.Key;
                var list = new List<KeywordInfo>();

                // Pra cada palavra ou frase dessa categoria...
                foreach (var raw in kv.Value ?? Enumerable.Empty<string>())
                {
                    var normalized = Normalize(raw);
                    if (string.IsNullOrWhiteSpace(normalized)) continue;

                    // Se for uma frase (tem espaço), damos mais peso.
                    bool isPhrase = Regex.IsMatch(normalized, @"\s+");
                    double weight = isPhrase ? _phraseWeight : _wordWeight;

                    // Aqui o regex é gerado pra identificar a palavra no texto de forma “inteligente”.
                    // Ele respeita fronteiras de palavra pra não confundir “casa” com “casado”, por exemplo.
                    string pattern = isPhrase
                        ? @"\b" + string.Join(@"\s+", Regex.Split(normalized, @"\s+").Select(Regex.Escape)) + @"\b"
                        : @"\b" + Regex.Escape(normalized) + @"\b";

                    list.Add(new KeywordInfo
                    {
                        Keyword = normalized,
                        Weight = weight,
                        Pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant)
                    });
                }

                // Terminando de montar a categoria, somamos o peso total dela.
                _byCategory[category] = list;
                _categoryTotalWeight[category] = list.Sum(k => k.Weight);
            }
        }

        // Essa é a parte que realmente “classifica” o texto.
        public IReadOnlyList<CategoryMatch> Classify(string text, int maxResults = 5, double minConfidence = 0.10)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<CategoryMatch>();

            var normText = Normalize(text);
            var results = new List<CategoryMatch>();

            // Vamos varrer cada categoria e ver quantas palavras dela aparecem na reclamação.
            foreach (var kv in _byCategory)
            {
                var category = kv.Key;
                var keywords = kv.Value;
                double totalWeight = _categoryTotalWeight.GetValueOrDefault(category);

                if (totalWeight <= 0) continue;

                double matchedWeight = 0.0;
                var matchedKeywords = new HashSet<string>();

                // Cada vez que uma palavra ou frase é encontrada, somamos seu peso.
                foreach (var k in keywords)
                {
                    var matches = k.Pattern.Matches(normText);
                    if (matches.Count > 0)
                    {
                        matchedWeight += k.Weight * matches.Count;
                        matchedKeywords.Add(k.Keyword);
                    }
                }

                // A confiança é o quanto de peso foi encontrado comparado ao total possível.
                double confidence = matchedWeight / totalWeight;
                if (confidence >= minConfidence)
                {
                    results.Add(new CategoryMatch(category, Math.Round(confidence, 4), matchedKeywords.ToList()));
                }
            }

            // No final, ordenamos as categorias mais prováveis primeiro.
            return results
                .OrderByDescending(r => r.Confidence)
                .ThenBy(r => r.Category)
                .Take(maxResults)
                .ToList();
        }

        // Essa função é quase invisível, mas essencial.
        // Ela transforma o texto pra um formato “neutro”, sem acentos, maiúsculas, etc.
        // Assim, “Aplicativo”, “aplicativo” ou “aplicátivo” são tratados da mesma forma.
        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var s = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in s)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            var result = sb.ToString().Normalize(NormalizationForm.FormC);
            result = result.Replace('ç', 'c').Replace('ñ', 'n');
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }
    }
}