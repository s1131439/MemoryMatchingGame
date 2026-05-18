using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

        // 淡化動畫用
        Dictionary<PictureBox, int> fadeCards =
            new Dictionary<PictureBox, int>();

        // 固定視窗大小（以 6x6 為基準）
        const int FORM_WIDTH = 700;
        const int FORM_HEIGHT = 760;

        public frmMemoryMatchingGame()
        {
            InitializeComponent();

            // 固定視窗大小
            this.ClientSize = new Size(FORM_WIDTH, FORM_HEIGHT);

            // 禁止調整大小
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.CenterToScreen();

            LoadImages();
            SetLevel(1);

            // timerFade 設定
            timerFade.Interval = 30;
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
                case 1:
                    gridSize = 2;
                    break;

                case 2:
                    gridSize = 4;
                    break;

                case 3:
                    gridSize = 6;
                    break;
            }

            RestartGame();
        }

        // =====================
        // 建立棋盤（自動縮放）
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

            // 預留上下空間
            int usableWidth = this.ClientSize.Width - 40;
            int usableHeight = this.ClientSize.Height - 120;

            // 動態計算卡片大小
            int cardSize = Math.Min(
                (usableWidth - (gridSize - 1) * margin) / gridSize,
                (usableHeight - (gridSize - 1) * margin) / gridSize
            );

            int boardWidth =
                gridSize * cardSize + (gridSize - 1) * margin;

            int boardHeight =
                gridSize * cardSize + (gridSize - 1) * margin;

            // 棋盤置中
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

                gameCards[i].Image =
                    Properties.Resources.card_back;

                gameCards[i].Tag =
                    new Tuple<Image, bool>(cards[i], false);
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

            // 已翻開
            if (tag.Item2) return;

            // 顯示圖片
            pic.Image = tag.Item1;

            // 更新狀態
            pic.Tag = new Tuple<Image, bool>(
                tag.Item1,
                true
            );

            // 第一張
            if (firstCard == null)
            {
                firstCard = pic;
                return;
            }

            // 第二張
            secondCard = pic;

            var img1 =
                ((Tuple<Image, bool>)firstCard.Tag).Item1;

            var img2 =
                ((Tuple<Image, bool>)secondCard.Tag).Item1;

            // 配對成功
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
        // 開始淡化動畫
        // =====================
        private void StartFadeEffect(PictureBox pic)
        {
            fadeCards[pic] = 255;

            // 已完成不能再點
            pic.Enabled = false;

            timerFade.Start();
        }

        // =====================
        // 淡化動畫 Timer
        // =====================
        private void timerFade_Tick(object sender, EventArgs e)
        {
            List<PictureBox> completed =
                new List<PictureBox>();

            foreach (var item in fadeCards.ToList())
            {
                PictureBox pic = item.Key;
                int alpha = item.Value;

                // 每次降低透明度
                alpha -= 15;

                // 最低透明度
                if (alpha <= 120)
                {
                    alpha = 120;
                    completed.Add(pic);
                }

                fadeCards[pic] = alpha;

                // 更新透明圖片
                pic.Image = SetImageOpacity(
                    ((Tuple<Image, bool>)pic.Tag).Item1,
                    alpha / 255f
                );
            }

            // 移除完成動畫
            foreach (var pic in completed)
            {
                fadeCards.Remove(pic);
            }

            // 全部完成後停止 Timer
            if (fadeCards.Count == 0)
            {
                timerFade.Stop();
            }
        }

        // =====================
        // 設定圖片透明度
        // =====================
        private Image SetImageOpacity(
            Image image,
            float opacity)
        {
            Bitmap bmp = new Bitmap(
                image.Width,
                image.Height
            );

            using (Graphics gfx =
                Graphics.FromImage(bmp))
            {
                ColorMatrix matrix =
                    new ColorMatrix();

                matrix.Matrix33 = opacity;

                ImageAttributes attributes =
                    new ImageAttributes();

                attributes.SetColorMatrix(
                    matrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap
                );

                gfx.DrawImage(
                    image,
                    new Rectangle(
                        0,
                        0,
                        bmp.Width,
                        bmp.Height
                    ),
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel,
                    attributes
                );
            }

            return bmp;
        }

        // =====================
        // 計時器
        // =====================
        private void timer1_Tick(
            object sender,
            EventArgs e)
        {
            timeCount++;

            int min = timeCount / 60;
            int sec = timeCount % 60;

            label1.Text =
                $"Level {level}   Time: {min:D2}:{sec:D2}";
        }

        // =====================
        // 翻回卡片
        // =====================
        private void timer2_Tick(
            object sender,
            EventArgs e)
        {
            timer2.Stop();

            var t1 =
                (Tuple<Image, bool>)firstCard.Tag;

            firstCard.Image =
                Properties.Resources.card_back;

            firstCard.Tag =
                new Tuple<Image, bool>(
                    t1.Item1,
                    false
                );

            var t2 =
                (Tuple<Image, bool>)secondCard.Tag;

            secondCard.Image =
                Properties.Resources.card_back;

            secondCard.Tag =
                new Tuple<Image, bool>(
                    t2.Item1,
                    false
                );

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
                var tag =
                    (Tuple<Image, bool>)pic.Tag;

                if (!tag.Item2)
                    return;
            }

            timer1.Stop();

            MessageBox.Show(
                $"Level {level} Complete!\nTime: {timeCount} sec"
            );

            if (level < 3)
            {
                SetLevel(level + 1);
            }
            else
            {
                MessageBox.Show(
                    "Congratulations!\nAll Levels Complete!"
                );
            }
        }

        // =====================
        // Restart
        // =====================
        private void restartToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "你確定要重新開始遊戲嗎？\n目前進度將不會保留。",
                    "重新開始",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (result == DialogResult.Yes)
            {
                SetLevel(1);
            }
        }

        // =====================
        // 關閉確認
        // =====================
        private void frmMemoryMatchingGame_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "你確定要離開遊戲嗎？\n遊戲紀錄將不會保留。",
                    "離開遊戲",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        // =====================
        // Exit
        // =====================
        private void exitToolStripMenuItem_Click(
            object sender,
            EventArgs e)
        {
            this.Close();
        }
    }
}