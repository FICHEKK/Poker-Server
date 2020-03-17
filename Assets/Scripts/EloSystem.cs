using System;
using System.Collections.Generic;

/// <summary>
/// Implementation of a classic chess ELO system that offers methods
/// for calculating expected score and new player's rating value.
/// </summary>
public static class EloSystem
{
    /// <summary>
    /// Defines the rating difference in which the higher rated player
    /// is 10 times more likely to win. Each subsequent multiple of this
    /// constant will increase the probability by a factor of 10.
    /// </summary>
    private const double TenFactor = 400;
        
    /// <summary> Defines the maximum possible rating gain/loss for a single match. </summary>
    private const double KFactor = 32;

    /// <summary> Defines the score gained by winning a match. </summary>
    private const double WinScore = 1;

    /// <summary> Defines the score gained by losing a match. </summary>
    private const double LossScore = 0;

    /// <summary>
    /// Calculates new ELO rating value for each rating provided in the list. List is expected
    /// to be sorted in descending order (meaning the first element is the rating value of a player
    /// that took the first place, second element is the rating of a player that took second place
    /// and so on). The original list will not be modified.
    /// </summary>
    public static List<int> CalculateNewRatings(List<int> sortedRatings)
    {
        var newRatings = new List<int>(sortedRatings.Count);

        for (int position = 0; position < sortedRatings.Count; position++)
            newRatings.Add(CalculateNewRatingForPosition(position, sortedRatings));

        return newRatings;
    }

    /// <summary>
    /// Calculates the new ELO rating for the player at the specified position in a list
    /// of sorted ratings.
    /// </summary>
    public static int CalculateNewRatingForPosition(int position, List<int> sortedRatings)
    {
        double score = 0;

        for (int i = 0; i < position; i++)
            score += LossScore - CalculateExpectedScore(sortedRatings[position], sortedRatings[i]);

        for (int i = position + 1; i < sortedRatings.Count; i++)
            score += WinScore - CalculateExpectedScore(sortedRatings[position], sortedRatings[i]);

        score /= sortedRatings.Count - 1;
        return sortedRatings[position] + (int) Math.Round(KFactor * score);
    }
        
    /// <summary>
    /// Calculates and returns the probability (value from 0 to 1) of me winning the match.
    /// </summary>
    public static double CalculateExpectedScore(int myRating, int opponentRating)
    {
        return 1 / (1 + Math.Pow(10, (opponentRating - myRating) / TenFactor));
    }
}