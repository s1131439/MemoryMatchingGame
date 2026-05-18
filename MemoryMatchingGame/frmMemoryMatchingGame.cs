using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Media;
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

        int level = 0;
        int gridSize = 2;

        Dictionary<PictureBox, int> fadeCards = new Dictionary<PictureBox, int>();

        const int FORM_WIDTH = 700;
        const int FORM_HEIGHT = 760;

        public frmMemoryMatchingGame()
        {
            InitializeComponent();

            this.ClientSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.CenterToScreen();

            LoadImages();

            SetLevel(0);

            timerFade.Interval = 30;
        }

        // =====================
        // Level 名稱
        // =====================
        private string GetLevelName(int lv)
        {
            switch (lv)
            {
                case 0: return "Practice";
                case 1: return "Level 1";
                case 2: return "Level 2";
                default: return "";
            }
        }

        // =====================
        // 時間格式
        // =====================
        private string FormatTime(int totalSeconds)
        {
            int min = totalSeconds / 60;
            int sec = totalSeconds % 60;
            return $"{min:D2}:{sec:D2}";
        }

        // =====================
        //  音效
        // =====================
        private void PlaySound(System.IO.Stream sound)
        {
            try
            {
                SoundPlayer player = new SoundPlayer(sound);
                player.Play();
            }
            catch
            {
                // 避免音效錯誤影響遊戲
            }
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
        // 設定關卡
        // =====================
        private void SetLevel(int lv)
        {
            level = lv;

            switch (level)
            {
                case 0:
                    gridSize = 2;
                    break;
                case 1:
                    gridSize = 4;
                    break;
                case 2:
                    gridSize = 6;
                    break;
            }

            RestartGame();
        }

        // =====================
        // 建立棋盤
        // =====================
        private void CreateBoard()
        {
            foreach (var pic in gameCards)
            {
                this.Controls.Remove(pic);
                pic.Dispose();
            }

            gameCards.Clear();

            int margin = 10;

            int usableWidth = this.ClientSize.Width - 40;
            int usableHeight = this.ClientSize.Height - 120;

            int cardSize = Math.Min((usableWidth - (gridSize - 1) * margin) / gridSize, (usableHeight - (gridSize - 1) * margin) / gridSize);

            int boardWidth = gridSize * cardSize + (gridSize - 1) * margin;

            int boardHeight = gridSize * cardSize + (gridSize - 1) * margin;

            int startX = (this.ClientSize.Width - boardWidth) / 2;
            int startY = (this.ClientSize.Height - boardHeight) / 2 + 20;

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
        // 重開
        // =====================
        private void RestartGame()
        {
            CreateBoard();

            timeCount = 0;
            label1.Text = GetLevelName(level);

            timer1.Stop();
            timerStarted = false;

            firstCard = null;
            secondCard = null;

            lockClick = false;

            fadeCards.Clear();

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
                gameCards[i].Enabled = true;

                gameCards[i].Image = Properties.Resources.card_back;

                gameCards[i].Tag = new Tuple<Image, bool>(cards[i], false);
            }
        }

        // =====================
        // 點擊
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
                StartFadeEffect(firstCard);
                StartFadeEffect(secondCard);

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
        // 淡化
        // =====================
        private void StartFadeEffect(PictureBox pic)
        {
            fadeCards[pic] = 255;
            pic.Enabled = false;
            timerFade.Start();
        }

        private void timerFade_Tick(object sender, EventArgs e)
        {
            List<PictureBox> completed = new List<PictureBox>();

            foreach (var item in fadeCards.ToList())
            {
                PictureBox pic = item.Key;
                int alpha = item.Value;

                alpha -= 15;

                if (alpha <= 120)
                {
                    alpha = 120;
                    completed.Add(pic);
                }

                fadeCards[pic] = alpha;

                pic.Image = SetImageOpacity(((Tuple<Image, bool>)pic.Tag).Item1, alpha / 255f);
            }

            foreach (var pic in completed)
                fadeCards.Remove(pic);

            if (fadeCards.Count == 0)
                timerFade.Stop();
        }

        private Image SetImageOpacity(Image image, float opacity)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = opacity;

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix);

                gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }

            return bmp;
        }

        // =====================
        // 計時
        // =====================
        private void timer1_Tick(object sender, EventArgs e)
        {
            timeCount++;

            label1.Text = $"{GetLevelName(level)}   Time: {FormatTime(timeCount)}";
        }

        // =====================
        // 翻回
        // =====================
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();

            var t1 = (Tuple<Image, bool>)firstCard.Tag;
            var t2 = (Tuple<Image, bool>)secondCard.Tag;

            firstCard.Image = Properties.Resources.card_back;
            secondCard.Image = Properties.Resources.card_back;

            firstCard.Tag = new Tuple<Image, bool>(t1.Item1, false);
            secondCard.Tag = new Tuple<Image, bool>(t2.Item1, false);

            firstCard = null;
            secondCard = null;

            lockClick = false;
        }

        // =====================
        // 過關
        // =====================
        private void CheckWin()
        {
            foreach (var pic in gameCards)
            {
                var tag = (Tuple<Image, bool>)pic.Tag;
                if (!tag.Item2) return;
            }

            timer1.Stop();

            int stars = GetStarCount();
            string starText = GetStarsText(stars);

            if (level == 0)
                PlaySound(Properties.Resources.practice_complete);
            else if (level == 1)
                PlaySound(Properties.Resources.level1_complete);
            else if (level == 2)
                PlaySound(Properties.Resources.level2_complete);

            MessageBox.Show($"{GetLevelName(level)} Complete!\n" + $"Time: {FormatTime(timeCount)}\n" + $"Stars: {starText}");

            if (level < 2)
            {
                SetLevel(level + 1);
            }
            else
            {
                PlaySound(Properties.Resources.all_complete);

                MessageBox.Show("Congratulations!\nAll Levels Complete!\n" + $"Final Stars: {starText}");
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("你確定要從 Practice 重新開始嗎？\n遊戲進度將不被保存。", "重新開始確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SetLevel(0);
            }
        }

        private void frmMemoryMatchingGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("你確定要退出遊戲嗎？\n遊戲進度將不被保存。", "退出確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private int GetStarCount()
        {
            int limit1, limit2;

            switch (level)
            {
                case 0: // Practice (2x2)
                    limit1 = 3;
                    limit2 = 6;
                    break;

                case 1: // Level 1 (4x4)
                    limit1 = 22;
                    limit2 = 40;
                    break;

                case 2: // Level 2 (6x6)
                    limit1 = 95;
                    limit2 = 120;
                    break;

                default:
                    return 1;
            }

            if (timeCount <= limit1) return 3;
            if (timeCount <= limit2) return 2;
            return 1;
        }

        private string GetStarsText(int stars)
        {
            switch (stars)
            {
                case 3: return "★★★";
                case 2: return "★★☆";
                case 1: return "★☆☆";
                default: return "";
            }
        }
        private void btnHint_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("確定要使用提示嗎？\n計時器將增加5秒", "提示確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            lockClick = true;

            timeCount += 5;
            label1.Text = $"{GetLevelName(level)}   Time: {FormatTime(timeCount)}";

            timer1.Stop();

            ShowHint();
        }
        private async void ShowHint()
        {
            List<PictureBox> tempRevealed = new List<PictureBox>();

            foreach (var pic in gameCards)
            {
                var tag = (Tuple<Image, bool>)pic.Tag;

                if (tag.Item2)
                    continue;

                pic.Image = tag.Item1;
                tempRevealed.Add(pic);
            }

            await System.Threading.Tasks.Task.Delay(1000);

            foreach (var pic in tempRevealed)
            {
                var tag = (Tuple<Image, bool>)pic.Tag;

                pic.Image = Properties.Resources.card_back;
                pic.Tag = new Tuple<Image, bool>(tag.Item1, false);
            }

            timer1.Start();
            lockClick = false;
        }
    }
}