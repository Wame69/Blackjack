using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Blackjack
{
    public partial class MainWindow : Window
    {
        private List<string> deck;
        private List<string> playerCards;
        private List<string> dealerCards;
        private Random random;
        private int playerBalance = 20000;
        private int currentBet = 0;
        private bool gameInProgress = false;

        public MainWindow()
        {
            InitializeComponent();
            UpdateBalanceDisplay();
            random = new Random();
        }

        private void UpdateBalanceDisplay()
        {
            PlayerBalanceText.Text = $"{playerBalance} руб";
        }

        private void CreateDeck()
        {
            deck = new List<string>();
            string[] suits = { "♠", "♥", "♦", "♣" };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            foreach (var suit in suits)
            {
                foreach (var value in values)
                {
                    deck.Add(value + suit);
                }
            }
        }

        private void ShuffleDeck()
        {
            deck = deck.OrderBy(x => random.Next()).ToList();
        }

        private string DrawCard()
        {
            if (deck.Count == 0) return null;
            string card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        private int CalculateScore(List<string> cards)
        {
            int score = 0;
            int aceCount = 0;

            foreach (var card in cards)
            {
                string rank = card.Substring(0, card.Length - 1); if (int.TryParse(rank, out int value))
                {
                    score += value;
                }
                else if (rank == "J" || rank == "Q" || rank == "K")
                {
                    score += 10;
                }
                else if (rank == "A")
                {
                    score += 11; aceCount++;
                }
            }

            while (score > 21 && aceCount > 0)
            {
                score -= 10; aceCount--;
            }

            return score;
        }

        private void PlaceBetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(BetAmountBox.Text, out currentBet) || currentBet <= 0 || currentBet > playerBalance)
            {
                MessageBox.Show("Введите корректную ставку.");
                return;
            }

            playerBalance -= currentBet;
            UpdateBalanceDisplay();
            StartNewGame();
        }

        private void StartNewGame()
        {
            ResetGameState(); CreateDeck();
            ShuffleDeck();

            playerCards = new List<string>();
            dealerCards = new List<string>();

            playerCards.Add(DrawCard());
            playerCards.Add(DrawCard());
            dealerCards.Add(DrawCard());
            dealerCards.Add(DrawCard());

            PlayerCards.Text = string.Join(", ", playerCards);
            UpdatePlayerScore();

            if (CalculateScore(playerCards) == 21)
            {
                GameStatus.Text = "Вы выиграли! У вас блэкджек!";
                playerBalance += currentBet * 2; UpdateBalanceDisplay();
                EndGame(); return;
            }

            DealerCards.Text = $"{dealerCards[0]}, [Скрытая карта]";

            gameInProgress = true;

            HitButton.IsEnabled = true;
            StandButton.IsEnabled = true;
            DoubleButton.IsEnabled = true;
            PlaceBetButton.IsEnabled = false; ContinueWithoutBetButton.IsEnabled = false;
        }


        private void ResetGameState()
        {
            playerCards = new List<string>();
            dealerCards = new List<string>();
            PlayerCards.Text = ""; DealerCards.Text = ""; PlayerScoreText.Text = "Сумма: 0"; DealerScoreText.Text = "Сумма дилера: 0"; GameStatus.Text = "";
        }

        private void UpdatePlayerScore()
        {
            int playerScore = CalculateScore(playerCards);
            PlayerScoreText.Text = $"Сумма: {playerScore}";
        }

        private void HitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!gameInProgress) return;

            playerCards.Add(DrawCard());
            PlayerCards.Text = string.Join(", ", playerCards);
            UpdatePlayerScore();

            int playerScore = CalculateScore(playerCards);

            if (playerScore > 21)
            {
                DealerCards.Text = string.Join(", ", dealerCards); GameStatus.Text = "Вы проиграли!";
                EndGame(playerScore);
            }
        }

        private void StandButton_Click(object sender, RoutedEventArgs e)
        {
            if (!gameInProgress) return;

            DealerCards.Text = string.Join(", ", dealerCards);
            int dealerScore = CalculateScore(dealerCards); DealerScoreText.Text = $"Сумма дилера: {dealerScore}";
            while (dealerScore < 17)
            {
                dealerCards.Add(DrawCard());
                dealerScore = CalculateScore(dealerCards);
                DealerCards.Text = string.Join(", ", dealerCards);
            }

            int playerScore = CalculateScore(playerCards);
            EndGame(playerScore, dealerScore);
        }

        private void EndGame(int playerScore = 0, int dealerScore = 0)
        {
            gameInProgress = false;

            HitButton.IsEnabled = false;
            StandButton.IsEnabled = false;
            DoubleButton.IsEnabled = false;

            PlaceBetButton.IsEnabled = true;
            BetAmountBox.IsEnabled = true; ContinueWithoutBetButton.IsEnabled = true;
            if (playerScore > 21)
            {
                DealerScoreText.Text = $"Сумма дилера: {dealerScore}"; GameStatus.Text = "Вы проиграли!";
                return;
            }

            if (playerScore == 21 && playerCards.Count == 2)
            {
                GameStatus.Text = "Вы выиграли! У вас блэкджек!";
                playerBalance += currentBet * 2;
                DealerScoreText.Text = $"Сумма дилера: {dealerScore}";
                UpdateBalanceDisplay();
                return;
            }

            DealerScoreText.Text = $"Сумма дилера: {dealerScore}";

            if (dealerScore > 21 || playerScore > dealerScore)
            {
                GameStatus.Text = "Вы выиграли!";
                playerBalance += currentBet * 2; DealerScoreText.Text = $"Сумма дилера: {dealerScore}";
            }
            else if (playerScore == dealerScore)
            {
                GameStatus.Text = "Ничья!";
                playerBalance += currentBet;
                DealerScoreText.Text = $"Сумма дилера: {dealerScore}";
            }
            else
            {
                GameStatus.Text = "Вы проиграли!";
            }

            UpdateBalanceDisplay();
        }



        private void DoubleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!gameInProgress || playerBalance < currentBet) return;

            playerBalance -= currentBet; currentBet *= 2; playerCards.Add(DrawCard()); PlayerCards.Text = string.Join(", ", playerCards);
            UpdatePlayerScore();

            int playerScore = CalculateScore(playerCards);
            if (playerScore > 21)
            {
                DealerCards.Text = string.Join(", ", dealerCards); int dealerScore = CalculateScore(dealerCards); DealerScoreText.Text = $"Сумма дилера: {dealerScore}"; GameStatus.Text = "Вы проиграли!"; EndGame(playerScore, dealerScore);
            }
            else
            {
                StandButton_Click(sender, e);
            }
        }





        private void ContinueWithoutBetButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewGameWithoutBet();
        }

        private void StartNewGameWithoutBet()
        {
            ResetGameState(); CreateDeck();
            ShuffleDeck();

            playerCards = new List<string>();
            dealerCards = new List<string>();

            playerCards.Add(DrawCard());
            playerCards.Add(DrawCard());
            dealerCards.Add(DrawCard());
            dealerCards.Add(DrawCard());

            PlayerCards.Text = string.Join(", ", playerCards);
            UpdatePlayerScore();

            DealerCards.Text = $"{dealerCards[0]}, [Скрытая карта]";

            gameInProgress = true;

            BetAmountBox.Text = string.Empty; currentBet = 0;
            HitButton.IsEnabled = true;
            StandButton.IsEnabled = true;
            DoubleButton.IsEnabled = false; BetAmountBox.IsEnabled = false; PlaceBetButton.IsEnabled = false; ContinueWithoutBetButton.IsEnabled = false;
        }


    }
}
