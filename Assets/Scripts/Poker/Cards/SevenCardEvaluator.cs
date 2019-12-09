namespace Poker.Cards {
    public class SevenCardEvaluator {
    
        public Hand BestHand { get; private set; }
    
        public SevenCardEvaluator(Card c0, Card c1, Card c2, Card c3, Card c4, Card c5, Card c6) {
            CheckAllSubsets(new[] {c0, c1, c2, c3, c4, c5, c6}, 5);
        }
    
        private void CheckAllSubsets(Card[] set, int subsetSize) {
            CheckAllSubsetsRecursive(set, new Card[subsetSize], 0, 0);
        }

        private void CheckAllSubsetsRecursive(Card[] set, Card[] subset, int index, int start) {
            if(index == subset.Length) {
                EvaluateHand(new Hand(subset[0], subset[1], subset[2], subset[3], subset[4]));
                return;
            }

            for(int i = start; i < set.Length; i++) {
                subset[index] = set[i];
                CheckAllSubsetsRecursive(set, subset, index + 1, i + 1);
            }
        }

        private void EvaluateHand(Hand hand) {
            if (BestHand == null) {
                BestHand = hand;
                return;
            }
        
            if (BestHand.CompareTo(hand) < 0) {
                BestHand = hand;
            }
        }
    }
}