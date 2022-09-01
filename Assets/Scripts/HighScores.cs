using System.Collections.Generic;
using System.Linq;

public class HighScores
{
    private static HighScores _instance;
    private List<int> _timeScores = new List<int>();
    private List<int> _survivalScores = new List<int>();

    private HighScores()
    {
    }

    public void AddTimeScore(int timeScore)
    {
        _timeScores = AddScore(timeScore, _timeScores);
    }

    public void AddSurvivalScore(int survivalScore)
    {
        _survivalScores = AddScore(survivalScore, _survivalScores);
    }

    private List<int> AddScore(int score, List<int> scores)
    {
        scores.Add(score);
        scores = scores.OrderByDescending(i => i).ToList();
        if (scores.Count > 5)
        {
            scores.Remove(scores.Last());
        }

        return scores;
    }

    public IEnumerable<int> GetTimeScores()
    {
        return _timeScores;
    }

    public IEnumerable<int> GetSurvivalScores()
    {
        return _survivalScores;
    }

    public static HighScores Instance => _instance ?? (_instance = new HighScores());
}