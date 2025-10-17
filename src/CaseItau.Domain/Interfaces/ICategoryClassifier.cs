using System.Collections.Generic;

namespace CaseItau.Domain.Interfaces
{
    public record CategoryMatch(string Category, double Confidence, List<string> MatchedKeywords);

    public interface ICategoryClassifier
    {
        IReadOnlyList<CategoryMatch> Classify(string text, int maxResults = 5, double minConfidence = 0.10);
    }
}
