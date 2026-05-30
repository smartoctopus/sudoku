using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuWinFormsApp
{
    public class SudokuForm : Form
    {
        private TextBox[,] cells;
        private bool[,] isFixed;
        private int[,] puzzle;
        private int[,] solution;
        private TableLayoutPanel gridPanel;
        private Button clearBtn, newGameBtn;
        private Label statusLabel;
        private Label scoreLabel;
        private int score = 0;
        private bool gameCompleted = false;

        public SudokuForm()
        {
            InitializeComponent();
            LoadNewGame();
        }

        // Funzione di inizializzazione
        private void InitializeComponent()
        {
            // Imposta le proprietà della finestra
            this.Text = "Sudoku";
            this.Size = new Size(420, 530);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 250);

            // Punteggio
            scoreLabel = new Label
            {
                Location = new Point(260, 15),
                Size = new Size(140, 25),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text = "Putneggio: 0",
                ForeColor = Color.DimGray,
                BackColor = Color.Transparent
            };
            this.Controls.Add(scoreLabel);

            // Tabella del Sudoku
            gridPanel = new TableLayoutPanel
            {
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                AutoSize = true,
                Location = new Point(15, 50),
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            this.Controls.Add(gridPanel);

            for (int i = 0; i < 9; i++)
            {
                gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
                gridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            }

            // Singole celle
            cells = new TextBox[9, 9];
            isFixed = new bool[9, 9];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tb = new TextBox
                    {
                        Font = new Font("Segoe UI", 22, FontStyle.Bold),
                        TextAlign = HorizontalAlignment.Center,
                        Dock = DockStyle.Fill,
                        MaxLength = 1,
                        BorderStyle = BorderStyle.None,
                        Margin = Padding.Empty,
                        Tag = new[] { r, c }
                    };

                    // Aggiungi gli handler per l'inserimento dei numeri
                    tb.KeyPress += Tb_KeyPress;
                    tb.TextChanged += Tb_TextChanged;

                    gridPanel.Controls.Add(tb, c, r);
                    cells[r, c] = tb;
                }
            }

            // Creo un FlowLayoutPanel per i pulsanti sotto al sudoku
            // PS: un FlowLayoutPanel è un riquadro che permette di
            // centrare e allineare i pulsanti che vengono aggiunti
            int btnY = gridPanel.Bottom + 20;
            int btnPanelWidth = 240;
            FlowLayoutPanel btnPanel = new FlowLayoutPanel
            {
                Location = new Point((this.ClientSize.Width - btnPanelWidth) / 2, btnY),
                Size = new Size(btnPanelWidth, 40),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // Aggiungi il pulsante che ripulisce il sudoku
            clearBtn = new Button { Text = "Ripulisci", Size = new Size(110, 30), Margin = new Padding(5, 5, 5, 5) };
            clearBtn.Click += ClearBtn_Click;
            btnPanel.Controls.Add(clearBtn);

            // Aggiungi il pulsante per creare una nuova partita del sudoku
            newGameBtn = new Button { Text = "Nuova partita", Size = new Size(110, 30), Margin = new Padding(5, 5, 5, 5) };
            newGameBtn.Click += NewGameBtn_Click;
            btnPanel.Controls.Add(newGameBtn);

            // Aggiungi tutto il flowLayoutPanel alla finestra
            this.Controls.Add(btnPanel);

            // Status sotto i pulsanti
            statusLabel = new Label
            {
                Location = new Point(15, btnPanel.Bottom + 10),
                Size = new Size(gridPanel.Width, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            this.Controls.Add(statusLabel);

            // Modifica la grandezza della finestra per accomodare tutto il sudoku
            this.ClientSize = new Size(this.ClientSize.Width, statusLabel.Bottom + 20);
        }

        // Crea una nuova partita
        private void LoadNewGame()
        {
            // Genera un nuovo puzzle
            var (newPuzzle, newSolution) = SudokuGenerator.GeneratePuzzle(emptyCells: 45);

            puzzle = newPuzzle;
            solution = newSolution;

            // Resetta lo stato del sudoku
            score = 0;
            gameCompleted = false;
            UpdateScoreDisplay();

            // Layout delle celle
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tb = cells[r, c];
                    if (puzzle[r, c] != 0)
                    {
                        isFixed[r, c] = true; // Segnalo come valore fissato dal puzzle
                        tb.Text = puzzle[r, c].ToString();
                        tb.BackColor = Color.LightGray;
                        tb.ForeColor = Color.DimGray;
                        tb.ReadOnly = true; // Metti il textbox in read-only in modo che non sia possibile modificare la casella
                        tb.Enabled = false; // Disabilita il textbox in modo che non sia possibile selezionare la casella
                    }
                    else
                    {
                        isFixed[r, c] = false;
                        tb.Text = "";
                        tb.BackColor = Color.White;
                        tb.ForeColor = Color.Black;
                        tb.ReadOnly = false;
                        tb.Enabled = true;
                    }
                }
            }

            // Aggiorna la scritta di status sotto al sudoku
            statusLabel.Text = "Riempi le caselle con i numeri 1-9!";
            statusLabel.ForeColor = Color.Black;
        }

        // Filtra gli input che *non* sono i numeri da 1 a 9
        private void Tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar) || e.KeyChar == '0'))
                e.Handled = true;
        }

        // Imposta il valore nella cella
        private void Tb_TextChanged(object sender, EventArgs e)
        {
            if (gameCompleted) return;

            if (sender is TextBox tb && tb.Tag is int[] pos)
            {
                // Se è una delle caselle fissate, esci prima
                int r = pos[0], c = pos[1];
                if (isFixed[r, c]) return;

                string text = tb.Text;
                if (string.IsNullOrEmpty(text))
                {
                    // Il valore è stato cancellato
                    tb.BackColor = Color.White;
                }
                else if (int.TryParse(text, out int val))
                {
                    // Imposta il colore in base al numero
                    tb.BackColor = (val == solution[r, c]) ? Color.LightGreen : Color.LightCoral;
                    tb.Enabled = (val != solution[r, c]);

                    // Aggiorna il punteggio
                    score += (val == solution[r, c] ? 10 : -5);
                    UpdateScoreDisplay();
                }
            }

            // Controlla se il sudoku è completato
            CheckCompletion();
        }

        // Aggiorna il testo con il punteggio
        private void UpdateScoreDisplay()
        {
            scoreLabel.Text = $"Punteggio: {score}";
            scoreLabel.ForeColor = score >= 0 ? Color.DarkGreen : Color.DarkRed;
        }

        // Controlla se il puzzle è completato
        private void CheckCompletion()
        {
            bool allFilled = true;
            bool allCorrect = true;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    // Se non è una delle celle fissate dal puzzle
                    if (!isFixed[r, c])
                    {
                        string t = cells[r, c].Text;
                        if (string.IsNullOrEmpty(t))
                        {
                            // Se t è vuota non tutte le celle sono state completate, quindi esci prima
                            allFilled = false;
                            break;
                        }
                        if (int.TryParse(t, out int val) && val != solution[r, c])
                        {
                            // Se il valore inserito è sbagliato, esci prima
                            allCorrect = false;
                            break;
                        }
                    }
                }

                // Esci in caso di caselle errate o mancanti
                if (!allFilled || !allCorrect) break;
            }

            // Mostra il popuo solo se tutto completo e corretto
            if (allFilled && allCorrect && !gameCompleted)
            {
                gameCompleted = true;
                MessageBox.Show("🎉 Congratulazioni! Puzzle risolto!", "Sudoku completato", MessageBoxButtons.OK, MessageBoxIcon.Information);
                statusLabel.Text = "🎉 Puzzle risolto! Click 'Nuova partita' per giocare ancora.";
                statusLabel.ForeColor = Color.Green;
            }
        }

        // Ripulisci il sudoko, mantenendo lo stesso puzzle
        private void ClearBtn_Click(object sender, EventArgs e)
        {
            // Per ogni cella del sudoku
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    if (!isFixed[r, c])
                    {
                        // Se la cella non è una di quelle fissate, togli il contenuto
                        cells[r, c].Text = "";
                        cells[r, c].BackColor = Color.White;
                        cells[r, c].Enabled = true;
                    }

            // Resetta il punteggio
            score = 0;
            UpdateScoreDisplay();

            // Resetta la scritta sotto al sudoku
            statusLabel.Text = "Riempi le caselle con i numeri 1-9!";
            statusLabel.ForeColor = Color.Black;
        }

        // Carica una nuova partita
        private void NewGameBtn_Click(object sender, EventArgs e)
        {
            LoadNewGame();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SudokuForm());
        }
    }
}