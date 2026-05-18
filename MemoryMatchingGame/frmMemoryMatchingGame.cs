using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MemoryMatchingGame
{
    public partial class frmMemoryMatchingGame : Form
    {
        List<Image> images = new List<Image>();
        List<PictureBox> gameCards = new List<PictureBox>();

        Random rand = new Random();

        PictureBox firstCard = null;
        PictureBox secondCard = null;

        bool lockClick = false;

        int timeCount = 0;
        bool timerStarted = false;

        int level = 1;
        int gridSize = 2;

        public frmMemoryMatchingGame()
        {
            InitializeComponent();

            LoadImages();
            SetLevel(1);
        }

        // =====================
        // 載入圖片
        // =====================
        private void LoadImages()
        {
            images.Add(Properties.Resources.american_football);
            images.Add(Properties.Resources.badminton);
            images.Add(Properties.Resources.barbell);
            images.Add(Properties.Resources.baseball);
            images.Add(Properties.Resources.basketball);
            images.Add(Properties.Resources.bowling);
            images.Add(Properties.Resources.golf_ball);
            images.Add(Properties.Resources.medal);
            images.Add(Properties.Resources.running_shoes);
            images.Add(Properties.Resources.soccer);
            images.Add(Properties.Resources.sportswear);
            images.Add(Properties.Resources.swimming_goggles);
            images.Add(Properties.Resources.table_tennis_paddle);
            images.Add(Properties.Resources.tennis);
            images.Add(Properties.Resources.training_cone);
            images.Add(Properties.Resources.trophy);
            images.Add(Properties.Resources.volleyball);
            images.Add(Properties.Resources.whistle);
        }

        // =====================
        // 關卡 + 視窗大小（已修正）
        // =====================
        private void SetLevel(int lv)
        {
            level = lv;

            switch (level)
            {
                case 1:
                    gridSize = 2;
                    this.ClientSize = new Size(300, 380);
                    break;

                case 2:
                    gridSize = 4;
                    this.ClientSize = new Size(480, 560);
                    break;

                case 3:
                    gridSize = 6;
                    this.ClientSize = new Size(640, 660);
                    break;
            }

            // ⭐ 重點：每次切關卡都置中螢幕
            this.CenterToScreen();

            RestartGame();
        }

        // =====================
        // 建立盤面（置中）
        // =====================
        private void CreateBoard()
        {
            foreach (var pic in gameCards)
            {
                this.Controls.Remove(pic);
                pic.Dispose();
            }

            gameCards.Clear();

            int cardSize = (gridSize == 6) ? 60 : 80;
            int margin = 10;

            int boardSize =
                gridSize * cardSize + (gridSize - 1) * margin;

            int startX = (this.ClientSize.Width - boardSize) / 2;
            int startY = (this.ClientSize.Height - boardSize) / 2;

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    PictureBox pic = new PictureBox();

                    pic.Width = cardSize;
                    pic.Height = cardSize;

                    pic.Left = startX + col * (cardSize + margin);
                    pic.Top = startY + row * (cardSize + margin);

                    pic.SizeMode = PictureBoxSizeMode.StretchImage;
                    pic.BorderStyle = BorderStyle.FixedSingle;

                    pic.Click += pictureBox_Click;

                    this.Controls.Add(pic);
                    gameCards.Add(pic);
                }
            }
        }

        // =====================
        // 開始 / 重開
        // =====================
        private void RestartGame()
        {
            CreateBoard();

            timeCount = 0;
            label1.Text = $"Level {level}";

            timer1.Stop();
            timerStarted = false;

            firstCard = null;
            secondCard = null;

            lockClick = false;

            List<Image> cards = new List<Image>();

            int pairCount = (gridSize * gridSize) / 2;

            for (int i = 0; i < pairCount; i++)
            {
                cards.Add(images[i]);
                cards.Add(images[i]);
            }

            cards = cards.OrderBy(x => rand.Next()).ToList();

            for (int i = 0; i < gameCards.Count; i++)
            {
                gameCards[i].Image = Properties.Resources.card_back;
                gameCards[i].Tag = new Tuple<Image, bool>(cards[i], false);
            }
        }

        // =====================
        // 點擊卡片
        // =====================
        private void pictureBox_Click(object sender, EventArgs e)
        {
            if (!timerStarted)
            {
                timer1.Start();
                timerStarted = true;
            }

            if (lockClick) return;

            PictureBox pic = (PictureBox)sender;
            var tag = (Tuple<Image, bool>)pic.Tag;

            if (tag.Item2) return;

            pic.Image = tag.Item1;
            pic.Tag = new Tuple<Image, bool>(tag.Item1, true);

            if (firstCard == null)
            {
                firstCard = pic;
                return;
            }

            secondCard = pic;

            var img1 = ((Tuple<Image, bool>)firstCard.Tag).Item1;
            var img2 = ((Tuple<Image, bool>)secondCard.Tag).Item1;

            if (img1 == img2)
            {
                firstCard = null;
                secondCard = null;
                CheckWin();
            }
            else
            {
                lockClick = true;
                timer2.Start();
            }
        }

        // =====================
        // 計時
        // =====================
        private void timer1_Tick(object sender, EventArgs e)
        {
            timeCount++;

            int min = timeCount / 60;
            int sec = timeCount % 60;

            label1.Text = $"Level {level}  Time: {min:D2}:{sec:D2}";
        }

        // =====================
        // 翻回卡片
        // =====================
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();

            var t1 = (Tuple<Image, bool>)firstCard.Tag;
            firstCard.Image = Properties.Resources.card_back;
            firstCard.Tag = new Tuple<Image, bool>(t1.Item1, false);

            var t2 = (Tuple<Image, bool>)secondCard.Tag;
            secondCard.Image = Properties.Resources.card_back;
            secondCard.Tag = new Tuple<Image, bool>(t2.Item1, false);

            firstCard = null;
            secondCard = null;
            lockClick = false;
        }

        // =====================
        // 過關判定
        // =====================
        private void CheckWin()
        {
            foreach (var pic in gameCards)
            {
                var tag = (Tuple<Image, bool>)pic.Tag;
                if (!tag.Item2) return;
            }

            timer1.Stop();

            MessageBox.Show($"Level {level} Complete!\nTime: {timeCount} sec");

            if (level < 3)
            {
                SetLevel(level + 1);
            }
            else
            {
                MessageBox.Show("Congratulations!\nAll Levels Complete!");
            }
        }

        // =====================
        // 重置
        // =====================
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLevel(1);
        }
    }
}