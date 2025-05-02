using System.Linq;
using System.Text.Json;

public class LogProbsDumper
{
   // ChatTokenLogProbabilityDetails vs. OpenAI.Chat.ChatTokenTopLogProbabilityDetails
   public static void DumpRawLogProbs(List<OpenAI.Chat.ChatTokenLogProbabilityDetails> tokenProbs, string path)
   {
      using (var writer = new StreamWriter(path))
      {
         try
         {
            if (tokenProbs != null)
            {
               Console.WriteLine("Content Token Log Probabilities:");

               for (int tokenIndex = 0; tokenIndex < tokenProbs.Count; tokenIndex++)
               {
                  Console.WriteLine($"Token Number: {tokenIndex + 1}");
                  var topLogProbs = tokenProbs[tokenIndex];

                  if (topLogProbs != null)
                  {
                     // output message to stderr if fewer than 20 log probabilities are returned
                     if (topLogProbs.TopLogProbabilities.Count < 20)
                     {
                        Console.WriteLine($"✅ 🚨🚨🚨 Warning: Fewer than 20 log probabilities returned. Only {topLogProbs.TopLogProbabilities.Count} log probabilities returned. ❌❌❌");
                     }
                     var logProbMassTop20 = CombineLogProbs(topLogProbs.TopLogProbabilities.Select(x => (double)x.LogProbability).ToArray());
                     var probMassTop20 = topLogProbs.TopLogProbabilities.Sum(x => LogProbToProb(x.LogProbability, 0.0000001d));
                     var chosenToken = topLogProbs.Token.Replace("\n", "↵").Replace("\r", "␍").Replace("\t", "⇥");
                     Console.WriteLine($"The Chosen Token:【{chosenToken}】[LogProbability = {topLogProbs.LogProbability:f5} && Probability = {LogProbToProb(topLogProbs.LogProbability, 0.0000001d):f3}] ⟬ Probably Mass of Top 20 = {probMassTop20:f5}, LogProb Mass of Top 20 = {logProbMassTop20:f5} <{LogProbToProb(logProbMassTop20, 0.0000001d):f3}> ⟭");

                     /*
                        ｟ ｠ (Halfwidth White Square Bracket)
                        『 』 (Corner Bracket)
                        「 」 (Corner Bracket)
                        【 】 (Black Lenticular Bracket)
                        〖 〗 (White Lenticular Bracket)
                        ⦑ ⦒ (Left and Right Square Bracket with Underbar)
                        ⧼ ⧽ (Left and Right Angle Bracket with Dot)
                        ⸢ ⸣ (Top Half Bracket)
                        ⸤ ⸥ (Bottom Half Bracket)
                        ⟬ ⟭ (Mathematical Left and Right White Tortoise Shell Bracket)
                        ⦋ ⦌ (Mathematical Left and Right Double Angle Bracket)
                     */
                     var found = false;
                     var nth = 1;
                     foreach (var logProbEntry in topLogProbs.TopLogProbabilities)
                     {
                        if (!found) found = logProbEntry.Token == topLogProbs.Token;
                        var isChosenOne = logProbEntry.Token == topLogProbs.Token ? $"‽ ✅ = {nth}" : string.Empty;
                        var displayToken = logProbEntry.Token.Replace("\n", "↵").Replace("\r", "␍").Replace("\t", "⇥");
                        var displayChosenToken = topLogProbs.Token.Replace("\n", "↵").Replace("\r", "␍").Replace("\t", "⇥");
                        // Console.WriteLine($"[{nth:2}] ({} LogProb: {logProbEntry.LogProbability} ... ({LogProbToProb(topLogProbs.LogProbability)}) vs. ⟬{displayToken}⟭ ({LogProbToProb(logProbEntry.LogProbability)})) {}");
                        Console.WriteLine($"[{nth:00}] ⟬{displayToken}⟭ [LogProbability = {logProbEntry.LogProbability:f6} && Probability = {LogProbToProb(logProbEntry.LogProbability):f3}] {isChosenOne}");
                        nth++;
                     }
                     if (!found)
                     {
                        Console.WriteLine($"‽ ✅ 🚨🚨 The Chosen Token: '{topLogProbs.Token}' not found in TopLogProbabilities. ❌");
                     }
                  }
               }
            }
            else
            {
               Console.WriteLine("Log probabilities deserialized to null.");
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Error dumping log probabilities: {ex.Message}");
         }
      }
   }

   public static double LogProbToProb(double logProb, double minMeaningful = 0.0001d)
   {
      // if the calculated probability is less than the minimum meaningful value, return 0
      var prob = Math.Exp(logProb); // e ^ logProb

      if (prob < minMeaningful)
      {
         return 0d;
      }
      return prob;
   }

   public static double ProbToLogProb(double prob)
   {
      if (prob <= 0 || prob > 1)
      {
         throw new ArgumentOutOfRangeException("Probability must be between 0 and 1.");
      }

      return Math.Log(prob); // base e
   }

   public static double CombineLogProbs(params double[] logProbs)
   {
      if (logProbs == null || logProbs.Length == 0)
      {
         return double.NegativeInfinity; // Or handle as appropriate for your case
      }

      if (logProbs.Length == 1)
      {
         return logProbs[0];
      }

      // Find the maximum log probability for numerical stability
      double maxLogProb = logProbs.Max();

      // Calculate the logsumexp
      double sumOfExponentials = 0.0;
      foreach (double logProb in logProbs)
      {
         sumOfExponentials += Math.Exp(logProb - maxLogProb);
      }

      return maxLogProb + Math.Log(sumOfExponentials);
   }
   public static void ExampleUsage()
   {
      // Example log probabilities
      double logProb1 = Math.Log(0.5); // Equivalent to probability 0.5
      double logProb2 = Math.Log(0.3); // Equivalent to probability 0.3
      double logProb3 = Math.Log(0.8);

      // Combine the log probabilities
      double combinedLogProb = CombineLogProbs(logProb1, logProb2, logProb3);

      // Convert the combined log probability back to a regular probability
      double combinedProbability = LogProbToProb(combinedLogProb);

      Console.WriteLine($"Log Prob 1: {logProb1}");
      Console.WriteLine($"Log Prob 2: {logProb2}");
      Console.WriteLine($"Log Prob 3: {logProb3}");
      Console.WriteLine($"Combined Log Prob: {combinedLogProb}");
      Console.WriteLine($"Combined Probability: {combinedProbability}");

      //Example to show how the result matches the multiplication of regular probabilities.
      double prob1 = LogProbToProb(logProb1);
      double prob2 = LogProbToProb(logProb2);
      double prob3 = LogProbToProb(logProb3);
      double multipliedProbs = prob1 * prob2 * prob3;
      double multipliedLogProbs = ProbToLogProb(multipliedProbs);

      Console.WriteLine($"Multiplied Probabilities: {multipliedProbs}");
      Console.WriteLine($"Log of multiplied Probabilities: {multipliedLogProbs}");
   }
}
