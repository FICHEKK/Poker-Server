using Poker.Cards;

namespace Poker {
    public class Dealer {
        private readonly Deck _deck = new Deck();
        private readonly Table _table;

        public Dealer(Table table) {
            _table = table;
        }

        public void StartRound() {
            _deck.Shuffle();
        }

        public void DealCards() {
            for (int position = 0; position < _table.MaxPlayers; position++) {
                if(_table.IsSeatEmpty(position)) continue;
                
                _table.Signal(position, ServerResponse.Hand);
                _table.Signal(position, _deck.GetNextCard().ToString());
                _table.Signal(position, _deck.GetNextCard().ToString());
            }
        }

        public void ShowFlop() {
            _table.Broadcast(ServerResponse.Flop);
            _table.Broadcast(_deck.GetNextCard().ToString());
            _table.Broadcast(_deck.GetNextCard().ToString());
            _table.Broadcast(_deck.GetNextCard().ToString());
        }

        public void ShowTurn() {
            _table.Broadcast(ServerResponse.Turn);
            _table.Broadcast(_deck.GetNextCard().ToString());
        }

        public void ShowRiver() {
            _table.Broadcast(ServerResponse.River);
            _table.Broadcast(_deck.GetNextCard().ToString());
        }

        public void FinishRound() {
            _table.IncrementButtonPosition();
            _table.Broadcast(ServerResponse.RoundFinished);
        }
        
//    public TextMeshProUGUI myBestHandText;
//    public TextMeshProUGUI opponentBestHandText;
//
//    public Image flopCard1;
//    public Image flopCard2;
//    public Image flopCard3;
//    public Image turnCard;
//    public Image riverCard;
//    
//    public Image handCard1;
//    public Image handCard2;
//
//    public Image opponentCard1;
//    public Image opponentCard2;
//
//    private Deck _deck = new Deck();
//
//    public void DealHand() {
//        _deck.Shuffle();
//
//        Card hc1 = _deck.GetNextCard();
//        Card hc2 = _deck.GetNextCard();
//
//        Card ohc1 = _deck.GetNextCard();
//        Card ohc2 = _deck.GetNextCard();
//        
//        Card fc1 = _deck.GetNextCard();
//        Card fc2 = _deck.GetNextCard();
//        Card fc3 = _deck.GetNextCard();
//        Card tc = _deck.GetNextCard();
//        Card rc = _deck.GetNextCard();
//
//        handCard1.sprite = LoadSprite(@"Assets\Graphics\Cards\" + hc1 + ".png");
//        handCard2.sprite = LoadSprite(@"Assets\Graphics\Cards\" + hc2 + ".png");
//        
//        opponentCard1.sprite = LoadSprite(@"Assets\Graphics\Cards\" + ohc1 + ".png");
//        opponentCard2.sprite = LoadSprite(@"Assets\Graphics\Cards\" + ohc2 + ".png");
//        
//        flopCard1.sprite = LoadSprite(@"Assets\Graphics\Cards\" + fc1 + ".png");
//        flopCard2.sprite = LoadSprite(@"Assets\Graphics\Cards\" + fc2 + ".png");
//        flopCard3.sprite = LoadSprite(@"Assets\Graphics\Cards\" + fc3 + ".png");
//        turnCard.sprite = LoadSprite(@"Assets\Graphics\Cards\" + tc + ".png");
//        riverCard.sprite = LoadSprite(@"Assets\Graphics\Cards\" + rc + ".png");
//        
//        SevenCardEvaluator myEvaluator = new SevenCardEvaluator(hc1, hc2, fc1, fc2, fc3, tc, rc);
//        myBestHandText.text = myEvaluator.BestHand.HandAnalyser.HandValue.ToString();
//        
//        SevenCardEvaluator opponentEvaluator = new SevenCardEvaluator(ohc1, ohc2, fc1, fc2, fc3, tc, rc);
//        opponentBestHandText.text = opponentEvaluator.BestHand.HandAnalyser.HandValue.ToString();
//
//        int result = myEvaluator.BestHand.CompareTo(opponentEvaluator.BestHand);
//        if (result > 0) {
//            myBestHandText.color = Color.green;
//            opponentBestHandText.color = Color.red;
//
//        } else if (result == 0) {
//            myBestHandText.color = Color.gray;
//            opponentBestHandText.color = Color.gray;
//
//        } else {
//            myBestHandText.color = Color.red;
//            opponentBestHandText.color = Color.green;
//
//        }
//    }
//
//    private static Sprite LoadSprite(string filePath) {
//        Texture2D tex = LoadPNG(filePath);
//        tex.filterMode = FilterMode.Point;
//        return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
//    }
//    
//    public static Texture2D LoadPNG(string filePath) {
// 
//        Texture2D tex = null;
//        byte[] fileData;
// 
//        if (File.Exists(filePath))     {
//            fileData = File.ReadAllBytes(filePath);
//            tex = new Texture2D(2, 2);
//            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
//        }
//        return tex;
//    }
    }
}
